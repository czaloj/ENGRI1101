using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ZLibrary.ADT
{
    public class IDListFactory<T> : IDList<T> where T : class, IIDHolder, ICloneable
    {
        public override T getObj(long ID)
        {
            return (T)base.getObj(ID).Clone();
        }
    }
}
