using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ZLibrary.IO
{
    public interface IXNASerializable
    {
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        /// <example>"System.String[]"</example>
        String getDataTypeName();

        void serialize(XNASerializer serializer);
        void deserialize(XNASerializer serializer);
    }
}
