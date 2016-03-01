<#
    Utility Cmdlets

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

function Get-SqlDirectories {
    <#
    .SYNOPSIS
        Gets important directory paths for the SQL server instance.
    #>
    [CmdletBinding()]
    param(
        # The connection to use.  This must be an object returned by the PSql\Connect-Sql -PassThru cmdlet.  If not given, the command is executed on the default connection.
        [PSCustomObject] $Connection = $DefaultContext
    )
    Invoke-Sql -Connection $Connection "
        DECLARE @BackupDirectory nvarchar(4000);
        EXEC master.dbo.xp_instance_regread
            N'HKEY_LOCAL_MACHINE'
          , N'Software\Microsoft\MSSQLServer\MSSQLServer'
          , N'BackupDirectory'
          , @BackupDirectory OUTPUT;
        SELECT
            BackupDirectory = @BackupDirectory
          , DataDirectory   = SERVERPROPERTY('InstanceDefaultDataPath')
          , LogDirectory    = SERVERPROPERTY('InstanceDefaultLogPath')
        ;
    "
}

function Invoke-Robocopy {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory, Position=1)]
        [object] $SourceDirectory,

        [Parameter(Mandatory, Position=2)]
        [object] $TargetDirectory,

        [Parameter(ValueFromPipeline, ValueFromRemainingArguments)]
        [string[]] $Files
    )
    process {
        & "robocopy" (
            $SourceDirectory, $TargetDirectory + $Files +
            "/z",    # Copy files in restartable mode
            "/j",    # Use unbuffered I/O (recommended for large files)
            "/xo",   # Don't copy older file over a newer file
            "/xx",   # Don't list other files already in target directory
            "/r:2",  # Retry 2 times
            "/w:5",  # Wait 5 seconds before retry
            "/njh"   # Don't display header
        )

        if ($LASTEXITCODE -ge 4) {
            # Robocopy exit codes below 4 are successful
            # http://ss64.com/nt/robocopy-exit.html
            throw "An error occurred while copying the file(s)."
        }

        # Prevent false positives when callers check the last exit code
        $global:LastExitCode = $null
    }
}
