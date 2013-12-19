using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace System.IO
{
    public interface ISerializable
    {
        void write(StreamWriter s);
        void read(StreamReader s);
    }
}
