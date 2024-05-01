# Changes in PSql
This file documents all notable changes.

Most lines should begin with one of these words:
*Add*, *Fix*, *Update*, *Change*, *Deprecate*, *Remove*.

## [Unreleased](https://github.com/sharpjs/PSql/compare/release/2.1.0..HEAD)
- Change target to PowerShell 7.2 / .NET 6.
- Change how the module loads dependencies.  Now, the module loads dependencies
  into a private context using [the recommended technique](https://learn.microsoft.com/en-us/powershell/scripting/dev-cross-plat/resolving-dependency-conflicts)
  to prevent errors if other moduels load conflicting dependencies.
- Remove `Cmdlet`, moving its `WriteHost` method to an extension method.
- Remove `IConsole`, replacing it with a simpler `ISqlMessageLogger`.
- Update Microsoft.Data.SqlClient to [5.1.5](https://github.com/dotnet/SqlClient/blob/main/release-notes/5.1/5.1.5.md)
- Add retries on transient failures.

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
  Copyright 2023 Subatomix Research Inc.
  SPDX-License-Identifier: ISC
-->
