using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace ZLibrary.IO.FileSystem
{
    public class IOSystem
    {
        private SystemDirectory root;
        public SystemDirectory Root { get { return root; } set { root = value; root.checkConsistency(); } }

        public void checkConsistency()
        {
            root.checkConsistency();
        }

        public List<FileError> buildSystem()
        {
            string newDir = root.CompletePath.Trim();
            if (newDir != null && !newDir.Equals(""))
            {
                if (!Directory.Exists(newDir))
                {
                    Directory.CreateDirectory(newDir);
                }
            }
            return buildDirectory(ref root);
        }

        private List<FileError> buildDirectory(ref SystemDirectory directory)
        {
            FileError error;
            List<FileError> errors = new List<FileError>();

            //Get Full Directory
            string dir = directory.CompletePath.Trim();

            //Check/Build New Files
            if (directory.HasFiles)
            {
                string newFile;
                for (int i = 0; i < directory.FileCount; i++)
                {
                    newFile = dir + "\\" + directory.Files[i].FullName;
                    if (!File.Exists(newFile))
                    {
                        File.Create(newFile).Close();

                        //Add The Error
                        error = new FileError();
                        error.Path = newFile;
                        error.File = directory.Files[i];
                        errors.Add(error);
                    }
                }
            }

            //Check/Build New Directories
            if (directory.HasDirectories)
            {
                string newDir;
                for (int i = 0; i < directory.DirectoryCount; i++)
                {
                    newDir = directory.Directories[i].CompletePath;
                    if (newDir != null && !newDir.Equals(""))
                    {
                        if (!Directory.Exists(newDir))
                        {
                            Directory.CreateDirectory(newDir);
                        }
                        errors.AddRange(buildDirectory(ref directory.Directories[i]));
                    }
                }
            }
            return errors;
        }


    }

    public struct FileError
    {
        public SystemFile File { get; set; }
        public string Path { get; set; }
    }
}
