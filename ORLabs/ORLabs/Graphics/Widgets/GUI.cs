using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace ORLabs.Graphics.Widgets {
    public static class GUI {
        public static class Colors {
            public static readonly Color Background = new Color(0, 0, 0, 255);
            public static readonly Color GraphScreenBackground = new Color(30, 30, 30, 255);

            public static readonly Color WidgetBackground = new Color(8, 8, 8, 255);
            public static readonly Color WidgetText = new Color(38, 108, 208, 255);
            public static readonly Color WidgetHighlight = new Color(255, 150, 0, 255);
        }
        public const string MenuFontFile = @"Fonts\FontStartMenu";
        public const string SmallFontFile = @"Fonts\Arial12";
        public const string MediumFontFile = @"Fonts\Arial16";
        public const string LargeFontFile = @"Fonts\Arial22";
    }
}
