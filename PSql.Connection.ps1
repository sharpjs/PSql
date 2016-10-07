<#
    Connection Management

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
    A 'connection context' holds a SqlConnection plus a few bits of extra state.
    All live connection contexts are tracked here.
#>
$Contexts = [hashtable] @{}

function Connect-Sql {
    <#
    .SYNOPSIS
        Connects to the specified SQL Server instance.
    #>
    [OutputType([System.Data.SqlClient.SqlConnection])]
    param (
        # Name of the server.  Must be a valid hostname or IP address, with an optional instance suffix (ex: "10.12.34.56\DEV").  A dot (".") may be used to specify a local server.
        [Parameter(Position = 1, ValueFromPipelineByPropertyName)]
        [string] $Server = ".",

        # Name of the initial database.  If not given, the initial database is the SQL Server default database.
        [Parameter(Position = 2, ValueFromPipelineByPropertyName)]
        [string] $Database,

        # Use SQL credentials instead of Windows authentication.  Must be used with -Password.
        [string] $Login,

        # Use SQL credentials instead of Windows authentication.  Must be used with -Login.
        [string] $Password
    )

    # Build connection string
    $Builder = New-Object System.Data.SqlClient.SqlConnectionStringBuilder
    $Builder.PSBase.DataSource = $Server
    if ($Database) {
        $Builder.PSBase.InitialCatalog = $Database
    }
    $Builder.PSBase.ApplicationName = "PowerShell"
    $Builder.PSBase.ConnectTimeout  = 30 # seconds
    $Builder.PSBase.Pooling         = $false;
   
    # Choose authentication method
    if ($Login -and $Password) {
        $Builder.PSBase.UserID   = $Login
        $Builder.PSBase.Password = $Password
    } elseif (!$Login -and !$Password) {
        $Builder.PSBase.IntegratedSecurity = $true
    } else {
        throw "-Login and -Password must be specified together."
    }

    # Create database connection
    $Connection = New-Object System.Data.SqlClient.SqlConnection
    $Connection.ConnectionString = $Builder.ConnectionString

    # Set up to print messages received from the server
    $Connection.FireInfoMessageEventOnUserErrors = $true;
    $Connection.add_InfoMessage({
        param($Sender, $Data)
        $Context = Get-ConnectionContext $Sender
        Write-SqlErrors $Data.Errors $Context
    }); 

    # Track contextual state for the connection
    $Context = Get-ConnectionContext $Connection

    # Open the connection
    $Connection.Open()

    # Return connection
    $Connection
}

function Disconnect-Sql {
    <#
    .SYNOPSIS
        Disconnects the given connection(s).
    #>
    param(
        # The connections to disconnect.
        [Parameter(ValueFromPipeline, ValueFromRemainingArguments)]
        [System.Data.SqlClient.SqlConnection[]] $Connections
    )
    process {
        $Connections | Get-ConnectionContext | % {
            $_.IsDisconnecting = $true
            $_.Connection.Dispose()
        }
    }
}

# [Internal]
function Get-ConnectionContext {
    <#
    .SYNOPSIS
        Gets or creates the connection context for the given connection.
    #>
    param (
        [Parameter(Mandatory, Position = 1, ValueFromPipeline)]
        [System.Data.SqlClient.SqlConnection] $Connection
    )
    process {
        Lock-Object $Contexts.SyncRoot {
            # Get existing context, if any
            $Context = $Contexts[$Connection]

            if (!$Context) {
                # Register context for connection
                $Context = $Contexts[$Connection] = [PSCustomObject] @{
                    Connection      = $Connection   # The SqlConnection itself
                    IsDisconnecting = $false        # Whether the script has requested to disconnect this connection
                    HasErrors       = $false        # Whether the connection has reported an error message
                }

                # Clean up when connection is disposed
                $Connection.add_Disposed({
                    param($Sender, $Data)
                    Lock-Object $Contexts.SyncRoot {
                        # Unregister context for connection
                        $Context = $Contexts[$Sender]
                        if (!$Context) { return }
                        $Contexts.Remove($Sender)

                        # Detect unexpected close
                        if ($Context.IsDisconnecting) { return }
                        throw "The connection to the database server was closed unexpectedly."
                    }
                }); 
            }

            # Return context
            $Context
        }
    }
}

# [Internal]
function Ensure-SqlConnection {
    <#
    .SYNOPSIS
        Ensures that the given variable holds an open connection.
    #>
    param (
        [ref] [System.Data.SqlClient.SqlConnection] $Connection
    )

    # Ensure that there is a connection object
    if ($Connection.Value) {
        # Use existing connection
        # Caller should NOT disconnect when done
        $OwnsConnection = $false
    } else {
        # No existing connection; create one now
        # Caller should disconnect when done
        $Connection.Value = Connect-Sql
        $OwnsConnection = $true
    }

    # Ensure the connection is open
    if ($Connection.Value.State -ne [System.Data.ConnectionState]::Open) {
        $Connection.Value.Open()
    }

    $OwnsConnection
}

# [Internal]
function Write-SqlErrors {
    <#
    .SYNOPSIS
        Prints messages received from the server.
    #>
    param (
        [System.Data.SqlClient.SqlErrorCollection] $Errors,
        [PSCustomObject] $Context
    )

    $Errors | % {
        if ($_.Class -le 10 <# max informational severity #>) {
            # Informational message
            Write-Host $_.Message
        } else {
            # Warning or error message
            $Message = ($_.Procedure, "(batch)" -ne "")[0]
            $Message = "$($Message):$($_.LineNumber): E$($_.Class): $($_.Message)"
            Write-Warning $Message

            # Mark current command as failed
            $Context.HasErrors = $true
        }
    }
}
