using System;
namespace EMS.Library.Exceptions
{
	public class HEMSApplicationException : Exception
	{
		public HEMSApplicationException(string? message) : base(message) { }
		public HEMSApplicationException(string? message, Exception? innerException) : base(message, innerException) { }
	}
}
