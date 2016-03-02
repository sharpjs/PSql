<#
    Text Transformation

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

function Split-SqlBatches {
    <#
    .SYNOPSIS
        Splits SQL text into batches separated by GO lines.
    #>
    [CmdletBinding()]
    [OutputType([string[]])]
    param (
        [Parameter(ValueFromPipeline)]
        [string] $Input
    )
    process {
        $Input -split "(?m)^GO(?:\r)?\n"
    }
}

function Expand-SqlCmdDirectives {
    <#
    .SYNOPSIS
        Processes a limited set of SQLCMD directives in the specified text.

    .DESCRIPTION
        Supports the following SQLCMD directives:

        $(name)                 Replaced with the value of the SQLCMD variable
        :r filename             Includes <filename>
        :setvar name value      Defines a SQLCMD variable
        :setvar name "value"    Defines a SQLCMD variable
    #>
    [CmdletBinding()]
    [OutputType([string])]
    param (
        # The text to process.
        [Parameter(ValueFromPipeline)]
        [string] $Input,

        # SQLCMD variables that will be defined when processing begins.
        [hashtable] $Define
    )
    process {
        # Clone hashtable so that our changes do not modify caller's hashtable.
        $Define = if ($Define) { $Define.Clone() } else { @{} }

        # Process each line individually
        ($Input -split "(?:\r)?\n" | % {
            # Perform replacements
            foreach ($Name in $Define.Keys) {
                $_ = $_ -replace "\$\($Name\)", $Define[$Name]
            } 

            # Interpret directives
            if ($_ -imatch '^:r\s+(.*?)\s*$') {
                # Include directive
                Get-Content $Matches[1] -Encoding UTF8 -Raw `
                    | Expand-SqlCmdDirectives -Define $Define
            } elseif ($_ -imatch '^:setvar\s+(\w+)\s+("?)(.*)\2\s*$') {
                # Define directive
                $Define[$Matches[1]] = $Matches[3]
            } else {
                # Not a directive
                Write-Output $_
            }
        }) -join "`r`n"
    }
}

<#

-- MODULE:   Blah
-- REQUIRES: Bcd
-- PROVIDES: Bldkf

#>

$LinesRe     = [regex] '\n'
$SpacesRe    = [regex] '\s+'
$DirectiveRe = [regex] '(?x)
    ^ --\# \s+ (?<dir>MODULE|PROVIDES|REQUIRES): \s+ (?<args>.*) $
'

function New-SqlModule {
    Write-Output ([PSCustomObject]@{
        Name     = $null
        Provides = New-Object System.Collections.ArrayList
        Requires = New-Object System.Collections.ArrayList
        Script   = New-Object System.Text.StringBuilder 4096
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
        [string] $Text
    )

    process {
        $Module = New-SqlModule

        $Text -split $LinesRe | % {
            if ($_ -match $DirectiveRe) {
                $Directive = $Matches['dir' ]
                $Arguments = $Matches['args'] -split $SpacesRe
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
                $Module.Script.Append($Text) | Out-Null
            }
        }

        Out-SqlModule $Module
    }
}
