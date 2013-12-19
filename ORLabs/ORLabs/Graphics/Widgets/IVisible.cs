using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BlisterUI;
using Microsoft.Xna.Framework;

namespace ORLabs.Graphics.Widgets
{
    public interface IVisible
    {
        bool IsVisible { get; }
        void setVisible(bool b);
    }
    public interface IMTransVisible : IMTransformed, IVisible
    {

    }

    public class MTransVisibleList : IMTransVisible
    {
        public MTVLBinding[] Children;

        private WidgetFrame world;
        public WidgetFrame World
        {
            get { return world; }
            set
            {
                world = value;
                foreach (MTVLBinding bind in Children)
                { bind.Child.World = bind.Offset * world; }
            }
        }

        private bool visible;
        public bool IsVisible
        {
            get { return visible; }
        }

        public MTransVisibleList(params MTVLBinding[] o)
        {
            Children = o;
        }

        public void setVisible(bool b)
        {
            visible = b;
            foreach (MTVLBinding bind in Children)
            { bind.Child.setVisible(bind.VisModifier ? visible : !visible); }
        }
    }
    public struct MTVLBinding
    {
        public WidgetFrame Offset;
        public bool VisModifier;
        public IMTransVisible Child;

        public MTVLBinding(IMTransVisible o, WidgetFrame m, bool vm = true)
        {
            Offset = m;
            Child = o;
            VisModifier = vm;
        }
    }
}
