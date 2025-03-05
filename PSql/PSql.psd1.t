# Copyright Subatomix Research Inc.
# SPDX-License-Identifier: MIT
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
    PowerShellVersion    = '7.2'
    #RequiredModules     = @(...)
    RequiredAssemblies   = @("PSql.Core.dll")

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
            ReleaseNotes = "https://github.com/sharpjs/PSql/blob/main/CHANGES.md"
            LicenseUri   = 'https://github.com/sharpjs/PSql/blob/main/LICENSE.txt'
            IconUri      = 'https://github.com/sharpjs/PSql/blob/main/icon.png'
            Tags         = @(
                "SQL", "SqlServer", "Azure", "Invoke", "SqlCmd",
                "PSEdition_Core", "Windows", "Linux", "MacOS"
            )
        }
    }
}
