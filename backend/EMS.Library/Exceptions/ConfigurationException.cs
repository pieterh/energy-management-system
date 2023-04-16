using System.Runtime.Serialization;

namespace EMS.Library.Exceptions
{
    [Serializable]
    public class ConfigurationException : ApplicationException
    {
        public ConfigurationException() { }
        public ConfigurationException(string message) : base(message) { }
        public ConfigurationException(string message, Exception innerException) : base(message, innerException) { }
        protected ConfigurationException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}
