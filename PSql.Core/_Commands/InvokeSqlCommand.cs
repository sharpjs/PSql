using System.Collections;
using System.Data;
using System.Data.SqlClient;
using System.Management.Automation;

namespace PSql
{
    [Cmdlet(VerbsLifecycle.Invoke, "Sql", DefaultParameterSetName = ContextName)]
    [OutputType(typeof(PSObject[]))]
    public class InvokeSqlCommand : ConnectedCmdlet
    {
        // -Sql
        [Parameter(Position = 0, Mandatory = true, ValueFromPipeline = true)]
        public string[] Sql { get; set; }

        // -Define
        [Parameter(Position = 1)]
        public Hashtable Define { get; set; }

        // // -Raw
        // [Parameter]
        // public SwitchParameter Raw { get; set; }

        private SqlCmdPreprocessor _preprocessor;
        private SqlCommand         _command;

        protected override void BeginProcessing()
        {
            // Will open a connection if one is not already open
            base.BeginProcessing();

            // Clear any failures from prior command
            ConnectionInfo.Get(Connection).HasErrors = false;

            _preprocessor = new SqlCmdPreprocessor().WithVariables(Define);

            _command             = Connection.CreateCommand();
            _command.Connection  = Connection;
            _command.CommandType = CommandType.Text;
        }

        protected override void ProcessRecord()
        {
            var scripts = Sql;
            if (scripts == null)
                return;

            foreach (var script in scripts)
                if (!string.IsNullOrEmpty(script))
                    ProcessScript(script);
        }

        protected override void EndProcessing()
        {
            base.EndProcessing();

            if (ConnectionInfo.Get(Connection).HasErrors)
                throw new DataException("An error occurred while executing the SQL batch.");
        }

        protected override void StopProcessing()
        {
            base.StopProcessing();

            if (ConnectionInfo.Get(Connection).HasErrors)
                throw new DataException("An error occurred while executing the SQL batch.");
        }

        private void ProcessScript(string script)
        {
            foreach (var batch in _preprocessor.Process(script))
                ProcessBatch(batch);
        }

        private void ProcessBatch(string batch)
        {
            _command.CommandText = batch;

            foreach (var obj in _command.ExecuteAndProjectToPSObjects())
                WriteObject(obj);
        }

        protected override void Dispose(bool managed)
        {
            if (managed)
            {
            _command?.Dispose();
            _command = null;
        }

            base.Dispose(managed);
        }
    }
}
