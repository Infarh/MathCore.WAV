using System;
using System.Runtime.Serialization;

namespace MathCore.WAV.Exceptions
{
    [Serializable]
    public class ArgumentArrayLengthException : ArgumentException
    {
        public int ActualLength { get; }
        public int ExpectedLength { get; }

        public ArgumentArrayLengthException(string ParamName, int ActualLength, int ExpectedLength, string Message)
            : base(Message, ParamName)
        {
            this.ActualLength = ActualLength;
            this.ExpectedLength = ExpectedLength;
        }

        public ArgumentArrayLengthException() { }
        public ArgumentArrayLengthException(string message) : base(message) { }
        public ArgumentArrayLengthException(string message, Exception inner) : base(message, inner) { }

        protected ArgumentArrayLengthException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }

        public override string ToString() => $"{Message} - {ParamName}.Length = {ActualLength}; Expected length = {ExpectedLength}";
    }
}
