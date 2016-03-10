<#
    Make-Like Dependent Step Runner

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

<#
--# MODULE:   A
--# PROVIDES: B C D
--# REQUIRES: E F G H I
#>

Get-Content $PSScriptRoot\PSql.ModuleRunner.cs -Raw `
    | % { Add-Type $_ -Language CSharp }

$LinesRe     = [regex] '\r?\n'
$SpacesRe    = [regex] '\s+'
$DirectiveRe = [regex] '(?x)
    ^ --\# \s+ (?<dir>MODULE|PROVIDES|REQUIRES|WORKER): \s+ (?<args>.*) $
'

function Read-SqlModules {
    [CmdletBinding()]
    [OutputType([PSql.ModuleRunner])]
    param (
        # The text to process.
        [Parameter(Position = 1, ValueFromPipeline)]
        [string] $Text,

        [Parameter(Position = 2, Mandatory)]
        [PSql.ModuleRunner] $Runner
    )
    process {
        $Text -split $LinesRe | % {
            if ($_ -match $DirectiveRe) {
                $Directive = $Matches['dir' ]
                $Arguments = [string[]]@($Matches['args'] -split $SpacesRe | ? { [string] $_ })
                switch ($Directive) {
                    'MODULE' {
                        $Name = $Arguments | Select-Object -First 1
                        $Runner.StartModule($Name)
                        $Runner.AddProvides($Arguments)
                    }
                    'PROVIDES' {
                        $Runner.AddProvides($Arguments)
                    }
                    'REQUIRES' {
                        $Runner.AddRequires($Arguments)
                    }
                    'WORKER' {
                        $Runner.SetRunOnAllWorkers()
                    }
                }
            } else {
                $Runner.AddScriptLine($_)
            }
        }
    }
}

function Invoke-SqlModules {
    [CmdletBinding()]
    param (
        # Text containing the modules to execute.
        [Parameter(Position = 1, ValueFromPipeline)]
        [string] $Text,

        # Name of the server.  Must be a valid hostname or IP address, with an optional instance suffix (ex: "10.12.34.56\DEV").  A dot (".") may be used to specify a local server.
        [Parameter(Position = 2)]
        [string] $Server = ".",

        # Name of the initial database.  If not given, the initial database is the SQL Server default database.
        [Parameter(Position = 3)]
        [string] $Database,

        # Use SQL credentials instead of Windows authentication.  Must be used with -Password.
        [string] $Login,

        # Use SQL credentials instead of Windows authentication.  Must be used with -Login.
        [string] $Password,

        # Command timeout, in seconds.  0 disables timeout.  The default is 0.
        [int] $Timeout = 0
    )
    process {
        $Params = @{
            Server      = $Server
            Database    = $Database
            Login       = $Login
            Password    = $Password
            Timeout     = $Timeout
            PSqlPath    = $PSScriptRoot
        }

        $Runner = New-Object PSql.ModuleRunner $RunLoop, $Params, -1
        Read-SqlModules $Text $Runner
        $Runner.Complete()
        $Runner.Run()
    }
}

$RunLoop = {
    $ErrorActionPreference = "Stop"
    Import-Module $PSqlPath\PSql.psm1 -Force

    $Connection = Connect-Sql $Server $Database `
        -Login $Login -Password $Password -PassThru

    try {
        while ($true) {
            $Module = $Modules.Next()
            if (!$Module) { break }

            $Module.Script `
                | Split-SqlBatches `
                | Invoke-Sql -Connection $Connection -Timeout $Timeout
        }
    } finally {
        Disconnect-Sql $Connection
    }
}
