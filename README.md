# PSql

Cmdlets for SQL Server and Azure SQL databases.

![SELECT * FROM YourTable;](https://raw.githubusercontent.com/sharpjs/PSql/main/misc/what-does-it-do.png)

## Status

[![Build](https://github.com/sharpjs/PSql/workflows/Build/badge.svg)](https://github.com/sharpjs/PSql/actions)
[![NuGet](https://img.shields.io/powershellgallery/v/PSql.svg)](https://www.powershellgallery.com/packages/PSql)
[![NuGet](https://img.shields.io/powershellgallery/dt/PSql.svg)](https://www.powershellgallery.com/packages/PSql)

PSql 2.x is a C# rewrite of what previously was a script module.  The script
module was used for for years in production code.  PSql 2.x has seen two years
of production use too.

## Installation

PSql requires PowerShell 7.0 or later and should work on any platform where
PowerShell runs.

To install PSql from [PowerShell Gallery](https://www.powershellgallery.com/packages/PSql),
run this PowerShell command:

```powershell
Install-Module PSql
```

To update PSql, run this PowerShell command:

```powershell
Update-Module PSql
```

To check what version(s) of PSql you have installed, run this PowerShell command:

```powershell
Get-Module PSql -ListAvailable | Format-List
```

## Usage

PSql provides these cmdlets:

Name                      | Description
:-------------------------|:---------------------------------------------------
`New-SqlContext`          | Sets up connection options.
`Connect-Sql`             | Opens connections to database servers.
`Disconnect-Sql`          | Closes connections to database servers.
`Invoke-Sql`              | Runs SQL scripts.
`Expand-SqlCmdDirectives` | Preprocesses SQL scripts.

Every PSql cmdlet has built-in documentation.  To view that documentation, run
a PowerShell command like this:

```powershell
Get-Help Invoke-Sql -Full
```

The core function of PSql is to run T-SQL scripts.  It can be this easy:

```powershell
Invoke-Sql "PRINT 'Hello, world.'" -Database master
```

Or, using a pipe:

```powershell
"SELECT * FROM sys.schemas" | Invoke-Sql -Database master
```

In its simplest form, `Invoke-Sql` assumes that the machine has a local
installation of SQL Server (or compatible product), that the installation is
registered as the [default instance](https://docs.microsoft.com/en-us/sql/database-engine/configure-windows/database-engine-instances-sql-server),
and that the current user can connect using [integrated authentication](https://docs.microsoft.com/en-us/sql/relational-databases/security/choose-an-authentication-mode).
If your situation is different, you need to prepare a **SQL context** object
that specifies how to connect to a database server.

```powershell
$login = Get-Credential pgibbons

$context = New-SqlContext `
    -ServerName      initech1 `
    -DatabaseName    TpsReports `
    -Credential      $login `
    -ApplicationName "TPS Report Generator"
```

When connecting to Azure SQL Database (or compatible product), use the `-Azure`
switch, which enables some Azure-specific parameters, like resource group name
and Azure Active Directory authentication modes.

```powershell
$login = Get-Credential pgibbons

$context = New-SqlContext -Azure `
    -ResourceGroupName  initech `
    -ServerResourceName initech-db01 `
    -DatabaseName       TpsReports `
    -AuthenticationMode AadPassword `
    -Credential         $login `
    -ApplicationName    "TPS Report Generator"
```

`New-SqlContext` supports a number of other parameters that generally
correspond to settings commonly specified in connection strings.  Most of them
are optional.  See the built-in help for `New-SqlContext` for details.

Once a SQL context is prepared, using (and reusing) it is easy:

```powershell
Invoke-Sql "EXEC dbo.GenerateTpsReport" -Context $context
```

When used as above, `Invoke-Sql` opens a new connection for each invocation and
closes the connection once the invocation completes.  In many situations, that
is adequate.  However, some items, like temporary tables, disappear when their
connection closes.  When those must persist across multiple uses of
`Invoke-Sql`, it is necessary to explicitly open and close a connection.

```powershell
$connection = Connect-Sql -Context $context
try {
    Invoke-Sql "..." -Connection $connection
}
finally {
    Disconnect-Sql $connection
}
```

### SQLCMD Compatibility

`Invoke-Sql` supports a limited set of preprocessing features intended to be
compatible with the [`sqlcmd`](https://docs.microsoft.com/en-us/sql/tools/sqlcmd-utility)
utility:

Example | Description
:-- | :--
`GO` | Ends the current SQL batch and begins a new one.
`$(Foo)` | Replaced with the value of the sqlcmd variable `Foo`.
`:setvar Foo Bar`<br/>`:setvar Foo "Bar"` | Sets the value of the sqlcmd variable `Foo` to `Bar`.<br/>Enclose the value in double-quotes (`"`) if it contains whitespace.
`:r Foo.sql`<br/>`:r "Foo.sql"` | Replaced with the preprocessed contents of the file `Foo.sql`.<br/>Enclose the path in double-quotes (`"`) if it contains whitespace.<br/>Paths are relative to the current directory.

Preprocessor directives are case-insensitive.  The `GO`, `:setvar`, and `:r`
directives must appear at the beginning of a line, and no other content may
appear on that line.  `$(…)` may appear anywhere, including inside other
preprocessor directives.

To disable `Invoke-Sql` preprocessing, use the `-NoPreprocessing` switch.

### Error Handling

By default, `Invoke-Sql` wraps SQL batches in [an error-handling shim](https://github.com/sharpjs/PSql/blob/main/PSql/_Utilities/SqlErrorHandling.cs#L67-L120).
The wrapper improves the diagnostic experience by printing the batch that
caused an error.  Here is an example:

![Example showing a divide-by-zero error](https://raw.githubusercontent.com/sharpjs/PSql/main/misc/psql-error-handling.png)

There are a few known scenarios in which the error-handling wrapper can *cause*
an error, requiring the use of a workaround.  The scenarios are:

- **Multi-batch transactions.**  Transactions cannot span batches.  If a batch
  begins a transaction but does not commit it (or vice versa), the batch will
  fail with an error.

  ![Example of BEGIN TRANSACTION without COMMIT TRANSACTION](https://raw.githubusercontent.com/sharpjs/PSql/main/misc/begin-transaction-error.png)

- **Multi-batch temporary tables.**  If a batch creates a temporary table, the
  temporary table is destroyed at the end of the batch.  The temporary table is
  not visible to subsequent batches.

  ![Example of temporary table not found in subsequent batch](https://raw.githubusercontent.com/sharpjs/PSql/main/misc/temp-table-error.png)

There are two ways to work around these known issues:

- **Pass the `-NoErrorHandling` switch** to `Invoke-Sql`.  When this switch is
  used, the error-handling wrapper is omitted.  SQL batches are executed bare.
  No enhanced error-handling is performed.

- **Include this magic comment** on any line in the batch:

  ```sql
  --# NOWRAP
  ```

  The magic comment must appear at the beginning of the line, and no other
  content may appear on that line.  The comment causes `Invoke-Sql` to place
  the batch's code verbatim into the error-handling wrapper's `TRY`/`CATCH`
  block, rather than within an `EXECUTE` statement.  This prevents the issues
  described above while preserving the enhanced diagnostics provided by the
  wrapper.  The drawback is that script hygiene is no longer perfect: an error
  in the batch might interfere with the wrapper itself, preventing the
  error-handling from working as intended.

  To prevent nasty surprises with `--# NOWRAP`, use it only when required, and
  keep the batches using it as small as possible.  Examples:

  ```sql
  --# NOWRAP
  BEGIN TRANSACTION;
  GO
  ```

  ```sql
  --# NOWRAP
  CREATE TABLE #T (X int NOT NULL);
  GO
  ```

## Contributors

Many thanks to the following contributors:

**@Jezour**:
  [#1](https://github.com/sharpjs/PSql/pull/1)
