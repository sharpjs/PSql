<#
    Backup/Restore

    Part of: PSql - Simple PowerShell Cmdlets for SQL Server
    Copyright (C) 2016 Jeffrey Sharp
    https://github.com/sharpjs/PSql

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

function Backup-SqlDatabase {
    [CmdletBinding()]
    param (
        # Name of the database to back up.
        [Parameter(Mandatory, Position=1)]
        [string] $Database,

        # Path on this computer of the backup file to create.
        [Parameter(Mandatory, Position=2)]
        [string] $Path,

        # Path on this computer of the database server's backup directory.  For a local server, the default is read from the instance configuration.  For a remote server, the default is the UNC path "\\<server>\Database Backups".
        [string] $BackupDirectory,

        # The connection to use.  Must be an object returned by the PSql\Connect-Sql -PassThru cmdlet.  If not given, the default connection is used.
        [PSCustomObject] $Connection,

        # Output a FileInfo object for the backup file.  By default, this cmdlet does not output an object.
        [switch] $PassThru
    )
    begin {
        # Open a connection if one is not already open
        $OwnsConnection = Test-SqlConnection([ref] $Connection)
    }
    process {
        # Get backup file info
        $FileName        = Split-Path $Path -Leaf
        $TargetDirectory = Split-Path $Path -Parent

        # Resolve backup directory
        $Info = Get-SqlDirectories -Connection $Connection
        $BackupDirectory = Resolve-BackupDirectory $BackupDirectory $Connection $Info

        # Backup the database
        Invoke-Sql -Connection $Connection "
            BACKUP DATABASE [$Database]
                TO DISK = '$FileName'
                WITH INIT, FORMAT, COMPRESSION
            ;
        "

        # Copy backup file to the target directory
        if ($TargetDirectory -ne $BackupDirectory) {
            Invoke-Robocopy `
                -SourceDirectory $BackupDirectory `
                -TargetDirectory $TargetDirectory `
                -Files           $FileName
        }

        # Output FileInfo for the backup file, if requested
        if ($PassThru) {
            Join-Path $TargetDirectory $FileName | Get-Item | Write-Output
        }
    }
    end {
        # Close a connection if we implicitly opened one
        if ($OwnsConnection) {
            Disconnect-Sql $Connection
        }
    }
}

function Restore-SqlDatabase {
    [CmdletBinding()]
    param (
        # Name of the database to create.
        [Parameter(Mandatory, Position=1)]
        [string] $Database,

        # Path on this computer of the backup file to read.
        [Parameter(Mandatory, Position=2)]
        [string] $Path,

        # Name of the database as stored in the backup file.  By default, this is assumed to be the same as the database to create.
        [string] $OriginalDatabase,

        # Path on this computer of the database server's backup directory.  For a local server, the default is read from the instance configuration.  For a remote server, the default is the UNC path "\\<server>\Database Backups".
        [string] $BackupDirectory,

        # Path on the server of the directory where SQL data file(s) should be stored.
        [string] $DataDirectory,

        # Path on the server of the directory where SQL log file(s) should be stored.
        [string] $LogDirectory,

        # The connection to use.  Must be an object returned by the PSql\Connect-Sql -PassThru cmdlet.  If not given, the default connection is used.
        [PSCustomObject] $Connection
    )
    begin {
        # Open a connection if one is not already open
        $OwnsConnection = Test-SqlConnection([ref] $Connection)
    }
    process {
        # Get backup file info
        $File            = Get-Item $Path
        $FileName        = $File.Name
        $SourceDirectory = $File.Directory.FullName
        if (!$OriginalDatabase) { $OriginalDatabase = $Database }

        # Get server info
        $Info = Get-SqlDirectories -Connection $Connection
        $BackupDirectory = Resolve-BackupDirectory $BackupDirectory $Connection $Info
        if (!$DataDirectory) { $DataDirectory = $Info.DataDirectory }
        if (!$LogDirectory ) { $LogDirectory  = $Info.LogDirectory  }

        # Decide where to put data/log files
        $DataFilePath = Join-Path $DataDirectory "$($Database).mdf"
        $LogFilePath  = Join-Path $LogDirectory  "$($Database)_log.ldf"

        # Copy file to server backups directory
        if ($SourceDirectory -ne $BackupDirectory) {
            Invoke-Robocopy `
                -SourceDirectory $SourceDirectory `
                -TargetDirectory $BackupDirectory `
                -Files           $FileName
        }

        # Restore backup file
        $Exists = (Invoke-Sql -Connection $Connection "
            SELECT Id = DB_ID('$Database');
        ").Id
        if ($Exists) {
            Invoke-Sql -Connection $Connection "
                ALTER DATABASE [$Database]
                    SET SINGLE_USER
                    WITH ROLLBACK IMMEDIATE
                ;
            "
            Invoke-Sql -Connection $Connection "
                DROP DATABASE [$Database];
            "
        }
        Invoke-Sql -Connection $Connection "
            RESTORE DATABASE [$Database]
                FROM DISK = '$FileName'
                WITH RECOVERY, REPLACE,
                MOVE '$($OriginalDatabase)'     TO '$DataFilePath',
                MOVE '$($OriginalDatabase)_log' TO '$LogFilePath',
                STATS = 10
            ;
        "
    } 
    end {
        # Close a connection if we implicitly opened one
        if ($OwnsConnection) {
            Disconnect-Sql $Connection
        }
    }
}

function Resolve-BackupDirectory($Path, $Connection, $SqlDirectories) {
    if ($Path) {
        Resolve-Path $Path | % Path
    } elseif ($Connection.Connection.DataSource -eq ".") {
        $SqlDirectories.BackupDirectory
    } else {
        "\\$($Connection.Connection.DataSource)\Database Backups"
    }
}
