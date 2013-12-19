using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using XNA3D.Cameras;

namespace XNA3D.ZDR
{
    public interface ITextureHolder : IDisposable
    {
        bool ShouldDisposeTextures { get; set; }
        Texture2D TextureAlbedo { get; set; }
    }
    public interface ITextureDeferredHolder : ITextureHolder
    {
        Texture2D TextureNormal { get; set; }
        Texture2D TextureSpecular { get; set; }
    }

    #region Models
    public interface IModel : IDisposableState<IModel>
    {
        VertexBuffer VBuffer { get; }
        bool HasVBuffer { get; }
        VertexDeclaration VDeclaration { get; }

        IndexBuffer IBuffer { get; }
        bool HasIBuffer { get; }

        bool CanDraw { get; }

        void setBuffer(VertexBuffer buffer);
        void setBuffer(IndexBuffer buffer);

        void setVIBuffers(GraphicsDevice g);
    }
    public interface ITexturedModel : IModel, ITextureHolder { }
    public interface IDeferredModel : IModel, ITextureDeferredHolder { }

    public class ZModel : IModel
    {
        #region Buffers
        protected VertexBuffer vBuffer;
        public VertexBuffer VBuffer
        {
            get { return vBuffer; }
        }
        public bool HasVBuffer { get { return vBuffer != null; } }

        public VertexDeclaration VDeclaration
        {
            get { return HasVBuffer ? vBuffer.VertexDeclaration : null; }
        }

        protected IndexBuffer iBuffer;
        public IndexBuffer IBuffer
        {
            get { return iBuffer; }
        }
        public bool HasIBuffer { get { return iBuffer != null; } }

        public bool CanDraw
        {
            get { return HasVBuffer && HasIBuffer; }
        }

        public void setBuffer(VertexBuffer buffer)
        {
            vBuffer = buffer;
        }
        public void setBuffer(IndexBuffer buffer)
        {
            iBuffer = buffer;
        }
        public void setVIBuffers(GraphicsDevice g)
        {
            g.SetVertexBuffer(vBuffer);
            g.Indices = iBuffer;
        }
        #endregion

        public ZModel()
        {
            isDisposed = false;
        }
        public ZModel(VertexBuffer vb, IndexBuffer ib)
            : this()
        {
            setBuffer(vb);
            setBuffer(ib);
        }

        public static M fromList<T, M>(GraphicsDevice g, T[] verts, int[] inds)
            where T : struct, IVertexType
            where M : ZModel, new()
        {
            M m = new M();
            VertexBuffer vb = new VertexBuffer(g, verts[0].VertexDeclaration, verts.Length, BufferUsage.WriteOnly);
            vb.SetData<T>(verts);
            IndexBuffer ib = new IndexBuffer(g, IndexElementSize.ThirtyTwoBits, inds.Length, BufferUsage.WriteOnly);
            ib.SetData<int>(inds);
            m.setBuffer(vb);
            m.setBuffer(ib);
            return m;
        }
        public static M fromList<T, M>(GraphicsDevice g, T[] verts, short[] inds)
            where T : struct, IVertexType
            where M : ZModel, new()
        {
            M m = new M();
            VertexBuffer vb = new VertexBuffer(g, verts[0].VertexDeclaration, verts.Length, BufferUsage.WriteOnly);
            vb.SetData<T>(verts);
            IndexBuffer ib = new IndexBuffer(g, IndexElementSize.SixteenBits, inds.Length, BufferUsage.WriteOnly);
            ib.SetData<short>(inds);
            m.setBuffer(vb);
            m.setBuffer(ib);
            return m;
        }

        #region Disposal
        private bool isDisposed;
        public bool IsDisposed
        {
            get { return isDisposed; }
        }

        public event Action<IModel> OnDisposal;

        ~ZModel() { dispose(); }
        public void Dispose()
        {
            GC.SuppressFinalize(this);
            dispose();
        }
        protected virtual void dispose()
        {
            isDisposed = true;
            if (OnDisposal != null) { OnDisposal(this); }
            if (HasVBuffer) { vBuffer.Dispose(); }
            if (HasIBuffer) { iBuffer.Dispose(); }
        }
        #endregion
    }
    public class ZTexturedModel : ZModel, ITexturedModel
    {
        protected bool disposeTextures;

        protected Texture2D tAlbedo;
        public Texture2D TextureAlbedo
        {
            get { return tAlbedo; }
            set { tAlbedo = value; }
        }

        public bool ShouldDisposeTextures
        {
            get { return disposeTextures; }
            set { disposeTextures = value; }
        }

        public ZTexturedModel() : base() { }
        public ZTexturedModel(VertexBuffer vb, IndexBuffer ib, Texture2D tA)
            : base(vb, ib)
        {
            tAlbedo = tA;
        }

        protected override void dispose()
        {
            base.dispose();
            if (ShouldDisposeTextures)
            {
                if (tAlbedo != null) { tAlbedo.Dispose(); }
            }
        }
    }
    public class ZDeferredModel : ZTexturedModel, IDeferredModel
    {
        protected Texture2D tNormal, tSpecular;
        public Texture2D TextureNormal
        {
            get { return tNormal; }
            set { tNormal = value; }
        }
        public Texture2D TextureSpecular
        {
            get { return tSpecular; }
            set { tSpecular = value; }
        }

        public ZDeferredModel() : base() { }
        public ZDeferredModel(VertexBuffer vb, IndexBuffer ib, Texture2D tA, Texture2D tN, Texture2D tS)
            : base(vb, ib, tA)
        {
            tNormal = tN;
            tSpecular = tS;
        }

        protected override void dispose()
        {
            base.dispose();
            if (ShouldDisposeTextures)
            {
                if (tNormal != null) { tNormal.Dispose(); }
                if (tSpecular != null) { tSpecular.Dispose(); }
            }
        }
    } 
    #endregion

    public interface IVisible : IVisible<IVisible>
    {
    }
    public interface IVisible<T>
    {
        event Action<T, bool> OnVisibilityChange;

        bool IsVisible { get; set; }
    }

    public interface IDrawableInstance
    {
        PrimitiveType Primitive { get; set; }
        int BaseVertex { get; set; }
        int MinVertex { get; set; }
        int VertexCount { get; set; }
        int StartIndex { get; set; }
        int PrimitiveCount { get; set; }

        void draw(GraphicsDevice g);
    }
    public class ZDrawableInstance : IDrawableInstance
    {
        protected ZDrawableInstance()
        {
            primitive = PrimitiveType.TriangleList;
            baseVertex = 0;
            minVertex = 0;
            startIndex = 0;
            primitiveCount = 0;
            vertexCount = 0;
        }

        #region Drawing
        protected PrimitiveType primitive;
        public PrimitiveType Primitive
        {
            get
            {
                return primitive;
            }
            set
            {
                primitive = value;
            }
        }
        protected int baseVertex;
        public int BaseVertex
        {
            get
            {
                return baseVertex;
            }
            set
            {
                baseVertex = value;
            }
        }
        protected int minVertex;
        public int MinVertex
        {
            get
            {
                return minVertex;
            }
            set
            {
                minVertex = value;
            }
        }
        protected int vertexCount;
        public int VertexCount
        {
            get
            {
                return vertexCount;
            }
            set
            {
                vertexCount = value;
            }
        }
        protected int startIndex;
        public int StartIndex
        {
            get
            {
                return startIndex;
            }
            set
            {
                startIndex = value;
            }
        }
        protected int primitiveCount;
        public int PrimitiveCount
        {
            get
            {
                return primitiveCount;
            }
            set
            {
                primitiveCount = value;
            }
        }
        public void draw(GraphicsDevice g)
        {
            g.DrawIndexedPrimitives(primitive, baseVertex, minVertex, vertexCount, startIndex, primitiveCount);
        }
        #endregion
    }

    #region Model Instances
    public interface IModelInstance : IDisposableState<IModelInstance>, IDrawableInstance
    {
        ZModel Model { get; }
        void setModel(ZModel m);
    }
    public class ZModelInstance : ZDrawableInstance, IModelInstance
    {
        protected ZModel model;
        public ZModel Model
        {
            get { return model; }
        }
        public void setModel(ZModel m)
        {
            model = m;
        }

        public ZModelInstance()
            : base() { }
        public ZModelInstance(ZModel model)
            : base()
        {
            primitiveCount = getPrimCount(primitive, model.IBuffer.IndexCount);
            vertexCount = model.VBuffer.VertexCount;
        }

        #region Disposal
        private bool isDisposed;
        public bool IsDisposed
        {
            get { return isDisposed; }
        }

        public event Action<IModelInstance> OnDisposal;

        ~ZModelInstance() { dispose(); }
        public void Dispose()
        {
            GC.SuppressFinalize(this);
            dispose();
        }
        protected virtual void dispose()
        {
            isDisposed = true;
            if (OnDisposal != null) { OnDisposal(this); }
        }
        #endregion

        private static int getPrimCount(PrimitiveType type, int indexCount)
        {
            switch (type)
            {
                case PrimitiveType.LineList:
                    return indexCount / 2;
                case PrimitiveType.LineStrip:
                    return indexCount - 1;
                case PrimitiveType.TriangleList:
                    return indexCount / 3;
                case PrimitiveType.TriangleStrip:
                    return indexCount - 2;
                default: return 0;
            }
        }
    }
    public interface ITexturedModelInstance : IDisposableState<ITexturedModelInstance>, IDrawableInstance
    {
        ZTexturedModel Model { get; }
        void setModel(ZTexturedModel m);
    }
    public class ZTexturedModelInstance : ZDrawableInstance, ITexturedModelInstance
    {
        protected ZTexturedModel model;
        public ZTexturedModel Model
        {
            get { return model; }
        }
        public void setModel(ZTexturedModel m)
        {
            model = m;
        }

        public ZTexturedModelInstance()
            : base() { }
        public ZTexturedModelInstance(ZTexturedModel model)
            : base()
        {
            primitiveCount = getPrimCount(primitive, model.IBuffer.IndexCount);
            vertexCount = model.VBuffer.VertexCount;
        }

        #region Disposal
        private bool isDisposed;
        public bool IsDisposed
        {
            get { return isDisposed; }
        }

        public event Action<ITexturedModelInstance> OnDisposal;

        ~ZTexturedModelInstance() { dispose(); }
        public void Dispose()
        {
            GC.SuppressFinalize(this);
            dispose();
        }
        protected virtual void dispose()
        {
            isDisposed = true;
            if (OnDisposal != null) { OnDisposal(this); }
        }
        #endregion

        private static int getPrimCount(PrimitiveType type, int indexCount)
        {
            switch (type)
            {
                case PrimitiveType.LineList:
                    return indexCount / 2;
                case PrimitiveType.LineStrip:
                    return indexCount - 1;
                case PrimitiveType.TriangleList:
                    return indexCount / 3;
                case PrimitiveType.TriangleStrip:
                    return indexCount - 2;
                default: return 0;
            }
        }
    }
    public interface IDeferredModelInstance : IDisposableState<IDeferredModelInstance>, IDrawableInstance
    {
        ZDeferredModel Model { get; }
        void setModel(ZDeferredModel m);
    }
    public class ZDeferredModelInstance : ZDrawableInstance, IDeferredModelInstance
    {
        protected ZDeferredModel model;
        public ZDeferredModel Model
        {
            get { return model; }
        }
        public void setModel(ZDeferredModel m)
        {
            model = m;
        }

        public ZDeferredModelInstance()
            : base() { }
        public ZDeferredModelInstance(ZDeferredModel model)
            : base()
        {
            primitiveCount = getPrimCount(primitive, model.IBuffer.IndexCount);
            vertexCount = model.VBuffer.VertexCount;
        }

        #region Disposal
        private bool isDisposed;
        public bool IsDisposed
        {
            get { return isDisposed; }
        }

        public event Action<IDeferredModelInstance> OnDisposal;

        ~ZDeferredModelInstance() { dispose(); }
        public void Dispose()
        {
            GC.SuppressFinalize(this);
            dispose();
        }
        protected virtual void dispose()
        {
            isDisposed = true;
            if (OnDisposal != null) { OnDisposal(this); }
        }
        #endregion

        private static int getPrimCount(PrimitiveType type, int indexCount)
        {
            switch (type)
            {
                case PrimitiveType.LineList:
                    return indexCount / 2;
                case PrimitiveType.LineStrip:
                    return indexCount - 1;
                case PrimitiveType.TriangleList:
                    return indexCount / 3;
                case PrimitiveType.TriangleStrip:
                    return indexCount - 2;
                default: return 0;
            }
        }
    } 
    #endregion
}
