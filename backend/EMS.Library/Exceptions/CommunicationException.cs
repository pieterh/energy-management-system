using System.Runtime.Serialization;

namespace EMS.Library.Exceptions
{
    [Serializable]
    public class CommunicationException : ApplicationException
    {
        public CommunicationException() { }
        public CommunicationException(string message) : base(message) { }
        public CommunicationException(string message, Exception innerException) : base(message, innerException) { }
        protected CommunicationException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}
