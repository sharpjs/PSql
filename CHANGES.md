# Changes in PSql
This file documents all notable changes.

Most lines should begin with one of these words:
*Add*, *Fix*, *Update*, *Change*, *Deprecate*, *Remove*.

## [Unreleased](https://github.com/sharpjs/PSConcurrent/compare/v2.0.0..HEAD)
- Fix `GetConnectionString` error with `-AuthenticationMode SqlPassword` and
  `SqlClientVersion.Legacy`:

  > The specified SqlClient version 'Legacy' does not support
  > authentication mode 'SqlPassword'."

- Fix `GetConnectionString` with `-AuthenticationMode Default` choosing mode
  `SqlPassword` instead of `AadIntegrated` when no credential is specified.
  This resulted in the error:

  > A credential is required when connecting to Azure SQL Database using
  > authentication mode 'SqlPassword'.

<!--
## [2.0.1](https://github.com/sharpjs/PSConcurrent/compare/v2.0.0..v2.0.1)
Future release.
-->

## [2.0.0](https://github.com/sharpjs/PSConcurrent/tree/v2.0.0)
The 2.0.0 release is a complete rewrite of PSql.
- Change module type to binary.
- Change target to PowerShell 7.0+ / .NET Core 3.1.
- Change ADO.NET implementation to [Microsoft.Data.SqlClient](https://github.com/dotnet/SqlClient).
- Add numerous SQL context properties and methods.
- Add support for Azure Active Directory authentication modes.
- Add automated build and publish via GitHub Actions.

