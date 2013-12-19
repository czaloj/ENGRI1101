using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using XNA2D.Interfaces;
using XNA2D.Framework;

namespace XNA2D.Sprites.Abstract
{
    public abstract class ACVisibleSprite : IXNA2DSprite
    {
        protected long id = 0;
        public long ID
        {
            get
            {
                return id;
            }
        }

        protected int textureID = 0;
        public int TextureID
        {
            get
            {
                return textureID;
            }
            set
            {
                textureID = value;
            }
        }
        protected SpriteTexture texture = null;

        protected Vector2 position = Vector2.Zero;
        public Vector2 Position
        {
            get
            {
                return position;
            }
            set
            {
                position = value;
            }
        }

        protected float rotation = 0f;
        public float Rotation
        {
            get
            {
                return rotation;
            }
            set
            {
                rotation = value;
            }
        }

        protected SpriteTextureOptions textureOptions = new SpriteTextureOptions(Vector2.One);
        public Vector2 TextureSize
        {
            get
            {
                return textureOptions.viewSize;
            }
            set
            {
                textureOptions.setViewSize(value);
            }
        }
        public Vector2 PivotCenterOffset
        {
            get
            {
                return textureOptions.pivotCenterOffset;
            }
            set
            {
                textureOptions.setPivotCenterOffset(value);
            }
        }

        protected bool visible = true;
        public bool Visible
        {
            get
            {
                return visible;
            }
            set
            {
                visible = value;
            }
        }

        protected Color spriteColor = Color.White;
        public Color SpriteColor
        {
            get
            {
                return spriteColor;
            }
            set
            {
                spriteColor = value;
            }
        }

        protected float layerDepth = 0f;
        public float LayerDepth
        {
            get
            {
                return layerDepth;
            }
            set
            {
                layerDepth = value;
            }
        }

        protected SpriteEffects spriteFlipping = SpriteEffects.None;

        //IXNA2DSprite
        public void setID(long ID)
        {
            id = ID;
        }
        public abstract void build(ContentManager content);

        //ISpriteVisible
        public virtual void draw(SpriteBatch batch)
        {
            batch.Draw(
                texture.Texture,
                Camera2D.calculateScreenLocation(position),
                null,
                spriteColor,
                rotation,
                textureOptions.TexturePivot,
                Camera2D.calculateScreenScale(textureOptions.Scale),
                spriteFlipping,
                layerDepth
                );
        }

        public void setSpriteFlipping(SpriteEffects spriteFlipping)
        {
            this.spriteFlipping = spriteFlipping;
        }
        public SpriteEffects getSpriteFlipping()
        {
            return spriteFlipping;
        }

        //ISpriteTextureHolder
        public bool hasSpriteTexture()
        {
            return texture.isValid();
        }
        public void setSpriteTexture(SpriteTexture texture)
        {
            this.texture = texture;
            textureOptions.recalculate(texture.TextureSize);
        }
        public SpriteTexture getSpriteTexture()
        {
            return texture;
        }

        //ICloneable
        public object Clone()
        {
            return this.MemberwiseClone();
        }
    }
}
