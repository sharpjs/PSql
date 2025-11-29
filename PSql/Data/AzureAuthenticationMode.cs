// Copyright Subatomix Research Inc.
// SPDX-License-Identifier: MIT

namespace PSql;

/// <summary>
///   Modes for authenticating connections to Azure SQL Database and compatible
///   databases.
/// </summary>
public enum AzureAuthenticationMode // ~> M.D.S.SqlAuthenticationMethod
{
    // Source: https://learn.microsoft.com/en-ca/sql/connect/ado-net/sql/azure-active-directory-authentication

    /// <summary>
    ///   Default authentication mode.
    /// </summary>
    /// <remarks>
    ///   The actual authentication mode depends on
    ///     the value of the <see cref="SqlContext.Credential"/> property.
    ///   If the property is non-<see langword="null"/>,
    ///     this mode selects SQL authentication using the credential.
    ///   If the property is <see langword="null"/>,
    ///     this mode selects Entra ID managed identity authentication.
    /// </remarks>
    Default = 0, // NotSpecified

    /// <summary>
    ///   SQL authentication mode.
    /// </summary>
    /// <remarks>
    ///   The <see cref="SqlContext.Credential"/> property should contain the
    ///   name and password stored for a server login or contained database
    ///   user.
    /// </remarks>
    SqlPassword = 1, // SqlPassword,

    /// <summary>
    ///   Entra ID password authentication mode.
    /// </summary>
    /// <remarks>
    ///   The <see cref="SqlContext.Credential"/> property should contain the
    ///   name and password of an Azure AD principal.
    /// </remarks>
    AadPassword = 2, // ActiveDirectoryPassword,

    /// <summary>
    ///   Entra ID integrated authentication mode.
    /// </summary>
    /// <remarks>
    ///   The identity of the process should be an Azure AD principal.
    /// </remarks>
    AadIntegrated = 3, // ActiveDirectoryIntegrated,

    /// <summary>
    ///   Entra ID interactive authentication mode, also known as Universal
    ///   Authentication with MFA.
    /// </summary>
    /// <remarks>
    ///   Authentication uses an interactive flow and supports multiple factors.
    /// </remarks>
    AadInteractive = 4, // ActiveDirectoryInteractive,

    /// <summary>
    ///   Entra ID service principal authentication mode.
    /// </summary>
    /// <remarks>
    ///   The <see cref="SqlContext.Credential"/> property should contain the
    ///   client ID and secret of an Azure AD service principal.
    /// </remarks>
    AadServicePrincipal = 5, // ActiveDirectoryServicePrincipal

    /// <summary>
    ///   Entra ID device code flow authentication mode.
    /// </summary>
    /// <remarks>
    ///   Use this mode to connect to Azure SQL Database from devices that do
    ///   not provide a web browser, using another device to perform
    ///   interactive authentication.
    /// </remarks>
    AadDeviceCodeFlow = 6, // ActiveDirectoryDeviceCodeFlow

    /// <summary>
    ///   Entra ID managed identity authentication mode.
    /// </summary>
    /// <remarks>
    ///   For a user-assigned managed identity,
    ///     the <see cref="SqlContext.Credential"/> property's username
    ///     should be the client ID of the identity; the password is ignored.
    ///   For a system-assigned managed identity,
    ///     the <see cref="SqlContext.Credential"/> property
    ///     should be <see langword="null"/>.
    /// </remarks>
    AadManagedIdentity = 7, // ActiveDirectoryManagedIdentity

    /// <summary>
    ///   Entra ID default authentication mode.  This mode attempts multiple
    ///   <strong>non-interactive</strong> authentication methods sequentially.
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     This mode attempts in order:
    ///   </para>
    ///   <list type="bullet">
    ///     <item>
    ///       <term>Environment</term>
    ///       <description>
    ///         Credential
    ///         <a href="https://learn.microsoft.com/en-ca/dotnet/api/azure.identity.environmentcredential"
    ///         >configured in environment variables</a>.
    ///       </description>
    ///     </item>
    ///     <item>
    ///       <term>Managed Identity</term>
    ///       <description>
    ///         Entra ID managed identity.
    ///         For a user-assigned managed identity,
    ///           the <see cref="SqlContext.Credential"/> property's username
    ///           should be the object ID of the identity; the password is ignored.
    ///         For a system-assigned managed identity,
    ///           the <see cref="SqlContext.Credential"/> property
    ///           should be <see langword="null"/>.
    ///       </description>
    ///     </item>
    ///     <item>
    ///       <term>Shared Token Cache</term>
    ///       <description>
    ///         Local cache shared between Microsoft applications.
    ///       </description>
    ///     </item>
    ///     <item>
    ///       <term>Visual Studio</term>
    ///       <description>
    ///         Token cached by Visual Studio.
    ///       </description>
    ///     </item>
    ///     <item>
    ///       <term>Visual Studio Code</term>
    ///       <description>
    ///         Token cached by Visual Studio Code.
    ///       </description>
    ///     </item>
    ///     <item>
    ///       <term>Azure CLI</term>
    ///       <description>
    ///         Token cached by the Azure CLI.
    ///       </description>
    ///     </item>
    ///   </list>
    /// </remarks>
    AadDefault = 9, // ActiveDirectoryDefault

    /// <summary>
    ///   Entra ID workload identity authentication mode.
    /// </summary>
    /// <remarks>
    ///   This mode is similar to managed identity authentication, except that
    ///   default values are taken from environment variables.  If the
    ///   <see cref="SqlContext.Credential"/> property is not
    ///   <see langword="null"/>, the credential's username overrides any
    ///   client ID found in the environment, and the credential's password is
    ///   ignored.
    /// </remarks>
    /// <see href="https://learn.microsoft.com/en-ca/sql/connect/ado-net/sql/azure-active-directory-authentication#using-workload-identity-authentication"/>
    AadWorkloadIdentity = 10, // ActiveDirectoryWorkloadIdentity
}
