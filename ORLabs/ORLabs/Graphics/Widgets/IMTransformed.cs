using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BlisterUI;
using Microsoft.Xna.Framework;

namespace ORLabs.Graphics.Widgets
{
    public interface IMTransformed
    {
        WidgetFrame World { get; set; }
    }

    public class MTransformList : IMTransformed
    {
        public MTLBinding[] Children;

        private WidgetFrame world;
        public WidgetFrame World
        {
            get { return world; }
            set
            {
                world = value;
                foreach (MTLBinding bind in Children)
                { bind.Child.World = bind.Offset * world; }
            }
        }

        public MTransformList(params MTLBinding[] o)
        {
            Children = o;
        }
    }
    public struct MTLBinding
    {
        public WidgetFrame Offset;
        public IMTransformed Child;

        public MTLBinding(IMTransformed o, WidgetFrame f)
        {
            Offset = f;
            Child = o;
        }
    }
}
