using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Microsoft.Xna.Framework.Content;

namespace ZLibrary.IO
{
    public class ContentSearchData<T> where T : class
    {
        public ContentSearchDir[] searches;
        private Dictionary<String, T> data = new Dictionary<String, T>();

        public T this[String name]
        {
            get
            {
                try
                {
                    return data[name];
                }
                catch (Exception)
                {
                    return null;
                }
            }
        }

        public ContentSearchData(ContentSearchDir[] dirs, ContentManager content)
        {
            searches = dirs;
            DirectoryInfo dir;
            foreach (ContentSearchDir search in searches)
            {
                dir = new DirectoryInfo(search.dir);
                if (dir.Exists)
                {
                    loadAll(search, dir, content);
                }
            }
        }

        private void loadAll(ContentSearchDir search, DirectoryInfo dir, ContentManager content)
        {
            String fName;
            String dName;
            foreach (FileInfo f in dir.GetFiles())
            {
                try
                {
                    fName = search.name(f);
                    dName = f.FullName.Replace(content.RootDirectory + "\\", "").Replace(f.Extension, "");
                    data[fName] = content.Load<T>(dName);
                }
                catch (Exception)
                {
                    //Nothing
                }
            }
            foreach (DirectoryInfo d in dir.GetDirectories())
            {
                //Repeat
                loadAll(search, d, content);
            }
        }
    }

    public struct ContentSearchDir
    {
        public String dir;
        public String title;

        public ContentSearchDir(String dir, String title)
        {
            this.dir = new DirectoryInfo(dir).FullName;
            this.title = title + "-";
        }

        public String name(FileInfo f)
        {
            String r = (f.Directory + f.Name).Replace(dir, title).Replace("\\", ".").Replace(f.Extension, "");
            return r;
        }
    }
}
