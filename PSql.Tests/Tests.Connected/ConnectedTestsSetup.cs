using NUnit.Framework;

#nullable enable

namespace PSql.Tests.Connected
{
    [SetUpFixture]
    public static class ConnectedTestsSetup
    {
        internal static SqlServer? SqlServer { get; private set; }

        [OneTimeSetUp]
        public static void SetUp()
        {
            SqlServer = new SqlServer();
        }

        [OneTimeTearDown]
        public static void TearDown()
        {
            SqlServer?.Dispose();
            SqlServer = null;
        }
    }
}
