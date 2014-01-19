using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace ZLibrary.Graphs
{
    #region Graph Events
    public delegate void NodeEvent<N, E>(object graph, N node)
        where E : Edge<N>
        where N : Node<E>;
    public delegate void EdgeEvent<N, E>(object graph, E edge)
        where E : Edge<N>
        where N : Node<E>;
    #endregion

    public class Graph<E, N> : IEnumerable<N>
        where E : Edge<N>, new()
        where N : Node<E>, new()
    {
        //Used To Pre Read Nodes And Edges
        private static readonly Signature SigNode = new Signature("n", 1);
        private static readonly Signature SigEdge = new Signature("e", 4);

        public event NodeEvent<N, E> OnNodeCreation;
        public event NodeEvent<N, E> OnNodeDeletion;
        public event EdgeEvent<N, E> OnEdgeCreation;
        public event EdgeEvent<N, E> OnEdgeDeletion;

        public E[] Edges;
        protected int eCount;
        public int EdgeCount { get { return eCount; } }
        public N[] Nodes;
        protected int nCount;
        public int NodeCount { get { return nCount; } }

        public Graph()
        {

        }
        public Graph(int nCapacity, int eCapacity)
        {
            Edges = new E[eCapacity];
            eCount = 0;

            Nodes = new N[nCapacity];
            eCount = 0;
        }

        public void addNode(N node)
        {
            if (nCount == Nodes.Length) { Array.Resize<N>(ref Nodes, Nodes.Length * 2 + 1); }
            Nodes[nCount] = node;
            Nodes[nCount].Index = nCount;
            nCount++;
            if (OnNodeCreation != null) { OnNodeCreation(this, Nodes[nCount - 1]); }
        }
        public void removeNode(N node)
        {
            int i = Array.IndexOf(Nodes, node, 0, nCount); if (i < 0) { return; }
            if (OnNodeDeletion != null) { OnNodeDeletion(this, Nodes[i]); }

            E[] le = new E[node.Degree]; int ei = 0;
            foreach (E edge in node) { le[ei++] = edge; }
            foreach (E e in le) { removeEdge(e); }

            Nodes[i] = null;
            nCount--;
            while (i < nCount)
            {
                Nodes[i] = Nodes[i + 1];
                Nodes[i].Index = i;
                i++;
            }
        }
        public void addEdge(E edge, int n1, int n2, bool isArc = false)
        {
            if (eCount == Edges.Length) { Array.Resize<E>(ref Edges, Edges.Length * 2 + 1); }
            Edges[eCount] = edge;
            Edges[eCount].Index = eCount;
            Edges[eCount].IsArc = isArc;
            tieEdge(Edges[eCount], Nodes[n1], Nodes[n2]);
            eCount++;
            if (OnEdgeCreation != null) { OnEdgeCreation(this, Edges[eCount - 1]); }
        }
        public void addArc(E edge, int ns, int ne)
        {
            addEdge(edge, ns, ne, true);
        }
        public void removeEdge(E edge)
        {
            int i = Array.IndexOf(Edges, edge, 0, eCount); if (i < 0) { return; }
            if (OnEdgeDeletion != null) { OnEdgeDeletion(this, Edges[i]); }
            untieEdge(edge);

            Edges[i] = null;
            eCount--;
            while (i < eCount)
            {
                Edges[i] = Edges[i + 1];
                Edges[i].Index = i;
                i++;
            }
        }
        public void tieEdge(E edge, N ns, N ne)
        {
            edge.setStartNode(ns);
            edge.setEndNode(ne);
            ns.addEdge(edge, true);
            ne.addEdge(edge, false);
        }
        public void untieEdge(E edge)
        {
            if (edge.IsCycle)
            {
                edge.Start.removeEdge(edge);
                edge.setStartNode(null);
                edge.setEndNode(null);
            }
            else
            {
                edge.Start.removeEdge(edge);
                edge.End.removeEdge(edge);
                edge.setStartNode(null);
                edge.setEndNode(null);
            }
        }

        public void clear(int newNCapacity, int newECapacity)
        {
            int i;
            #region Delete Edges
            i = EdgeCount;
            while (i > 0)
            {
                i--;
                if (OnEdgeDeletion != null) { OnEdgeDeletion(this, Edges[i]); }
                untieEdge(Edges[i]);
                Edges[i] = null;
                eCount--;
            }
            #endregion
            #region Delete Nodes
            i = NodeCount;
            while (i > 0)
            {
                i--;
                if (OnNodeDeletion != null) { OnNodeDeletion(this, Nodes[i]); }
                Nodes[i] = null;
                nCount--;
            }
            #endregion
            Nodes = new N[newNCapacity];
            Edges = new E[newECapacity];
        }

        public int edgeCount(int n1, int n2)
        {
            int c = 0;
            if (Nodes[n1].InDegree + Nodes[n1].OutDegree > 0)
            {
                foreach (E edge in Nodes[n1])
                {
                    if (edge.End.Index == n2 || edge.Start.Index == n2) { c++; }
                }
            }
            return c;
        }

        public void resizeFit()
        {
            if (EdgeCount != Edges.Length) { Array.Resize<E>(ref Edges, EdgeCount); }
            if (NodeCount != Nodes.Length) { Array.Resize<N>(ref Nodes, NodeCount); }
        }

        public void readNodes(TokenStream s)
        {
            Token t;
            if (!s.readUntilHeader("count"))
            { throw new ArgumentException("Node Count Expected"); }
            s.removeLastAdded(out t);
            if (!t.getArg<int>(0, ref nCount))
            { throw new ArgumentException("Integer Node Count Expected"); }
            Nodes = new N[nCount];
            nCount = 0;

            s.readAll();
            int i = 0;
            while (NodeCount < Nodes.Length && s.TokensRemaining > 0 && s.search("node", out t))
            {
                if (t.ArgumentCount < 1 || !t.getArg<int>(0, ref i)) { t.throwBadArg<int>(0); }
                N n = new N();
                n.read(s);
                addNode(n);
            }
        }
        public void writeNodes(TokenStream s)
        {
            s.write("Count", NodeCount);
            foreach (var n in Nodes)
            {
                s.write(Token.fromHeaderArgs("Node", n.Index));
                n.write(s);
            }
        }
        public void readEdges(TokenStream s)
        {
            Token t;
            if (!s.readUntilHeader("count"))
            { throw new ArgumentException("Edge Count Expected"); }
            s.removeLastAdded(out t);
            if (!t.getArg<int>(0, ref eCount))
            { throw new ArgumentException("Integer Edge Count Expected"); }
            Edges = new E[eCount];
            eCount = 0;

            s.readAll();
            int i = 0, si = 0, ei = 0;
            bool ae = false;
            while (EdgeCount < Edges.Length && s.TokensRemaining > 0 && s.search("edge", out t))
            {
                if (t.ArgumentCount < 1 ||
                    !t.getArg<int>(0, ref i) ||
                    !t.getArg<int>(1, ref si) ||
                    !t.getArg<int>(2, ref ei) ||
                    !t.getArg<bool>(3, ref ae)
                    ) { t.throwBadArg<int>(0); }
                E e = new E();
                e.read(s);
                addEdge(e, si, ei, !ae);
            }
        }
        public void writeEdges(TokenStream s)
        {
            s.write("Count", EdgeCount);
            foreach (var e in Edges)
            {
                s.write(Token.fromHeaderArgs("Edge", e.Index, e.StartNodeIndex, e.EndNodeIndex, e.IsArc));
                e.write(s);
            }
        }
        public virtual void readMisc(TokenStream s)
        {
            throw new NotImplementedException();
        }
        public virtual void writeMisc(TokenStream s)
        {
            throw new NotImplementedException();
        }

        #region Node Enumeration
        public IEnumerator<N> GetEnumerator()
        {
            int i = 0;
            while (i < NodeCount)
            {
                if (Nodes[i] != null) { yield return Nodes[i]; }
                i++;
            }
        }
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            int i = 0;
            while (i < NodeCount)
            {
                if (Nodes[i] != null) { yield return Nodes[i]; }
                i++;
            }
        }

        public IEnumerable<N> ActiveNodes
        {
            get
            {
                int i = 0;
                while (i < NodeCount)
                {
                    if (Nodes[i] != null) { yield return Nodes[i]; }
                    i++;
                }
            }
        }
        public IEnumerable<E> ActiveEdges
        {
            get
            {
                int i = 0;
                while (i < EdgeCount)
                {
                    if (Edges[i] != null) { yield return Edges[i]; }
                    i++;
                }
            }
        }
        #endregion
    }

    #region Node
    public interface INode
    {
        int Index { get; set; }
        int Degree { get; }
        int InDegree { get; }
        int OutDegree { get; }

        void read(TokenStream s);
        void write(TokenStream s);
    }
    public interface INode<E> : INode, IEnumerable<E> where E : IEdge
    {
        void addEdge(E edge, bool emanating);
        void removeEdge(E edge);
    }
    public abstract class Node<E> : INode<E> where E : IEdge
    {
        private int index;
        public int Index
        {
            get { return index; }
            set { index = value; }
        }

        protected LinkedList<E> inEdges;
        public IEnumerable<E> InEdges
        {
            get { foreach (E e in inEdges) { yield return e; } }
        }
        public int InDegree
        {
            get { return inEdges.Count; }
        }
        protected LinkedList<E> outEdges;
        public IEnumerable<E> OutEdges
        {
            get { foreach (E e in outEdges) { yield return e; } }
        }
        public int OutDegree
        {
            get { return outEdges.Count; }
        }
        public int Degree
        {
            get { return InDegree + OutDegree; }
        }

        public Node() { inEdges = new LinkedList<E>(); outEdges = new LinkedList<E>(); }

        public void addEdge(E edge, bool emanating)
        {
            if (emanating) { outEdges.AddLast(edge); }
            else { inEdges.AddLast(edge); }
        }
        public void removeEdge(E edge)
        {
            if (edge.StartNodeIndex == Index)
            { outEdges.Remove(edge); }
            if (edge.EndNodeIndex == Index)
            { outEdges.Remove(edge); }
        }

        public abstract void read(TokenStream s);
        public abstract void write(TokenStream s);

        public bool edgeBetween(Node<E> other, out E edge)
        {
            foreach (E e in other.OutEdges) { if (e.EndNodeIndex == other.index) { edge = e; return true; } }
            foreach (E e in other.InEdges) { if (e.StartNodeIndex == other.index) { edge = e; return true; } }
            edge = default(E);
            return false;
        }

        #region Edge Enumeration
        public IEnumerator<E> GetEnumerator()
        {
            foreach (E e in inEdges) { yield return e; }
            foreach (E e in outEdges) { yield return e; }
        }
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            foreach (E e in inEdges) { yield return e; }
            foreach (E e in outEdges) { yield return e; }
        }
        #endregion
    }
    #endregion

    #region Edge
    public interface IEdge
    {
        int Index { get; set; }

        int StartNodeIndex { get; }
        int EndNodeIndex { get; }

        bool IsArc { get; set; }

        void read(TokenStream s);
        void write(TokenStream s);
    }
    public interface IEdge<N> : IEdge where N : INode
    {
        N Start { get; }
        N End { get; }

        void setStartNode(N node);
        void setEndNode(N node);
    }
    public abstract class Edge<N> : IEdge<N> where N : INode
    {
        private int index;
        public int Index
        {
            get { return index; }
            set { index = value; }
        }

        private bool arc;
        public bool IsArc
        {
            get { return arc; }
            set { arc = value; }
        }

        N start, end;
        public N Start { get { return start; } }
        public int StartNodeIndex { get { return start.Index; } }
        public N End { get { return end; } }
        public int EndNodeIndex { get { return end.Index; } }

        public bool IsCycle { get { return StartNodeIndex == EndNodeIndex; } }

        public void setStartNode(N node)
        {
            start = node;
        }
        public void setEndNode(N node)
        {
            end = node;
        }

        public abstract void read(TokenStream s);
        public abstract void write(TokenStream s);
    }
    #endregion
}
