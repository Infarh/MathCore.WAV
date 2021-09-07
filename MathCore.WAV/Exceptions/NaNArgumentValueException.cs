using System;
using System.Runtime.Serialization;

namespace MathCore.WAV.Exceptions
{
    [Serializable]
    public class NaNArgumentValueException : ArgumentException
    {
        public NaNArgumentValueException(string Message, string ParamName) : base(Message, ParamName) { }

        public NaNArgumentValueException() { }
        public NaNArgumentValueException(string message) : base(message) { }
        public NaNArgumentValueException(string message, Exception inner) : base(message, inner) { }

        protected NaNArgumentValueException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }
    }
}