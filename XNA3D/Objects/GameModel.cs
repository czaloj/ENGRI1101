using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using XNA3D.Objects.Data;
using XNA3D.Objects.Graphics;
using XNA3D.Cameras;
using XNA3D.Objects.Interfaces;
using XNA3D.DeferredRendering.Lights;

namespace XNA3D.Objects
{
    public class GameModel : AC3DDrawable, IModelHolder
    {
        //Visible Data
        protected Model model = null;
        public bool HasModel
        {
            get
            {
                return model != null;
            }
        }
        public String ModelName;

        protected Texture2D texture = null;
        protected Texture2D normalTexture = null;
        protected Texture2D specularTexture = null;

        public bool HasTextures
        {
            get
            {
                return texture != null;
            }
        }
        public String ModelTextureName;

        //Effect Data
        protected Matrix world = Matrix.CreateScale(1f) * Matrix.CreateTranslation(Vector3.Zero);
        public Matrix World
        {
            get
            {
                return world;
            }
        }
        protected Vector4 lighting = Vector4.One;

        public GameModel()
        {
            drawable = false;
            visible = false;
        }

        public override void draw(GraphicsDevice g, ACCamera camera, Effect e)
        {
            e.Parameters["World"].SetValue(world);
            e.Parameters["Texture"].SetValue(texture);
            e.Parameters["NormalMap"].SetValue(normalTexture);
            e.Parameters["SpecularMap"].SetValue(specularTexture);
            e.CurrentTechnique.Passes[0].Apply();
            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (ModelMeshPart part in mesh.MeshParts)
                {
                    g.SetVertexBuffer(part.VertexBuffer, part.VertexOffset);
                    g.Indices = part.IndexBuffer;
                    g.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, part.NumVertices, part.StartIndex, part.PrimitiveCount);
                }
            }
        }
        public virtual void drawShadowDepth(GraphicsDevice g, ACCamera camera, Effect e)
        {
            e.Parameters["World"].SetValue(world);
            e.CurrentTechnique.Passes[LightEffect.PassShadowing].Apply();
            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (ModelMeshPart part in mesh.MeshParts)
                {
                    g.SetVertexBuffer(part.VertexBuffer, part.VertexOffset);
                    g.Indices = part.IndexBuffer;
                    g.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, part.NumVertices, part.StartIndex, part.PrimitiveCount);
                }
            }
        }

        public override void setVisible(bool b)
        {
            visible = b && drawable;
        }

        public void setModel(Model model)
        {
            this.model = model;
            if (this.model != null)
            {
                drawable = texture != null;
                visible &= drawable;
            }
            else
            {
                drawable = false;
                visible = false;
            }
        }
        public Model getModel()
        {
            return model;
        }

        public void setModelTextures(Texture2D texture, Texture2D normal, Texture2D specular)
        {
            this.texture = texture;
            specularTexture = specular;
            normalTexture = normal;
            if (this.texture != null && normalTexture != null && specularTexture != null)
            {
                drawable = model != null;
                visible &= drawable;
            }
            else
            {
                drawable = false;
                visible = false;
            }
        }
        public Texture2D getTexture()
        {
            return texture;
        }
        public Texture2D getNormalTexture()
        {
            return normalTexture;
        }
        public Texture2D getSpecularTexture()
        {
            return specularTexture;
        }

        Texture3D t3;
        public void set3DTexture(Texture3D t)
        {
            t3 = t;
        }
        public void draw3D(GraphicsDevice g, ACCamera camera, Effect e)
        {
            e.Parameters["World"].SetValue(world);
            e.Parameters["Texture"].SetValue(t3);
            //e.Parameters["NormalMap"].SetValue(normalTexture);
            //e.Parameters["SpecularMap"].SetValue(specularTexture);
            EffectTechnique old = e.CurrentTechnique;
            e.CurrentTechnique = e.Techniques["New"];
            e.CurrentTechnique.Passes[0].Apply();
            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (ModelMeshPart part in mesh.MeshParts)
                {
                    g.SetVertexBuffer(part.VertexBuffer, part.VertexOffset);
                    g.Indices = part.IndexBuffer;
                    g.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, part.NumVertices, part.StartIndex, part.PrimitiveCount);
                }
            }
            e.CurrentTechnique = old;
        }

        public override void build()
        {
            //setModel(ModelLibrary.get(ModelName));
            //setModelTexture(TextureLibrary.get(ModelTextureName));
        }

        public override object Clone()
        {
            return this.MemberwiseClone();
        }


    }
}
