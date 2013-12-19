using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.IO.Compression;

using Microsoft.Xna.Framework;

namespace ZLibrary.IO
{
    /// <summary>
    /// A Class That Provides Functions For
    /// Performing Higher-Level IO Operations
    /// </summary>
    public static class FileIO
    {
        #region Write Specific Value Types
        public static void write(BinaryWriter f, float value)
        {
            byte[] b = BitConverter.GetBytes(value);
            f.Write(b, 0, 4);
        }
        public static void write(BinaryWriter f, Vector2 value)
        {
            write(f, value.X);
            write(f, value.Y);
        }
        public static void write(BinaryWriter f, Vector3 value)
        {
            write(f, value.X);
            write(f, value.Y);
            write(f, value.Z);
        }
        public static void write(BinaryWriter f, Vector4 value)
        {
            write(f, value.X);
            write(f, value.Y);
            write(f, value.Z);
            write(f, value.W);
        }
        #endregion

        #region Read Specific Value Types
        public static float readFloat(BinaryReader f)
        {
            return BitConverter.ToSingle(f.ReadBytes(4), 0);
        }
        public static Vector2 readVector2(BinaryReader f)
        {
            Vector2 ret = Vector2.Zero;
            ret.X = readFloat(f);
            ret.Y = readFloat(f);
            return ret;
        }
        public static Vector3 readVector3(BinaryReader f)
        {
            Vector3 ret = Vector3.Zero;
            ret.X = readFloat(f);
            ret.Y = readFloat(f);
            ret.Z = readFloat(f);
            return ret;
        }
        public static Vector4 readVector4(BinaryReader f)
        {
            Vector4 ret = Vector4.Zero;
            ret.X = readFloat(f);
            ret.Y = readFloat(f);
            ret.Z = readFloat(f);
            ret.W = readFloat(f);
            return ret;
        }
        #endregion

        /// <summary>
        /// Decompresses A Chunk Of Data From A File
        /// </summary>
        /// <param name="fs">The File Name</param>
        /// <param name="offset">Chunk Location In Compressed File</param>
        /// <param name="length">The Length Of The Written Data</param>
        /// <param name="decompressLength">Length Of Data Array To Be Returned</param>
        /// <returns>The Decompressed Data Array</returns>
        public static byte[] decompressFileChunk(String fileName, int offset, int length, int decompressLength)
        {
            FileStream fs = new FileStream(fileName, FileMode.Open);
            return decompressFileChunk(fs, offset, length, decompressLength);
        }
        /// <summary>
        /// Decompresses A Chunk Of Data From A File
        /// </summary>
        /// <param name="fs">The File (Will Be Closed)</param>
        /// <param name="offset">Chunk Location In Compressed File</param>
        /// <param name="length">The Length Of The Written Data</param>
        /// <param name="decompressLength">Length Of Data Array To Be Returned</param>
        /// <returns>The Decompressed Data Array</returns>
        public static byte[] decompressFileChunk(FileStream fs, int offset, int length, int decompressLength)
        {
            byte[] compressed = new byte[length];
            fs.Position = offset;
            fs.Read(compressed, 0, length);
            fs.Close();
            byte[] decompressed = new byte[decompressLength];
            using (MemoryStream inStream = new MemoryStream(compressed))
            using (GZipStream bigStream = new GZipStream(inStream, CompressionMode.Decompress))
            using (MemoryStream bigStreamOut = new MemoryStream())
            {
                bigStream.CopyTo(bigStreamOut);
                bigStreamOut.Position = 0;
                bigStreamOut.Read(decompressed, 0, decompressLength);
            }
            return decompressed;
        }
        /// <summary>
        /// Compress A Data Array To A File
        /// </summary>
        /// <param name="b">The Data</param>
        /// <param name="fileName">The File Name</param>
        /// <param name="maxLength">The Maximum Length Of Compressed Data To Write</param>
        /// <param name="offset">The Location To Write It In The File</param>
        /// <returns>The Length Of Written Compressed Data</returns>
        public static int compressFileChunk(byte[] b, String fileName, int maxLength, int offset)
        {
            FileStream fs = new FileStream(fileName, FileMode.OpenOrCreate);
            return compressFileChunk(b, fs, maxLength, offset);
        }
        /// <summary>
        /// Compress A Data Array To A File
        /// </summary>
        /// <param name="b">The Data</param>
        /// <param name="fs">The File (Will Not Be Closed)</param>
        /// <param name="maxLength">The Maximum Length Of Compressed Data To Write</param>
        /// <param name="offset">The Location To Write It In The File</param>
        /// <returns>The Length Of Written Compressed Data</returns>
        public static int compressFileChunk(byte[] b, FileStream fs, int maxLength, int offset)
        {
            byte[] compressed;
            using (MemoryStream outStream = new MemoryStream())
            {
                using (GZipStream tinyStream = new GZipStream(outStream, CompressionMode.Compress))
                using (MemoryStream mStream = new MemoryStream(b))
                    mStream.CopyTo(tinyStream);
                compressed = outStream.ToArray();
            }
            int length = compressed.Length < maxLength ? compressed.Length : maxLength;
            fs.Position = offset;
            fs.Write(compressed, 0, length);
            fs.Flush();
            return length;
        }
    }
}
