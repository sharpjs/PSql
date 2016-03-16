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
    The 'connection' returned from Connect-Sql -PassThru is actually one of these.
#>
function New-ConnectionContext {
    param(
        [Parameter(ValueFromPipeline)]
        [System.Data.SqlClient.SqlConnection] $Connection
    )
    process {
        Write-Output ([PSCustomObject] @{
            Connection      = $Connection   # The SqlConnection itself
            IsDisconnecting = $false        # Whether the script has requested to disconnect this connection
            HasErrors       = $false        # Whether the connection has reported an error message
        })
    }
}

<#
    All live connection contexts are tracked here.
#>
$Contexts = [hashtable]::Synchronized(@{})

<#
    For convenience, there is a default connection context for the script.
    Cmdlets use the default context when one is not specified via arguments.
#>
$DefaultContext = $null

function Connect-Sql {
    <#
    .SYNOPSIS
        Connects to the specified SQL Server instance.
    #>
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
        [string] $Password,

        # Return the connection object; leave the default connection unchanged.
        [Parameter()]
        [switch] $PassThru
    )

    # Disconnect if using default context
    if (!$PassThru) {
        Disconnect-Sql -Connections $DefaultContext
    }

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
        Write-SqlErrors $Data.Errors $Contexts[$Sender]
    }); 

    # Set up to catch unexpected disconnection
    $Connection.add_StateChange({
        param($Sender, $Data)
        if ($Data.CurrentState -eq [System.Data.ConnectionState]::Open) { return }
        if ($Contexts[$Sender].IsDisconnecting) { return }
        throw "The connection to the database server was closed unexpectedly."
    })

    # Track contextual state for the connection
    $Context = New-ConnectionContext $Connection
    $Contexts[$Connection] = $Context

    # Open the connection
    $Connection.Open()

    # Return connection context
    if ($PassThru) {
        Write-Output $Context
    } else {
        $script:DefaultContext = $Context
    }
}

function Disconnect-Sql {
    <#
    .SYNOPSIS
        Disconnects the given connection(s).
    #>
    param(
        # The connections to disconnect.  These must be objects returned by the PSql\Connect-Sql -PassThru cmdlet.  If none are given, the default connection is disconnected.
        [Parameter(ValueFromPipeline, ValueFromRemainingArguments)]
        [PSCustomObject[]] $Connections = (,$DefaultContext)
    )
    process {
        $Connections | ? { $_ } | % {
            $_.IsDisconnecting = $true
            $_.Connection.Dispose()
            $Contexts.Remove($_.Connection)
            $_.Connection = $null
        }
    }
}

function Write-SqlErrors {
    param (
        [System.Data.SqlClient.SqlErrorCollection] $Errors,
        [PSCustomObject] $Context
    )

    foreach ($e in $Errors) {
        if ($e.Class -le 10 <# max informational severity #>) {
            # Informational message
            Write-Verbose $e.Message
        } else {
            # Warning or error message
            $Message = ($e.Procedure, "(batch)" -ne "")[0]
            $Message = "$($Message):$($e.LineNumber): E$($e.Class): $($e.Message)"
            Write-Warning $Message

            # Mark current command as failed
            $Context.HasErrors = $true
        }
    }
}
