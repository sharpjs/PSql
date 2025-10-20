// Copyright Subatomix Research Inc.
// SPDX-License-Identifier: MIT

using System.Net;

namespace PSql.Integration;

using static FormattableString;
using static SecureStringHelpers;

internal class SqlServerContainer : IDisposable
{
    private const long
        ReadyWaitTime = TimeSpan.TicksPerMinute;

    private const string
        Collation     = "Latin1_General_100_CI_AI_SC_UTF8",
        MemoryLimitMb = "2048";

    public SqlServerContainer(params ushort[] ports)
    {
        Credential = new("sa", GeneratePassword());
        Id         = Start(ports);

        try
        {
            WaitForReady();
        }
        catch
        {
            Stop();
            throw;
        }
    }

    public virtual void Dispose()
    {
        Stop();
    }

    public string Id { get; }

    public NetworkCredential Credential { get; }

    private string Start(ushort[] ports)
    {
        var id = new ExternalProgram("docker")
            .WithArguments("run", "-d", "--rm", "--name", "psql-test-mssql")
            .WithArguments(Publish(ports))
            .WithArguments(
                "--env",                 "ACCEPT_EULA="           + "Y",
                "--env",                 "MSSQL_SA_PASSWORD="     + Credential.Password,
                "--env",                 "MSSQL_COLLATION="       + Collation,
                "--env",                 "MSSQL_MEMORY_LIMIT_MB=" + MemoryLimitMb,
                "--health-cmd",          "/opt/mssql-tools18/bin/sqlcmd -S . -U sa -P $MSSQL_SA_PASSWORD -No -Q 'PRINT HOST_NAME();'",
                "--health-start-period", "20s",
                "--health-interval",     "15s",
                "--health-timeout",      "10s",
                "--health-retries",      "2",
                "mcr.microsoft.com/mssql/server:2022-latest"
            )
            .Run(expecting: 0);

        id = id.TrimEnd();
        id.ShouldNotBeEmpty("docker run should output a container id");

        return id;
    }

    private static string[] Publish(ushort[] ports)
    {
        var args  = new string[ports.Length * 2];
        var index = 0;

        foreach (var port in ports)
        {
            args[index++] = "--publish";
            args[index++] = Invariant($"{port}:1433");
        }

        return args;
    }

    private void WaitForReady()
    {
        var deadline = DateTime.UtcNow + new TimeSpan(ReadyWaitTime);

        for(;;)
        {
            var isHealthy = new ExternalProgram("docker")
                .WithArguments("inspect", Id)
                .Run(expecting: 0)
                .Contains(@"""Status"": ""healthy""", StringComparison.OrdinalIgnoreCase);

            if (isHealthy)
                return;

            if (DateTime.UtcNow >= deadline)
                throw new TimeoutException(
                    "The SQL Server container did not become ready within the expected time."
                );

            Thread.Sleep(millisecondsTimeout: 1000);
        }
    }

    private void Stop()
    {
        new ExternalProgram("docker")
            .WithArguments("kill", Id)
            .Run(expecting: 0);
    }
}
