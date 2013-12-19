using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ZLibrary.ADT
{
    public interface IIDHolder
    {
        long ID { get; }
        void setID(long ID);
    }
}
