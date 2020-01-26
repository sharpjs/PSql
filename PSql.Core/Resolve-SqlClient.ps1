if ($PSEdition -eq 'Desktop')
{
    # PowerShell 5.1 on Windows
    Add-Type -Path (Join-Path $PSScriptRoot desktop\Microsoft.Data.SqlClient.dll)
}
elseif ($IsWindows)
{
    # PowerShell 6.x+ on Windows
    Add-Type -Path (Join-Path $PSScriptRoot core\runtimes\win\lib\netcoreapp2.1\Microsoft.Data.SqlClient.dll)
}
else
{
    # PowerShell 6.x+ on *nix
    Add-Type -Path (Join-Path $PSScriptRoot core\runtimes\unix\lib\netcoreapp2.1\Microsoft.Data.SqlClient.dll)
}
