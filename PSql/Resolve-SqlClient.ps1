if ($IsWindows)
{
    Add-Type -Path (Join-Path $PSScriptRoot runtimes\win\lib\netcoreapp3.1\Microsoft.Data.SqlClient.dll)
}
else
{
    Add-Type -Path (Join-Path $PSScriptRoot runtimes/unix/lib/netcoreapp3.1/Microsoft.Data.SqlClient.dll)
}

# Required for Azure Active Directory authentication modes
Add-Type -Path (Join-Path $PSScriptRoot Microsoft.Identity.Client.dll)
