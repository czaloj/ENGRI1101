using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace System.IO
{
    [Serializable]
    public class SerializingException : Exception
    {
        public SerializingException() { }
        public SerializingException(string message) : base(message) { }
        public SerializingException(string message, Exception inner) : base(message, inner) { }
        protected SerializingException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context)
            : base(info, context) { }
    }
}
