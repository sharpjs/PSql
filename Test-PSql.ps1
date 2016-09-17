<#
    Automated Tests

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

$TestCountTotal  = 0
$TestCountPassed = 0
$TestCountFailed = 0

function test([string] $Name, [scriptblock] $Block) {
    Write-Host "==> Test: $Name" -NoNewline -ForegroundColor Cyan
    $script:TestCountTotal += 1
    try {
        & $Block
    }
    catch {
        Write-Host " [FAIL]" -ForegroundColor Magenta
        Write-Output $_.ToString(), $_.ScriptStackTrace
        Write-Host
        $script:TestCountFailed += 1
        return
    }
    Write-Host " [PASS]" -ForegroundColor Green
    $script:TestCountPassed += 1
}

function assert([scriptblock] $Block) {
    try {
        if (& $Block) { return }
    }
    catch {
        throw "Assertion failed: $("$Block".Trim()) [$_]"
    }
    throw "Assertion failed: $("$Block".Trim())"
}

test "Import-Module" {
    Import-Module $PSScriptRoot\PSql.psm1 -Force
}

test "Connect-Sql" {
    $C = Connect-Sql -Server . -Database master -PassThru
    assert { $C -ne $null }
    Disconnect-Sql $C
}

Write-Host
Write-Host "Total: $TestCountTotal, Passed: $TestCountPassed, Failed: $TestCountFailed"
