using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Host;
using System.Security;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace PSql
{
    internal static class Worker
    {
        internal const int All = -1, Any = 0;
    }

    public class Module
    {
        public const string InitModuleName = "init";

        public Module(string name)
        {
            Name     = Helpers.RequireName(name);
            Provides = new SortedSet<string>();
            Requires = new SortedSet<string>();
            Script   = new StringBuilder();    
            WorkerId = Worker.Any;

            Provides.Add(name);

            if (name != InitModuleName)
                Requires.Add(InitModuleName);
        }

        public Module(Module other, int workerId)
        {
            Name     = other.Name;
            Provides = other.Provides;
            Requires = other.Requires;
            Script   = other.Script;
            WorkerId = workerId;
        }

        public string            Name     { get; private set; }
        public SortedSet<string> Provides { get; private set; }
        public SortedSet<string> Requires { get; private set; }
        public StringBuilder     Script   { get; private set; }

        // Worker affinity:
        //   Worker.All => run on all workers
        //   Worker.Any => run on any worker
        //   some n > 0 => run on a specific worker
        public int WorkerId { get; set; }

        public bool CanRunOnWorker(int workerId)
        {
            return WorkerId == Worker.Any
                || WorkerId == workerId;
        }

        public override string ToString()
        {
            return string.Format("{0} (WorkerId: {1}, Provides: {2}, Requires: {3})",
                Name, WorkerId,
                string.Join(", ", Provides),
                string.Join(", ", Requires)
            );
        }
    }

    public class Subject
    {
        public Subject(string name)
        {
            Name       = Helpers.RequireName(name);
            ProvidedBy = new List<Module>();
            RequiredBy = new List<Module>();
        }

        public string       Name       { get; private set; }
        public List<Module> ProvidedBy { get; private set; }
        public List<Module> RequiredBy { get; private set; }

        public override string ToString()
        {
            return string.Format("{0} (ProvidedBy: {1}, RequiredBy: {2})",
                Name,
                string.Join(", ", ProvidedBy.Select(m => m.Name)),
                string.Join(", ", RequiredBy.Select(m => m.Name))
            );
        }
    }

    public class ModuleRunner
    {
        private static readonly Regex EolRegex = new Regex(
            @"\r?\n",
            RegexOptions.Compiled |
            RegexOptions.CultureInvariant
        );

        private static readonly Regex SpaceRegex = new Regex(
            @"\s+",
            RegexOptions.Compiled |
            RegexOptions.CultureInvariant
        );

        private static readonly Regex DirectiveRegex = new Regex(
            @"^ --\# \s+ (?<dir>MODULE|PROVIDES|REQUIRES): \s+ (?<args>.*) $",
            RegexOptions.Compiled |
            RegexOptions.CultureInvariant |
            RegexOptions.IgnorePatternWhitespace
        );

        private readonly Guid
            _runId = Guid.NewGuid();

        private readonly object
            _lock = new object();

        private readonly Queue<Module>
            _queue = new Queue<Module>();

        private readonly Dictionary<string, Subject>
            _subjects = new Dictionary<string, Subject>();

        private readonly string    _script;
        private readonly Hashtable _parameters;
        private readonly int       _parallelism;
        private readonly PSHost    _host;

        private Module _module;
        private bool   _ending;

        public ModuleRunner(string script, Hashtable parameters, int parallelism, PSHost host)
        {
            if (script == null)
                throw new ArgumentNullException("script");
            if (parameters == null)
                throw new ArgumentNullException("parameters");
            if (parallelism < 0)
                parallelism = Environment.ProcessorCount;
            if (host == null)
                throw new ArgumentNullException("host");

            _script      = script;
            _parameters  = parameters;
            _parallelism = parallelism;
            _host        = host;
            _module      = new Module(Module.InitModuleName);
        }

        public void StartModule(string name)
        {
            AcceptModule();
            _module = new Module(name);
        }

        public void AddProvides(IEnumerable<string> names)
        {
            RequireModule().Provides. UnionWith(names);
            RequireModule().Requires.ExceptWith(names);
        }

        public void AddRequires(IEnumerable<string> names)
        {
            RequireModule().Requires. UnionWith(names);
            RequireModule().Provides.ExceptWith(names);
        }

        public void AddScriptLine(string text)
        {
            RequireModule().Script.AppendLine(text);
        }

        public void SetRunOnAllWorkers()
        {
            RequireModule().WorkerId = Worker.All;
        }

        private void AcceptModule()
        {
            var module = RequireModule();

            if (module.WorkerId == Worker.All)
                for (var id = 1; id <= _parallelism; id++)
                    AddModule(new Module(module, id));
            else
                AddModule(module);

            _module = null;
        }

        private void AddModule(Module module)
        {
            foreach (var name in module.Provides)
                GetOrAddSubject(name).ProvidedBy.Add(module);

            foreach (var name in module.Requires)
                GetOrAddSubject(name).RequiredBy.Add(module);

            if (!module.Requires.Any())
                _queue.Enqueue(module);
        }

        private Module RequireModule()
        {
            var module = _module;
            if (module == null)
                throw new InvalidOperationException(
                    "Module definitions are frozen after Complete() is called."
                );
            return module;
        }

        private Subject GetOrAddSubject(string name)
        {
            Helpers.RequireName(name);
            Subject subject;

            if (!_subjects.TryGetValue(name, out subject))
                _subjects[name] = subject = new Subject(name);

            return subject;
        }

        public void Complete()
        {
            AcceptModule();

            var missing = string.Join
            (
                ", ",
                _subjects.Values
                    .Where(s => !s.ProvidedBy.Any())
                    .Select(s => s.Name)
                    .OrderBy(n => n)
            );

            if (missing.Any())
                throw new InvalidOperationException(
                    "Modules are required but not provided: " + missing
                );
        }

        public void Run()
        {
            Console.CancelKeyPress += HandleCancel;
            Parallel.For(1, _parallelism + 1, WorkerMain);
        }

        // Thread-safe
        private Module GetNextModule(int workerId, Module priorModule)
        {
            lock (_lock)
            {
                if (priorModule != null)
                    CompleteModule(priorModule);

                return DequeueModule(workerId);
            }
        }

        private Module DequeueModule(int workerId)
        {
            for (;;)
            {
                if (_ending)
                    // Alredy ending; don't bother checking queue
                    return null;
                else if (_queue.Any())
                    if (_queue.Peek().CanRunOnWorker(workerId))
                        // Queue has next module to do
                        return _queue.Dequeue();
                    else
                        // Next module can't run on this worker.
                        Monitor.Wait(_lock, 1000);
                else if (!_subjects.Any())
                    // Queue empty, no modules in progress
                    return null;
                else
                    // Wait for modules in progress
                    Monitor.Wait(_lock, 1000);
            }
        }

        private void CompleteModule(Module module)
        {
            var notify = false; // Notify waiting workers to get the next module.

            foreach (var name in module.Provides)
            {
                var subject = _subjects[name];

                // Mark this module as done
                subject.ProvidedBy.Remove(module);

                // Check if all subject's modules are done
                if (subject.ProvidedBy.Any())
                    continue;

                // Mark subject as done
                _subjects.Remove(name);
                if (!_subjects.Any())
                    // No more modules; wake sleeping workers so they can exit
                    notify = true;

                // Update dependents
                foreach (var dependent in subject.RequiredBy)
                {
                    // Mark requirement as met
                    dependent.Requires.Remove(name);

                    // Check if all of dependent's requrements are met
                    if (dependent.Requires.Any())
                        continue;

                    // All requirements met; queue the dependent
                    _queue.Enqueue(dependent);
                    notify = true;
                }
            }

            // Wake up sleeping workers
            if (notify)
                Monitor.PulseAll(_lock);
        }

        private void SetEnding()
        {
            lock (_lock)
            {
                _ending = true;
                Monitor.PulseAll(_lock);
            }
        }

        public class ModuleDispenser
        {
            private readonly int          _workerId;
            private readonly ModuleRunner _runner;
            private          Module       _module;

            internal ModuleDispenser(int workerId, ModuleRunner runner)
            {
                _workerId = workerId;
                _runner   = runner;
            }

            // Thread-safe
            public Module Next()
            {
                return _module = _runner.GetNextModule(_workerId, _module);
            }
        }

        private void WorkerMain(int id)
        {
            var host = new WorkerHost(_host, id);
            try
            {
                host.UI.WriteLine("Starting");
                var shell = PowerShell.Create();
                var state = shell.Runspace.SessionStateProxy;

                foreach (DictionaryEntry entry in _parameters)
                    state.SetVariable(entry.Key.ToString(), entry.Value);
                state.SetVariable("RunId",   _runId);
                state.SetVariable("Modules", new ModuleDispenser(id, this));

                var settings = new PSInvocationSettings
                {
                    Host = host,
                    ErrorActionPreference = ActionPreference.Stop
                };

                shell.AddScript(_script).Invoke(null, settings);
            }
            catch (Exception e)
            {
                host.UI.WriteErrorLine(e.Message);
            }
            finally
            {
                host.UI.WriteLine("Ended");
                SetEnding();
            }
        }

        private static void HandleCancel(object sender, ConsoleCancelEventArgs e)
        {
            Environment.Exit(1);
        }
    }

    internal class Helpers
    {
        internal static string RequireName(string name)
        {
            if (name == null)
                throw new ArgumentNullException("name");
            if (name.Length == 0)
                throw new ArgumentOutOfRangeException("name");
            return name;
        }
    }

    internal class WorkerHost : PSHost
    {
        private readonly PSHost _host;
        private readonly WorkerHostUI _ui;

        public WorkerHost(PSHost host, int workerId)
        {
            if (host == null)
                throw new ArgumentNullException("host");

            _host = host;
            _ui   = new WorkerHostUI(_host.UI, workerId);
        }

        public override Guid InstanceId
        {
            get { return _host.InstanceId; }
        }

        public override string Name
        {
            get { return _host.Name; }
        }

        public override Version Version
        {
            get { return _host.Version; }
        }

        public override PSHostUserInterface UI
        {
            get { return _ui; }
        }

        public override CultureInfo CurrentCulture
        {
            get { return _host.CurrentCulture; }
        }

        public override CultureInfo CurrentUICulture
        {
            get { return _host.CurrentUICulture; }
        }

        public override void EnterNestedPrompt()
        {
            _host.EnterNestedPrompt();
        }

        public override void ExitNestedPrompt()
        {
            _host.ExitNestedPrompt();
        }

        public override void NotifyBeginApplication()
        {
            _host.NotifyBeginApplication();
        }
        public override void NotifyEndApplication()
        {
            _host.NotifyEndApplication();
        }
        public override void SetShouldExit(int exitCode)
        {
            _host.SetShouldExit(exitCode);
        }
    }

    internal class WorkerHostUI : PSHostUserInterface
    {
        private readonly PSHostUserInterface _ui;
        private readonly int _workerId;
        private bool _bol;

        public WorkerHostUI(PSHostUserInterface ui, int workerId)
        {
            if (ui == null)
                throw new ArgumentNullException("ui");

            _ui       = ui;
            _workerId = workerId;
            _bol      = true;
        }

        public override PSHostRawUserInterface RawUI
        {
            get { return _ui.RawUI; }
        }

        private string Format(string value)
        {
            return _bol
                ? string.Format("[Worker {0}]: {1}", _workerId, value)
                : value;
        }

        public override void Write(string value)
        {
            _ui.Write(Format(value));
            _bol = value.EndsWith("\n");
        }

        public override void Write(ConsoleColor foregroundColor, ConsoleColor backgroundColor, string value)
        {
            _ui.Write(foregroundColor, backgroundColor, Format(value));
            _bol = value.EndsWith("\n");
        }

        public override void WriteLine(string value)
        {
            _ui.WriteLine(Format(value));
            _bol = true;
        }

        public override void WriteDebugLine(string message)
        {
            _ui.WriteDebugLine(Format(message));
            _bol = true;
        }

        public override void WriteVerboseLine(string message)
        {
            _ui.WriteVerboseLine(Format(message));
            _bol = true;
        }

        public override void WriteWarningLine(string message)
        {
            _ui.WriteWarningLine(Format(message));
            _bol = true;
        }

        public override void WriteErrorLine(string value)
        {
            _ui.WriteErrorLine(Format(value));
            _bol = true;
        }

        public override void WriteProgress(long sourceId, ProgressRecord record)
        {
            record.StatusDescription = Format(record.StatusDescription);
            _ui.WriteProgress(sourceId, record);
        }

        public override string ReadLine()
        {
            _bol = true;
            return _ui.ReadLine();
        }

        public override SecureString ReadLineAsSecureString()
        {
            _bol = true;
            return _ui.ReadLineAsSecureString();
        }

        public override Dictionary<string, PSObject> Prompt(string caption, string message, Collection<FieldDescription> descriptions)
        {
            return _ui.Prompt(caption, message, descriptions);
        }

        public override int PromptForChoice(string caption, string message, Collection<ChoiceDescription> choices, int defaultChoice)
        {
            return _ui.PromptForChoice(caption, message, choices, defaultChoice);
        }

        public override PSCredential PromptForCredential(string caption, string message, string userName, string targetName)
        {
            return _ui.PromptForCredential(caption, message, userName, targetName);
        }

        public override PSCredential PromptForCredential(string caption, string message, string userName, string targetName, PSCredentialTypes allowedCredentialTypes, PSCredentialUIOptions options)
        {
            return _ui.PromptForCredential(caption, message, userName, targetName, allowedCredentialTypes, options);
        }
    }
}
