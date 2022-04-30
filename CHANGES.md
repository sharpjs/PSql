# Changes in PSql
This file documents all notable changes.

Most lines should begin with one of these words:
*Add*, *Fix*, *Update*, *Change*, *Deprecate*, *Remove*.

<!--
## [Unreleased](https://github.com/sharpjs/PSql/compare/release/2.1.0..HEAD)
-->

## [2.1.0](https://github.com/sharpjs/PSql/compare/release/2.0.1..2.1.0)
- Update Microsoft.Data.SqlClient to [4.1.0](https://github.com/dotnet/SqlClient/blob/v4.1.0/release-notes/4.1/4.1.0.md)
- Add dependency [Prequel](https://www.nuget.org/packages/Prequel), which is PSql's SQLCMD preprocessor moved to its own NuGet package.
- Add support for a line comment at end of a SQLCMD directive.

## [2.0.1](https://github.com/sharpjs/PSql/compare/release/2.0.0..release/2.0.1)
- Fix `GetConnectionString` error with `-AuthenticationMode SqlPassword` and
  `SqlClientVersion.Legacy`:

  > The specified SqlClient version 'Legacy' does not support
  > authentication mode 'SqlPassword'."

- Fix `GetConnectionString` with `-AuthenticationMode Default` choosing mode
  `SqlPassword` instead of `AadIntegrated` when no credential is specified.
  This resulted in the error:

  > A credential is required when connecting to Azure SQL Database using
  > authentication mode 'SqlPassword'.

## [2.0.0](https://github.com/sharpjs/PSql/tree/release/2.0.0)
The 2.0.0 release is a complete rewrite of PSql.
- Change module type to binary.
- Change target to PowerShell 7.0+ / .NET Core 3.1.
- Change ADO.NET implementation to [Microsoft.Data.SqlClient](https://github.com/dotnet/SqlClient).
- Add numerous SQL context properties and methods.
- Add support for Azure Active Directory authentication modes.
- Add automated build and publish via GitHub Actions.

<!--
  Copyright 2022 Jeffrey Sharp

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
-->
