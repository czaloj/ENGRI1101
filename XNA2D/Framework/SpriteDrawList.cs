using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using XNA2D.Interfaces;

namespace XNA2D.Framework
{
    public class SpriteDrawList<T> where T : ISpriteVisible
    {
        protected List<T> drawable;

        public SpriteDrawList()
        {
            drawable = new List<T>();
        }

        public void add(T obj)
        {
            drawable.Add(obj);
        }
        public void remove(T obj)
        {
            drawable.Remove(obj);
        }
        public void removeAll()
        {
            drawable.Clear();
        }

        public void drawAll(SpriteBatch batch)
        {
            foreach (T obj in drawable)
            {
                if (obj.Visible)
                {
                    obj.draw(batch);
                }
            }
        }
    }
}
