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

function Complete-SqlModule($Module, $ModuleSet) {
    $Queue        = $ModuleSet.Queue
    $Subjects     = $ModuleSet.Subjects
    $QueueChanged = $false

    # Update each subject provided by this module
    foreach ($Name in $Module.Provides) {
        $Subject = $Subjects[$Name]

        # Mark this module as done
        $Subject.ProvidedBy.Remove($Module)

        # Check if all subject's modules are done
        if ($Subject.ProvidedBy) { continue }

        # Mark subject as done
        $Subjects.Remove($Name)

        # Update dependents
        foreach ($Dependent in $Subject.RequiredBy) {

            # Mark requirement as met
            $Dependent.Requires.Remove($Name)

            # Check if all of dependent's requrements are met
            if ($Dependent.Requires) { continue }

            # All requirements met; queue the dependent
            $Queue.Enqueue($Dependent)
            $QueueChanged = $true
        }
    }

    if ($QueueChanged) {
        [System.Threading.Monitor]::PulseAll($Queue)
    }
}

function Invoke-SqlModules ($ModuleSet) {
    $Queue    = $ModuleSet.Queue
    $Subjects = $ModuleSet.Subjects

    $Module     = $null
    $Connection = $null

    while ($true) {
        # Advance to next module
        $Module = Use-Lock $Queue {
            # Mark prior module completed
            if ($Module) {
                Complete-SqlModule $Module $ModuleSet
            }

            # Dequeue next module
            while ($true) {
                if ($Queue.Count) {
                    # Take next queued module
                    $Queue.Dequeue() | Write-Output
                    break
                } elseif (!$Subjects.Count) {
                    # No modules queued, none in progress; done
                    break
                } else {
                    # Wait for other modules to finish
                    [System.Threading.Monitor]::Wait($Queue)
                }
            }
        }

        # Check if done
        if (!$Module) { return }

        # Run module
        Write-Host "Invoking $($Module.Name)" -ForegroundColor Yellow
    }

    Disconnect-Sql $Connection
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
