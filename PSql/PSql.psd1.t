<#
    Copyright 2020 Jeffrey Sharp

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
    RootModule    = 'PSql.dll'
    ModuleVersion = '{VersionPrefix}'

    # General
    Description = 'Provides basic cmdlets to connect and invoke commands against SQL Server and Azure SQL databases.'
    Author      = 'Jeffrey Sharp'
    CompanyName = 'Subatomix Research Inc.'
    Copyright   = '{Copyright}'

    # Requirements
    CompatiblePSEditions = 'Core'
    PowerShellVersion    = '7.0'
    #RequiredModules     = @(...)
    #RequiredAssemblies  = @(...)

    # Initialization
    #ScriptsToProcess = @(...)
    #TypesToProcess   = @(...)
    #FormatsToProcess = @(...)
    #NestedModules    = @(...)

    # Exports
    # NOTE: Use empty arrays to indicate no exports.
    FunctionsToExport    = @()
    VariablesToExport    = @()
    AliasesToExport      = @()
    DscResourcesToExport = @()
    CmdletsToExport      = @(
        "New-SqlContext"
        "Connect-Sql"
        "Disconnect-Sql"
        "Expand-SqlCmdDirectives"
        "Invoke-Sql"
    )

    # Discoverability and URLs
    PrivateData = @{
        PSData = @{
            # Additional metadata
            Prerelease   = '{VersionSuffix}'
            ProjectUri   = 'https://github.com/sharpjs/PSql'
            ReleaseNotes = "https://github.com/sharpjs/PSql/blob/master/CHANGES.md"
            LicenseUri   = 'https://github.com/sharpjs/PSql/blob/master/LICENSE.txt'
            IconUri      = 'https://github.com/sharpjs/PSql/blob/master/icon.png'
            Tags         = @(
                "SQL", "Server", "Azure", "Invoke", "SqlCmd",
                "PSEdition_Core", "Windows", "Linux", "MacOS"
            )
        }
    }
}
