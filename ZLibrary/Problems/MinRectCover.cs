using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using ZLibrary.ADT;

namespace ZLibrary.Problems
{
    public class MinRectCover
    {
        public static LinkedList<Rectangle> getMinCover(bool[,] solid, int w, int h)
        {
            //This Will Be The List Of Rectangles Returned
            LinkedList<Rectangle> r = new LinkedList<Rectangle>();
            int x = 0, y = 0, s = 0, ex = 0, ey = 0;

            #region Construct A Vertex Graph
            Vertex[,] vertices = new Vertex[w + 1, h + 1];
            for (x = 0; x <= w; x++)
            {
                for (y = 0; y <= h; y++)
                {
                    vertices[x, y] = new Vertex(x, y);
                    vertices[x, y].checkSolidity(solid, w, h);
                }
            }
            #endregion

            #region First Generate All The Outlines Of The Shapes
            LinkedList<Line> vOutlines = new LinkedList<Line>();
            LinkedList<Line> hOutlines = new LinkedList<Line>();
            Vertex vStart, vEnd;

            #region Horizontal Outlines
            for (y = 0; y <= h; y++)
            {
                x = 0;
                while (x < w)
                {
                    while (x < w && !vertices[x, y].PassHLineTestBegin) { x++; }
                    if (x < w)
                    {
                        vStart = vertices[x, y];
                        x++;
                        while (x <= w && vertices[x, y].PassHLineTest)
                        {
                            if (vertices[x, y].PassHLineTestEnd)
                            {
                                x++;
                                break;
                            }
                            else
                            {
                                x++;
                            }
                        }
                        vEnd = vertices[x - 1, y];
                        hOutlines.AddLast(new Line(vStart, vEnd, false, true));
                    }
                    else { break; }
                }
            }
            #endregion

            #region Vertical Outlines
            for (x = 0; x <= w; x++)
            {
                y = 0;
                while (y < h)
                {
                    while (y < h && !vertices[x, y].PassVLineTestBegin) { y++; }
                    if (y < h)
                    {
                        vStart = vertices[x, y];
                        y++;
                        while (y <= h && vertices[x, y].PassVLineTest)
                        {
                            if (vertices[x, y].PassVLineTestEnd)
                            {
                                y++;
                                break;
                            }
                            else
                            {
                                y++;
                            }
                        }
                        vEnd = vertices[x, y - 1];
                        vOutlines.AddLast(new Line(vStart, vEnd, false, false));
                    }
                    else { break; }
                }
            }
            #endregion
            #endregion

            //Create A List Of Concave Vertices
            LinkedList<Vertex> vConcave = new LinkedList<Vertex>();
            foreach (Vertex v in vertices) { if (v.IsConcave) vConcave.AddLast(v); }

            //Create Lists Of All Colinear Edges That Will Be A Possible Edge In Shape
            LinkedList<Line> innerHLines = new LinkedList<Line>();
            LinkedList<Line> innerVLines = new LinkedList<Line>();


            //Run Max Independent Set Algorithm



            #region Old
            ////Trace Around Shape To Get All Corners
            //LinkedList<Vertex> corners = new LinkedList<Vertex>();
            //LinkedList<Vertex> concave = new LinkedList<Vertex>();
            //LinkedList<Vertex> rectCheck = new LinkedList<Vertex>();
            //int[] pcw = new int[w + 1], pch = new int[h + 1];
            //Vertex[][] pYByX = new Vertex[w + 1][];
            //Vertex[][] pXByY = new Vertex[h + 1][];

            //for (y = 0; y <= h; y++)
            //{
            //    for (x = 0; x <= w; x++)
            //    {
            //        s = getSolidity(solid, x, y, w, h);
            //        if (s != 0 && s != 4)
            //        {
            //            //On An Edge
            //            Vertex vert = new Vertex(x, y);
            //            vertices[x, y] = vert;
            //            if (s != 2)
            //            {
            //                //Will Create A Line
            //                if (vert.IsConcave)
            //                {
            //                    concave.AddLast(vert);
            //                }
            //                corners.AddLast(vert);
            //                pcw[x]++;
            //                pch[y]++;
            //            }
            //            else if (shouldStillGiveLine(solid, x, y, w, h))
            //            {
            //                rectCheck.AddLast(vert);
            //            }
            //        }
            //    }
            //}

            ////Sort Points By X,Y In Both Ways
            //for (x = 0; x <= w; x++) { if (pcw[x] != 0) { pYByX[x] = new Vertex[pcw[x]]; pcw[x] = 0; } }
            //for (y = 0; y <= h; y++) { if (pch[y] != 0) { pXByY[y] = new Vertex[pch[y]]; pch[y] = 0; } }
            //foreach (Vertex v in corners)
            //{
            //    pYByX[v.X][pcw[v.X]] = v; pcw[v.X]++;
            //    pXByY[v.Y][pch[v.Y]] = v; pch[v.Y]++;
            //}

            //#region Get Outlines And Possible Optimal Inner Lines
            ////Must Get All Possible Lines
            //LinkedList<Line> completedVLines = new LinkedList<Line>();
            //LinkedList<Line> innerVLines = new LinkedList<Line>();
            //LinkedList<Line> completedHLines = new LinkedList<Line>();
            //LinkedList<Line> innerHLines = new LinkedList<Line>();
            //int p1 = 0, p2 = 1;
            //int cl, c1, c2;

            //#region Get Vertical Lines
            //for (x = 0; x <= w; x++)
            //{
            //    if (pYByX[x] != null)
            //    {
            //        p1 = 0;
            //        p2 = 1;
            //        while (p2 < pYByX[x].Length)
            //        {
            //            if (p1 % 2 == 0)
            //            {
            //                //Outline Of Shape
            //                Line l = new Line(pYByX[x][p1], pYByX[x][p2], false, false);
            //                completedVLines.AddLast(l);
            //                addLineV(vertices, l);
            //            }
            //            else
            //            {
            //                //Cuts Through Shape
            //                //Must Check To Not Cut Through Empty Space
            //                bool hole = false;
            //                cl = x < w ? x : w - 1;
            //                c1 = pYByX[x][p1].Y;
            //                c2 = ((pYByX[x][p2].Y < h) ? pYByX[x][p2].Y : h - 1);
            //                for (y = c1; y < c2; y++)
            //                {
            //                    if (!solid[y, cl])
            //                    {
            //                        //Hole Detected
            //                        hole = true;
            //                        break;
            //                    }
            //                }
            //                if (!hole)
            //                {
            //                    innerVLines.AddLast(new Line(pYByX[x][p1], pYByX[x][p2], true, false));
            //                }
            //            }
            //            p1++;
            //            p2++;
            //        }
            //    }
            //}
            //#endregion

            //#region Get Horizontal Lines
            //for (y = 0; y <= h; y++)
            //{
            //    if (pXByY[y] != null)
            //    {
            //        p1 = 0;
            //        p2 = 1;
            //        while (p2 < pXByY[y].Length)
            //        {
            //            if (p1 % 2 == 0)
            //            {
            //                //Outline Of Shape
            //                Line l = new Line(pXByY[y][p1], pXByY[y][p2], false, true);
            //                completedHLines.AddLast(l);
            //                addLineH(vertices, l);
            //            }
            //            else
            //            {
            //                //Cuts Through Shape
            //                //Must Check To Not Cut Through Empty Space
            //                bool hole = false;
            //                cl = y < h ? y : h - 1;
            //                c1 = pXByY[y][p1].X;
            //                c2 = ((pXByY[y][p2].X < w) ? pXByY[y][p2].X : w - 1);
            //                for (x = c1; x < c2; x++)
            //                {
            //                    if (!solid[cl, x])
            //                    {
            //                        //Hole Detected
            //                        hole = true;
            //                        break;
            //                    }
            //                }
            //                if (!hole)
            //                {
            //                    innerHLines.AddLast(new Line(pXByY[y][p1], pXByY[y][p2], true, true));
            //                }
            //            }
            //            p1++;
            //            p2++;
            //        }
            //    }
            //}
            //#endregion

            //#endregion

            //LinkedList<Line> lp = new LinkedList<Line>();

            #endregion

            LinkedList<Line> allInnerLines = new LinkedList<Line>();
            LinkedList<Line> colinearIndependent = new LinkedList<Line>();
            LinkedList<Line> vInline = new LinkedList<Line>();
            LinkedList<Line> hInline = new LinkedList<Line>();

            if (vConcave.Count > 0)
            {
                if (innerHLines.Count > 0 && innerVLines.Count > 0)
                {
                    allInnerLines = new LinkedList<Line>(innerHLines.Union(innerVLines));
#if ALG_COMPLETE

                        //Build Bipartite Intersect Graph
                        Node[] vNodes = new Node[innerVLines.Count];
                        Node[] hNodes = new Node[innerHLines.Count];
                        int i = -1;
                        LinkedListNode<Line> node;
                        node = innerVLines.First;
                        while (++i < innerVLines.Count)
                        {
                            vNodes[i] = new Node(node.Value);
                            node = node.Next;
                        }
                        i = -1;
                        node = innerHLines.First;
                        while (++i < innerHLines.Count)
                        {
                            hNodes[i] = new Node(node.Value);
                            node = node.Next;
                        }
                        foreach (Node vl in vNodes)
                        {
                            foreach (Node hl in hNodes)
                            {
                                if (Line.intersects(hl.Line, vl.Line))
                                {
                                    hl.addConnection(vl);
                                    vl.addConnection(hl);
                                }
                            }
                        }

                        //Find Maximum Independent Set
                        LinkedList<Node> vn = new LinkedList<Node>(), hn = new LinkedList<Node>();
                        lp = new LinkedList<Line>();
                        innerVLines = new LinkedList<Line>(); innerHLines = new LinkedList<Line>();
                        i = 0;
                        foreach (Node n in vNodes) { if (n.Connections.Count > 0) { vn.AddLast(n); n.ID = i++; } else { lp.AddLast(n.Line); } }
                        foreach (Node n in hNodes) { if (n.Connections.Count > 0) { hn.AddLast(n); n.ID = i++; } else { lp.AddLast(n.Line); } }

                        HashSet<Node> G = new HashSet<Node>(new HashSet<Node>(vn).Union(new HashSet<Node>(hn)));
                        //Not A Hopcroft Karp / But Still Optimal With Some Pruning
                        Line[] lKeep;
                        if (vn.Count + hn.Count > 0)
                        {
                            VertexCover vc = new VertexCover();
                            vc.VertexCount = G.Count;
                            vc.Edges = new HashSet<int>[vc.VertexCount];
                            vc.LeftVertices = new HashSet<int>();
                            vc.RightVertices = new HashSet<int>();
                            vc.Matching = Enumerable.Repeat(-1, vc.VertexCount).ToArray();
                            foreach (Node n in G)
                            {
                                if (n.Line.IsHorizontal) { vc.RightVertices.Add(n.ID); }
                                else { vc.LeftVertices.Add(n.ID); }
                            }
                            for (i = 0; i < vc.Edges.Length; ++i)
                            {
                                vc.Edges[i] = new HashSet<int>();
                            }
                            foreach (Node n in G)
                            {
                                foreach (Node o in n.Connections)
                                {
                                    vc.Edges[n.ID].Add(o.ID);
                                }
                            }
                            vc.FindVertexCover();
                            Node[] nG = G.ToArray<Node>();
                            lKeep = new Line[vc.IndependentSetResult.Count];
                            i = 0;
                            foreach (int e in vc.IndependentSetResult)
                            {
                                lKeep[i++] = nG[e].Line;
                            }
                        }
                        else
                        {
                            lKeep = lp.ToArray<Line>();
                        }

                        foreach (Line l in lKeep)
                        {
                            if (l.IsHorizontal) { innerHLines.AddLast(l); }
                            else { innerVLines.AddLast(l); }
                        }
#endif

                    HashSet<Node> nGraph = new HashSet<Node>();
                    foreach (Line l in allInnerLines) { nGraph.Add(new Node(l)); }
                    LinkedList<Node> lHoriz = new LinkedList<Node>(nGraph.Where((n) => { return n.Line.IsHorizontal; }));
                    LinkedList<Node> lVert = new LinkedList<Node>(nGraph.Where((n) => { return !n.Line.IsHorizontal; }));
                    foreach (Node n in lHoriz)
                    {
                        foreach (Node m in lVert)
                        {
                            if (Line.touches(n.Line, m.Line))
                            {
                                n.addConnection(m);
                                m.addConnection(n);
                            }
                        }
                    }
                    lHoriz = new LinkedList<Node>(lHoriz.Where((n) => { return n.Connections.Count > 0; }));
                    lVert = new LinkedList<Node>(lVert.Where((n) => { return n.Connections.Count > 0; }));


                    LinkedList<Line> lRemove = HopcroftKarp(lHoriz, lVert);
                    innerHLines = new LinkedList<Line>(allInnerLines.Except(lRemove).Where((l) => { return l.IsHorizontal; }));
                    innerVLines = new LinkedList<Line>(allInnerLines.Except(lRemove).Where((l) => { return !l.IsHorizontal; }));
                    colinearIndependent = new LinkedList<Line>(innerHLines.Union(innerVLines));
                    foreach (Node n in nGraph)
                    {
                        if (n.Connections.Count == 0) { colinearIndependent.AddLast(n.Line); }
                    }

                    vInline = new LinkedList<Line>(innerVLines);
                    hInline = new LinkedList<Line>(innerHLines);

                    //Until Algorithm Complete, Just Use The Maximum Of The Lists
                    //if (innerHLines.Count > innerVLines.Count)
                    //{
                    //    foreach (Line l in innerVLines) { lp.AddLast(l); } innerVLines.Clear();
                    //}
                    //else
                    //{
                    //    foreach (Line l in innerHLines) { lp.AddLast(l); } innerHLines.Clear();
                    //}
                }

                #region Complete Optimized Inner Lines
                foreach (Line l in colinearIndependent)
                {
                    l.P1.EmanatesLine = true; l.P2.EmanatesLine = true;
                }
                #endregion
            }

            LinkedList<Line> hCompleteLines = new LinkedList<Line>(hOutlines.Union(hInline));
            IOrderedEnumerable<Line> hCompleteOrderedLines = hCompleteLines.OrderBy((l) => { return l.Location; });
            LinkedList<Line> vCompleteLines = new LinkedList<Line>(vOutlines.Union(vInline));
            IOrderedEnumerable<Line> vCompleteOrderedLines = vCompleteLines.OrderBy((l) => { return l.Location; });

            IEnumerator<Line> lNode;
            LinkedList<Line>[] hLinesByY = new LinkedList<Line>[h + 1];
            lNode = hCompleteOrderedLines.GetEnumerator();
            for (y = 0; y <= h; y++)
            {
                hLinesByY[y] = new LinkedList<Line>();
                while (lNode.Current.Location == y) { hLinesByY[y].AddLast(lNode.Current); if (!lNode.MoveNext()) { break; } }
            }
            LinkedList<Line>[] vLinesByX = new LinkedList<Line>[w + 1];
            lNode = hCompleteOrderedLines.GetEnumerator();
            for (x = 0; x <= w; x++)
            {
                vLinesByX[x] = new LinkedList<Line>();
                while (lNode.Current.Location == y) { hLinesByY[y].AddLast(lNode.Current); if (!lNode.MoveNext()) { break; } }
            }

            #region Emanate Lines From Remaining Concave Vertices
            //Make Sure The Rest Of All The Concave Vertices Have One Emanating Line
            Line tl1, tl2;
            foreach (Vertex v in vConcave)
            {
                if (!v.EmanatesLine)
                {
                    v.EmanatesLine = true;

                    if (v.Solid[Vertex.TopLeft] && v.Solid[Vertex.TopRight])
                    { tl1 = findLineInnerUp(v, vertices, hLinesByY); if (!tl1.P1.IsConvex) { hCompleteLines.AddLast(tl1); continue; } }
                    else
                    { tl1 = findLineInnerDown(v, vertices, hLinesByY); if (!tl1.P2.IsConvex) { hCompleteLines.AddLast(tl1); continue; } }

                    if (v.Solid[Vertex.TopLeft] && v.Solid[Vertex.BottomLeft])
                    { tl2 = findLineInnerLeft(v, vertices, vLinesByX); if (!tl1.P1.IsConvex) { vCompleteLines.AddLast(tl2); continue; } }
                    else
                    { tl2 = findLineInnerRight(v, vertices, vLinesByX); if (!tl1.P2.IsConvex) { vCompleteLines.AddLast(tl2); continue; } }

                    //Else Add A Random Line
                    hCompleteLines.AddLast(tl1);
                }
            }
            #endregion

            //Build The Rectangles From All The Completed Lines
            int[,] rUsed = new int[h, w];
            int ri = 0;
            LinkedList<Vertex> verts = new LinkedList<Vertex>();
            foreach (Line l in hCompleteLines)
            {
                if (l.P1.Y < h && l.P1.Lines[Vertex.Right] && l.P1.Lines[Vertex.Down] && solid[l.P1.Y, l.P1.X])
                {
                    verts.AddLast(l.P1);
                }
            }
            foreach (Line l in vCompleteLines)
            {
                if (l.P1.X < w && l.P1.Lines[Vertex.Right] && l.P1.Lines[Vertex.Down] && solid[l.P1.Y, l.P1.X])
                {
                    verts.AddLast(l.P1);
                }
            }
            //foreach (Vertex vert in rectCheck)
            //{
            //    if (solid[vert.Y, vert.X])
            //    {
            //        verts.AddLast(vert);
            //    }
            //}
            while (verts.Count > 0)
            {
                ri++;
                Vertex v = verts.First.Value; verts.RemoveFirst();
                if (rUsed[v.Y, v.X] == 0)
                {
                    //Expand Out A Rectangle
                    rUsed[v.Y, v.X] = ri;
                    Vertex vx = vertices[v.X + 1, v.Y], vy = vertices[v.X, v.Y + 1];
                    if (vx != null && vy != null)
                    {
                        while (vx != null && vx.X < w && solid[vx.Y, vx.X]/* && !vx.Lines[Vertex.LineDown]*/) { vx = vertices[vx.X + 1, vx.Y]; ex++; }
                        while (vy != null && vy.Y < h && solid[vy.Y, vy.X]/* && !vy.Lines[Vertex.LineRight]*/) { vy = vertices[vy.X, vy.Y + 1]; ey++; }

                        //From Current Point, Try Expanding Out On A Line
                        //Vertex vmx = vertices[vx.X, vy.Y];
                        //Vertex vmy = vertices[vx.X, vy.Y];
                        //int emx = 0, emy = 0;
                        //while (vmx != null && vmx.X < w && vmx.Lines[Vertex.LineRight] && vx != null) { vmx = vertices[vmx.X + 1, vmx.Y]; vx = vertices[vx.X + 1, vx.Y]; emx++; }
                        //while (vmy != null && vmy.Y < h && vmy.Lines[Vertex.LineDown] && vy != null) { vmy = vertices[vmy.X, vmy.Y + 1]; vy = vertices[vy.X, vy.Y + 1]; emy++; }
                        //if (emx > emy) { ex += emx; }
                        //else { ey += emy; }


                        for (x = v.X; x < v.X + ex; x++)
                        {
                            for (y = v.Y; y < v.Y + ey; y++)
                            {
                                rUsed[y, x] = ri;
                            }
                        }
                    }
                    r.AddLast(new Rectangle(v.X, v.Y, ex, ey));
                }
            }

            return r;
        }

        private static Line findLineInnerDown(Vertex vStart, Vertex[,] vertices, LinkedList<Line>[] hLinesByY)
        {
            Line l = new Line(vStart, vertices[vStart.X, vStart.Y + 1], true, false);
            while (hLinesByY[l.End].Count((line) => { return Line.touches(line, l); }) == 0)
            {
                l.P2 = vertices[l.P2.X, l.P2.Y + 1];
            }
            return l;
        }
        private static Line findLineInnerUp(Vertex vStart, Vertex[,] vertices, LinkedList<Line>[] hLinesByY)
        {
            Line l = new Line(vertices[vStart.X, vStart.Y - 1], vStart, true, false);
            while (hLinesByY[l.Start].Count((line) => { return Line.touches(line, l); }) == 0)
            {
                l.P1 = vertices[l.P1.X, l.P1.Y - 1];
            }
            return l;
        }
        private static Line findLineInnerRight(Vertex vStart, Vertex[,] vertices, LinkedList<Line>[] vLinesByX)
        {
            Line l = new Line(vStart, vertices[vStart.X + 1, vStart.Y], true, true);
            while (vLinesByX[l.End].Count((line) => { return Line.touches(l, line); }) == 0)
            {
                l.P2 = vertices[l.P2.X + 1, l.P2.Y];
            }
            return l;
        }
        private static Line findLineInnerLeft(Vertex vStart, Vertex[,] vertices, LinkedList<Line>[] vLinesByX)
        {
            Line l = new Line(vertices[vStart.X - 1, vStart.Y], vStart, true, true);
            while (vLinesByX[l.Start].Count((line) => { return Line.touches(l, line); }) == 0)
            {
                l.P1 = vertices[l.P1.X - 1, l.P1.Y];
            }
            return l;
        }

        private static void addLineH(Vertex[,] vertices, Line l)
        {
            l.P1.Lines[Vertex.Right] = true;
            l.P2.Lines[Vertex.Left] = true;

            Vertex v;
            for (int x = l.P1.X + 1; x < l.P2.X; x++)
            {
                if (vertices[x, l.P1.Y] == null)
                {
                    v = new Vertex(x, l.P1.Y);
                    vertices[x, l.P1.Y] = v;
                }
                else
                {
                    v = vertices[x, l.P1.Y];
                }
                v.Lines[Vertex.Right] = true;
                v.Lines[Vertex.Left] = true;
            }
        }
        private static void addLineV(Vertex[,] vertices, Line l)
        {
            l.P1.Lines[Vertex.Down] = true;
            l.P2.Lines[Vertex.Up] = true;

            Vertex v;
            for (int y = l.P1.Y + 1; y < l.P2.Y; y++)
            {
                if (vertices[l.P1.X, y] == null)
                {
                    v = new Vertex(l.P1.X, y);
                    vertices[l.P1.X, y] = v;
                }
                else
                {
                    v = vertices[l.P1.X, y];
                }
                v.Lines[Vertex.Down] = true;
                v.Lines[Vertex.Up] = true;
            }
        }
        public static int getSolidity(bool[,] solid, int px, int py, int w, int h)
        {
            if (py > 0 && py < h && px > 0 && px < w)
            {
                return
                    (solid[py - 1, px - 1] ? 1 : 0) +
                    (solid[py, px - 1] ? 1 : 0) +
                    (solid[py - 1, px] ? 1 : 0) +
                    (solid[py, px] ? 1 : 0)
                    ;
            }
            else
            {
                return
                    (py > 0 && px > 0 && solid[py - 1, px - 1] ? 1 : 0) +
                    (py < h && px > 0 && solid[py, px - 1] ? 1 : 0) +
                    (py > 0 && px < w && solid[py - 1, px] ? 1 : 0) +
                    (py < h && px < w && solid[py, px] ? 1 : 0)
                    ;
            }
        }
        public static bool shouldStillGiveLine(bool[,] solid, int px, int py, int w, int h)
        {
            if (py > 0 && py < h && px > 0 && px < w)
            {
                return solid[py - 1, px - 1] == solid[py, px];
            }
            return false;
        }

        private static LinkedList<Line> HopcroftKarp(LinkedList<Node> G1, LinkedList<Node> G2)
        {
            int matching = 0;
            Node[] G = new Node[G1.Count + G2.Count + 1];
            int i = 0;
            foreach (Node n in G1) { G[i] = n; n.ID = i; i++; }
            foreach (Node n in G2) { G[i] = n; n.ID = i; i++; }
            G[i] = new Node(null); G[i].ID = i;
            Node[] PairG1 = new Node[G.Length];
            Node[] PairG2 = new Node[G.Length];
            double[] Dist = new double[G.Length];
            for (i = 0; i < Dist.Length; i++) { Dist[i] = double.PositiveInfinity; }
            foreach (Node v in G)
            {
                PairG1[v.ID] = G[G.Length - 1];
                PairG2[v.ID] = G[G.Length - 1];
            }
            LinkedList<Line> removeLines = new LinkedList<Line>();
            while (HopcroftKarpBFS(G1, PairG1, PairG2, Dist) == true)
            {
                foreach (Node v in G1)
                {
                    if (PairG1[v.ID].ID == Dist.Length - 1)
                    {
                        if (HopcroftKarpDFS(v, G1, PairG1, PairG2, Dist, removeLines) == true)
                        {
                            matching = matching + 1;
                        }
                    }
                }
            }
            return removeLines;
        }
        private static bool HopcroftKarpBFS(LinkedList<Node> G1, Node[] PairG1, Node[] PairG2, double[] Dist)
        {
            Queue<Node> Q = new Queue<Node>();
            foreach (Node v in G1)
            {
                if (PairG1[v.ID].ID == Dist.Length - 1)
                {
                    Dist[v.ID] = 0;
                    Q.Enqueue(v);
                }
                else
                {
                    Dist[v.ID] = double.PositiveInfinity;
                }
            }
            Dist[Dist.Length - 1] = double.PositiveInfinity;
            while (Q.Count > 0)
            {
                Node v = Q.Dequeue();
                if (Dist[v.ID] < Dist[Dist.Length - 1])
                {
                    foreach (Node u in v.Connections)
                    {
                        if (double.IsPositiveInfinity(Dist[PairG2[u.ID].ID]))
                        {
                            Dist[PairG2[u.ID].ID] = Dist[v.ID] + 1;
                            Q.Enqueue(PairG2[u.ID]);
                        }
                    }
                }
            }
            return !double.IsPositiveInfinity(Dist[Dist.Length - 1]);
        }
        private static bool HopcroftKarpDFS(Node v, LinkedList<Node> G1, Node[] PairG1, Node[] PairG2, double[] Dist, LinkedList<Line> lRemoves)
        {
            if (v.ID != Dist.Length - 1)
            {
                foreach (Node u in v.Connections)
                {
                    if (Dist[PairG2[u.ID].ID] == Dist[v.ID] + 1)
                    {
                        if (HopcroftKarpDFS(PairG2[u.ID], G1, PairG1, PairG2, Dist, lRemoves) == true)
                        {
                            PairG2[u.ID] = v;
                            PairG1[v.ID] = u;
                            lRemoves.AddLast(PairG1[u.ID].Line);
                            return true;
                        }
                    }
                    Dist[v.ID] = double.PositiveInfinity;
                }
                return false;
            }
            return true;
        }

        private static void ZalojDFS(LinkedList<Node> G1, LinkedList<Node> G2, out Line[] maxIndSet)
        {
            Node[] G = new Node[G1.Count + G2.Count];
            bool[] flags = new bool[G.Length];
            bool[] best = new bool[G.Length];
            int i = 0, max;
            if (G1.Count > G2.Count)
            {
                max = G1.Count;
                foreach (Node n in G1) { G[i] = n; n.ID = i; best[i] = true; i++; }
                foreach (Node n in G2) { G[i] = n; n.ID = i; i++; }
            }
            else
            {
                max = G2.Count;
                foreach (Node n in G1) { G[i] = n; n.ID = i; i++; }
                foreach (Node n in G2) { G[i] = n; n.ID = i; best[i] = true; i++; }
            }

            ZalojDFS(0, G, flags, ref max, best);

            maxIndSet = new Line[max];
            int si = 0;
            for (i = 0; i < best.Length; i++)
            {
                if (best[i]) { maxIndSet[si++] = G[i].Line; }
            }
        }
        private static void ZalojDFS(int ni, Node[] G, bool[] flags, ref int max, bool[] bestSolution)
        {
            //Calculate Current Solution With Node
            flags[ni] = true;
            bool[] killFlags = new bool[flags.Length];
            int pcount = G.Length;
            int count = 0;
            for (int i = 0; i < G.Length; i++)
            {
                if (flags[i])
                {
                    count++;
                    foreach (Node n in G[i].Connections)
                    {
                        if (flags[n.ID])
                        {
                            //Independency Not Found
                            flags[ni] = false;
                            return;
                        }
                        else
                        {
                            if (!killFlags[n.ID])
                            {
                                pcount--;
                                killFlags[n.ID] = true;
                            }
                        }
                    }
                }
            }
            if (pcount == count)
            {
                //A Solution Has Been Found For Constraints
                if (max < count)
                {
                    //New Best Solution Has Been Found
                    Array.Copy(flags, bestSolution, flags.Length);
                    max = count;
                }
            }
            else if (pcount > max)
            {
                //Can Still Do Better
                for (int i = ni + 1; i < G.Length; i++)
                {
                    if (!flags[i] && !killFlags[i])
                    {
                        ZalojDFS(i, G, flags, ref max, bestSolution);
                    }
                }
            }
            else
            {
                //Prune
            }

            //Calculate Solutions Without Node
            flags[ni] = false;
            for (int i = ni + 1; i < G.Length; i++)
            {
                if (!flags[i])
                {
                    ZalojDFS(i, G, flags, ref max, bestSolution);
                }
            }
        }

        public static void maxMatch(HashSet<Node> hSet, HashSet<Node> vSet)
        {

        }

        public class Vertex
        {
            public const int Up = 0;
            public const int Right = 1;
            public const int Down = 2;
            public const int Left = 3;

            public const int TopRight = 0;
            public const int TopLeft = 1;
            public const int BottomRight = 2;
            public const int BottomLeft = 3;

            public int X, Y;
            public int SolidCount;

            public bool IsInShape { get { return SolidCount > 0; } }
            public bool IsConvex { get { return SolidCount == 1; } }
            public bool IsOnLine { get { return SolidCount == 2; } }
            public bool IsConcave { get { return SolidCount == 3; } }
            public bool PassLineTest { get { return SolidCount > 0 && SolidCount < 4; } }
            public bool PassHLineTest { get { return Solid[TopLeft] != Solid[BottomLeft] || Solid[TopRight] != Solid[BottomRight]; } }
            public bool PassHLineTestBegin { get { return Solid[TopLeft] == Solid[BottomLeft] && Solid[TopRight] != Solid[BottomRight]; } }
            public bool PassHLineTestEnd { get { return Solid[TopLeft] != Solid[BottomLeft] && Solid[TopRight] == Solid[BottomRight]; } }
            public bool PassVLineTest { get { return Solid[TopLeft] != Solid[TopRight] || Solid[BottomLeft] != Solid[BottomRight]; } }
            public bool PassVLineTestBegin { get { return Solid[TopLeft] == Solid[TopRight] && Solid[BottomLeft] != Solid[BottomRight]; } }
            public bool PassVLineTestEnd { get { return Solid[TopLeft] != Solid[TopRight] && Solid[BottomLeft] == Solid[BottomRight]; } }

            public bool EmanatesLine;

            public bool[] Lines;
            public int LinesUsed
            {
                get
                {
                    return
                        (Lines[0] ? 1 : 0) +
                        (Lines[1] ? 1 : 0) +
                        (Lines[2] ? 1 : 0) +
                        (Lines[3] ? 1 : 0);
                }
            }

            public bool[] Solid;

            public Vertex(int x, int y)
            {
                X = x;
                Y = y;
                EmanatesLine = false;
                Lines = new bool[4];
                Solid = new bool[4];
            }

            public override string ToString()
            {
                return X + " " + Y + " - " +
                    (Lines[Up] ? "^" : " ") +
                    (Lines[Down] ? "V" : " ") +
                    (Lines[Right] ? ">" : " ") +
                    (Lines[Left] ? "<" : " ")
                    ;
            }

            public void checkSolidity(bool[,] solid, int w, int h)
            {
                Solid[TopLeft] = (X > 0 && Y > 0 && solid[Y - 1, X - 1]);
                Solid[TopRight] = (X < w && Y > 0 && solid[Y - 1, X]);
                Solid[BottomLeft] = (X > 0 && Y < h && solid[Y, X - 1]);
                Solid[BottomRight] = (X < w && Y < h && solid[Y, X]);
                SolidCount = Solid.Sum<bool>((i) => { return i ? 1 : 0; });
                return;
            }
        }
        public class Line
        {
            private bool isHorizontal;
            public bool IsHorizontal { get { return isHorizontal; } }
            public bool IsVertical { get { return !isHorizontal; } }

            private bool isInner;
            public bool IsInner { get { return isInner; } }

            public Vertex P1;
            public Vertex P2;

            public int Location { get { return isHorizontal ? P1.Y : P1.X; } }
            public int Start { get { return isHorizontal ? P1.X : P1.Y; } }
            public int End { get { return isHorizontal ? P2.X : P2.Y; } }
            public int Extent { get { return isHorizontal ? P2.X - P1.X : P2.Y - P1.Y; } }

            public Line(Vertex p1, Vertex p2, bool inner, bool horizontal)
            {
                P1 = p1;
                P2 = p2;
                isInner = inner;
                isHorizontal = horizontal;
            }
            public Line(Vertex p1, Vertex p2, bool inner)
                : this(p1, p2, inner, p1.Y == p2.Y)
            {
            }

            public static bool intersects(Line horizontal, Line vertical)
            {
                return
                    horizontal.P1.X < vertical.P1.X &&
                    horizontal.P2.X > vertical.P1.X &&
                    vertical.P1.Y < horizontal.P1.Y &&
                    vertical.P2.Y > horizontal.P1.Y
                    ;
            }
            public static bool touches(Line horizontal, Line vertical)
            {
                return
                    horizontal.P1.X <= vertical.P1.X &&
                    horizontal.P2.X >= vertical.P1.X &&
                    vertical.P1.Y <= horizontal.P1.Y &&
                    vertical.P2.Y >= horizontal.P1.Y
                    ;
            }
            public bool intersects(Line other)
            {
                if (isHorizontal)
                {
                    return !other.isHorizontal && Line.intersects(this, other);
                }
                else
                {
                    return other.isHorizontal && Line.intersects(other, this);
                }
            }

            public static Point getIntersectPoint(Line horizontal, Line vertical)
            {
                return new Point(vertical.P1.X, horizontal.P1.Y);
            }

            public class IntersectComparer : IEqualityComparer<Line>
            {
                public bool Equals(Line x, Line y)
                {
                    return Line.intersects(x, y);
                }

                public int GetHashCode(Line obj)
                {
                    return obj.GetHashCode();
                }
            }
            public class TouchComparer : IEqualityComparer<Line>
            {
                public bool Equals(Line x, Line y)
                {
                    return Line.touches(x, y);
                }

                public int GetHashCode(Line obj)
                {
                    return obj.GetHashCode();
                }
            }
            public class SideComparer : IEqualityComparer<Line>
            {
                private bool cHorizontal;
                public SideComparer(bool h)
                {
                    cHorizontal = h;
                }

                public bool Equals(Line x, Line y)
                {
                    return x.isHorizontal == cHorizontal;
                }

                public int GetHashCode(Line obj)
                {
                    return obj.GetHashCode();
                }
            }

            public override string ToString()
            {
                return (isHorizontal ? "H " : "V ") +
                    Location + " | " + Start + "-" + End;
            }
        }
        public class Node
        {
            public Line Line;
            public int ID;
            public LinkedList<Node> Connections = new LinkedList<Node>();

            public Node(Line l)
            {
                Line = l;
            }
            public void addConnection(Node node)
            {
                Connections.AddLast(node);
            }
        }

        public class VertexCover
        {
            static void Main(string[] args)
            {
                var v = new VertexCover();
                v.ParseInput();
                v.FindVertexCover();
                v.PrintResults();
            }

            private void PrintResults()
            {
                Console.WriteLine(String.Join(" ", VertexCoverResult.Select(x => x.ToString()).ToArray()));
            }

            public void FindVertexCover()
            {
                //FindBipartiteMatching();

                var TreeSet = new HashSet<int>();
                foreach (var v in LeftVertices)
                    if (Matching[v] < 0)
                        DepthFirstSearch(TreeSet, v, false);

                VertexCoverResult = new HashSet<int>(LeftVertices.Except(TreeSet).Union(RightVertices.Intersect(TreeSet)));
                IndependentSetResult = new HashSet<int>((LeftVertices.Union(RightVertices)).Except(VertexCoverResult));
            }

            private void DepthFirstSearch(HashSet<int> TreeSet, int v, bool left)
            {
                if (TreeSet.Contains(v))
                    return;
                TreeSet.Add(v);
                if (left)
                {
                    foreach (var u in Edges[v])
                        if (u != Matching[v])
                            DepthFirstSearch(TreeSet, u, true);
                }
                else if (Matching[v] >= 0)
                    DepthFirstSearch(TreeSet, Matching[v], false);

            }

            private void FindBipartiteMatching()
            {
                Bicolorate();
                Matching = Enumerable.Repeat(-1, VertexCount).ToArray();
                var cnt = 0;
                foreach (var i in LeftVertices)
                {
                    var seen = new bool[VertexCount];
                    if (BipartiteMatchingInternal(seen, i)) cnt++;
                }
            }
            private bool BipartiteMatchingInternal(bool[] seen, int u)
            {
                foreach (var v in Edges[u])
                {
                    if (seen[v]) continue;
                    seen[v] = true;
                    if (Matching[v] < 0 || BipartiteMatchingInternal(seen, Matching[v]))
                    {
                        Matching[u] = v;
                        Matching[v] = u;
                        return true;
                    }
                }
                return false;
            }

            private void Bicolorate()
            {
                LeftVertices = new HashSet<int>();
                RightVertices = new HashSet<int>();

                var colors = new int[VertexCount];
                for (int i = 0; i < VertexCount; ++i)
                    if (colors[i] == 0 && !BicolorateInternal(colors, i, 1))
                        throw new InvalidOperationException("Graph is NOT bipartite.");
            }
            private bool BicolorateInternal(int[] colors, int i, int color)
            {
                if (colors[i] == 0)
                {
                    if (color == 1) LeftVertices.Add(i);
                    else RightVertices.Add(i);
                    colors[i] = color;
                }
                else if (colors[i] != color)
                    return false;
                else
                    return true;
                foreach (var j in Edges[i])
                    if (!BicolorateInternal(colors, j, 3 - color))
                        return false;
                return true;
            }

            public int VertexCount;
            public HashSet<int>[] Edges;
            public HashSet<int> LeftVertices;
            public HashSet<int> RightVertices;
            public HashSet<int> VertexCoverResult;
            public HashSet<int> IndependentSetResult;
            public int[] Matching;

            private void ReadIntegerPair(out int x, out int y)
            {
                var input = Console.ReadLine();
                var splitted = input.Split(new char[] { ' ' }, 2);
                x = int.Parse(splitted[0]);
                y = int.Parse(splitted[1]);
            }

            private void ParseInput()
            {
                int EdgeCount;
                ReadIntegerPair(out VertexCount, out EdgeCount);
                Edges = new HashSet<int>[VertexCount];
                for (int i = 0; i < Edges.Length; ++i)
                    Edges[i] = new HashSet<int>();

                for (int i = 0; i < EdgeCount; i++)
                {
                    int x, y;
                    ReadIntegerPair(out x, out y);
                    Edges[x].Add(y);
                    Edges[y].Add(x);
                }
            }
        }
    }
}
