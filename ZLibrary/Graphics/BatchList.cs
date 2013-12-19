using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;

namespace ZLibrary.Graphics
{
    public abstract class BatchList<D> : IBatchList<D>
    {
        protected int maxCount;
        public int MaxCount
        {
            get { return maxCount; }
        }

        protected int count;
        public int Count
        {
            get { return count; }
        }

        public BatchList(int maxCount)
        {
            this.maxCount = maxCount;
            createList();
        }
        public BatchList(GraphicsDevice g, int maxCount)
            : this(maxCount)
        {
            buildBuffers(g);
        }
        protected abstract void createList();

        #region IBatchList<L> Members
        public abstract void add(D data);
        public abstract void set(int i, D data);

        public void append(GraphicsDevice g, D[] d)
        {
            if (count + d.Length > maxCount)
            {
                resizeList(count + d.Length);
                buildBuffers(g);
            }
            foreach (D data in d)
            {
                add(data);
            }
            end();
        }
        public void append(GraphicsDevice g, D[] d, int off, int count)
        {
            if (this.count + count > maxCount)
            {
                resizeList(this.count + count);
                buildBuffers(g);
            }
            int last = off + count;
            for (int i = off; i < last; i++)
            {
                add(d[i]);
            }
            end();
        }
        #endregion

        #region IBatchList Members
        public abstract void clearList();
        public abstract void resizeList(int maxCount);
        public abstract void buildBuffers(GraphicsDevice g);

        public void begin()
        {
            clearList();
        }
        public abstract void end();

        public abstract void set(GraphicsDevice g);
        public abstract void draw(GraphicsDevice g);
        #endregion
    }

    public abstract class BatchList<D, V> : BatchList<D>, IBatchList<D, V>
        where V : IVertexType
    {
        protected int vPerInstance;

        public BatchList(GraphicsDevice g, int maxCount, int vpi = 4)
            : base(maxCount)
        {
            vPerInstance = vpi;
            buildBuffers(g);
        }
        protected override void createList()
        {
            vertices = new V[maxCount * vPerInstance];
            count = 0;
        }
        public override void clearList()
        {
            vertices = new V[maxCount * vPerInstance];
            count = 0;
        }
        public override void resizeList(int maxCount)
        {
            this.maxCount = maxCount;
            Array.Resize<V>(ref vertices, this.maxCount * vPerInstance);
            if (count > maxCount) { count = maxCount; }
        }

        protected V[] vertices;
        public V getVertex(int i)
        {
            return vertices[i];
        }
        public  V[] getVertexArray()
        {
            return vertices;
        }

        public void append(GraphicsDevice g, IBatchList<D, V> l)
        {
            V[] ov = l.getVertexArray();
            if (l.Count + count > maxCount)
            {
                resizeList(l.Count + count);
            }
            int oei = l.Count * vPerInstance;
            for (int si = count * vPerInstance, oi = 0; oi < oei; oi++, si++)
            {
                vertices[si] = ov[oi];
            }
            count += l.Count;
        }
        public void append(GraphicsDevice g, IBatchList<D, V> l, int off, int instanceCount)
        {
            V[] ov = l.getVertexArray();
            if (l.Count + count > maxCount)
            {
                resizeList(l.Count + count);
            }
            int oei = (off + instanceCount) * vPerInstance;
            for (int si = count * vPerInstance, oi = off * vPerInstance; oi < oei; oi++, si++)
            {
                vertices[si] = ov[oi];
            }
            count += instanceCount;
        }
    }
}
