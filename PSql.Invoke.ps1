<#
    SQL Command Invocation

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

function Invoke-Sql {
    <#
    .SYNOPSIS
        Invokes the specified SQL command or query.
    #>
    param (
        # The command to invoke.
        [Parameter(Position=1, ValueFromPipeline)]
        [string] $Query,

        # The connection on which to invoke the command.  This must be an object returned by the PSql\Connect-Sql -PassThru cmdlet.  If not given, the command is executed on the default connection.
        [PSCustomObject] $Connection = $DefaultContext,

        # Do not throw an exception if an error message is received from the server.
        [switch] $CanFail,

        # Command timeout, in seconds.  0 disables timeout.  The default is 0.
        [int] $Timeout = 0
    )
    begin {
        # Clear any failures from prior command
        $Connection.HasErrors = $false
    }
    process {
        if (!$Query) {return}
        $Command = $NULL
        $Reader  = $NULL
        try {
            # Execute the command
            $Command                = $Connection.Connection.CreateCommand()
            $Command.CommandText    = $Query
            $Command.CommandType    = [System.Data.CommandType]::Text
            $Command.CommandTimeout = $Timeout
            $Reader                 = $Command.ExecuteReader()

            # Transform result rows into PowerShell objects
            while ($true) {
                while ($Reader.Read()) {
                    $Row = @{}
                    0..($Reader.FieldCount - 1) | % {
                        $Name  = $Reader.GetName($_)
                        $Value = $Reader.GetValue($_)
                        if (!$Name) { $Name = "Col$_" } 
                        if ($Value -is [System.DBNull]) { $Value = $NULL }
                        $Row[$Name] = $Value
                    }
                    [PSCustomObject] $Row | Write-Output
                }
                if (!$Reader.NextResult()) { break }
            }
        }
        catch [System.Data.SqlClient.SqlException] {
            Write-SqlErrors $_.Exception.Errors $Connection
        }
        finally {
            if ($Reader ) { $Reader. Dispose() }
            if ($Command) { $Command.Dispose() }
        }

        # Terminate script on error
        if ($Connection.HasErrors -and !$CanFail) {
            throw "An error occurred while executing the SQL batch."
        }
    }
}
