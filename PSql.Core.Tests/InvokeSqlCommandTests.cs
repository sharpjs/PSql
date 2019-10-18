using System;
using System.Data.SqlTypes;
using System.Linq;
using System.Management.Automation;
using FluentAssertions;
using NUnit.Framework;
using static System.Data.SqlTypes.SqlCompareOptions;
using static PSql.ScriptExecutor;

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

        [Test]
        public void ProjectBinary_ToClrByteArray()
        {
            var (objects, exception) = Execute(@"
                Invoke-Sql ""
                    SELECT
                        [Null]  = CONVERT(binary(8), NULL),
                        [Empty] = CONVERT(binary(8), 0x),
                        [One]   = CONVERT(binary(8), 0xAA),
                        [Full]  = CONVERT(binary(8), 0xDECAFC0C0AC0FFEE);
                ""
            ");

            exception.Should().BeNull();

            objects.Should().ContainSingle().Which.ShouldHaveProperties(p => p
                .Property("Null",  default(object?))
                .Property("Empty", new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 }, EqualBytes)
                .Property("One",   new byte[] { 0xAA, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 }, EqualBytes)
                .Property("Full",  new byte[] { 0xDE, 0xCA, 0xFC, 0x0C, 0x0A, 0xC0, 0xFF, 0xEE }, EqualBytes)
            );
        }

        [Test]
        public void ProjectBinary_ToSqlBinary()
        {
            var (objects, exception) = Execute(@"
                Invoke-Sql -UseSqlTypes ""
                    SELECT
                        [Null]  = CONVERT(binary(8), NULL),
                        [Empty] = CONVERT(binary(8), 0x),
                        [One]   = CONVERT(binary(8), 0xAA),
                        [Full]  = CONVERT(binary(8), 0xDECAFC0C0AC0FFEE);
                ""
            ");

            exception.Should().BeNull();

            objects.Should().ContainSingle().Which.ShouldHaveProperties(p => p
                .Property("Null",  SqlBinary.Null)
                .Property("Empty", new SqlBinary(new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 }))
                .Property("One",   new SqlBinary(new byte[] { 0xAA, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 }))
                .Property("Full",  new SqlBinary(new byte[] { 0xDE, 0xCA, 0xFC, 0x0C, 0x0A, 0xC0, 0xFF, 0xEE }))
            );
        }

        [Test]
        public void ProjectVarBinary_ToClrByteArray()
        {
            var (objects, exception) = Execute(@"
                Invoke-Sql ""
                    SELECT
                        [Null]  = CONVERT(varbinary(8), NULL),
                        [Empty] = CONVERT(varbinary(8), 0x),
                        [One]   = CONVERT(varbinary(8), 0xAA),
                        [Full]  = CONVERT(varbinary(8), 0xDECAFC0C0AC0FFEE);
                ""
            ");

            exception.Should().BeNull();

            objects.Should().ContainSingle().Which.ShouldHaveProperties(p => p
                .Property("Null",  default(object?))
                .Property("Empty", new byte[] {                                                }, EqualBytes)
                .Property("One",   new byte[] { 0xAA                                           }, EqualBytes)
                .Property("Full",  new byte[] { 0xDE, 0xCA, 0xFC, 0x0C, 0x0A, 0xC0, 0xFF, 0xEE }, EqualBytes)
            );
        }

        [Test]
        public void ProjectVarBinary_ToSqlBinary()
        {
            var (objects, exception) = Execute(@"
                Invoke-Sql -UseSqlTypes ""
                    SELECT
                        [Null]  = CONVERT(varbinary(8), NULL),
                        [Empty] = CONVERT(varbinary(8), 0x),
                        [One]   = CONVERT(varbinary(8), 0xAA),
                        [Full]  = CONVERT(varbinary(8), 0xDECAFC0C0AC0FFEE);
                ""
            ");

            exception.Should().BeNull();

            objects.Should().ContainSingle().Which.ShouldHaveProperties(p => p
                .Property("Null",  SqlBinary.Null)
                .Property("Empty", new SqlBinary(new byte[] {                                                }))
                .Property("One",   new SqlBinary(new byte[] { 0xAA                                           }))
                .Property("Full",  new SqlBinary(new byte[] { 0xDE, 0xCA, 0xFC, 0x0C, 0x0A, 0xC0, 0xFF, 0xEE }))
            );
        }

        [Test]
        public void ProjectSmallDateTime_ToClrDateTime()
        {
            var (objects, exception) = Execute(@"
                Invoke-Sql ""
                    SELECT
                        [Null] = CONVERT(smalldatetime, NULL),
                        [Min]  = CONVERT(smalldatetime, '1900-01-01 00:00:00'),
                        [Max]  = CONVERT(smalldatetime, '2079-06-06 23:59:00');
                ""
            ");

            exception.Should().BeNull();

            objects.Should().ContainSingle().Which.ShouldHaveProperties(p => p
                .Property("Null", default(object?))
                .Property("Min",  new DateTime(1900, 1, 1,  0,  0, 0, DateTimeKind.Unspecified), EqualStrictly)
                .Property("Max",  new DateTime(2079, 6, 6, 23, 59, 0, DateTimeKind.Unspecified), EqualStrictly)
            );
        }

        [Test]
        public void ProjectSmallDateTime_ToSqlDateTime()
        {
            var (objects, exception) = Execute(@"
                Invoke-Sql -UseSqlTypes ""
                    SELECT
                        [Null] = CONVERT(smalldatetime, NULL),
                        [Min]  = CONVERT(smalldatetime, '1900-01-01 00:00:00'),
                        [Max]  = CONVERT(smalldatetime, '2079-06-06 23:59:00');
                ""
            ");

            exception.Should().BeNull();

            objects.Should().ContainSingle().Which.ShouldHaveProperties(p => p
                .Property("Null", SqlDateTime.Null)
                .Property("Min",  new SqlDateTime(1900, 1, 1,  0,  0, 0))
                .Property("Max",  new SqlDateTime(2079, 6, 6, 23, 59, 0))
            );
        }

        [Test]
        public void ProjectDateTime_ToClrDateTime()
        {
            var (objects, exception) = Execute(@"
                Invoke-Sql ""
                    SELECT
                        [Null] = CONVERT(datetime, NULL),
                        [Min]  = CONVERT(datetime, '1753-01-01 00:00:00.000'),
                        [Max]  = CONVERT(datetime, '9999-12-31 23:59:59.997');
                ""
            ");

            exception.Should().BeNull();

            objects.Should().ContainSingle().Which.ShouldHaveProperties(p => p
                .Property("Null", default(object?))
                .Property("Min",  new DateTime(1753,  1,  1,  0,  0,  0,   0, DateTimeKind.Unspecified), EqualStrictly)
                .Property("Max",  new DateTime(9999, 12, 31, 23, 59, 59, 997, DateTimeKind.Unspecified), EqualStrictly)
            );
        }

        [Test]
        public void ProjectDateTime_ToSqlDateTime()
        {
            var (objects, exception) = Execute(@"
                Invoke-Sql -UseSqlTypes ""
                    SELECT
                        [Null] = CONVERT(datetime, NULL),
                        [Min]  = CONVERT(datetime, '1753-01-01 00:00:00.000'),
                        [Max]  = CONVERT(datetime, '9999-12-31 23:59:59.997');
                ""
            ");

            exception.Should().BeNull();

            objects.Should().ContainSingle().Which.ShouldHaveProperties(p => p
                .Property("Null", SqlDateTime.Null)
                .Property("Min",  new SqlDateTime(1753,  1,  1,  0,  0,  0,   0.0))
                .Property("Max",  new SqlDateTime(9999, 12, 31, 23, 59, 59, 997.0))
            );
        }

        [Test]
        public void ProjectDateTime2_UseClrTypes()
        {
            var (objects, exception) = Execute(@"
                Invoke-Sql ""
                    SELECT
                        [Null] = CONVERT(datetime2(7), NULL),
                        [Min]  = CONVERT(datetime2(7), '0001-01-01 00:00:00.0000000'),
                        [Max]  = CONVERT(datetime2(7), '9999-12-31 23:59:59.9999999');
                ""
            ");

            exception.Should().BeNull();

            objects.Should().ContainSingle().Which.ShouldHaveProperties(p => p
                .Property("Null", default(object?))
                .Property("Min",  new DateTime(   1,  1,  1,  0,  0,  0, DateTimeKind.Unspecified).AddTicks(000_000_0), EqualStrictly)
                .Property("Max",  new DateTime(9999, 12, 31, 23, 59, 59, DateTimeKind.Unspecified).AddTicks(999_999_9), EqualStrictly)
            );
        }

        [Test]
        public void ProjectDateTime2_UseSqlTypes()
        {
            var (objects, exception) = Execute(@"
                Invoke-Sql -UseSqlTypes ""
                    SELECT
                        [Null] = CONVERT(datetime2(7), NULL),
                        [Min]  = CONVERT(datetime2(7), '0001-01-01 00:00:00.0000000'),
                        [Max]  = CONVERT(datetime2(7), '9999-12-31 23:59:59.9999999');
                ""
            ");

            exception.Should().BeNull();

            // ATTN: Does not use SQL types
            objects.Should().ContainSingle().Which.ShouldHaveProperties(p => p
                .Property("Null", default(object?))
                .Property("Min",  new DateTime(   1,  1,  1,  0,  0,  0, DateTimeKind.Unspecified).AddTicks(000_000_0), EqualStrictly)
                .Property("Max",  new DateTime(9999, 12, 31, 23, 59, 59, DateTimeKind.Unspecified).AddTicks(999_999_9), EqualStrictly)
            );
        }

        private static SqlString Greenlandic(string s)
            => new SqlString(s, GreenlandicLcid, IgnoreCase | IgnoreKanaType | IgnoreWidth);

        private static bool EqualBytes(byte[] a, byte[] b)
            => a.AsSpan().SequenceEqual(b);

        private static bool EqualStrictly(DateTime a, DateTime b)
            => a.Ticks == b.Ticks
            && a.Kind  == b.Kind;

        private const int
            GreenlandicLcid = 1135;

        private const string
            GreenlandicCulture = "kl-GL";
    }
}
