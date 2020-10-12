# PSql

Cmdlets for SQL Server and Azure SQL databases.

## Status

[![Build](https://github.com/sharpjs/PSql/workflows/Build/badge.svg)](https://github.com/sharpjs/PSql/actions)

This is a new C# rewrite of a previous script module already used in production
code.  PSql is moving slowly towards a 2.0 release, which will occur when
documentation and test coverage is complete.

## Installation

PSql requires PowerShell 7.0 or later and should work on any platform where
PowerShell runs.

To install PSql from [PowerShell Gallery](https://www.powershellgallery.com/packages/PSql),
run this PowerShell command:

```powershell
Install-Module PSql -AllowPrerelease
```

To update PSql, run this PowerShell command:

```powershell
Update-Module PSql -AllowPrerelease
```

To check what version of PSql you have installed, run this PowerShell command:

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

Or, using pipes:

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
    -ServerName         initech-db01 `
    -DatabaseName       TpsReports `
    -AuthenticationMode AadPassword
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

TODO: Describe SQLCMD compatibility.

## Contributors

Many thanks to the following contributors:

**@Jezour**:
  [#1](https://github.com/sharpjs/PSql/pull/1)
