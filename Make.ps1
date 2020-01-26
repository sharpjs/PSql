<#
.SYNOPSIS
    Invokes various build commands.

.DESCRIPTION
    This script is similar to a makefile.

    Copyright (C) 2020 Jeffrey Sharp

    Permission to use, copy, modify, and distribute this software for any
    purpose with or without fee is hereby granted, provided that the above
    copyright notice and this permission notice appear in all copies.

    THE SOFTWARE IS PROVIDED "AS IS" AND THE AUTHOR DISCLAIMS ALL WARRANTIES
    WITH REGARD TO THIS SOFTWARE INCLUDING ALL IMPLIED WARRANTIES OF
    MERCHANTABILITY AND FITNESS. IN NO EVENT SHALL THE AUTHOR BE LIABLE FOR
    ANY SPECIAL, DIRECT, INDIRECT, OR CONSEQUENTIAL DAMAGES OR ANY DAMAGES
    WHATSOEVER RESULTING FROM LOSS OF USE, DATA OR PROFITS, WHETHER IN AN
    ACTION OF CONTRACT, NEGLIGENCE OR OTHER TORTIOUS ACTION, ARISING OUT OF
    OR IN CONNECTION WITH THE USE OR PERFORMANCE OF THIS SOFTWARE.
#>
[CmdletBinding(DefaultParameterSetName="Test")]
param (
    # Build.
    [Parameter(Mandatory, ParameterSetName="Build")]
    [switch] $Build,

    # Build, run tests.
    [Parameter(ParameterSetName="Test")]
    [switch] $Test,

    # Build, run tests, and produce code covarage report.
    [Parameter(Mandatory, ParameterSetName="Coverage")]
    [switch] $Coverage,

    # Open code coverage report in a web browser.
    [Parameter(ParameterSetName="Coverage")]
    [switch] $Open,

    # The configuration to build: Debug or Release.  The default is Debug.
    [Parameter(ParameterSetName="Build")]
    [Parameter(ParameterSetName="Test")]
    [Parameter(ParameterSetName="Coverage")]
    [ValidateSet("Debug", "Release")]
    [string] $Configuration = "Debug"
)

#Requires -Version 5
$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

$Command = $PSCmdlet.ParameterSetName
if ($Command -eq "Test") { $Test = $true }

# http://patorjk.com/software/taag/#p=display&f=Slant
Write-Host -ForegroundColor Cyan @' 

        ____  _____       __
       / __ \/ ___/____ _/ /
      / /_/ /\__ \/ __ `/ / 
     / ____/___/ / /_/ / /  
    /_/    /____/\__, /_/   
                   /_/      
'@

function Main {
    Invoke-Build

    if ($Test -or $Coverage) {
        Invoke-Test
    }

    if ($Coverage) {
        Export-CoverageReport
    }
} 

function Invoke-Build {
    Write-Phase "Build"
    Invoke-DotNetExe build --configuration:$Configuration
}

function Invoke-Test {
    Write-Phase "Test$(if ($Coverage) {" + Coverage"})"
    Remove-Item test -Recurse -ErrorAction SilentlyContinue
    Invoke-DotNetExe -Arguments @(
        "test"
        "--no-build"
        "--configuration:$Configuration"
        if ($Coverage) {
            "--collect:XPlat Code Coverage"
            "--results-directory:test"
        }
    )
}

function Export-CoverageReport {
    Write-Phase "Coverage Report"
    Invoke-DotNetExe -Arguments "tool", "restore"
    Invoke-DotNetExe -Arguments @(
        "reportgenerator"
        "-reports:test\*\coverage.cobertura.xml"
        "-targetdir:coverage"
        "-reporttypes:HtmlInline_AzurePipelines_Dark;TeamCitySummary;Badges"
        "-verbosity:Warning"
    )

    if ($Open) {
        & .\coverage\index.htm
    }
}

function Invoke-DotNetExe {
    param (
        [Parameter(Mandatory, ValueFromRemainingArguments)]
        [string[]] $Arguments
    )
    & dotnet.exe $Arguments
    if ($LASTEXITCODE -ne 0) { throw "dotnet.exe exited with an error." }
}

function Write-Phase {
    param (
        [Parameter(Mandatory)]
        [string] $Name
    )
    Write-Host "`n===== $Name =====`n" -ForegroundColor Cyan
}

# Invoke Main
try {
    Push-Location $PSScriptRoot
    Main
}
finally {
    Pop-Location
}
