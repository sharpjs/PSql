namespace PSql
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Management.Automation;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Threading;
    using System.Threading.Tasks;

    public class ModuleRunnerParameters
    {
        public string Server   { get; set; }
        public string Database { get; set; }
        public string Login    { get; set; }
        public string Password { get; set; }
        public int    Timeout  { get; set; }
        public string PSqlPath { get; set; }

        public string ScriptBlock { get; set; }
    }

    public class Module
    {
        public Module(string name)
        {
            Name     = Helpers.RequireName(name);
            Provides = new SortedSet<string>();
            Requires = new SortedSet<string>();
            Script   = new StringBuilder();    
            Provides.Add(name);
        }

        public string            Name     { get; private set; }
        public SortedSet<string> Provides { get; private set; }
        public SortedSet<string> Requires { get; private set; }
        public StringBuilder     Script   { get; private set; }
    }

    public class Subject
    {
        public Subject(string name)
        {
            Name = Helpers.RequireName(name);
            ProvidedBy = new List<Module>();
            RequiredBy = new List<Module>();
        }

        public string       Name       { get; private set; }
        public List<Module> ProvidedBy { get; private set; }
        public List<Module> RequiredBy { get; private set; }
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

        private readonly object
            _lock = new object();

        private readonly Queue<Module>
            _queue = new Queue<Module>();

        private readonly Dictionary<string, Subject>
            _subjects = new Dictionary<string, Subject>();

        private readonly ModuleRunnerParameters
            _parameters;

        private Module _module;
        private bool _ending;

        public ModuleRunner(ModuleRunnerParameters parameters)
        {
            if (parameters == null)
                throw new ArgumentNullException("parameters");

            _parameters = parameters;
            _module = new Module("init");
        }

        public void StartModule(string name)
        {
            AcceptModule();
            _module = new Module(name);
            _module.Requires.Add("init");
        }

        public void AddProvides(IEnumerable<string> names)
        {
            RequireModule().Provides.UnionWith(names);
        }

        public void AddRequires(IEnumerable<string> names)
        {
            RequireModule().Requires.UnionWith(names);
        }

        public void AddScriptLine(string text)
        {
            RequireModule().Script.AppendLine(text);
        }

        private void AcceptModule()
        {
            var module = RequireModule();

            foreach (var name in module.Provides)
                GetOrAddSubject(name).ProvidedBy.Add(module);

            foreach (var name in module.Requires)
                GetOrAddSubject(name).RequiredBy.Add(module);

            if (!module.Requires.Any())
                _queue.Enqueue(module);

            _module = null;
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
            Parallel.For(1, Environment.ProcessorCount + 1, ThreadMain);
        }

        // Thread-safe
        private Module GetNextModule(Module priorModule)
        {
            lock (_lock)
            {
                if (priorModule != null)
                    CompleteModule(priorModule);

                return DequeueModule();
            }
        }

        private Module DequeueModule()
        {
            for (;;)
            {
                if (_ending)
                    // Alredy ending; don't bother checking queue
                    return null;
                else if (_queue.Any())
                    // Queue has next module to do
                    return _queue.Dequeue();
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
            private readonly ModuleRunner _runner;
            private          Module       _module;

            internal ModuleDispenser(ModuleRunner runner)
            {
                _runner = runner;
            }

            // Thread-safe
            public Module Next()
            {
                return _module = _runner.GetNextModule(_module);
            }
        }

        private static void Echo(int id, string text)
        {
            Console.WriteLine("[Worker {0}]: {1}", id, text);
        }

        private void ThreadMain(int id)
        {
            try
            {
                Echo(id, "Starting");
                var shell = PowerShell.Create();

                var state = shell.Runspace.SessionStateProxy;
                state.SetVariable("Modules" ,  new ModuleDispenser(this));
                state.SetVariable("Server"  ,  _parameters.Server  );
                state.SetVariable("Database",  _parameters.Database);
                state.SetVariable("Login"   ,  _parameters.Login   );
                state.SetVariable("Password",  _parameters.Password);
                state.SetVariable("Timeout" ,  _parameters.Timeout );
                state.SetVariable("PSqlPath",  _parameters.PSqlPath );

                shell.AddScript(_parameters.ScriptBlock);

                var streams = shell.Streams;
                streams.Debug       .DataAdded += HandleData<DebugRecord      >(id);
                streams.Verbose     .DataAdded += HandleData<VerboseRecord    >(id);
                streams.Information .DataAdded += HandleData<InformationRecord>(id);
                streams.Warning     .DataAdded += HandleData<WarningRecord    >(id);
                streams.Error       .DataAdded += HandleData<ErrorRecord      >(id);

                shell.Invoke();
            }
            catch (Exception e)
            {
                Echo(id, e.Message);
            }
            finally
            {
                Echo(id, "Ending");
                SetEnding();
            }
        }

        private EventHandler<DataAddedEventArgs> HandleData<TRecord>(int id)
        {
            return new EventHandler<DataAddedEventArgs>
            (
                (sender, args) =>
                {
                    var records = (PSDataCollection<TRecord>) sender;
                    foreach (var record in records.ReadAll())
                        Echo(id, record.ToString());
                }
            );
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
}
