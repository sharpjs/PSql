using System;
using System.Runtime.Serialization;

namespace PSql
{
    [Serializable]
    public class SqlCmdException : Exception
    {
        public SqlCmdException()
            : base ("An error occurred during SqlCmd preprocessing.") { }

        public SqlCmdException(string message)
            : base(message) { }

        public SqlCmdException(string message, Exception innerException)
            : base(message, innerException) { }

        protected SqlCmdException(SerializationInfo info, StreamingContext context)
            : base(info, context) { }
    }
}
