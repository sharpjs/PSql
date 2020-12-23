using System;

namespace PSql
{
    public class SqlConnection : IDisposable
    {
        private readonly dynamic _connection;

        internal SqlConnection(Cmdlet cmdlet, SqlContext context)
        {
            var client           = PSqlClient.Instance;
            var connectionString = context.GetConnectionString();
            var credential       = context.Credential;
            var writeInformation = new Action<string>(s => cmdlet.WriteHost   (s));
            var writeWarning     = new Action<string>(s => cmdlet.WriteWarning(s));

            _connection = credential.IsNullOrEmpty()
                ? client.Connect(
                    connectionString,
                    writeInformation,
                    writeWarning
                )
                : client.Connect(
                    connectionString,
                    credential!.UserName,
                    credential!.Password,
                    writeInformation,
                    writeWarning
                );
        }

        public void Dispose()
        {
            Dispose(managed: true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool managed)
        {
            if (managed)
                _connection.Dispose();
        }
    }
}
