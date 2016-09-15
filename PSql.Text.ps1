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

$IgnoreCase = [System.StringComparison]::OrdinalIgnoreCase

$SqlBatchTokens = [regex]'(?minx:
    '' ([^''] |'''')* (''|\z) |
    \[ ([^\]] |\]\])* (\]|\z) |
    ^GO (\r?\n|\z)
)'

$Newline = [regex]'\r?\n'

function Split-SqlBatches {
    <#
    .SYNOPSIS
        Splits SQL text into batches separated by GO lines.
    #>
    [CmdletBinding()]
    [OutputType([string[]])]
    param (
        # The SQL text to split.
        [Parameter(ValueFromPipeline)]
        [string] $Sql
    )
    process {
        $Start = 0
        $SqlBatchTokens.Matches($Sql) `
            | ? { $_.Value.StartsWith("G", $IgnoreCase) } `
            | % {
                Write-Output $Sql.Substring($Start, $_.Index - $Start)
                $Start = $_.Index + $_.Length
            }
        if ($Start -lt $Sql.Length) {
            Write-Output $Sql.Substring($Start)
        }
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
                Get-Content -LiteralPath $Matches[1] -Encoding UTF8 -Raw `
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
