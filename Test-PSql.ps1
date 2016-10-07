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
    $C = Connect-Sql -Server . -Database master
    assert { $C -ne $null }
    assert { $C -is [System.Data.SqlClient.SqlConnection] }
    assert { $C.State -eq [System.Data.ConnectionState]::Open }
    assert { $C.DataSource -eq "." }
    assert { $C.Database -eq "master" }
    Disconnect-Sql $C
}

test "Invoke-Sql -> nchar" {
    $X = Invoke-Sql "select a = N'Hello';"
    assert { $X.a -eq "Hello" }
}

test "Invoke-Sql -> int" {
    $X = Invoke-Sql "select a = 42;"
    assert { $X -ne $null }
    assert { $X -is [PSCustomObject] }
    assert { $X.a -eq 42 }
}

test "Invoke-Sql -> bit" {
    $X = Invoke-Sql "select a = CONVERT(bit, 0), b = CONVERT(bit, 1);"
    assert { $X.a -eq $false }
    assert { $X.b -eq $true }
}

test "Invoke-Sql -> date" {
    $X = Invoke-Sql "select a = CONVERT(date, SYSUTCDATETIME());"
    assert { $X.a -eq [datetime]::UtcNow.Date }
}

test "Invoke-Sql -> time" {
    $X = Invoke-Sql "select a = CONVERT(time, SYSUTCDATETIME());"
    assert { [datetime]::UtcNow.TimeOfDay - $X.a -lt [timespan]::FromMinutes(1) }
}

test "Invoke-Sql -> datetime2" {
    $X = Invoke-Sql "select a = SYSUTCDATETIME();"
    assert { [datetime]::UtcNow - $X.a -lt [timespan]::FromMinutes(1) }
}

test "Invoke-Sql -> uniqueidentifier" {
    $X = Invoke-Sql "select a = NEWID();"
    assert { $X.a -is [guid] -and $X -ne [guid]::Empty }
}

test "Invoke-Sql -> binary" {
    $X = Invoke-Sql "select a = 0xDEADBEEF;"
    assert { $X.a -is [byte[]] }
    assert { $X.a.Length -eq 4 }
    assert { $X.a[0] -eq 0xDE  }
    assert { $X.a[1] -eq 0xAD  }
    assert { $X.a[2] -eq 0xBE  }
    assert { $X.a[3] -eq 0xEF  }
}

Write-Host
Write-Host "Total: $TestCountTotal, Passed: $TestCountPassed, Failed: $TestCountFailed"
