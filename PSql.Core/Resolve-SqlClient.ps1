if ($PSEdition -eq 'Desktop')
{
    # PowerShell 5.1 on Windows
    Add-Type -Path (Join-Path $PSScriptRoot deps\win-net461\Microsoft.Data.SqlClient.dll)
}
elseif ($IsWindows)
{
    # PowerShell 6.x+ on Windows
    Add-Type -Path (Join-Path $PSScriptRoot deps\win-netcoreapp2.1\Microsoft.Data.SqlClient.dll)
}
else
{
    # PowerShell 6.x+ on *nix
    Add-Type -Path (Join-Path $PSScriptRoot deps\unix-netcoreapp2.1\Microsoft.Data.SqlClient.dll)
}
