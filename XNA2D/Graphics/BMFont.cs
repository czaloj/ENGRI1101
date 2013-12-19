using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Drawing;
using System.Drawing.Text;
using Color = System.Drawing.Color;

namespace XNA2D.Graphics
{
    public class BMFont
    {
        public static readonly CharRegion DefaultCharRegion = new CharRegion((char)32, (char)126);
        public static readonly Color DefaultColor = Color.White;

        protected Texture2D fontTexture;
        protected CharRegion charRegion;
        protected GlyphInformation[] gInfo;

        public GlyphInformation this[char c]
        {
            get
            {
                return charRegion.isInRegion(c) ? gInfo[charRegion.bmIndex(c)] : GlyphInformation.None;
            }
        }

        public BMFont(string fontFile, GraphicsDevice gDevice, int size, CharRegion cRegion, int charsPerRow, Color c)
        {
            charRegion = cRegion;
            gInfo = new GlyphInformation[charRegion.Count];

            PrivateFontCollection pfc = new PrivateFontCollection();
            pfc.AddFontFile(fontFile);
            FontFamily f = pfc.Families[0];

            Font font = new Font(f, size, FontStyle.Regular, GraphicsUnit.Pixel);

            Bitmap bpMeasure = new Bitmap(1024, 1024);
            System.Drawing.Graphics g = System.Drawing.Graphics.FromImage(bpMeasure);

            float[] fw = new float[charRegion.Count];
            float[] kw = new float[charRegion.Count];
            string cs1, cs2;
            float fs2, fs3;
            for (int i = 0; i < charRegion.Count; i++)
            {
                cs1 = new string(charRegion[i], 1);
                fw[i] = g.MeasureString(cs1, font).Width;
                if (!char.IsWhiteSpace(charRegion[i]))
                {
                    cs2 = new string(charRegion[i], 2);
                    fs2 = g.MeasureString(cs2, font).Width;
                    fs3 = 2 * fw[i] - fs2;
                    kw[i] = fw[i] - fs3;
                }
                else
                {
                    kw[i] = fw[i];
                }
            }

            CharInfo[] info = new CharInfo[charRegion.Count];
            CharGrid cg = new CharGrid(charsPerRow);
            SizeF s;
            for (int i = 0; i < info.Length; i++)
            {
                s = g.MeasureString(charRegion[i].ToString(), font);
                s.Width = kw[i];
                info[i] = new CharInfo(charRegion[i], new Vector2(kw[i], s.Height));
                cg.addInfo(info[i]);
            }
            cg.end();
            g.Dispose();

            Bitmap bmp = new Bitmap(cg.MaxWidth + cg.CharRows.First.Value.Chars[0].Width, cg.TotalHeight);
            SolidBrush brush = new SolidBrush(c);
            SolidBrush trans = new SolidBrush(Color.FromArgb(0, 0, 0, 0));
            g = System.Drawing.Graphics.FromImage(bmp);
            Vector2 bmpSize = new Vector2(bmp.Width,bmp.Height);
            GraphicsUnit gu = g.PageUnit;
            g.FillRectangle(trans, bmp.GetBounds(ref gu));

            for (int i = 0; i < info.Length; i++)
            {
                info[i].Location.X += (fw[i] - kw[i]) / 2f;
                info[i].Location.X -= (fw[i] - kw[i]) / 2f - 1;
                g.DrawString(info[i].Character.ToString(), font, brush, info[i].PointF);
                info[i].Location.X += (fw[i] - kw[i]) / 2f - 1;
                gInfo[i] = new GlyphInformation()
                {
                    Character = info[i].Character,
                    UVStart = info[i].Location / bmpSize,
                    UVSize = info[i].Size / bmpSize,
                    Size = info[i].Size
                };
            }

            using (MemoryStream ms = new MemoryStream())
            {
                bmp.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                ms.Seek(0, SeekOrigin.Begin);
                fontTexture = Texture2D.FromStream(gDevice, ms);
            }
            g.Dispose();
        }

        public void set(GraphicsDevice g)
        {
            g.Textures[0] = fontTexture;
        }
        public void saveTexture(string file)
        {
            using (FileStream fs = File.Open(file, FileMode.Create))
            {
                fontTexture.SaveAsPng(fs, fontTexture.Width, fontTexture.Height);
            }
        }

        public struct CharRegion
        {
            public char Start;
            public char End;
            public int Count { get { return End - Start + 1; } }
            public char this[int i] { get { return (char)(Start + i); } }

            public CharRegion(char start, char end)
            {
                Start = start;
                End = end;
            }

            public int bmIndex(char c)
            {
                return c - Start;
            }
            public bool isInRegion(char c)
            {
                return c >= Start && c <= End;
            }
        }

        public class GlyphInformation
        {
            public static readonly GlyphInformation None = new GlyphInformation(){Character = char.MaxValue, UVStart = Vector2.Zero, UVSize = Vector2.One};

            public char Character;
            public Vector2 UVStart;
            public Vector2 UVSize;

            public Vector2 Size;
        }

        private class CharInfo
        {
            public char Character;
            public Vector2 Location;
            public Vector2 Size;
            public int Width;
            public int Height;
            public PointF PointF { get { return new PointF(Location.X, Location.Y); } }
            public SizeF SizeF { get { return new SizeF(Size.X, Size.Y); } }

            public CharInfo(char c, Vector2 s)
            {
                Character = c;
                Size = s;
                Width = (int)Math.Ceiling(s.X);
                Height = (int)Math.Ceiling(s.Y);
                Location = Vector2.Zero;
            }
            public CharInfo(char c)
                : this(c, Vector2.One)
            {

            }
        }
        private class CharRow
        {
            public const int Spacing = 2;

            public CharInfo[] Chars;
            public int MaxHeight;
            public int TotalWidth;

            public int CharsAdded;
            public bool Ended
            {
                get
                {
                    return CharsAdded == Chars.Length;
                }
            }

            public CharRow(int charCount)
            {
                Chars = new CharInfo[charCount];
                CharsAdded = 0;
                MaxHeight = 0;
                TotalWidth = 0;
            }

            public void addChar(CharInfo ci)
            {
                ci.Location.X = TotalWidth + Spacing;
                if (ci.Height > MaxHeight) { MaxHeight = ci.Height; }
                TotalWidth += Spacing + ci.Width;
                Chars[CharsAdded] = ci;
                CharsAdded++;
                if (Ended) { TotalWidth += Spacing; }
            }
            public void setStartHeight(float h)
            {
                foreach (CharInfo c in Chars) { if (c != null) { c.Location.Y = h; } }
            }
        }
        private class CharGrid
        {
            public const int Spacing = 2;

            protected int cPerRow;
            public LinkedList<CharRow> CharRows;
            protected CharRow cRow;

            public int MaxWidth;
            public int TotalHeight;

            public CharGrid(int charsPerRow)
            {
                cPerRow = charsPerRow;
                CharRows = new LinkedList<CharRow>();
                TotalHeight = Spacing;
            }
            public void addInfo(CharInfo ci)
            {
                if (cRow == null || cRow.Ended)
                {
                    if (cRow != null)
                    {
                        cRow.setStartHeight(TotalHeight);
                        TotalHeight += cRow.MaxHeight + Spacing;
                        if (cRow.TotalWidth > MaxWidth)
                        {
                            MaxWidth = cRow.TotalWidth;
                        }
                    }
                    cRow = new CharRow(cPerRow);
                    CharRows.AddLast(cRow);
                }
                cRow.addChar(ci);
            }
            public void end()
            {
                if (cRow != null)
                {
                    cRow.setStartHeight(TotalHeight);
                    TotalHeight += cRow.MaxHeight + Spacing;
                }
            }
        }
    }
}
