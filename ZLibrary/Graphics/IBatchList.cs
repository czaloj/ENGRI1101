using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;

namespace ZLibrary.Graphics
{
    public interface IBatchList
    {
        int Count { get; }
        int MaxCount { get; }

        void clearList();
        void resizeList(int maxCount);
        void buildBuffers(GraphicsDevice g);

        void begin();
        void end();

        void set(GraphicsDevice g);
        void draw(GraphicsDevice g);
    }

    public interface IBatchList<D> : IBatchList
    {
        void add(D data);
        void set(int i, D data);

        void append(GraphicsDevice g, D[] d);
        void append(GraphicsDevice g, D[] d, int off, int count);
    }

    public interface IBatchList<D, V> : IBatchList<D>
        where V : IVertexType
    {
        V getVertex(int i);
        V[] getVertexArray();

        void append(GraphicsDevice g, IBatchList<D, V> l);
        void append(GraphicsDevice g, IBatchList<D, V> l, int off, int instanceCount);
    }
}
