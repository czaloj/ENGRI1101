using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ZLibrary.IO.FileSystem
{
    public struct SystemFile
    {
        private string name;
        public string Name { get { return name; } set { name = value; } }

        private string extension;
        public string Extension { get { return extension; } set { extension = value; } }

        public string FullName
        {
            get { return name + "." + extension; }
        }

        public SystemFile(string full)
        {
            string[] split = full.Split(".".ToArray<char>(), 2, StringSplitOptions.RemoveEmptyEntries);
            if (split != null && split.Length == 2)
            {
                name = split[0];
                extension = split[1];
            }
            else
            {
                name = "[_]";
                extension = "[_]";
            }
        }
        public SystemFile(string name, string extension)
        {
            this.name = (name != null) ? name : "[_]";
            this.extension = (extension != null) ? extension : "[_]";
        }
    }
}
