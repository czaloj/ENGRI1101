using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using ZLibrary.ADT;
using XNA2D.Framework;
using XNA2D.Interfaces;
using XNA2D.Sprites;

namespace XNA2D
{
    public static class XNA2DProcessor
    {
        private static bool isInitialized;
        private static ContentManager content;
        private static IDList<SpriteTexture> textures;
        private static IDList<IXNA2DSprite> sprites;

        static XNA2DProcessor()
        {
            isInitialized = false;
            content = null;
            textures = new IDList<SpriteTexture>();
            sprites = new IDList<IXNA2DSprite>();
        }

        public static void initialize(ContentManager c)
        {
            content = c;
            if (content != null)
            {
                isInitialized = true;
            }
        }

        private static long addSpriteTexture(string fileName)
        {
            SpriteTexture texture = new SpriteTexture(fileName, content);
            if (texture.Texture == null && isInitialized)
            {
                texture.build(content);
            }
            textures.addObj(texture);
            return texture.ID;
        }
        public static void addTextureList(string fileName)
        {
            SpriteTextureList list = content.Load<SpriteTextureList>(fileName);
            foreach (string file in list.textures)
            {
                addSpriteTexture(file);
            }
        }
        public static void removeSpriteTexture(long ID)
        {
            textures.removeObj(ID);
        }
        public static SpriteTexture getSpriteTexture(int ID)
        {
            return textures[ID];
        }

        public static long addSprite<T>(T sprite) where T : class, IXNA2DSprite
        {
            sprites.addObj(sprite);
            return sprite.ID;
        }
        public static long addSprite<T>(string fileName) where T : class, IXNA2DSprite
        {
            T sprite = content.Load<T>(fileName);
            if (isInitialized)
            {
                sprite.build(content);
            }
            sprites.addObj(sprite);
            return sprite.ID;
        }
        public static void addSpriteList(string fileName)
        {
            SpriteList list = content.Load<SpriteList>(fileName);
            foreach (IXNA2DSprite sprite in list.sprites)
            {
                if (isInitialized)
                {
                    sprite.build(content);
                }
                sprites.addObj(sprite);
            }
        }
        public static void removeSprite(long ID)
        {
            sprites.removeObj(ID);
        }
        public static T getSprite<T>(int ID) where T : class, IXNA2DSprite
        {
            return (T)sprites[ID].Clone();
        }
    }
}
