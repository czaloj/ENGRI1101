using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using ZLibrary.Algorithms;
using ORLabs.Framework;

namespace ORLabs.Algorithms
{
    public enum ChangeType : byte
    {
        Unknown = 0x00,
        Node = 0x01,
        Edge = 0x02,
        Other = 0x04
    }
    public struct TSAORChange
    {
        public const double NoNumber = double.MinValue;

        public ChangeType Type;
        public int Index;
        public Color Color;
        public double Number;
        public bool HasNumber { get { return Number != NoNumber; } }
        public string CurrentText;

        public TSAORChange(string s, ChangeType t, int i, Color c, double d = NoNumber)
        {
            Type = t;
            Index = i;
            Color = c;
            Number = d;
            CurrentText = s;
        }
        public TSAORChange(string s, ORGraph.Edge e, Color c, double d = NoNumber)
            : this(s, ChangeType.Edge, e.Index, c, d)
        { }
        public TSAORChange(string s, ORGraph.Node n, Color c, double d = NoNumber)
            : this(s, ChangeType.Node, n.Index, c, d)
        { }
    }

    public abstract class TSAOR<I, O> : ACStateAlgorithm<I, O, TSAORChange>
        where I : struct
        where O : struct
    {

    }
    public static class ORColors
    {
        public static readonly Color CExcluded = new Color(0, 0, 0, 10);

        public static readonly Color CNPassive = new Color(30, 30, 30, 155);
        public static readonly Color CNStart = new Color(0, 0, 255, 255);
        public static readonly Color CNEnd = new Color(255, 155, 0, 255);
        public static readonly Color CNLooking = new Color(255, 155, 155, 255);
        public static readonly Color CNIndexed = new Color(0, 255, 255, 255);
        public static readonly Color CNResult = new Color(55, 0, 0, 255);

        public static readonly Color CEPassive = new Color(30, 30, 30, 155);
        public static readonly Color CELooking = new Color(155, 155, 255, 255);
        public static readonly Color CEIndexed = new Color(40, 255, 255, 255);
        public static readonly Color CEResult = new Color(155, 0, 0, 255);
    }
}
