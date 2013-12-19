using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using BlisterUI;
using BlisterUI.Input;
using XNA3D.Graphics;
using XNA2D.Cameras;

namespace ORLabs.Graphics.Widgets
{
    public class TimedHoverFrame : HoverFrame
    {
        delegate void Elapsed(TimeSpan et);
        private static event Elapsed OnElapsedTime;

        public const double RefreshRate = 10.0;
        static System.Timers.Timer timer;
        static TimedHoverFrame()
        {
            timer = new System.Timers.Timer(RefreshRate);
            lastTimeSignal = DateTime.Now;
            timer.Start();
            timer.Elapsed += (s, a) =>
            {
                TimeSpan ts = a.SignalTime - lastTimeSignal;
                if (OnElapsedTime != null) { OnElapsedTime(ts); }
                lastTimeSignal = a.SignalTime;
            };
        }
        static DateTime lastTimeSignal;
        new public static void create<T>(ref T f, WidgetFrame frame, Vector2 size, Color cb, Color ch) where T : TimedHoverFrame
        {
            HoverFrame.create<T>(ref f, frame, size, cb, ch);
            f.et = 0;
            OnElapsedTime += f.onElapsedTime;
        }
        
        void onElapsedTime(TimeSpan e)
        {
            et += (float)e.TotalSeconds;
        }

        protected float et;
    }
}
