<#
    Copyright (C) 2019 Jeffrey Sharp

    Permission to use, copy, modify, and distribute this software for any
    purpose with or without fee is hereby granted, provided that the above
    copyright notice and this permission notice appear in all copies.

    THE SOFTWARE IS PROVIDED "AS IS" AND THE AUTHOR DISCLAIMS ALL WARRANTIES
    WITH REGARD TO THIS SOFTWARE INCLUDING ALL IMPLIED WARRANTIES OF
    MERCHANTABILITY AND FITNESS. IN NO EVENT SHALL THE AUTHOR BE LIABLE FOR
    ANY SPECIAL, DIRECT, INDIRECT, OR CONSEQUENTIAL DAMAGES OR ANY DAMAGES
    WHATSOEVER RESULTING FROM LOSS OF USE, DATA OR PROFITS, WHETHER IN AN
    ACTION OF CONTRACT, NEGLIGENCE OR OTHER TORTIOUS ACTION, ARISING OUT OF
    OR IN CONNECTION WITH THE USE OR PERFORMANCE OF THIS SOFTWARE.
#>
@{
    # Identity
    GUID          = '218cb4b2-911a-46b1-b47c-d3504acd4627'
    RootModule    = 'PSql.Core.dll'
    ModuleVersion = '2.0.0'

    # General
    Author      = 'Jeffrey Sharp'
    CompanyName = 'Subatomix Research, Inc.'
    Copyright   = 'Copyright (C) 2019 Jeffrey Sharp'
    Description = 'Provides basic cmdlets to connect and invoke commands against SQL Server and Azure SQL databases.'

    # Requirements
    PowerShellVersion      = '5.1'
    CompatiblePSEditions   = "Desktop", "Core"  # Added in PowerShell 5.1
    DotNetFrameworkVersion = '4.5.2'            # Valid for Desktop edition only
    CLRVersion             = '4.0'              # Valid for Desktop edition only

    # Exports
    # NOTE: Use empty arrays to indicate no exports.
    FunctionsToExport    = @()
    VariablesToExport    = @()
    AliasesToExport      = @()
    DscResourcesToExport = @()
    CmdletsToExport      = @(
        "New-SqlContext",
        "Connect-Sql",
        "Disconnect-Sql",
        "Expand-SqlCmdDirectives",
        "Invoke-Sql"
    )

    # Discoverability and URLs
    PrivateData = @{
        PSData = @{
            Tags = @("SQL", "Server", "Azure", "Invoke", "SqlCmd")
            LicenseUri = 'https://github.com/sharpjs/PSql/blob/master/LICENSE.txt'
            ProjectUri = 'https://github.com/sharpjs/PSql'
            # IconUri = ''
            ReleaseNotes = @"
Release notes are available at:
https://github.com/sharpjs/PSql/releases
"@
        }
    }
}
