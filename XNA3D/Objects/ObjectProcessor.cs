using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Content;
using XNA3D.Objects.Interfaces;
using ZLibrary.ADT;

namespace XNA3D.Objects
{
    public static class ObjectProcessor
    {
        private static IDList<I3DObject> objects;

        static ObjectProcessor()
        {
            objects = new IDList<I3DObject>();
        }

        public static long processObject<T>(String file, ContentManager content) where T : class, I3DObject, new()
        {
            T o = content.Load<T>(file);
            o.build();
            objects.addObj(o);
            return o.ID;
        }
        public static I3DObject getObject(long ID)
        {
            return objects.getObj(ID).Clone() as I3DObject;
        }
        public static T getObject<T>(long ID) where T : class, I3DObject, new()
        {
            return objects.getObj(ID).Clone() as T;
        }
    }
}
