using Sam = Microsoft.Data.SqlClient.SqlAuthenticationMethod;

namespace PSql
{
    /// <summary>
    ///   Modes for authentiating connections to Azure SQL Database and
    ///   compatible databases.
    /// </summary>
    public enum AzureAuthenticationMode
    {
        /// <summary>
        ///   Default authentication mode.  The actual authentication mode
        ///   depends on the value of the <see cref="SqlContext.Credential"/>
        ///   property.  If the property is non-<c>null</c>, this mode selects
        ///   SQL authentication using the credential.  If the property is
        ///   <c>null</c>, this mode selects Azure AD integrated
        ///   authentication.
        /// </summary>
        Default = Sam.NotSpecified,

        /// <summary>
        ///   SQL authentication mode.  The <see cref="SqlContext.Credential"/>
        ///   property should contain the name and password stored for a server
        ///   login or contained database user.
        /// </summary>
        SqlPassword = Sam.SqlPassword,

        /// <summary>
        ///   Azure Active Directory password authentication mode.  The
        ///   <see cref="SqlContext.Credential"/> property should contain the
        ///   name and password of an Azure AD principal.
        /// </summary>
        AadPassword = Sam.ActiveDirectoryPassword,

        /// <summary>
        ///   Azure Active Directory integrated authentication mode.  The
        ///   identity of the process should be an Azure AD principal.
        /// </summary>
        AadIntegrated = Sam.ActiveDirectoryIntegrated,

        /// <summary>
        ///   Azure Active Directory interactive authentication mode, also
        ///   known as Universal Authentication with MFA.  Authentication uses
        ///   an interactive flow and supports multiple factors.
        /// </summary>
        AadInteractive = Sam.ActiveDirectoryInteractive,

        /// <summary>
        ///   Azure Active Directory service principal authentication mode.
        ///   The <see cref="SqlContext.Credential"/> property contains the
        ///   client ID and secret of an Azure AD service principal.
        /// </summary>
        AadServicePrincipal = Sam.ActiveDirectoryServicePrincipal
    }
}
