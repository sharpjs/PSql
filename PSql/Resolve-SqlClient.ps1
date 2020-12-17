@(
    # Required for basic usage
    if (-not ('Microsoft.Data.SqlClient.SqlConnection' -as [type])) {
        Join-Path $PSScriptRoot runtimes ($IsWindows ? "win" : "unix") `
            lib netcoreapp3.1 Microsoft.Data.SqlClient.dll
    }

    # Required for Azure Active Directory authentication modes
    if (-not ('Microsoft.Identity.Client.IAccount' -as [type])) {
        Join-Path $PSScriptRoot Microsoft.Identity.Client.dll
    }
) `
| ForEach-Object { Add-Type -Path $_ }
