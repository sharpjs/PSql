/*
    Copyright 2020 Jeffrey Sharp

    Permission to use, copy, modify, and distribute this software for any
    purpose with or without fee is hereby granted, provided that the above
    copyright notice and this permission notice appear in all copies.

    THE SOFTWARE IS PROVIDED "AS IS" AND THE AUTHOR DISCLAIMS ALL WARRANTIES
    WITH REGARD TO THIS SOFTWARE INCLUDING ALL IMPLIED WARRANTIES OF
    MERCHANTABILITY AND FITNESS. IN NO EVENT SHALL THE AUTHOR BE LIABLE FOR
    ANY SPECIAL, DIRECT, INDIRECT, OR CONSEQUENTIAL DAMAGES OR ANY DAMAGES
    WHATSOEVER RESULTING FROM LOSS OF USE, DATA OR PROFITS, WHETHER IN AN
    ACTION OF CONTRACT, NEGLIGENCE OR OTHER TORTIOUS ACTION, ARISING OUT OF
    OR IN CONNECTION WITH THE USE OR PERFORMANCE OF THIS SOFTWARE.
*/

using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Linq;
using System.Management.Automation;
using FluentAssertions;
using FluentAssertions.Extensions;
using NUnit.Framework;

namespace PSql.Tests.Integration
{
    using static FormattableString;
    using static SqlCompareOptions;

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

                    INSERT @Values VALUES (
                        NULL,
                        ''           COLLATE Danish_Greenlandic_100_CI_AS,
                        'Å'          COLLATE Danish_Greenlandic_100_CI_AS,
                        'Åbcdefghij' COLLATE Danish_Greenlandic_100_CI_AS
                    );

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

                    INSERT @Values VALUES (
                        NULL,
                        ''           COLLATE Danish_Greenlandic_100_CI_AS,
                        'Å'          COLLATE Danish_Greenlandic_100_CI_AS,
                        'Åbcdefghij' COLLATE Danish_Greenlandic_100_CI_AS
                    );

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

                    INSERT @Values VALUES (
                        NULL,
                        ''           COLLATE Danish_Greenlandic_100_CI_AS,
                        'Å'          COLLATE Danish_Greenlandic_100_CI_AS,
                        'Åbcdefghij' COLLATE Danish_Greenlandic_100_CI_AS
                    );

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

                    INSERT @Values VALUES (
                        NULL,
                        ''           COLLATE Danish_Greenlandic_100_CI_AS,
                        'Å'          COLLATE Danish_Greenlandic_100_CI_AS,
                        'Åbcdefghij' COLLATE Danish_Greenlandic_100_CI_AS
                    );

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

        [Test]
        public void ProjectDateTimeOffset_UseClrTypes()
        {
            var (objects, exception) = Execute(@"
                Invoke-Sql ""
                    SELECT
                        [Null] = CONVERT(datetimeoffset(7), NULL),
                        [Min]  = CONVERT(datetimeoffset(7), '0001-01-01 00:00:00.0000000-14:00'),
                        [Max]  = CONVERT(datetimeoffset(7), '9999-12-31 23:59:59.9999999+14:00');
                ""
            ");

            exception.Should().BeNull();

            objects.Should().ContainSingle().Which.ShouldHaveProperties(p => p
                .Property("Null", default(object?))
                .Property("Min",  new DateTimeOffset(   1,  1,  1,  0,  0,  0, -14.Hours()).AddTicks(000_000_0), EqualStrictly)
                .Property("Max",  new DateTimeOffset(9999, 12, 31, 23, 59, 59, +14.Hours()).AddTicks(999_999_9), EqualStrictly)
            );
        }

        [Test]
        public void ProjectDateTimeOffset_UseSqlTypes()
        {
            var (objects, exception) = Execute(@"
                Invoke-Sql -UseSqlTypes ""
                    SELECT
                        [Null] = CONVERT(datetimeoffset(7), NULL),
                        [Min]  = CONVERT(datetimeoffset(7), '0001-01-01 00:00:00.0000000-14:00'),
                        [Max]  = CONVERT(datetimeoffset(7), '9999-12-31 23:59:59.9999999+14:00');
                ""
            ");

            exception.Should().BeNull();

            // ATTN: Does not use SQL types
            objects.Should().ContainSingle().Which.ShouldHaveProperties(p => p
                .Property("Null", default(object?))
                .Property("Min",  new DateTimeOffset(   1,  1,  1,  0,  0,  0, -14.Hours()).AddTicks(000_000_0), EqualStrictly)
                .Property("Max",  new DateTimeOffset(9999, 12, 31, 23, 59, 59, +14.Hours()).AddTicks(999_999_9), EqualStrictly)
            );
        }

        [Test]
        public void ProjectDate_UseClrTypes()
        {
            var (objects, exception) = Execute(@"
                Invoke-Sql ""
                    SELECT
                        [Null] = CONVERT(date, NULL),
                        [Min]  = CONVERT(date, '0001-01-01'),
                        [Max]  = CONVERT(date, '9999-12-31');
                ""
            ");

            exception.Should().BeNull();

            objects.Should().ContainSingle().Which.ShouldHaveProperties(p => p
                .Property("Null", default(object?))
                .Property("Min",  new DateTime(   1,  1,  1, 0, 0, 0, DateTimeKind.Unspecified), EqualStrictly)
                .Property("Max",  new DateTime(9999, 12, 31, 0, 0, 0, DateTimeKind.Unspecified), EqualStrictly)
            );
        }

        [Test]
        public void ProjectDate_UseSqlTypes()
        {
            var (objects, exception) = Execute(@"
                Invoke-Sql -UseSqlTypes ""
                    SELECT
                        [Null] = CONVERT(date, NULL),
                        [Min]  = CONVERT(date, '0001-01-01'),
                        [Max]  = CONVERT(date, '9999-12-31');
                ""
            ");

            exception.Should().BeNull();

            // ATTN: Does not use SQL types
            objects.Should().ContainSingle().Which.ShouldHaveProperties(p => p
                .Property("Null", default(object?))
                .Property("Min",  new DateTime(   1,  1,  1, 0, 0, 0, DateTimeKind.Unspecified), EqualStrictly)
                .Property("Max",  new DateTime(9999, 12, 31, 0, 0, 0, DateTimeKind.Unspecified), EqualStrictly)
            );
        }

        [Test]
        public void ProjectTime_UseClrTypes()
        {
            var (objects, exception) = Execute(@"
                Invoke-Sql ""
                    SELECT
                        [Null] = CONVERT(time(7), NULL),
                        [Min]  = CONVERT(time(7), '00:00:00.0000000'),
                        [Max]  = CONVERT(time(7), '23:59:59.9999999');
                ""
            ");

            exception.Should().BeNull();

            objects.Should().ContainSingle().Which.ShouldHaveProperties(p => p
                .Property("Null", default(object?))
                .Property("Min",  new TimeSpan( 0,  0,  0) + new TimeSpan(000_000_0L))
                .Property("Max",  new TimeSpan(23, 59, 59) + new TimeSpan(999_999_9L))
            );
        }

        [Test]
        public void ProjectTime_UseSqlTypes()
        {
            var (objects, exception) = Execute(@"
                Invoke-Sql -UseSqlTypes ""
                    SELECT
                        [Null] = CONVERT(time(7), NULL),
                        [Min]  = CONVERT(time(7), '00:00:00.0000000'),
                        [Max]  = CONVERT(time(7), '23:59:59.9999999');
                ""
            ");

            exception.Should().BeNull();

            // ATTN: Does not use SQL types
            objects.Should().ContainSingle().Which.ShouldHaveProperties(p => p
                .Property("Null", default(object?))
                .Property("Min",  new TimeSpan( 0,  0,  0) + new TimeSpan(000_000_0L))
                .Property("Max",  new TimeSpan(23, 59, 59) + new TimeSpan(999_999_9L))
            );
        }

        [Test]
        public void ProjectUniqueIdentifier_UseClrTypes()
        {
            var (objects, exception) = Execute(@"
                Invoke-Sql ""
                    SELECT
                        [Null]  = CONVERT(uniqueidentifier, NULL),
                        [Empty] = CONVERT(uniqueidentifier, '00000000-0000-0000-0000-000000000000'),
                        [Rand]  = CONVERT(uniqueidentifier, '3061c9f2-7464-4b2b-ab0d-9de762f9ef65')
                ""
            ");

            exception.Should().BeNull();

            objects.Should().ContainSingle().Which.ShouldHaveProperties(p => p
                .Property("Null",  default(object?))
                .Property("Empty", Guid.Empty)
                .Property("Rand",  new Guid("3061c9f2-7464-4b2b-ab0d-9de762f9ef65"))
            );
        }

        [Test]
        public void ProjectUniqueIdentifier_UseSqlTypes()
        {
            var (objects, exception) = Execute(@"
                Invoke-Sql -UseSqlTypes ""
                    SELECT
                        [Null]  = CONVERT(uniqueidentifier, NULL),
                        [Empty] = CONVERT(uniqueidentifier, '00000000-0000-0000-0000-000000000000'),
                        [Rand]  = CONVERT(uniqueidentifier, '3061c9f2-7464-4b2b-ab0d-9de762f9ef65')
                ""
            ");

            exception.Should().BeNull();

            objects.Should().ContainSingle().Which.ShouldHaveProperties(p => p
                .Property("Null",  SqlGuid.Null)
                .Property("Empty", new SqlGuid("00000000-0000-0000-0000-000000000000"))
                .Property("Rand",  new SqlGuid("3061c9f2-7464-4b2b-ab0d-9de762f9ef65"))
            );
        }

        [Test]
        public void ProjectHierarchyId_UseClrTypes()
        {
            // hierarchyid is not supported by .NET Standard or Core

            var (objects, exception) = Execute(@"
                Invoke-Sql ""
                    SELECT
                        [Null]  = CONVERT(hierarchyid, NULL),
                        [Root]  = CONVERT(hierarchyid, '/'),
                        [Child] = CONVERT(hierarchyid, '/0/1.2/3/');
                ""
            ");

            exception.Should().BeOfType<CmdletInvocationException>()
                .Which.InnerException.Should().BeOfType<InvalidCastException>();

            objects.Should().BeEmpty();
        }

        [Test]
        public void ProjectHierarchyId_UseSqlTypes()
        {
            // hierarchyid is not supported by .NET Standard or Core

            var (objects, exception) = Execute(@"
                Invoke-Sql -UseSqlTypes ""
                    SELECT
                        [Null]  = CONVERT(hierarchyid, NULL),
                        [Root]  = CONVERT(hierarchyid, '/'),
                        [Child] = CONVERT(hierarchyid, '/0/1.2/3/');
                ""
            ");

            exception.Should().BeOfType<CmdletInvocationException>()
                .Which.InnerException.Should().BeOfType<InvalidCastException>();

            objects.Should().BeEmpty();
        }

        private static SqlString Greenlandic(string s)
            => new SqlString(s, GreenlandicLcid, IgnoreCase | IgnoreKanaType | IgnoreWidth);

        private static bool EqualBytes(byte[] a, byte[] b)
            => a.AsSpan().SequenceEqual(b);

        private static bool EqualStrictly(DateTime a, DateTime b)
            => a.Ticks == b.Ticks
            && a.Kind  == b.Kind;

        private static bool EqualStrictly(DateTimeOffset a, DateTimeOffset b)
            => a.Ticks  == b.Ticks
            && a.Offset == b.Offset;

        private const int
            GreenlandicLcid = 1135;

        private const string
            GreenlandicCulture = "kl-GL";

        private static SqlServer Server
            => IntegrationTestsSetup.SqlServer!;

        private readonly string Prelude = Invariant($@"
            $Credential = [PSCredential]::new(
                ('{Server.Credential.UserName}'),
                ('{Server.Credential.Password}' | ConvertTo-SecureString -AsPlainText -Force)
            )
            #
            $Context = New-SqlContext -ServerPort {Server.Port} -Credential $Credential
            #
            function Invoke-Sql {{ PSql\Invoke-Sql -Context $Context @args }}
        ").Unindent();

        private (IReadOnlyList<PSObject?>, Exception?) Execute(string script)
        {
            return ScriptExecutor.Execute(Prelude + script.Unindent());
        }
    }
}
