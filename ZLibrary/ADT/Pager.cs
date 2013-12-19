using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace System.Collections.Generic
{
    public interface IPageable
    {
        int PageIndex { get; }
    }

    public class Pager<I, T> : IEnumerable<PageNode<I, T>>, IEnumerable<T> where I : IPageable
    {
        private PageNode<I, T>[] nodes;
        public T this[I index]
        {
            get
            {
                PageNode<I, T> node = nodes[index.PageIndex];
                while (node != null && !index.Equals(node.Index))
                { node = node.Next; }
                return node == null ? default(T) : node.Data;
            }
            set
            {
                int pi = index.PageIndex;
                PageNode<I, T> node = nodes[pi], pnode = null;
                while (node != null && !index.Equals(node.Index))
                { pnode = node; node = node.Next; }

                if (node != null) { node.setData(value); }
                else if (pnode != null) { new PageNode<I, T>(pnode, index, value); }
                else { nodes[pi] = new PageNode<I, T>(index, value); }
            }
        }
        public T this[int pageIndex]
        {
            get { return nodes[pageIndex] == null ? default(T) : nodes[pageIndex].Data; }
            set { if (nodes[pageIndex] != null) { nodes[pageIndex].setData(value); } }
        }
        private int count;
        public int Count { get { return count; } }

        public Pager(int maxPageIndex)
        {
            nodes = new PageNode<I, T>[maxPageIndex];
            count = 0;
        }

        public void add(I index, T data)
        {
            int i = index.PageIndex;
            PageNode<I, T> node = nodes[i];
            if (node == null)
            {
                nodes[i] = new PageNode<I, T>(index, data);
            }
            else
            {
                while (node.HasNext) { node = node.Next; }
                new PageNode<I, T>(node, index, data);
            }
            count++;
        }
        public bool get(I index, out T o)
        {
            PageNode<I, T> node = nodes[index.PageIndex];
            while (node != null && !index.Equals(node.Index))
            { node = node.Next; }
            if (node == null)
            {
                o = default(T);
                return false;
            }
            else
            {
                o = node.Data;
                return true;
            }
        }
        public bool remove(I index)
        {
            int i = index.PageIndex;
            PageNode<I, T> node = nodes[i], pNode = null;
            while (node != null && !index.Equals(node.Index))
            { pNode = node; node = node.Next; }
            if (node != null)
            {
                if (pNode != null)
                {
                    PageNode<I, T>.split(pNode, node.Next);
                }
                else
                {
                    nodes[i] = node.Next;
                }
                count--;
                return true;
            }
            return false;
        }
        public bool remove(I index, out T o)
        {
            int i = index.PageIndex;
            PageNode<I, T> node = nodes[i], pNode = null;
            while (node != null && !index.Equals(node.Index))
            { pNode = node; node = node.Next; }
            if (node != null)
            {
                o = node.Data;
                if (pNode != null)
                {
                    PageNode<I, T>.split(pNode, node.Next);
                }
                else
                {
                    nodes[i] = node.Next;
                }
                count--;
                return true;
            }
            o = default(T);
            return false;
        }

        public T[] toArray()
        {
            T[] a = new T[count];
            int i = 0;
            foreach (var p in this)
            {
                a[i++] = p.Data;
            }
            return a;
        }

        #region Enumeration
        public IEnumerator<PageNode<I, T>> GetEnumerator()
        {
            PageNode<I, T> next;
            foreach (PageNode<I, T> node in nodes)
            {
                if (node != null)
                {
                    yield return node;
                    next = node.Next;
                    while (next != null)
                    {
                        yield return next;
                        next = next.Next;
                    }
                }
            }
        }
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            PageNode<I, T> next;
            foreach (PageNode<I, T> node in nodes)
            {
                if (node != null)
                {
                    yield return node;
                    next = node.Next;
                    while (next != null)
                    {
                        yield return next;
                        next = next.Next;
                    }
                }
            }
        }
        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            PageNode<I, T> next;
            foreach (PageNode<I, T> node in nodes)
            {
                if (node != null)
                {
                    yield return node.Data;
                    next = node.Next;
                    while (next != null)
                    {
                        yield return next.Data;
                        next = next.Next;
                    }
                }
            }
        }
        #endregion
    }

    public class PageNode<I, T> where I : IPageable
    {
        private PageNode<I, T> next;
        public PageNode<I, T> Next
        {
            get { return next; }
        }
        public bool HasNext
        {
            get { return next != null; }
        }

        private T data;
        public T Data
        {
            get { return data; }
        }

        private I index;
        public I Index
        {
            get { return index; }
        }
        public int PageIndex
        {
            get { return index.PageIndex; }
        }

        public PageNode(I index, T data)
        {
            this.index = index;
            setData(data);
        }
        public PageNode(PageNode<I, T> parent, I index, T data)
            : this(index, data)
        {
            parent.next = this;
        }

        public static PageNode<I, T> before(PageNode<I, T> next, I index, T data)
        {
            PageNode<I, T> n = new PageNode<I, T>(index, data);
            n.next = next;
            return n;
        }
        public static PageNode<I, T> insertAfter(PageNode<I, T> previous, I index, T data)
        {
            PageNode<I, T> n = new PageNode<I, T>(index, data);
            n.next = previous.next;
            previous.next = n;
            return n;
        }
        public static void split(PageNode<I, T> p, PageNode<I, T> n)
        {
            p.next = n;
        }

        public void setData(T data)
        {
            this.data = data;
        }
    }
}
