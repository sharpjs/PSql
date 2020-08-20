using System.Management.Automation;

namespace PSql
{
    internal static class PSCredentialExtensions
    {
        public static bool IsNullOrEmpty(this PSCredential credential)
            => credential == null
            || credential == PSCredential.Empty;
    }
}
