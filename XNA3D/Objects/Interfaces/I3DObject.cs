using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ZLibrary.ADT;

namespace XNA3D.Objects.Interfaces
{
    public interface I3DObject : I3DDrawable, IIDHolder, ICloneable
    {
        void build();
    }
}
