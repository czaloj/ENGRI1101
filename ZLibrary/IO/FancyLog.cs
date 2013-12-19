using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace System.IO
{
    public class FancyLog : IDisposable
    {
        //All The Added Formats
        protected Stack<Format> formats;
        protected Format format;
        public Format CurrentFormat
        {
            get
            {
                return format;
            }
        }

        //The Log
        protected StreamWriter log;
        protected StringBuilder sb;

        //Constructors
        protected FancyLog(Format f)
        {
            formats = new Stack<Format>(10);
            format = f;

            sb = new StringBuilder();
        }
        public FancyLog(Stream s, Format f)
            : this(f)
        {
            log = new StreamWriter(s);
            log.AutoFlush = format.AutoFlush;
        }
        public FancyLog(string logPath, Format f)
            : this(File.Open(logPath, FileMode.Create), f)
        {
        }
        public void Dispose()
        {
            log.Dispose();
        }

        //Format Changers
        public void beginFormat(Format f)
        {
            formats.Push(format);
            format = f;
            log.AutoFlush = format.AutoFlush;
        }
        public void beginFormat_ChangeWidth(int width)
        {
            beginFormat(new Format(format, width, format.Alignment));
        }
        public void beginFormat_ChangeAlignment(int align)
        {
            beginFormat(new Format(format, format.InnerWidth, align));
        }
        public void beginFormat_ChangeBounds(string left, string right)
        {
            beginFormat(new Format(format, left, right, format.IsBounded));
        }
        public void beginFormat_ChangeBounds(string left, string right, bool useBounds)
        {
            beginFormat(new Format(format, left, right, useBounds));
        }
        public void beginFormat_ChangeBoundsUsage(bool useBounds)
        {
            beginFormat(new Format(format, format.LeftBound, format.RightBound, useBounds));
        }
        public void beginFormat_ChangeFillerChar(char filler)
        {
            beginFormat(new Format(format, filler));
        }
        public void beginFormat_ChangeAutoFlush(bool flush)
        {
            beginFormat(new Format(format, flush));
        }
        public void endFormat()
        {
            format = formats.Pop();
            log.AutoFlush = format.AutoFlush;
        }

        //Log Writing
        public void newLines(int lines)
        {
            for (int i = 0; i < lines; i++)
            {
                write("");
            }
        }
        public void write(string s)
        {
            log.WriteLine(format.getFormatted(s));
        }
        public void write(string format, params object[] args)
        {
            write(string.Format(format, args));
        }
        public void writeContained(string s)
        {
            LinkedList<string> l = splitString(s, format.InnerWidth, sb);
            foreach (string str in l) { write(str); }
        }
        public void writeContained(string format, params object[] args)
        {
            writeContained(string.Format(format, args));
        }
        public void fillThroughInner(char c)
        {
            write(new string(c, format.InnerWidth));
        }
        public void fillThroughAll(char c)
        {
            log.WriteLine(new string(c, format.TotalWidth));
        }

        //String Splitter
        public const int TabWith = 4;
        public static LinkedList<string> splitString(string s, int maxWidth, StringBuilder sb)
        {
            LinkedList<string> l = new LinkedList<string>();

            if (sb.Capacity < maxWidth)
            {
                sb.Capacity = maxWidth;
            }

            string[] split = s.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.None);

            foreach (string str in split)
            {
                splitString(str, maxWidth, sb, l);
            }

            return l;
        }
        private static void splitString(string s, int maxWidth, StringBuilder sb, LinkedList<string> l)
        {
            int si = 0, siLast, sbi;
            int cFirst, cLast;

            while (si < s.Length)
            {
                //Get A New Line
                sbi = 0; sb.Clear();
                cFirst = 0; cLast = -1; siLast = 0;

                //Get Split Indeces
                while (sbi < maxWidth && si < s.Length)
                {
                    #region White
                    if (char.IsWhiteSpace(s[si]))
                    {
                        do
                        {
                            //Add Whitespace Until End Reached
                            switch (s[si])
                            {
                                case ' ':
                                    sb.Append(' ');
                                    sbi++;
                                    break;
                                case '\t':
                                    if (sbi + TabWith > maxWidth)
                                    {
                                        //Set To End
                                        sbi = maxWidth; break;
                                    }
                                    else
                                    {
                                        //Add Tab Spaces
                                        sb.Append(' ', TabWith);
                                        sbi += TabWith;
                                    }
                                    break;
                            }
                            //Increment String Pointer
                            si++;
                        }
                        while (sbi < maxWidth && si < s.Length && char.IsWhiteSpace(s[si]));
                    }
                    #endregion
                    #region Words
                    else
                    {
                        //Check To Set The First Character
                        if (cFirst == -1)
                        {
                            cFirst = sbi;
                        }

                        int cPrev = cLast;


                        //Word Found
                        do
                        {
                            //Add Characters Until Whitespace Found Or End Reached
                            sb.Append(s[si]); sbi++; si++; cLast = sbi;
                        }
                        while (sbi < maxWidth && si < s.Length && !char.IsWhiteSpace(s[si]));

                        if (sbi == maxWidth && (si < s.Length && !char.IsWhiteSpace(s[si])))
                        {
                            //Overflowing Word
                            if (cPrev != -1)
                            {
                                //Word Is Not The Whole Line
                                cLast = cPrev;
                                si = siLast;
                            }
                        }
                        else
                        {
                            siLast = si;
                        }
                    }
                    #endregion
                }

                //Add The Necessary Substring
                if (cFirst != -1 && cLast != -1)
                {
                    l.AddLast(sb.ToString().Substring(cFirst, cLast - cFirst));
                }
            }
        }

        public struct Format
        {
            public static Format Default;
            static Format()
            {
                Default = new Format(
                    80, AlignLeft,
                    "|| ", " ||", ' ',
                    true, true
                    );
            }
            public const int AlignLeft = -1;
            public const int AlignCenter = 0;
            public const int AlignRight = 1;

            public bool AutoFlush;
            public int Alignment;

            public int InnerWidth;
            public int TotalWidth;
            public char FillerChar;

            public bool IsBounded;
            public string LeftBound, RightBound;

            private StringBuilder sb;

            public Format(int innerWidth, int align, string lBound, string rBound, char fillChar = ' ', bool autoflush = true, bool bound = true)
            {
                InnerWidth = innerWidth;
                Alignment = align;
                IsBounded = bound;
                AutoFlush = autoflush;
                if (IsBounded)
                {
                    LeftBound = lBound;
                    RightBound = rBound;
                }
                else
                {
                    LeftBound = "";
                    RightBound = "";
                }

                TotalWidth = LeftBound.Length + RightBound.Length + InnerWidth;

                sb = new StringBuilder(TotalWidth);
                FillerChar = fillChar;
            }
            public Format(Format f)
                : this(f.InnerWidth, f.Alignment, f.LeftBound, f.RightBound, f.FillerChar, f.AutoFlush, f.IsBounded)
            {
            }
            public Format(Format f, int innerWidth, int align)
                : this(innerWidth, align, f.LeftBound, f.RightBound, f.FillerChar, f.AutoFlush, f.IsBounded)
            {
            }
            public Format(Format f, string lBound, string rBound, bool bound = true)
                : this(f.InnerWidth, f.Alignment, lBound, rBound, f.FillerChar, f.AutoFlush, bound)
            {
            }
            public Format(Format f, char fillChar)
                : this(f.InnerWidth, f.Alignment, f.LeftBound, f.RightBound, fillChar, f.AutoFlush, f.IsBounded)
            {
            }
            public Format(Format f, bool autoflush = true)
                : this(f.InnerWidth, f.Alignment, f.LeftBound, f.RightBound, f.FillerChar, autoflush, f.IsBounded)
            {
            }


            public string getFormatted(string s)
            {
                switch (Alignment)
                {
                    case AlignLeft: return getFormattedLeft(s);
                    case AlignCenter: return getFormattedCenter(s);
                    case AlignRight: return getFormattedRight(s);
                    default: return s;
                }
            }
            public string getFormattedLeft(string s)
            {
                sb.Clear();
                int left = InnerWidth - s.Length;
                if (IsBounded)
                {
                    sb.Append(LeftBound);
                    if (left <= 0)
                    {
                        sb.Append(s);
                    }
                    else
                    {
                        sb.Append(s);
                        sb.Append(FillerChar, left);
                    }
                    sb.Append(RightBound);
                }
                else
                {
                    if (left <= 0)
                    {
                        sb.Append(s);
                    }
                    else
                    {
                        sb.Append(s);
                        sb.Append(FillerChar, left);
                    }
                }
                return sb.ToString();
            }
            public string getFormattedCenter(string s)
            {
                sb.Clear();
                int left = InnerWidth - s.Length;
                if (IsBounded)
                {
                    sb.Append(LeftBound);
                    if (left <= 0)
                    {
                        sb.Append(s);
                    }
                    else
                    {
                        int h1, h2;
                        h1 = left / 2;
                        h2 = left - h1;
                        sb.Append(FillerChar, h1);
                        sb.Append(s);
                        sb.Append(FillerChar, h2);
                    }
                    sb.Append(RightBound);
                }
                else
                {
                    if (left <= 0)
                    {
                        sb.Append(s);
                    }
                    else
                    {
                        int h1, h2;
                        h1 = left / 2;
                        h2 = left - h1;
                        sb.Append(FillerChar, h1);
                        sb.Append(s);
                        sb.Append(FillerChar, h2);
                    }
                }

                return sb.ToString();
            }
            public string getFormattedRight(string s)
            {
                sb.Clear();
                int left = InnerWidth - s.Length;
                if (IsBounded)
                {
                    sb.Append(LeftBound);
                    if (left <= 0)
                    {
                        sb.Append(s);
                    }
                    else
                    {
                        sb.Append(FillerChar, left);
                        sb.Append(s);
                    }
                    sb.Append(RightBound);
                }
                else
                {
                    if (left <= 0)
                    {
                        sb.Append(s);
                    }
                    else
                    {
                        sb.Append(FillerChar, left);
                        sb.Append(s);
                    }
                }
                return sb.ToString();
            }
        }
    }
}
