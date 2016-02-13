<#
.SYNOPSIS
    Performs automated deployment of a SQL Server database as defined in plain SQL scripts.

.DESCRIPTION
    There are three general modes of operation:

    -DropExisting -Create => drop existing, if any, then create and migrate
                  -Create => create and migrate
                          => migrate existing
#>
[CmdletBinding()]
param(
    # Name of the target database.
    [Parameter(Mandatory = $true)]
    [string] $Database,

    # Name of the target database server.  Must be a valid hostname or IP address, with an optional instance suffix (ex: "10.12.34.56\DEV").  A dot (".") may be used to specify a local server.
    [string] $Server = ".",

    # Drop any existing database with the same name as the target database.  Can be used only in combination with the -Create switch.
    #
    # /// WARNING /// The above option will destroy data!
    [switch] $DropExisting,

    # Create the target database.
    [switch] $Create,

    # Migrate the target database to the specified migration name.  Later migrations are reverted if already applied.  The default is to ensure that all migrations have been applied.
    [string] $TargetMigration,

    # A list of seed scripts to execute.  Seeds are executed after any migrations, in the order specified by this parameter.
    [string[]] $Seeds,

    # Full path from the current computer to a directory where the target dabase server can find backup files.  For a local server, the default is read from the registry.  For a remote server, the default is the UNC path "\\<server>\Database Backups".
    [string] $BackupDirectory,

    # Output the generated deployment script instead of executing it.
    [switch] $WhatIf,

    # Run deployment script asynchronously via a temporary SQL Server Agent job.
    [switch] $Async = $false,

    # Do not create a full backup of the target database prior to deployment.  The default is to create a backup.
    [switch] $NoBackup,

    # Do not execute migrations and seeds in transactions.  The default is to use transactions.
    [switch] $NoTransactions,

    # Restore from a cached backup if one exists.  Cache a backup after deployment to speed up redeployments of identical databases.
    [switch] $Cache,

    # Name/value pairs to define as SqlCmd variables.
    [hashtable] $Define,

    # Use SQL credentials instead of Windows authentication.  Must be used with -Password.
    [string] $Login,

    # Use SQL credentials instead of Windows authentication.  Must be used with -Login.
    [string] $Password,

    # Script timeout, in seconds.  Range is 0 to 65534.  0 disables timeout.  The default is 0.
    [int] $ScriptTimeout = 0,

    # Path to base folder of migrations and seeds. The Default is db
    [string] $BaseFolder = "db"
)

# -------------------------------------------------------------------------------------------------
# Initialization

# Terminate this script if an error occurs
$ErrorActionPreference = "Stop"

# Clean up after ourselves on exit
function CleanUp {
    # Delete temp files
    if ($HashFile -and (Test-Path $HashFile)) {
        Remove-Item $HashFile -Force -ErrorAction SilentlyContinue
    }
    if ($ScriptFile -and (Test-Path $ScriptFile)) {
        Remove-Item $ScriptFile -Force -ErrorAction SilentlyContinue
    }
    # Prune old shortcut backups
    if ($BackupDirectory) {
        Get-ChildItem $BackupDirectory -Filter 'sc-*.bak' `
        | Sort-Object LastWriteTimeUtc -Descending -ErrorAction SilentlyContinue `
        | Select-Object -Skip 5 `
        | % { Info "Pruning $_"; $_ } `
        | Remove-Item -Force -ErrorAction SilentlyContinue
    }
}
trap { CleanUp; break }

# Info: Output an informational message
function Info ([object] $Object) {
    Write-Host $Object -ForegroundColor Cyan
}

# Output startup message
Info "PSqlDeploy v0.0.1 - SQL Database Deployer for PowerShell"
Info "Target Server:   $Server"
Info "Target Database: $Database"
Info "Login Name:      $Login"

# Validate arguments
if ($DropExisting -and !$Create) {
    throw "-DropExisting can be specified only with -Create."
}

# Determine path to db
$BasePath = Resolve-Path -Path $BaseFolder -ErrorAction SilentlyContinue
if ($BasePath -eq $NULL) {
    Push-Location $PSScriptRoot
    $BasePath = Resolve-Path -Path $BaseFolder -ErrorAction SilentlyContinue
    Pop-Location
    if ($BasePath -eq $NULL) {
        throw "-BaseFolder not found."
    }
}

# Set arguments for SqlCmd
$SqlCmdArgs =
    "-I",           # enable quoted identifiers (required for some commands)
    "-b",           # terminate script if an error occurs
    "-h-1",         # do not output column headers
    "-S", $Server   # server name ("." is localhost)

# Set timeout for SqlCmd
if ($ScriptTimeout -gt 0 -and $ScriptTimeout -lt 65535) {
    $SqlCmdArgs += "-t", $ScriptTimeout
}

# Set authentication mode for SqlCmd
if ($Login -and $Password) {
    $SqlCmdArgs += "-U", $Login, "-P", $Password
} elseif (!$Login -and !$Password) {
    $SqlCmdArgs += "-E" # integrated authentication
} else {
    throw "-Login and -Password must be specified together."
}

# Define variables for SqlCmd
$Define = if ($Define) { $Define.Clone() } else { @{} }
$Define["TargetDatabase"] = $Database
$SqlCmdVars += ($Define.GetEnumerator() | % { "-v", $_.Name, "=", $_.Value })

# Invoke-Sql: Execute a SQL query string or a file.
function Invoke-Sql ([string] $Path, [string] $Query, [switch] $Master, [switch] $CanFail) {
    if (!$Path -and !$Query) { return }
    & "sqlcmd.exe" ($SqlCmdArgs +
        $(if ($Master) { "-d", "master" } else { "-d", $Database }) +
        $(if ($Path  ) { "-i", $Path    } else { "-Q", $Query    }) +
        $SqlCmdVars
    )
    if ($LASTEXITCODE) {
        if ($CanFail) { return }
        Write-Error "An error occurred while executing the SQL script." -ErrorId "ServerError" `
            -CategoryReason "ServerError" -CategoryTargetName $Query -CategoryTargetType "String"
    }
}

# -------------------------------------------------------------------------------------------------
Info "Checking if target database exists on server."

$DatabaseExists = $false

# Discover if database exists
Invoke-Sql -Master -Query @"
    SET NOCOUNT ON;
    SELECT 1 WHERE DB_ID('`$(TargetDatabase)') IS NOT NULL;
"@ | % { $DatabaseExists = $true }

# Output results and check arguments
if ($DatabaseExists) {
    Info "        * YES, target database exists."
    if ($Create -and !$DropExisting) {
        throw "-Create was specified without -DropExisting, and the target database already exists.  Cannot create the database."
    }
} else {
    Info "        * NO, target database does NOT exist."
    if (!$Create) {
        throw "Target database does not exist, and -Create was not specified.  There is nothing to do."
    }
}

# -------------------------------------------------------------------------------------------------
Info "Computing migrations to apply:"

$MigrationsUp   = [ordered]@{}
$MigrationsDown = New-Object 'System.Collections.Generic.List[System.IO.FileInfo]'

# A migration is represented by the FileInfo for its "main" script"
#     _Main.Up.sql   for up   migrations
#     _Main.Down.sql for down migrations

# ShouldApply: determines if a migration should be applied
function ShouldApply ([string] $Name) {
    !$TargetMigration -or $Name -le $TargetMigration
}

$HashFile = [IO.Path]::GetTempFileName()
Clear-Content $HashFile

# Discover defined migrations
# Add applicable migrations to shortcut detection hash
Join-Path $BasePath "Migrations\*\_Main.Up.sql" `
    | Get-Item `
    | % { $MigrationsUp.Add($_.Directory.Name, $_) | Out-Null; $_.Directory } `
    | ? { ShouldApply $_.Name } `
    | % { Get-ChildItem $_ -Recurse | Sort-Object FullName } `
    | Get-FileHash -Algorithm SHA1 | % Hash `
    | Add-Content $HashFile -Encoding UTF8

# Discover applied migrations
if ($DatabaseExists -and !$DropExisting) {
    Invoke-Sql -Query @"
        SET NOCOUNT ON;
        IF OBJECT_ID('_deploy.AppliedMigrations', 'U') IS NOT NULL
            SELECT Name FROM _deploy.AppliedMigrations ORDER BY Name;
"@  | % {
        $Name = $_.Trim()
        if (ShouldApply $Name) {
            # Applied migration that we want to keep
            $MigrationsUp.Remove($Name) | Out-Null
        } else {
            # Applied migration that we want to undo
            $Migration = $MigrationsUp[$Name]
            $MigrationsUp.Remove($Name) | Out-Null
            if (!$Migration) {
                throw "Cannot revert migration $($Name): Migration scripts not found."
            }
            $Migration = $Migration.Directory.GetFiles("_Main.Down.sql") | select -First 1
            if (!$Migration) {
                throw "Cannot revert migration $($Name): Migration scripts do not provide a down script."
            }
            $MigrationsDown.Add($Migration)
        }
    }
}

# Don't apply up migrations beyond the target migration (if specified)
$MigrationsUp = $MigrationsUp.Values | ? { ShouldApply $_.Directory.Name }

# Must apply down migrations in reverse order
$MigrationsDown.Reverse()

# Output results
if ($MigrationsUp.Count + $MigrationsDown.Count -gt 0) {
    $MigrationsDown | % { Info "        - $($_.Directory.Name)" }
    $MigrationsUp   | % { Info "        + $($_.Directory.Name)" }
} else {
    Info "        * No migrations to apply."
}

# -------------------------------------------------------------------------------------------------
Info "Verifying specified seeds:"

# Discover specified seeds
$VerifiedSeeds = $Seeds `
    | ? { [bool]$_ } `
    | % { Join-Path $BasePath "Seeds\$_\_Main.sql" } `
    | Get-Item

# Add seeds to shortcut detection hash
$VerifiedSeeds `
    | % { Get-ChildItem $_.Directory -Recurse | Sort-Object FullName } `
    | Get-FileHash -Algorithm SHA1 | % Hash `
    | Add-Content $HashFile -Encoding UTF8

# Select a backup directory
if ($BackupDirectory) {
    # Use value specified on command line
} elseif ($Server -eq ".") {
    # Ask local server for its configured backup directory
    Invoke-Sql -Master -Query @"
        SET NOCOUNT ON;
        DECLARE @Path nvarchar(4000);
        EXEC master.dbo.xp_instance_regread
            N'HKEY_LOCAL_MACHINE'
          , N'Software\Microsoft\MSSQLServer\MSSQLServer'
          , N'BackupDirectory'
          , @Path OUTPUT;
        SELECT @Path;
"@ | % { $BackupDirectory = $_.Trim() }
} else {
    # Build a UNC path that (hopefully) is shared by the server
    $BackupDirectory = "\\$Server\Database Backups"
}

# Output results
if ($VerifiedSeeds) {
    $VerifiedSeeds | % { Info "        + $($_.Directory.Name)" }
    Info "Destination for restore files:"
    Info "        > $BackupDirectory"
} else {
    Info "        * No seeds to apply."
}

# -------------------------------------------------------------------------------------------------
# Check for shotcuts

# Stop if there is no work to do
if (!$Create -and $MigrationsUp.Count + $MigrationsDown.Count + $VerifiedSeeds.Count -eq 0) {
    Info "Nothing to do."
    break
}

# Compute shortcut detection hash
$Hash = Get-FileHash $HashFile -Algorithm MD5 | % Hash

# Restore from shortcut backup if possible
if ($Create) {
    Info "Attempting to restore a shortcut backup."
    Invoke-Sql -Master -CanFail -Query @"
        SET NOCOUNT ON;
        RESTORE DATABASE [$Database]
            FROM DISK = 'sc-$Hash.bak'
            WITH RECOVERY $(if ($DropExisting) { ", REPLACE" } else { "" }) , STATS = 25;
"@
    if (!$LASTEXITCODE) { Cleanup; Exit }
}

# -------------------------------------------------------------------------------------------------
Info "Generating deployment script."

# Start generating script to a temporary file.
# This file will be deleted at exit by the CleanUp function.
$ScriptFile = [IO.Path]::GetTempFileName()
$Date       = $(Get-Date).ToUniversalTime()

# Add-SqlText: Adds text to the generated script file.
function Add-SqlText ([string] $Text) {
    Add-Content $ScriptFile $Text -Encoding UTF8
}

# (Script) preamble
Add-SqlText @"
PRINT '--------------------------------------------------------------------------------'
PRINT '>>> PSqlDeploy Database Deployment Script'
PRINT '--------------------------------------------------------------------------------'
PRINT 'Date Executed:    ' + CONVERT(varchar(20), SYSUTCDATETIME(), 120) + ' UTC';
PRINT 'Target Server:    ' + @@SERVERNAME;
PRINT 'Target Database:  `$(TargetDatabase)';
PRINT '--------------------------------------------------------------------------------'
SET NOCOUNT ON;
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
SET XACT_ABORT ON;
SET TRANSACTION ISOLATION LEVEL SERIALIZABLE;
GO
"@

# (Script) Acquire exclusive access to existing database
if ($DropExisting -or !$Create) { Add-SqlText @"

-- ---------------------------------------------------------------------------------------------------------
-- Set existing database, if it exists, to admin-only mode, killing connections

IF DB_ID('`$(TargetDatabase)') IS NOT NULL
BEGIN
    RAISERROR ('Set existing database to admin-only mode, killing connections', 0, 0) WITH NOWAIT;
    ALTER DATABASE [`$(TargetDatabase)] SET RESTRICTED_USER WITH ROLLBACK IMMEDIATE;
END;
GO
"@
}

# (Script) Back up existing database
if (!$NoBackup) { Add-SqlText @"

-- ---------------------------------------------------------------------------------------------------------
-- Back up existing database, if it exists

IF DB_ID('`$(TargetDatabase)') IS NOT NULL
BEGIN
    RAISERROR ('Back up existing database to `$(TargetDatabase)-$($Date.ToString("yyyy-MM-dd-HHmmss")).bak', 0, 0) WITH NOWAIT;
    BACKUP DATABASE [`$(TargetDatabase)]
        TO DISK = '`$(TargetDatabase)-$($Date.ToString("yyyy-MM-dd-HHmmss")).bak'
        WITH FORMAT;
END;
GO
"@
}

# (Script) Drop existing database
if ($DropExisting) { Add-SqlText @"

-- ---------------------------------------------------------------------------------------------------------
-- Drop existing database, if it exists

IF DB_ID('`$(TargetDatabase)') IS NOT NULL
BEGIN
    RAISERROR ('Drop existing database', 0, 0) WITH NOWAIT;
    DROP DATABASE [`$(TargetDatabase)];
END;
GO
"@
}

# (Script) Create new database
if ($Create) { Add-SqlText @"

-- ---------------------------------------------------------------------------------------------------------
RAISERROR ('Create database `$(TargetDatabase)', 0, 0) WITH NOWAIT;

CREATE DATABASE [`$(TargetDatabase)];
GO
-- Allow transaction log to settle
WAITFOR DELAY '00:00:01';
GO
"@
}

# (Script) Set current database to created database
Add-SqlText @"

-- ---------------------------------------------------------------------------------------------------------
RAISERROR ('Set current database to `$(TargetDatabase)', 0, 0) WITH NOWAIT;

USE [`$(TargetDatabase)];
GO
"@

# (Script) Acquire exclusive access to created database
if ($Create) { Add-SqlText @"

-- ---------------------------------------------------------------------------------------------------------
RAISERROR ('Set current database to admin-only mode, killing connections', 0, 0) WITH NOWAIT;

ALTER DATABASE CURRENT SET RESTRICTED_USER WITH ROLLBACK IMMEDIATE;
GO
"@
}

# (Script) Begin transaction
if (!$NoTransactions) { Add-SqlText @"

-- ---------------------------------------------------------------------------------------------------------
-- Begin transaction

RAISERROR ('Begin transaction', 0, 0) WITH NOWAIT;
BEGIN TRANSACTION;
GO
"@
}

# (Script) Begin transaction, ensure that _deploy schema exists
Add-SqlText @"
-- ---------------------------------------------------------------------------------------------------------
RAISERROR ('Create deployment helpers', 0, 0) WITH NOWAIT;

IF SCHEMA_ID('_deploy') IS NULL
EXEC('
    CREATE SCHEMA _deploy AUTHORIZATION dbo;
');

-- ---------------------------------------------------------------------------------------------------------
-- Ensure registry of applied migrations exists

IF OBJECT_ID('_deploy.AppliedMigrations', 'U') IS NULL
EXEC('
    CREATE TABLE _deploy.AppliedMigrations
    (
        Name
            nvarchar(256)
            NOT NULL
            CONSTRAINT AppliedMigrations_PK
                PRIMARY KEY

      , DateApplied
            datetime2(0)
            NOT NULL
            CONSTRAINT AppliedMigrations_DF_DateApplied
                DEFAULT SYSUTCDATETIME()
    );
');

-- ---------------------------------------------------------------------------------------------------------
-- ConnectionId: Gets an id that is unique for the connection and session

IF OBJECT_ID('_deploy.ConnectionId', 'FN') IS NULL
EXEC('
    CREATE FUNCTION _deploy.ConnectionId()
    RETURNS uniqueidentifier
    BEGIN
        RETURN
        (
            SELECT connection_id
            FROM sys.dm_exec_connections
            WHERE session_id = @@SPID
        );
    END;
');

-- ---------------------------------------------------------------------------------------------------------
-- Replacement: Text replacements applied by _deploy.Do

IF OBJECT_ID('_deploy.Replacement', 'U') IS NULL
EXEC('
    CREATE TABLE _deploy.Replacement
    (
        Id
            int IDENTITY
            NOT NULL
            CONSTRAINT Replacement_PK
                PRIMARY KEY (Id)

      , ConnectionId
            uniqueidentifier
            NOT NULL
            CONSTRAINT Replacement_DF_ConnectionId
                DEFAULT _deploy.ConnectionId()

      , Name
            sysname
            COLLATE Latin1_General_100_BIN2
            NOT NULL

      , Sql
            nvarchar(max)
            NOT NULL

      , CONSTRAINT Replacement_UQ_0
            UNIQUE (ConnectionId, Id)
    );
');

-- ---------------------------------------------------------------------------------------------------------
-- Abort: Aborts the entire session

IF OBJECT_ID('_deploy.Abort', 'P') IS NOT NULL
EXEC('
    DROP PROCEDURE _deploy.Abort;
');
EXEC('
    CREATE PROCEDURE _deploy.Abort
        @Message nvarchar(max) = NULL
    AS BEGIN
        IF ISNULL(@Message, '''') != ''''
        BEGIN
            PRINT '''';
            PRINT @Message;
        END;

        RAISERROR ('''', 0, 0) WITH NOWAIT; -- Send buffered messages to client
        SELECT CONVERT(int, ''ABORT'');     -- Poison pill to abort entire session
    END;
');

-- ---------------------------------------------------------------------------------------------------------
-- Do: Executes a SQL string with placeholder replacement and enhanced error handling

IF OBJECT_ID('_deploy.Do', 'P') IS NOT NULL
EXEC('
    DROP PROCEDURE _deploy.Do;
');
EXEC('
    CREATE PROCEDURE _deploy.Do
        @Sql         nvarchar(max)
      , @Message     nvarchar(max) = NULL
      , @Database    sysname       = NULL
      , @Transaction bit           = 1
    AS BEGIN
        IF ISNULL(@Sql, '''') = ''''
            RETURN;

        IF @Message IS NOT NULL
            RAISERROR(''%s'', 0, 0, @Message) WITH NOWAIT;

        DECLARE @StartTime datetime2(7) = SYSUTCDATETIME();

        -- Perform placeholder replacement
        UPDATE _deploy.Replacement
        SET @Sql = REPLACE(@Sql COLLATE Latin1_General_100_BIN2, Name, Sql)
        WHERE ConnectionId = _deploy.ConnectionId();

        DECLARE @Batch nvarchar(max)
            = CASE WHEN @Transaction = 0
                -- If non-transactional, commit any pending transaction
                THEN ''
                    DECLARE @PriorTranCount int = @@TRANCOUNT;
                    WHILE @@TRANCOUNT > 0
                        COMMIT TRANSACTION;
                ''
                ELSE ''''
              END
            + CASE WHEN LEN(@Database) > 0
                -- Temporarily switch to different database
                THEN ''
                    USE '' + QUOTENAME(@Database) + '';
                ''
                ELSE ''''
              END
            + CASE WHEN @Transaction = 0 OR LEN(@Database) > 0
                -- Wrap in EXEC if we want to avoid clobbering parent scope
                THEN ''
                    EXEC(N'''''' + REPLACE(@Sql, '''''''', '''''''''''') + '''''');
                ''
                ELSE @Sql
              END
            + CASE WHEN @Transaction = 0
                -- If non-transactional, restore previous pending transaction count
                THEN ''
                    WHILE @@TRANCOUNT < @PriorTranCount
                        BEGIN TRANSACTION;
                ''
                ELSE ''''
              END
        ;

        BEGIN TRY
            EXEC sp_executesql @Batch;
        END TRY
        BEGIN CATCH
            PRINT ''''
            PRINT ''An error occurred while executing this batch:'';

            -- Print offending batch
            IF LEFT(@Sql, 2) != CHAR(13) + CHAR(10)
                PRINT '''';
            PRINT @Sql;
            IF RIGHT(@Sql, 2) != CHAR(13) + CHAR(10)
                PRINT '''';

            -- Print error info
            PRINT ''Msg ''     + CONVERT(nvarchar, ERROR_NUMBER())
                + '', Level '' + CONVERT(nvarchar, ERROR_SEVERITY())
                + '', State '' + CONVERT(nvarchar, ERROR_STATE())
                + ISNULL('', Procedure '' + ERROR_PROCEDURE(), '''')
                + '', Line ''  + CONVERT(nvarchar, ERROR_LINE());
            PRINT ERROR_MESSAGE();

            -- Abort session
            RAISERROR ('''', 0, 0) WITH NOWAIT; -- Send buffered messages to client
            SELECT CONVERT(int, ''ABORT'');     -- Poison pill to abort entire session
        END CATCH;

        DECLARE @Elapsed int = DATEDIFF(ms, @StartTime, SYSUTCDATETIME());
        RAISERROR(''%9dms batch runtime'', 0, 0, @Elapsed) WITH NOWAIT;
    END;
');

GO
"@

# Add-SqlFile: Add an external file to the generated script (via sqlcmd include)
function Add-SqlFile ([System.IO.FileInfo] $File) {

if (!$NoTransactions) { Add-SqlText @"

-- ---------------------------------------------------------------------------------------------------------
-- Ensure transaction

IF @@TRANCOUNT = 0
BEGIN
    RAISERROR ('Begin transaction', 0, 0) WITH NOWAIT;
    BEGIN TRANSACTION;
END;
GO
"@
}

Add-SqlText @"

-- ---------------------------------------------------------------------------------------------------------
RAISERROR ('--------------------------------------------------------------------------------', 0, 0) WITH NOWAIT;
RAISERROR ('>>> [$($File.Directory.Parent.Name): $($File.Directory.Name)]', 0, 0) WITH NOWAIT;

:setvar Path "$($File.Directory.FullName)"
:r `$(Path)\$($File.Name)
GO
"@
}

# Add-MigrationRegistration: Add code to the generated script to register an applied migration
function Add-MigrationRegistration ([System.IO.FileInfo] $Migration) { Add-SqlText @"

RAISERROR ('Register migration', 0, 0) WITH NOWAIT;
INSERT _deploy.AppliedMigrations (Name)
VALUES ('$($_.Directory.Name)');
GO
"@
}

# Add-MigrationUnregistration: Add code to the generated script to unregister a reverted migration
function Add-MigrationUnregistration ([System.IO.FileInfo] $Migration) { Add-SqlText @"

RAISERROR ('Unregister migration', 0, 0) WITH NOWAIT;
DELETE _deploy.AppliedMigrations
WHERE Name = '$($_.Directory.Name)';
GO
"@
}

# (Script) Apply migrations
if ($MigrationsUp.Count + $MigrationsDown.Count -gt 0) {
    Join-Path $BasePath "Migrations\_Begin\_Main.sql" | ? { Test-Path $_ } | Get-Item | % { Add-SqlFile $_ }
    $MigrationsDown | % { Add-SqlFile $_ ; Add-MigrationUnregistration $_ }
    $MigrationsUp   | % { Add-SqlFile $_ ; Add-MigrationRegistration   $_ }
    Join-Path $BasePath "Migrations\_End\_Main.sql" | ? { Test-Path $_ } | Get-Item | % { Add-SqlFile $_ }
}

# (Script) Commit transaction
Add-SqlText @"

-- ---------------------------------------------------------------------------------------------------------
RAISERROR ('--------------------------------------------------------------------------------', 0, 0) WITH NOWAIT;
-- Commit transaction

WHILE @@TRANCOUNT > 0
BEGIN
    RAISERROR ('Commit transaction', 0, 0) WITH NOWAIT;
    COMMIT TRANSACTION;
END;
GO
"@

# (Script) Release exclusive access to database
Add-SqlText @"

-- ---------------------------------------------------------------------------------------------------------
RAISERROR ('Set current database to multi-user mode', 0, 0) WITH NOWAIT;

ALTER DATABASE CURRENT SET MULTI_USER;
GO
"@

# (Script) Apply seeds
$VerifiedSeeds | % { Add-SqlFile $_ }

# (Script) Commit transaction
Add-SqlText @"

-- ---------------------------------------------------------------------------------------------------------
-- Finalize

PRINT '--------------------------------------------------------------------------------';
GO

-- Commit transaction
WHILE @@TRANCOUNT > 0
BEGIN
    RAISERROR ('Commit transaction', 0, 0) WITH NOWAIT;
    COMMIT TRANSACTION;
END;
"@

# (Script) Create shortcut backup
if ($Cache) { Add-SqlText @"

-- Take a backup to shortcut future deployments
PRINT 'Taking a backup to shortcut future deployments.';
GO

BACKUP DATABASE [`$(TargetDatabase)]
    TO DISK = 'sc-$Hash.bak'
    WITH INIT, FORMAT, COMPRESSION;
"@ }

# -------------------------------------------------------------------------------------------------
# Preprocess the Script File

function Expand-SqlIncludes ([string] $Path) {
    (Get-Content $Path -Encoding UTF8 -ReadCount 1000) | %{ $_ <# flatten #> } | % {
        foreach ($Name in $Define.Keys) {
            $_ = $_ -replace "\$\($Name\)", $Define[$Name]
        }
        if ($_ -imatch '^:r\s+(.*?)\s*$') {
            Expand-SqlIncludes $Matches[1]
        } elseif ($_ -imatch '^:setvar\s+(\w+)\s+("?)(.*)\2\s*$') {
            $Define[$Matches[1]] = $Matches[3]
        } else {
            Write-Output $_
        }
    }
}

Expand-SqlIncludes $ScriptFile | Set-Content $ScriptFile -Encoding UTF8

# -------------------------------------------------------------------------------------------------
# Perform Async Transformation

if ($Async) {
@"
SET NOCOUNT ON;

DECLARE @Sql nvarchar(max) = N'
$((Get-Content $ScriptFile -Encoding UTF8 -Raw) -replace "'", "''")
';

DECLARE
    @ConnectionId uniqueidentifier = [`$(TargetDatabase)]._deploy.ConnectionId()
  , @JobId        uniqueidentifier
  , @Name         sysname          = N'PSqlDeploy ' + CONVERT(nvarchar, SYSDATETIME())
;

-- Create job
EXEC msdb.dbo.sp_add_job
    @job_id                = @JobId OUTPUT
  , @job_name              = @Name
  , @description           = N'PSqlDeploy'
  , @notify_level_eventlog = 0 -- don''t spam the event log
--, @delete_level          = 3 -- Delete after running once
;

-- Set job to run locally
EXEC msdb.dbo.sp_add_jobserver
    @job_id = @JobId
;

-- Add script as job step
EXEC msdb.dbo.sp_add_jobstep
    @job_id    = @JobId
  , @step_name = N'Run Deployment Script'
  , @command   = @Sql
  , @flags     = 8 -- Write log to table (overwrite existing history)
;

-- Start job
EXEC msdb.dbo.sp_start_job
    @job_id = @JobId
;

SELECT JobId = @JobId;
"@ | Set-Content $ScriptFile -Encoding UTF8
}

# -------------------------------------------------------------------------------------------------
# Perform the migration

if ($WhatIf) {
    # Output deployment script
    Get-Content $ScriptFile -Encoding UTF8 -ReadCount 1000
} else {
    if ($VerifiedSeeds) {
        Info "Transferring restore files."
        $VerifiedSeeds | % {
            & "robocopy" (
                $_.Directory, $BackupDirectory, "*.backup", "*.dll",
                "/s", "/z", "/xo", "/r:1", "/w:5", "/njh"
            )
            if ($LASTEXITCODE -ge 4) {
                Write-Error "An error occurred while copying the backup file." -ErrorId "ServerError" `
                    -CategoryReason "ServerError" -CategoryTargetName 'Robocopy' -CategoryTargetType "String"
            }
        }
    }
    Info "Executing deployment script."
    try {
        $JobId = $NULL
        Invoke-Sql -Path $ScriptFile -Master `
            | % { Write-Host $_; $_ -as [guid] } `
            | % { $JobId = $_ }
    }
    catch {
        # Release exclusive access to database
        Invoke-Sql -Master -Query @"
            SET NOCOUNT ON;
            IF DB_ID('`$(TargetDatabase)') IS NOT NULL
            BEGIN
                RAISERROR ('Set existing database to multi-user mode', 0, 0) WITH NOWAIT;
                ALTER DATABASE [`$(TargetDatabase)] SET MULTI_USER;
            END;
"@
        throw
    }
}

Write-Host ""

while ($JobId) {
    $Status = ''
    Invoke-Sql -Master -Query @"
        SET NOCOUNT ON;
        SELECT
            Status = CASE
                WHEN a.start_execution_date IS NULL THEN 'Not Started'
                WHEN a.stop_execution_date  IS NULL THEN 'Running'
                ELSE CASE h.run_status
                    WHEN 0 THEN 'Failed'
                    WHEN 1 THEN IIF(h.step_id > 0, 'Running', 'Succeeded')
                    WHEN 2 THEN 'Retry'
                    WHEN 3 THEN 'Canceled'
                    ELSE 'Unknown'
                END
            END
          , Duration = DATEDIFF(ss,
                a.start_execution_date,
                ISNULL(a.stop_execution_date, SYSDATETIME())
            )
        --, Log = (SELECT [processing-instruction(log)] = l.log FOR XML PATH(''), TYPE)
        FROM
            msdb.dbo.sysjobs j
        INNER JOIN
            msdb.dbo.sysjobactivity a
            ON a.job_id = j.job_id
        LEFT JOIN
            msdb.dbo.sysjobhistory h
            ON  h.instance_id = a.job_history_id
            AND h.step_id = 0
        LEFT JOIN
            msdb.dbo.sysjobsteps s
            ON  s.job_id  = j.job_id
            AND s.step_id = 1
        LEFT JOIN
            msdb.dbo.sysjobstepslogs l
            ON l.step_uid = s.step_uid
        WHERE
            j.job_id = '$JobId'
        ;
"@  | % { $Status = $_ }

    # Display Status
    Write-Host ("`r" + " " * 80 + "`r") -NoNewline
    Write-Host $Status -NoNewline

    if ($Status -imatch '^(Not Started|Running)') {
        Start-Sleep -Seconds 1
    } elseif ($Status -imatch '^(Succeeded)') {
        Write-Host ""
        Break
    } else {
        Write-Host ""
        Write-Error "An error occurred while executing the SQL script." -ErrorId "ServerError" `
            -CategoryReason "ServerError" -CategoryTargetName $Query -CategoryTargetType "String"
    }
}

CleanUp

<#
    Copyright (c) 2016 Jeffrey Sharp

    Permission is hereby granted, free of charge, to any person obtaining a copy
    of this software and associated documentation files (the "Software"), to deal
    in the Software without restriction, including without limitation the rights
    to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
    copies of the Software, and to permit persons to whom the Software is
    furnished to do so, subject to the following conditions:

    The above copyright notice and this permission notice shall be included in all
    copies or substantial portions of the Software.

    THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
    IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
    FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
    AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
    LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
    OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
    SOFTWARE.
#>
