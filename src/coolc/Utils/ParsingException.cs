using System;
using Antlr4.Runtime;
using System.Runtime.Serialization;

namespace coolc.AST
{
    class ParsingException : Exception
    {
        public ParsingException()
        {
        }

        public ParsingException(string message) : base(message)
        {
        }

        public ParsingException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected ParsingException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
        public RecognitionException RecognitionError
        {
            get
            {
                return InnerException as RecognitionException;
            }
        }
    }
}
