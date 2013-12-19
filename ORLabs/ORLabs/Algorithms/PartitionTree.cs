using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ZLibrary.ADT;
using ZLibrary.Graphs;
using ZLibrary.Algorithms;
using ZLibrary.Math;
using ZLibrary.IO;
using Microsoft.Xna.Framework;
using ORLabs.Framework;

namespace ORLabs.Algorithms
{
    public class Partition
    {
        public int Count { get { return LinkedPartitions.Count; } }
        public List<Partition> LinkedPartitions;

        public ORGraph.Node node;
        public int PartitionIndex;

        public Partition(ORGraph.Node n)
        {
            node = n;
            PartitionIndex = node.Index;
            LinkedPartitions = new List<Partition>(this);
        }

        public void appendPartition(Partition pAdd)
        {
            LinkedPartitions.append(pAdd.LinkedPartitions);
            var n = pAdd.LinkedPartitions.First;
            while (n != null)
            {
                n.Data.PartitionIndex = PartitionIndex;
                n.Data.LinkedPartitions = LinkedPartitions;
                n = n.Next;
            }
            return;
        }

        public class List<T> : IEnumerable<T>
        {
            Node<T> first, last;
            public int Count { get; private set; }
            public Node<T> First { get { return first; } }
            public Node<T> Last { get { return last; } }

            public List(T data)
            {
                Count = 1;
                Node<T> node = new Node<T>(data);
                first = node;
                last = node;
            }

            public void append(T data) { last.append(new Node<T>(data)); last = last.Next; Count++; }
            public void prepend(T data) { first.prepend(new Node<T>(data)); first = first.Previous; Count++; }
            public void append(Node<T> node) { last.append(node); last = node; Count++; }
            public void prepend(Node<T> node) { first.prepend(node); first = node; Count++; }
            public void append(List<T> list) { last.append(list.first); last = list.last; Count += list.Count; }
            public void prepend(List<T> list) { first.prepend(list.last); first = list.first; Count += list.Count; }


            public IEnumerator<T> GetEnumerator()
            {
                Node<T> n = first;
                while (n != null)
                {
                    yield return n.Data;
                    n = n.Next;
                }
            }
            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
            {
                Node<T> n = first;
                while (n != null)
                {
                    yield return n.Data;
                    n = n.Next;
                }
            }

            public override string ToString()
            {
                return string.Format("[{0}] -> [{1}]", first, last);
            }
        }
        public class Node<T>
        {
            Node<T> next, previous;
            public Node<T> Next { get { return next; } }
            public Node<T> Previous { get { return previous; } }
            T data;
            public T Data { get { return data; } }

            public Node(T o) { data = o; next = null; previous = null; ; }

            public void append(Node<T> n)
            {
                if (next != null) { throw new ArgumentException(); }
                next = n; n.previous = this;
            }
            public void prepend(Node<T> n)
            {
                if (previous != null) { throw new ArgumentException(); }
                previous = n; n.next = this;
            }

            public override string ToString()
            {
                return data.ToString();
            }
        }

        public override string ToString()
        {
            return string.Format("{0} -> Part: {1}", node.Index, PartitionIndex);
        }

        public bool conflictsWith(Partition p)
        {
            return p.PartitionIndex == PartitionIndex;
        }
    }
    public class PartitionTree
    {
        public Partition[] Partitions;

        public PartitionTree(ORGraph.Node[] nodes)
        {
            Partitions = new Partition[nodes.Length];
            for (int i = 0; i < Partitions.Length; i++)
            {
                Partitions[i] = new Partition(nodes[i]);
            }
        }

        public void mergePartitions(ORGraph.Node n1, ORGraph.Node n2)
        {
            if (Partitions[n1.Index].Count > Partitions[n2.Index].Count)
            {
                Partitions[n1.Index].appendPartition(Partitions[n2.Index]);
            }
            else
            {
                Partitions[n2.Index].appendPartition(Partitions[n1.Index]);
            }
        }

        public bool loopBetween(ORGraph.Node n1, ORGraph.Node n2)
        {
            return Partitions[n1.Index].conflictsWith(Partitions[n2.Index]);
        }

        public bool tryAddEdge(ORGraph.Node n1, ORGraph.Node n2)
        {
            if (loopBetween(n1, n2)) { return false; }
            else { mergePartitions(n1, n2); return true; }
        }
    }

}
