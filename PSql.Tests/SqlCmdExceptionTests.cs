using NUnit.Framework;

namespace PSql
{
    [TestFixture]
    [Parallelizable(ParallelScope.All)]
    public class SqlCmdExceptionTests : ExceptionTests<SqlCmdException> { }
}
