using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;

namespace EMS.Library.Exceptions
{
    [Serializable]
    public class NullException : ApplicationException
    {
        public NullException() { }
        public NullException(string message) : base(message) { }
        public NullException(string message, Exception innerException) : base(message, innerException) { }
        protected NullException(SerializationInfo info, StreamingContext context) : base(info, context) { }
        public static void ThrowIfNull([NotNull]object? obj, string message, [CallerArgumentExpression("obj")] string? paramName = null)
        {
            if (obj == null)
                throw new NullException(message);
        }
    }
}
