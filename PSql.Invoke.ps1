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
        Invokes the specified SQL command. Outputs results, if any, as PSCustomObjects.
    #>
    param (
        # The connection on which to invoke the command.  If not given, a connection is opened to the default database on the local host using integrated authentication.
        [Parameter(Mandatory, ParameterSetName="Database")]
        [string] $Database,

        # The SQL command to invoke.
        [Parameter(Position=1, ValueFromPipeline)]
        [string] $Sql,

        # The connection on which to invoke the command.  If not given, a connection is opened to the default database on the local host using integrated authentication.
        [System.Data.SqlClient.SqlConnection] $Connection,

        # Do not wrap the command with error-handling code.
        [switch] $Raw,

        # Do not throw an exception if an error message is received from the server.
        [switch] $CanFail,

        # Command timeout, in seconds.  0 disables timeout.  The default is 0.
        [int] $Timeout = 0
    )
    begin {
        # Open a connection if one is not already open
        $OwnsConnection = Ensure-SqlConnection ([ref] $Connection) $Database

        # Clear any failures from prior command
        $Context = Get-ConnectionContext $Connection
        $Context.HasErrors = $false
    }
    process {
        if (!$Sql) { return }
        $Command = $NULL
        $Reader  = $NULL

        if (!$Raw) {
            $Sql = Use-SqlErrorHandler $Sql
        }

        try {
            # Execute the command
            $Command                = $Connection.CreateCommand()
            $Command.CommandText    = $Sql
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
            Write-SqlErrors $_.Exception.Errors $Context
        }
        finally {
            if ($Reader ) { $Reader. Dispose() }
            if ($Command) { $Command.Dispose() }
        }

        # Terminate script on error
        if ($Context.HasErrors -and !$CanFail) {
            throw "An error occurred while executing the SQL batch."
        }
    }
    end {
        # Close connection if we implicitly opened it
        if ($OwnsConnection) {
            Disconnect-Sql $Connection
        }
    }
}

function Use-SqlErrorHandler {
    <#
    .SYNOPSIS
        Wraps SQL batches with an error handler that prints the offending batch and stops execution.
    #>
    param (
        # The SQL batch to wrap.
        [Parameter(Position=1, ValueFromPipeline)] 
        [string] $Sql
    )
    begin {
        $Out = New-Object System.Text.StringBuilder
        $Out.AppendLine(@"
DECLARE @sql nvarchar(max);
BEGIN TRY
"@) | Out-Null
    }
    process {
        $Sql = $Sql -replace "'", "''"
        $Out.AppendLine(@"
    SET @sql = '$Sql';
    EXEC sp_executesql @sql;
"@) | Out-Null
    }
    end {
        $Out.AppendLine(@"
END TRY
BEGIN CATCH
    PRINT ''
    PRINT 'An error occurred while executing this batch:';

    -- Print @sql in chunks of no more than 4000 characters to work around the
    -- SQL Server limit of 4000 nvarchars per PRINT.  Break chunks at line
    -- boundaries, if possible, but ensure that the whole batch gets printed.

    DECLARE
        @crlf  nvarchar(2) = CHAR(13) + CHAR(10),   -- end of line
        @pos   bigint      =  1,                    -- a char pos
        @start bigint      =  1,                    -- first char pos of chunk
        @end   bigint      = -1,                    -- first char pos after chunk
        @len   bigint      = LEN(@sql);             -- last  char pos of SQL

    IF LEFT(@sql, 2) != @crlf
        PRINT '';

    WHILE @start <= @len
    BEGIN
        WHILE @end <= @len
        BEGIN
            SET @pos = CHARINDEX(@crlf, @sql, @end + 2);
            IF @pos = 0
                SET @pos = @len + 1;

            IF @pos <= @start + 4000
                SET @end = @pos; -- line fits in this chunk
            ELSE IF @start = @end
                SET @end = @start + 4000; -- line won't fit in any chunk
            ELSE
                BREAK; -- chunk full
        END;

        PRINT SUBSTRING(@sql, @start, @end - @start);

        SET @start = @end + 2;
        SET @end   = @start;
    END;

    IF RIGHT(@sql, 2) != @crlf
        PRINT '';

    THROW;
END CATCH;
"@) | Out-Null
        Write-Output $Out.ToString()
    }
}
