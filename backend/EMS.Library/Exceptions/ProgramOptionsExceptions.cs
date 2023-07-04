using System.Runtime.Serialization;

namespace EMS.Library.Exceptions;

[Serializable]
public class ProgramOptionsException : Exception
{
    public ProgramOptionsException() { }
    public ProgramOptionsException(string? message) : base(message) { }
    public ProgramOptionsException(string? message, Exception? innerException) : base(message, innerException) { }
    protected ProgramOptionsException(SerializationInfo info, StreamingContext context) : base(info, context) { }
}
