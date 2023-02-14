using System;
using System.Runtime.Serialization;

namespace EMS.Library.Exceptions
{
    [Serializable]
#pragma warning disable S101
    public class HEMSApplicationException : Exception
	{
#pragma warning restore S101
        public HEMSApplicationException(string? message) : base(message) { }
		public HEMSApplicationException(string? message, Exception? innerException) : base(message, innerException) { }
        protected HEMSApplicationException(SerializationInfo info, StreamingContext context) : base(info, context) { }

    }
}
