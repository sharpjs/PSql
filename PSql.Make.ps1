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

--# MODULE
--# REQUIRES: Bcd
--# PROVIDES: Bldkf

#>

$LinesRe     = [regex] '\n'
$SpacesRe    = [regex] '\s+'
$DirectiveRe = [regex] '(?x)
    ^ --\# \s+ (?<dir>MODULE|PROVIDES|REQUIRES): \s+ (?<args>.*) $
'

function New-SqlModule {
    Write-Output ([PSCustomObject]@{
        Name       = $null
        Provides   = New-Object System.Collections.ArrayList
        Requires   = New-Object System.Collections.ArrayList
        Script     = New-Object System.Text.StringBuilder 4096
    })
}

function Out-SqlModule($Module) {
    if ($Module.Provides.Count -or
        $Module.Requires.Count -or
        $Module.Script.Length) { Write-Output $Module }
}

function Read-SqlModules {
    [CmdletBinding()]
    [OutputType([object[]])]
    param (
        # The text to process.
        [Parameter(ValueFromPipeline)]
        [string] $Text
    )
    process {
        $Module = New-SqlModule

        $Text -split $LinesRe | % {
            if ($_ -match $DirectiveRe) {
                $Directive =  $Matches['dir' ]
                $Arguments = @($Matches['args'] -split $SpacesRe | ? { $_ })
                switch ($Directive) {
                    'MODULE' {
                        Out-SqlModule $Module
                        $Module = New-SqlModule
                        $Module.Name = $Arguments | Select-Object -First 1
                        $Module.Provides.AddRange($Arguments)
                    }
                    'PROVIDES' {
                        $Module.Provides.AddRange($Arguments)
                    }
                    'REQUIRES' {
                        $Module.Requires.AddRange($Arguments)
                    }
                }
            } else {
                $Module.Script.AppendLine($_) | Out-Null
            }
        }

        Out-SqlModule $Module
    }
}

function Get-Subject {
    [OutputType([PSCustomObject])]
    param([hashtable] $Subjects, [string] $Name)

    ($Subject = $Subjects[$Name]) -or
    ($Subject = $Subjects[$Name] = [PSCustomObject] @{
        Name       = $Name
        ProvidedBy = New-Object System.Collections.ArrayList
        RequiredBy = New-Object System.Collections.ArrayList
    }) | Out-Null
    $Subject
}

function Merge-SqlModules {
    param (
        [Parameter(ValueFromPipeline)]
        [object[]] $Modules
    )
    begin {
        $Queue    = New-Object System.Collections.Queue
        $Subjects = @{}
    }
    process {
        foreach ($Module in $Modules) {
            foreach ($Name in $Module.Provides) {
                (Get-Subject $Subjects $Name).ProvidedBy.Add($Module) | Out-Null
            }
            foreach ($Name in $Module.Requires) {
                (Get-Subject $Subjects $Name).RequiredBy.Add($Module) | Out-Null
            }
            if (!$Module.Requires) {
                $Queue.Enqueue($Module)
            }
        }
    }
    end {
        $Missing = (
            $Subjects.Values | ? { !$_.ProvidedBy } | % { $_.Name } | sort -Unique
        ) -join ", "
        if ($Missing) {
            throw "Subjects are required but not provided: $Missing"
        }
        Write-Output ([PSCustomObject] @{
            Queue    = $Queue
            Subjects = $Subjects
        })
    }
}

function Invoke-SqlModules {
    [CmdletBinding()]
    param (
        # The set of modules to execute.
        [Parameter(Position = 1, Mandatory)]
        $ModuleSet,

        # Name of the server.  Must be a valid hostname or IP address, with an optional instance suffix (ex: "10.12.34.56\DEV").  A dot (".") may be used to specify a local server.
        [Parameter(Position = 2, ValueFromPipelineByPropertyName)]
        [string] $Server = ".",

        # Name of the initial database.  If not given, the initial database is the SQL Server default database.
        [Parameter(Position = 3, ValueFromPipelineByPropertyName)]
        [string] $Database,

        # Use SQL credentials instead of Windows authentication.  Must be used with -Password.
        [string] $Login,

        # Use SQL credentials instead of Windows authentication.  Must be used with -Login.
        [string] $Password,

        # Command timeout, in seconds.  0 disables timeout.  The default is 0.
        [int] $Timeout = 0
    )

    $OldInformationPreference = $InformationPreference
    $InformationPreference = "Continue"

    $Workers = foreach ($Id in 1..$env:NUMBER_OF_PROCESSORS) {
        Write-Information "[Thread $Id]: Starting."

        # Create worker thread to run modules asynchronously
        $Shell = [powershell]::Create()
        $State = $Shell.Runspace.SessionStateProxy
        $State.SetVariable('ModuleSet', $ModuleSet   )
        $State.SetVariable('Server'  ,  $Server      )
        $State.SetVariable('Database',  $Database    )
        $State.SetVariable('Login'   ,  $Login       )
        $State.SetVariable('Password',  $Password    )
        $State.SetVariable('Timeout' ,  $Timeout     )
        $State.SetVariable('PSqlDir' ,  $PSScriptRoot)
        $Shell.AddScript($ThreadMain) | Out-Null

        # Pipe worker output streams to main output streams
        foreach ($Kind in 'Debug', 'Verbose', 'Information', 'Warning' ,'Error') {
            Register-ObjectEvent $Shell.Streams.$Kind DataAdded -SupportEvent `
                -MessageData @{ Id = $Id; Kind = $Kind } `
                -Action {
                    $Id   = $Event.MessageData.Id
                    $Kind = $Event.MessageData.Kind
                    $Event.Sender.ReadAll() | % {
                        if ($Kind -ne 'Error') {
                            & "Write-$Kind" "[Thread $Id]: $_"
                        } else {
                            $Host.UI.WriteErrorLine("[Thread $Id]: $_")
                        }
                    }
                } `
            | Out-Null 
        }

        # Start worker
        $Invocation = $Shell.BeginInvoke()
        @{ Id = $Id; Shell = $Shell; Invocation = $Invocation }
    }

    # Wait for workers to finish
    $Failed = $false
    foreach ($Worker in $Workers) {
        try {
            $Worker.Shell.EndInvoke($Worker.Invocation) | Out-Null
            Write-Information "[Thread $($Worker.Id)]: Ended."
        }
        catch {
            $Failed = $true
            $Host.UI.WriteErrorLine("[Thread $($Worker.Id)]: $($_.Exception.Message)")
            $Host.UI.WriteErrorLine("[Thread $($Worker.Id)]: Terminated due to exception.")
        }
    }

    if ($Failed) {
        throw 'One or more threads ended with an error.'
    }

    $InformationPreference = $OldInformationPreference 
}

$ThreadMain = {
    $ErrorActionPreference = "Stop"
    Import-Module $PSqlDir\PSql.psm1 -Force

    function Invoke-SqlModules {
        param ($ModuleSet)

        $Queue      = $ModuleSet.Queue
        $Subjects   = $ModuleSet.Subjects
        $Module     = @{ Value = $null }

        $Connection = Connect-Sql $Server $Database `
            -Login $Login -Password $Password -PassThru

        try {
            while ($true) {
                # Advance to next module
                Use-Lock $Queue {
                    # Mark prior module completed
                    if ($Module["Value"]) {
                        Complete-SqlModule $Module["Value"] $ModuleSet
                    }

                    # Dequeue next module
                    while ($true) {
                        if ($Queue.Count) {
                            # Take next queued module
                            $Module["Value"] = $Queue.Dequeue()
                            break
                        } elseif (!$Subjects.Count) {
                            # No modules queued, none in progress; done
                            $Module["Value"] = $null
                            break
                        } else {
                            # Wait for other modules to finish
                            [System.Threading.Monitor]::Wait($Queue)
                        }
                    }
                }

                # Check if done
                if (!$Module["Value"]) { return }

                # Run module
                Write-Host "Running $($Module["Value"].Name)" -ForegroundColor Yellow
                Invoke-Sql -Query $Module["Value"].Script -Connection $Connection -Timeout $Timeout
            }
        }
        finally {
            Disconnect-Sql $Connection
        }
    }

    function Complete-SqlModule($Module, $ModuleSet) {
        $Queue       = $ModuleSet.Queue
        $Subjects    = $ModuleSet.Subjects
        $WorkChanged = $false

        # Update each subject provided by this module
        foreach ($Name in $Module.Provides) {
            $Subject = $Subjects[$Name]

            # Mark this module as done
            $Subject.ProvidedBy.Remove($Module)

            # Check if all subject's modules are done
            if ($Subject.ProvidedBy) { continue }

            # Mark subject as done
            $Subjects.Remove($Name)
            if (!$Subjects.Count) { $WorkChanged = $true }

            # Update dependents
            foreach ($Dependent in $Subject.RequiredBy) {

                # Mark requirement as met
                $Dependent.Requires.Remove($Name)

                # Check if all of dependent's requrements are met
                if ($Dependent.Requires) { continue }

                # All requirements met; queue the dependent
                $Queue.Enqueue($Dependent)
                $WorkChanged = $true
            }
        }

        if ($WorkChanged) {
            [System.Threading.Monitor]::PulseAll($Queue)
        }
    }

    function Use-Lock {
        param([object] $Object, [scriptblock] $ScriptBlock)
        $Locked = $false
        try {
            [System.Threading.Monitor]::Enter($Object, [ref] $Locked)
            & $ScriptBlock
        }
        finally {
            if ($Locked) {
                [System.Threading.Monitor]::Exit($Object)
            }
        }
    }

    Invoke-SqlModules $ModuleSet
}
