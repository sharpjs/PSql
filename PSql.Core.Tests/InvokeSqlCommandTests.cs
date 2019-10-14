using System;
using System.Data.SqlTypes;
using System.Management.Automation;
using FluentAssertions;
using NUnit.Framework;
using static PSql.ScriptExecutor;
using static System.Data.SqlTypes.SqlCompareOptions;

#nullable enable

namespace PSql
{
    [TestFixture]
    [Parallelizable(ParallelScope.All)]
    public class InvokeSqlCommandTests
    {
        [Test]
        public void ProjectBit_ToClrBoolean()
        {
            var (objects, exception) = Execute(@"
                Invoke-Sql ""
                    SELECT
                        [Null]  = CONVERT(bit, NULL),
                        [False] = CONVERT(bit,    0),
                        [True]  = CONVERT(bit,    1);
                ""
            ");

            exception.Should().BeNull();

            objects.Should().ContainSingle().Which.ShouldHaveProperties(p => p
                .Property("Null",  default(object?))
                .Property("False", false)
                .Property("True",  true)
            );
        }

        [Test]
        public void ProjectBit_ToSqlBoolean()
        {
            var (objects, exception) = Execute(@"
                Invoke-Sql -UseSqlTypes ""
                    SELECT
                        [Null]  = CONVERT(bit, NULL),
                        [False] = CONVERT(bit,    0),
                        [True]  = CONVERT(bit,    1);
                ""
            ");

            exception.Should().BeNull();

            objects.Should().ContainSingle().Which.ShouldHaveProperties(p => p
                .Property("Null",  SqlBoolean.Null)
                .Property("False", SqlBoolean.False)
                .Property("True",  SqlBoolean.True)
            );
        }

        [Test]
        public void ProjectTinyInt_ToClrByte()
        {
            var (objects, exception) = Execute(@"
                Invoke-Sql ""
                    SELECT
                        [Null] = CONVERT(tinyint, NULL),
                        [Zero] = CONVERT(tinyint,    0),
                        [Max]  = CONVERT(tinyint,  255);
                ""
            ");

            exception.Should().BeNull();

            objects.Should().ContainSingle().Which.ShouldHaveProperties(p => p
                .Property("Null", default(object?))
                .Property("Zero", default(byte))
                .Property("Max",  byte.MaxValue)
            );
        }

        [Test]
        public void ProjectTinyInt_ToSqlByte()
        {
            var (objects, exception) = Execute(@"
                Invoke-Sql -UseSqlTypes ""
                    SELECT
                        [Null] = CONVERT(tinyint, NULL),
                        [Zero] = CONVERT(tinyint,    0),
                        [Max]  = CONVERT(tinyint,  255);
                ""
            ");

            exception.Should().BeNull();

            objects.Should().ContainSingle().Which.ShouldHaveProperties(p => p
                .Property("Null", SqlByte.Null)
                .Property("Zero", SqlByte.Zero)
                .Property("Max",  SqlByte.MaxValue)
            );
        }

        [Test]
        public void ProjectSmallInt_ToClrInt16()
        {
            var (objects, exception) = Execute(@"
                Invoke-Sql ""
                    SELECT
                        [Null] = CONVERT(smallint,   NULL),
                        [Zero] = CONVERT(smallint,      0),
                        [Min]  = CONVERT(smallint, -32768),
                        [Max]  = CONVERT(smallint,  32767);
                ""
            ");

            exception.Should().BeNull();

            objects.Should().ContainSingle().Which.ShouldHaveProperties(p => p
                .Property("Null", default(object?))
                .Property("Zero", default(short))
                .Property("Min",  short.MinValue)
                .Property("Max",  short.MaxValue)
            );
        }

        [Test]
        public void ProjectSmallInt_ToSqlInt16()
        {
            var (objects, exception) = Execute(@"
                Invoke-Sql -UseSqlTypes ""
                    SELECT
                        [Null] = CONVERT(smallint,   NULL),
                        [Zero] = CONVERT(smallint,      0),
                        [Min]  = CONVERT(smallint, -32768),
                        [Max]  = CONVERT(smallint,  32767);
                ""
            ");

            exception.Should().BeNull();

            objects.Should().ContainSingle().Which.ShouldHaveProperties(p => p
                .Property("Null", SqlInt16.Null)
                .Property("Zero", SqlInt16.Zero)
                .Property("Min",  SqlInt16.MinValue)
                .Property("Max",  SqlInt16.MaxValue)
            );
        }

        [Test]
        public void ProjectInt_ToClrInt32()
        {
            var (objects, exception) = Execute(@"
                Invoke-Sql ""
                    SELECT
                        [Null] = CONVERT(int,        NULL),
                        [Zero] = CONVERT(int,           0),
                        [Min]  = CONVERT(int, -2147483648),
                        [Max]  = CONVERT(int,  2147483647);
                ""
            ");

            exception.Should().BeNull();

            objects.Should().ContainSingle().Which.ShouldHaveProperties(p => p
                .Property("Null", default(object?))
                .Property("Zero", default(int))
                .Property("Min",  int.MinValue)
                .Property("Max",  int.MaxValue)
            );
        }

        [Test]
        public void ProjectInt_ToSqlInt32()
        {
            var (objects, exception) = Execute(@"
                Invoke-Sql -UseSqlTypes ""
                    SELECT
                        [Null] = CONVERT(int,        NULL),
                        [Zero] = CONVERT(int,           0),
                        [Min]  = CONVERT(int, -2147483648),
                        [Max]  = CONVERT(int,  2147483647);
                ""
            ");

            exception.Should().BeNull();

            objects.Should().ContainSingle().Which.ShouldHaveProperties(p => p
                .Property("Null", SqlInt32.Null)
                .Property("Zero", SqlInt32.Zero)
                .Property("Min",  SqlInt32.MinValue)
                .Property("Max",  SqlInt32.MaxValue)
            );
        }

        [Test]
        public void ProjectBigInt_ToClrInt64()
        {
            var (objects, exception) = Execute(@"
                Invoke-Sql ""
                    SELECT
                        [Null] = CONVERT(bigint,                 NULL),
                        [Zero] = CONVERT(bigint,                    0),
                        [Min]  = CONVERT(bigint, -9223372036854775808),
                        [Max]  = CONVERT(bigint,  9223372036854775807);
                ""
            ");

            exception.Should().BeNull();

            objects.Should().ContainSingle().Which.ShouldHaveProperties(p => p
                .Property("Null", default(object?))
                .Property("Zero", default(long))
                .Property("Min",  long.MinValue)
                .Property("Max",  long.MaxValue)
            );
        }

        [Test]
        public void ProjectBigInt_ToSqlInt64()
        {
            var (objects, exception) = Execute(@"
                Invoke-Sql -UseSqlTypes ""
                    SELECT
                        [Null] = CONVERT(bigint,                 NULL),
                        [Zero] = CONVERT(bigint,                    0),
                        [Min]  = CONVERT(bigint, -9223372036854775808),
                        [Max]  = CONVERT(bigint,  9223372036854775807);
                ""
            ");

            exception.Should().BeNull();

            objects.Should().ContainSingle().Which.ShouldHaveProperties(p => p
                .Property("Null", SqlInt64.Null)
                .Property("Zero", SqlInt64.Zero)
                .Property("Min",  SqlInt64.MinValue)
                .Property("Max",  SqlInt64.MaxValue)
            );
        }

        [Test]
        public void ProjectDecimal_ToClrDecimal()
        {
            var (objects, exception) = Execute(@"
                Invoke-Sql ""
                    SELECT
                        [Null] = CONVERT(decimal(38, 0),                           NULL),
                        [Zero] = CONVERT(decimal(38, 0),                              0),
                        [Min]  = CONVERT(decimal(38, 0), -79228162514264337593543950335),
                        [Max]  = CONVERT(decimal(38, 0),  79228162514264337593543950335);
                        --                                |       |         |         |
                        --                                2       2         1         0
                        --                                87654321098765432109876543210
                ""
            ");

            exception.Should().BeNull();

            objects.Should().ContainSingle().Which.ShouldHaveProperties(p => p
                .Property("Null", default(object?))
                .Property("Zero", default(decimal))
                .Property("Min",  decimal.MinValue)
                .Property("Max",  decimal.MaxValue)
            );
        }

        [Test]
        public void ProjectDecimal_ToClrDecimal_TooLow()
        {
            var (objects, exception) = Execute(@"
                Invoke-Sql ""
                    SELECT [Value] = CONVERT(decimal(38, 0), -79228162514264337593543950336);
                ""
            ");

            exception.Should().BeOfType<CmdletInvocationException>()
                .Which.InnerException.Should().BeOfType<OverflowException>();

            objects.Should().BeEmpty();
        }

        [Test]
        public void ProjectDecimal_ToClrDecimal_TooHigh()
        {
            var (objects, exception) = Execute(@"
                Invoke-Sql ""
                    SELECT [Value] = CONVERT(decimal(38, 0), 79228162514264337593543950336);
                ""
            ");

            exception.Should().BeOfType<CmdletInvocationException>()
                .Which.InnerException.Should().BeOfType<OverflowException>();

            objects.Should().BeEmpty();
        }

        [Test]
        public void ProjectDecimal_ToSqlDecimal()
        {
            var (objects, exception) = Execute(@"
                Invoke-Sql -UseSqlTypes ""
                    SELECT
                        [Null] = CONVERT(decimal(38, 0),                                    NULL),
                        [Zero] = CONVERT(decimal(38, 0),                                       0),
                        [Min]  = CONVERT(decimal(38, 0), -99999999999999999999999999999999999999),
                        [Max]  = CONVERT(decimal(38, 0),  99999999999999999999999999999999999999);
                        --                                |      |         |         |         |
                        --                                3      3         2         1         0
                        --                                76543210987654321098765432109876543210
                ""
            ");

            exception.Should().BeNull();

            objects.Should().ContainSingle().Which.ShouldHaveProperties(p => p
                .Property("Null", SqlDecimal.Null)
                .Property("Zero", (SqlDecimal) 0)
                .Property("Min",  SqlDecimal.MinValue)
                .Property("Max",  SqlDecimal.MaxValue)
            );
        }

        [Test]
        [SetCulture(GreenlandicCulture)]
        public void ProjectChar_ToClrString()
        {
            var (objects, exception) = Execute(@"
                Invoke-Sql ""
                    DECLARE @Values TABLE (
                        [Null]  char(10) COLLATE Danish_Greenlandic_100_CI_AS NULL,
                        [Empty] char(10) COLLATE Danish_Greenlandic_100_CI_AS NULL,
                        [One]   char(10) COLLATE Danish_Greenlandic_100_CI_AS NULL,
                        [Full]  char(10) COLLATE Danish_Greenlandic_100_CI_AS NULL
                    );

                    INSERT @Values VALUES (NULL, '', 'Å', 'Åbcdefghij');

                    SELECT * FROM @Values;
                ""
            ");

            exception.Should().BeNull();

            objects.Should().ContainSingle().Which.ShouldHaveProperties(p => p
                .Property("Null",  default(object?))
                .Property("Empty", "          ")
                .Property("One",   "Å         ")
                .Property("Full",  "Åbcdefghij")
            );
        }

        [Test]
        [SetCulture(GreenlandicCulture)]
        public void ProjectChar_ToSqlString()
        {
            var (objects, exception) = Execute(@"
                Invoke-Sql -UseSqlTypes ""
                    DECLARE @Values TABLE (
                        [Null]  char(10) COLLATE Danish_Greenlandic_100_CI_AS NULL,
                        [Empty] char(10) COLLATE Danish_Greenlandic_100_CI_AS NULL,
                        [One]   char(10) COLLATE Danish_Greenlandic_100_CI_AS NULL,
                        [Full]  char(10) COLLATE Danish_Greenlandic_100_CI_AS NULL
                    );

                    INSERT @Values VALUES (NULL, '', 'Å', 'Åbcdefghij');

                    SELECT * FROM @Values;
                ""
            ");

            exception.Should().BeNull();

            objects.Should().ContainSingle().Which.ShouldHaveProperties(p => p
                .Property("Null",  SqlString.Null)
                .Property("Empty", Greenlandic("          "))
                .Property("One",   Greenlandic("Å         "))
                .Property("Full",  Greenlandic("Åbcdefghij"))
            );
        }

        [Test]
        [SetCulture(GreenlandicCulture)]
        public void ProjectNChar_ToClrString()
        {
            var (objects, exception) = Execute(@"
                Invoke-Sql ""
                    DECLARE @Values TABLE (
                        [Null]  nchar(10) COLLATE Danish_Greenlandic_100_CI_AS NULL,
                        [Empty] nchar(10) COLLATE Danish_Greenlandic_100_CI_AS NULL,
                        [One]   nchar(10) COLLATE Danish_Greenlandic_100_CI_AS NULL,
                        [Full]  nchar(10) COLLATE Danish_Greenlandic_100_CI_AS NULL
                    );

                    INSERT @Values VALUES (NULL, N'', N'Å', N'Åbcde何でやねん');

                    SELECT * FROM @Values;
                ""
            ");

            exception.Should().BeNull();

            objects.Should().ContainSingle().Which.ShouldHaveProperties(p => p
                .Property("Null",  default(object?))
                .Property("Empty", "          ")
                .Property("One",   "Å         ")
                .Property("Full",  "Åbcde何でやねん")
            );
        }

        [Test]
        [SetCulture(GreenlandicCulture)]
        public void ProjectNChar_ToSqlString()
        {
            var (objects, exception) = Execute(@"
                Invoke-Sql -UseSqlTypes ""
                    DECLARE @Values TABLE (
                        [Null]  nchar(10) COLLATE Danish_Greenlandic_100_CI_AS NULL,
                        [Empty] nchar(10) COLLATE Danish_Greenlandic_100_CI_AS NULL,
                        [One]   nchar(10) COLLATE Danish_Greenlandic_100_CI_AS NULL,
                        [Full]  nchar(10) COLLATE Danish_Greenlandic_100_CI_AS NULL
                    );

                    INSERT @Values VALUES (NULL, N'', N'Å', N'Åbcde何でやねん');

                    SELECT * FROM @Values;
                ""
            ");

            exception.Should().BeNull();

            objects.Should().ContainSingle().Which.ShouldHaveProperties(p => p
                .Property("Null",  SqlString.Null)
                .Property("Empty", Greenlandic("          "))
                .Property("One",   Greenlandic("Å         "))
                .Property("Full",  Greenlandic("Åbcde何でやねん"))
            );
        }

        [Test]
        [SetCulture(GreenlandicCulture)]
        public void ProjectVarChar_ToClrString()
        {
            var (objects, exception) = Execute(@"
                Invoke-Sql ""
                    DECLARE @Values TABLE (
                        [Null]  varchar(10) COLLATE Danish_Greenlandic_100_CI_AS NULL,
                        [Empty] varchar(10) COLLATE Danish_Greenlandic_100_CI_AS NULL,
                        [One]   varchar(10) COLLATE Danish_Greenlandic_100_CI_AS NULL,
                        [Full]  varchar(10) COLLATE Danish_Greenlandic_100_CI_AS NULL
                    );

                    INSERT @Values VALUES (NULL, '', 'Å', 'Åbcdefghij');

                    SELECT * FROM @Values;
                ""
            ");

            exception.Should().BeNull();

            objects.Should().ContainSingle().Which.ShouldHaveProperties(p => p
                .Property("Null",  default(object?))
                .Property("Empty", "")
                .Property("One",   "Å")
                .Property("Full",  "Åbcdefghij")
            );
        }

        [Test]
        [SetCulture(GreenlandicCulture)]
        public void ProjectVarChar_ToSqlString()
        {
            var (objects, exception) = Execute(@"
                Invoke-Sql -UseSqlTypes ""
                    DECLARE @Values TABLE (
                        [Null]  varchar(10) COLLATE Danish_Greenlandic_100_CI_AS NULL,
                        [Empty] varchar(10) COLLATE Danish_Greenlandic_100_CI_AS NULL,
                        [One]   varchar(10) COLLATE Danish_Greenlandic_100_CI_AS NULL,
                        [Full]  varchar(10) COLLATE Danish_Greenlandic_100_CI_AS NULL
                    );

                    INSERT @Values VALUES (NULL, '', 'Å', 'Åbcdefghij');

                    SELECT * FROM @Values;
                ""
            ");

            exception.Should().BeNull();

            objects.Should().ContainSingle().Which.ShouldHaveProperties(p => p
                .Property("Null",  SqlString.Null)
                .Property("Empty", Greenlandic(""))
                .Property("One",   Greenlandic("Å"))
                .Property("Full",  Greenlandic("Åbcdefghij"))
            );
        }

        [Test]
        [SetCulture(GreenlandicCulture)]
        public void ProjectNVarChar_ToClrString()
        {
            var (objects, exception) = Execute(@"
                Invoke-Sql ""
                    DECLARE @Values TABLE (
                        [Null]  nvarchar(10) COLLATE Danish_Greenlandic_100_CI_AS NULL,
                        [Empty] nvarchar(10) COLLATE Danish_Greenlandic_100_CI_AS NULL,
                        [One]   nvarchar(10) COLLATE Danish_Greenlandic_100_CI_AS NULL,
                        [Full]  nvarchar(10) COLLATE Danish_Greenlandic_100_CI_AS NULL
                    );

                    INSERT @Values VALUES (NULL, N'', N'Å', N'Åbcde何でやねん');

                    SELECT * FROM @Values;
                ""
            ");

            exception.Should().BeNull();

            objects.Should().ContainSingle().Which.ShouldHaveProperties(p => p
                .Property("Null",  default(object?))
                .Property("Empty", "")
                .Property("One",   "Å")
                .Property("Full",  "Åbcde何でやねん")
            );
        }

        [Test]
        [SetCulture(GreenlandicCulture)]
        public void ProjectNVarChar_ToSqlString()
        {
            var (objects, exception) = Execute(@"
                Invoke-Sql -UseSqlTypes ""
                    DECLARE @Values TABLE (
                        [Null]  nvarchar(10) COLLATE Danish_Greenlandic_100_CI_AS NULL,
                        [Empty] nvarchar(10) COLLATE Danish_Greenlandic_100_CI_AS NULL,
                        [One]   nvarchar(10) COLLATE Danish_Greenlandic_100_CI_AS NULL,
                        [Full]  nvarchar(10) COLLATE Danish_Greenlandic_100_CI_AS NULL
                    );

                    INSERT @Values VALUES (NULL, N'', N'Å', N'Åbcde何でやねん');

                    SELECT * FROM @Values;
                ""
            ");

            exception.Should().BeNull();

            objects.Should().ContainSingle().Which.ShouldHaveProperties(p => p
                .Property("Null",  SqlString.Null)
                .Property("Empty", Greenlandic(""))
                .Property("One",   Greenlandic("Å"))
                .Property("Full",  Greenlandic("Åbcde何でやねん"))
            );
        }

        private static SqlString Greenlandic(string s)
        {
            return new SqlString(s, GreenlandicLcid, IgnoreCase | IgnoreKanaType | IgnoreWidth);
        }

        private const int
            GreenlandicLcid = 1135;

        private const string
            GreenlandicCulture = "kl-GL";
    }
}
