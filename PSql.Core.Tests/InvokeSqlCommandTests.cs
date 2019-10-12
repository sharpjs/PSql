using NUnit.Framework;
using static PSql.ScriptExecutor;

namespace PSql
{
    [TestFixture]
    [Parallelizable(ParallelScope.All)]
    public class InvokeSqlCommandTests
    {
        [Test]
        public void JustATest()
        {
            Execute(@"
                Invoke-Sql ""PRINT 42;""
            ");
        }
    }
}
