using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ZLibrary.ADT;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace XNA2D.Framework
{
    public class SpriteTexture : IIDHolder
    {
        public string file;

        private Texture2D texture;
        public Texture2D Texture { get { return texture; } }

        private Vector2 textureSize;
        public Vector2 TextureSize { get { return textureSize; } }

        private long id;
        public long ID { get { return id; } }

        public SpriteTexture()
        {

        }
        public SpriteTexture(string fileName)
        {
            file = fileName;
            texture = null;
            textureSize = Vector2.Zero;
            id = -1;
        }
        public SpriteTexture(string fileName, ContentManager content)
        {
            file = fileName;
            try
            {
                texture = content.Load<Texture2D>(file);
                textureSize = new Vector2(
                    texture.Width,
                    texture.Height
                    );
            }
            catch (Exception)
            {
                texture = null;
                textureSize = Vector2.Zero;
            }
            id = -1;
        }

        public bool isNull()
        {
            return file == null;
        }
        public void build(ContentManager content)
        {
            try
            {
                texture = content.Load<Texture2D>(file);
                textureSize = new Vector2(
                    texture.Width,
                    texture.Height
                    );
            }
            catch (Exception)
            {
                texture = null;
                textureSize = Vector2.Zero;
            }
        }
        public void setID(long id)
        {
            this.id = id;
        }

        public bool isValid()
        {
            return texture != null;
        }
        public bool hasID()
        {
            return id >= 0;
        }
    }
}
