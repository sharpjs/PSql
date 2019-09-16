if ($PSEdition -eq 'Desktop') {
    Add-Type -Path (Join-Path $PSScriptRoot net461\System.Data.SqlClient.dll)
}
