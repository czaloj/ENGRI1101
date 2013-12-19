using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ZLibrary.IO.FileSystem
{
    public class SystemDirectory
    {
        private string name;
        public string Name { get { return name; } set { name = value; } }

        private string rootPath;
        public string RootPath { get { return rootPath; } }
        public string CompletePath
        {
            get
            {
                if (HasParent)
                {
                    return rootPath + "\\" + name;
                }
                else
                {
                    return name;
                }
            }
        }

        private int depth;
        public int Depth { get { return depth; } }

        protected SystemDirectory parent;
        public bool HasParent { get { return parent != null; } }

        private SystemDirectory[] directories;
        public SystemDirectory[] Directories
        {
            get { return directories; }
            set
            {
                directories = value;
                if (directories != null)
                {
                    for (int i = 0; i < directories.Length; i++)
                    {
                        directories[i].parent = this;
                        directories[i].checkConsistency();
                    }
                }
            }
        }
        public bool HasDirectories { get { return directories != null; } }
        public int DirectoryCount { get { return directories.Length; } }

        private SystemFile[] files;
        public SystemFile[] Files { get { return files; } set { files = value; } }
        public bool HasFiles { get { return files != null; } }
        public int FileCount { get { return files.Length; } }

        public SystemDirectory()
        {
            //Set Name
            this.name = "[_]";

            //Set Parent And Depth
            parent = null;
            depth = 0;
            rootPath = "";

            //Set The Subdirectories and Files
            directories = null;
            files = null;
        }
        public SystemDirectory(string name, SystemDirectory parent, SystemDirectory[] subDirectories, params SystemFile[] files)
        {
            //Set Name
            this.name = name;

            //Set Parent And Depth
            if (parent != null)
            {
                this.parent = parent;
                depth = this.parent.depth + 1;
                rootPath = parent.CompletePath;
            }
            else
            {
                this.parent = null;
                depth = 0;
                rootPath = "";
            }

            //Set The Subdirectories and Files
            Directories = subDirectories;
            this.files = files;
        }

        public void checkConsistency()
        {
            if (parent != null)
            {
                depth = parent.depth + 1;
                rootPath = parent.CompletePath;
            }
            else
            {
                depth = 0;
                rootPath = "";
            }

            if (directories != null)
            {
                for (int i = 0; i < directories.Length; i++)
                {
                    directories[i].checkConsistency();
                }
            }
        }
    }
}
