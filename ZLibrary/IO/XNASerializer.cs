using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading;
using System.Security.AccessControl;

using Microsoft.Xna.Framework;

namespace ZLibrary.IO
{
    public class XNASerializer
    {
        #region Local

        //Main File Stream
        private FileStream file = null;
        //For Serializing
        private StreamWriter writer = null;
        //For Deserializing
        private StreamReader reader = null;

        //Path Information
        private String fullPath = "";
        private String fileName = "";
        private String pathName = "";

        //Serialization Information
        private int tabAmount;
        private String tab;

        //Deserialization Information
        String lastRead = "";
        Stack<String> tagsRead = new Stack<String>();
        String currentTag = "";

        /// <summary>
        /// Creates A New Serializer For The Specified File
        /// </summary>
        /// <param name="fullPath">The Path For The File</param>
        public XNASerializer(String fullPath)
        {
            this.fullPath = fullPath;
            pathName = Path.GetDirectoryName(fullPath);
            fileName = Path.GetFileName(fullPath);
        }

        /// <summary>
        /// Opens The File Associated With The Serializer
        /// </summary>
        /// <param name="toWrite">To Open A Write- Or Read-Only File </param>
        /// <exception cref="FileNotFoundException">When Trying To Read And File Cannot Be Found</exception>
        public void openFile(bool toWrite)
        {
            if (writer != null || reader != null || file != null)
            {
                #region Make Sure To Close All The Streams
                try
                {
                    if (writer != null)
                    {
                        writer.Flush();
                        writer.Close();
                        writer = null;
                    }
                }
                catch (Exception)
                {
                    //Nothing Bad Will Happen
                }
                try
                {
                    if (reader != null)
                    {
                        reader.Close();
                        reader = null;
                    }
                }
                catch (Exception)
                {
                    //Nothing Bad Will Happen
                }
                try
                {
                    if (file != null)
                    {
                        file.Flush();
                        file.Close();
                        file = null;
                    }
                }
                catch (Exception)
                {
                    //Nothing Bad Will Happen
                }
                #endregion
            }
            if (toWrite)
            {
                #region Open A Writeable File
                while (file == null)
                {
                    try
                    {
                        //Just Create A New File (Overwriting Previous Ones
                        file = File.Open(fullPath, FileMode.Create);
                    }
                    catch (DirectoryNotFoundException)
                    {
                        //Create The Directory And The File
                        DirectorySecurity sec = new DirectorySecurity();
                        Directory.CreateDirectory(Path.GetDirectoryName(fullPath), sec);
                        file = File.Open(fullPath, FileMode.Create);
                    }
                    catch (Exception)
                    {
                        //File Is Currently In Use
                        //Wait A Little While
                        Thread.Sleep(25);
                    }
                }
                writer = new StreamWriter(file);
                #endregion
            }
            else
            {
                #region Try To Open A Readable File
                while (file == null)
                {
                    try
                    {
                        //Open An Existing File
                        file = File.Open(fullPath, FileMode.Open);
                    }
                    catch (FileNotFoundException)
                    {
                        //No File To Open
                        throw new FileNotFoundException("{0} Does Not Exist", fileName);
                    }
                    catch (DirectoryNotFoundException)
                    {
                        //No Directory To House File
                        throw new FileNotFoundException("{0} Does Not Exist", pathName);
                    }
                    catch (Exception)
                    {
                        //File Is Currently In Use
                        //Wait A Little While
                        Thread.Sleep(25);
                    }
                }
                reader = new StreamReader(file);
                #endregion
            }
        }
        /// <summary>
        /// Fully Closes A File And All Its Associated Streams
        /// </summary>
        public void closeFile()
        {
            if (file != null)
            {
                try
                {
                    if (writer != null)
                    {
                        writer.Flush();
                        writer.Close();
                        writer = null;
                        file = null;
                    }
                    else if (reader != null)
                    {
                        reader.Close();
                        reader = null;
                        file = null;
                    }
                    else
                    {
                        file.Flush();
                        file.Close();
                        file = null;
                    }
                }
                catch (Exception)
                {
                    //Nothing Bad Happened
                }
            }
        }

        /// <summary>
        /// Tabs Over In Serializer
        /// </summary>
        public void tabOver()
        {
            tabAmount++;
            tab = XNASerializer.getTab(tabAmount);
        }
        /// <summary>
        /// Tabs Back In The Serializer, If Possible
        /// </summary>
        public void tabBack()
        {
            if (tabAmount > 0)
            {
                tabAmount--;
                tab = XNASerializer.getTab(tabAmount);
            }
        }

        /// <summary>
        /// Writes A Line That Is Tabbed Over In The Serializable
        /// </summary>
        /// <param name="line">The Line To Be Written</param>
        public void writeLine(String line)
        {
            writer.WriteLine(tab + line);
        }

        /// <summary>
        /// Reads Data From File Without Any Whitespace
        /// </summary>
        /// <returns>String Representation Of Data</returns>
        public String readLine()
        {
            lastRead = reader.ReadLine().Trim();
            return lastRead;
        }
        /// <summary>
        /// Checks If The Serializer Has Reached The End Of A Tag
        /// </summary>
        /// <param name="name">The Name Of The Tag</param>
        /// <returns>True If End Tag Is Reached</returns>
        public bool hasReadEndTag(String name)
        {
            String tempTag = breakTag(lastRead);
            if (!tempTag.Equals(lastRead))
            {
                currentTag = tempTag;
                return lastRead.Equals(XNASerializer.getEndTag(name));
            }
            return false;
        }
        /// <summary>
        /// Checks If The Serializer Has Reached The Ended Of The File
        /// </summary>
        /// <returns>True If End Of File Is Reached</returns>
        public bool isAtEndOfFile()
        {
            return hasReadEndTag("Asset") || hasReadEndTag("XnaContent");
        }

        #endregion

        #region Static

        public const String TAB = "  ";
        public const String NEW_LINE = "\r\n";

        /// <summary>
        /// Fully Serializes An Object
        /// </summary>
        /// <typeparam name="T">Type Of Object To Be Serialized</typeparam>
        /// <param name="obj">The Object</param>
        /// <param name="fullPath">The Path Of The File To Serialize Of Which To Serialize It</param>
        public static void serialize<T>(T obj, String fullPath) where T : IXNASerializable
        {
            //Create The Serialization Stream For The Object
            XNASerializer serializer = new XNASerializer(fullPath);

            //Open The File
            serializer.openFile(true);

            //Serialize The Object
            obj.serialize(serializer);
        }
        /// <summary>
        /// Deserialize An Object From A File
        /// </summary>
        /// <typeparam name="T">The XNA-Serializable Object Type To Be Built</typeparam>
        /// <param name="fullPath">The File Containing The Information</param>
        /// <returns>A New Deserialized Object</returns>
        /// <exception cref="FileNotFoundException">File Was Not Found For The Object</exception>
        public static T deserialize<T>(String fullPath) where T : IXNASerializable, new()
        {
            //Create The Object
            T obj = new T();

            //Create The Serialization Stream For The Object
            XNASerializer serializer = new XNASerializer(fullPath);
            try
            {
                //Try To Open The File
                serializer.openFile(false);
            }
            catch (FileNotFoundException e)
            {
                //The File Was Not Found
                throw new FileNotFoundException(e.Message);
            }

            //Build The Object
            obj.deserialize(serializer);

            //Return The Object
            return obj;
        }


        public static String getTab(int amount)
        {
            String r = "";
            for (int i = 0; i < amount; i++) { r += TAB; }
            return r;
        }
        public static String getStartTag(String name)
        {
            return "<" + name + ">";
        }
        public static String getEndTag(String name)
        {
            return "</" + name + ">";
        }
        public static string breakTag(String tag)
        {
            string r = tag;
            //Chop Off Beginning / End
            if (r[0] == '<') { r = r.Substring(1, r.Length - 1); }
            if (r[0] == '/') { r = r.Substring(1, r.Length - 1); }
            if (r[r.Length - 1] == '>') { r = r.Substring(0, r.Length - 1); }
            return r;
        }

        public static String getHeader()
        {
            return
                "<?xml version=\"1.0\" encoding=\"utf-8\" ?>" + NEW_LINE +
                getStartTag("XnaContent");
        }
        public static void writeFullHeader(XNASerializer serializer, IXNASerializable obj)
        {
            String header = getHeader();
            serializer.writeLine(header);

            serializer.tabOver();

            serializer.writeLine(getStartTag("Asset Type=\"" + obj.getDataTypeName() + "\""));

            serializer.tabOver();
        }
        public static String getFooter()
        {
            return getEndTag("XnaContent");
        }
        public static void writeFullFooter(XNASerializer serializer)
        {
            serializer.tabBack();

            serializer.writeLine(getEndTag("Asset"));

            serializer.tabBack();

            String footer = getFooter();
            serializer.writeLine(footer);
        }

        public static String convert(Color color)
        {
            byte[] a = new byte[] { color.A, color.R, color.G, color.B };
            char[] c = new char[8];
            byte b;
            for (int i = 0; i < 8; )
            {
                b = ((byte)(a[i] >> 4));
                c[i] = (char)(b > 9 ? b + 0x37 : b + 0x30);

                b = ((byte)(a[i++] & 0xF));
                c[i++] = (char)(b > 9 ? b + 0x37 : b + 0x30);
            }
            return new String(c);
        }
        public static String convert(byte n)
        {
            return Convert.ToString(n);
        }
        public static String convert(int n)
        {
            return Convert.ToString(n);
        }
        public static String convert(long n)
        {
            return Convert.ToString(n);
        }
        public static String convert(float n)
        {
            return Convert.ToString(n);
        }
        public static String convert(double n)
        {
            return Convert.ToString(n);
        }
        public static String convert(String s)
        {
            return getStartTag("Item") + s + getEndTag("Item");
        }

        #endregion
    }
}
