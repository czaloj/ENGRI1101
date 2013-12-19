using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Threading;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System.Windows.Input;
using WinKB = System.Windows.Input.Keyboard;
using KEA = System.Windows.Input.KeyEventArgs;

namespace BlisterUI.Input
{
    public static class WMHookInput
    {
        //Mouse Handlers
        public static event MouseMotionHandler MouseMotion;
        public static event MouseButtonHandler MouseButton;
        public static event MouseWheelHandler MouseWheel;

        delegate IntPtr WndProc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

        static bool initialized;
        static IntPtr prevWndProc;
        static WndProc hookProcDelegate;
        static IntPtr hIMC;

        //various Win32 constants that we need
        const int GWL_WNDPROC = -4;

        const int WM_IME_SETCONTEXT = 0x0281;
        const int WM_INPUTLANGCHANGE = 0x51;
        const int WM_GETDLGCODE = 0x87;
        const int WM_IME_COMPOSITION = 0x10f;

        //Key Events
        const int WM_KEYDOWN = 0x100;
        const int WM_KEYUP = 0x101;
        const int WM_CHAR = 0x102;

        //Mouse Events
        const int WM_MOUSEMOVE = 0x200;
        const int WM_LBUTTONDOWN = 0x201;
        const int WM_LBUTTONUP = 0x202;
        const int WM_MBUTTONDOWN = 0x207;
        const int WM_MBUTTONUP = 0x208;
        const int WM_RBUTTONDOWN = 0x204;
        const int WM_RBUTTONUP = 0x205;
        const int WM_XBUTTONDOWN = 0x20B;
        const int WM_XBUTTONUP = 0x20C;
        const int WM_MOUSEWHEEL = 0x20A;
        const int WM_MOUSEHWHEEL = 0x20E;

        const int DLGC_WANTALLKEYS = 4;

        // Win32 DLL Imports
        [DllImport("Imm32.dll", CharSet = CharSet.Unicode)]
        static extern IntPtr ImmGetContext(IntPtr hWnd);

        [DllImport("Imm32.dll", CharSet = CharSet.Unicode)]
        static extern IntPtr ImmAssociateContext(IntPtr hWnd, IntPtr hIMC);

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        static extern IntPtr CallWindowProc(IntPtr lpPrevWndFunc, IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        /// <summary>
        /// Initialize the TextInput with the given GameWindow.
        /// </summary>
        /// <param name="window">The XNA window to which text input should be linked.</param>
        public static void Initialize(GameWindow window)
        {
            if (initialized)
                throw new InvalidOperationException("TextInput.Initialize can only be called once!");

            hookProcDelegate = new WndProc(HookProc);
            prevWndProc = (IntPtr)SetWindowLong(window.Handle, GWL_WNDPROC,
                (int)Marshal.GetFunctionPointerForDelegate(hookProcDelegate));

            hIMC = ImmGetContext(window.Handle);
            initialized = true;

            MouseEventDispatcher.setToHook();
        }
        static IntPtr HookProc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam)
        {
            IntPtr returnCode = CallWindowProc(prevWndProc, hWnd, msg, wParam, lParam);

            switch (msg)
            {
                case WM_GETDLGCODE:
                    returnCode = (IntPtr)(returnCode.ToInt32() | DLGC_WANTALLKEYS);
                    break;
                case WM_IME_SETCONTEXT:
                    if (wParam.ToInt32() == 1) { ImmAssociateContext(hWnd, hIMC); }
                    break;
                case WM_INPUTLANGCHANGE:
                    ImmAssociateContext(hWnd, hIMC);
                    returnCode = (IntPtr)1;
                    break;

                //Key Events
                case WM_KEYDOWN:
                    KeyboardEventDispatcher.EventInput_KeyDown(null, (Keys)wParam);
                    break;
                case WM_KEYUP:
                    KeyboardEventDispatcher.EventInput_KeyUp(null, (Keys)wParam);
                    break;
                case WM_CHAR:
                    KeyboardEventDispatcher.EventInput_CharEntered(null, (char)wParam, lParam.ToInt32());
                    break;

                //Mouse Events
                case WM_MOUSEMOVE:
                    if (MouseMotion != null) { MouseMotion(null, new MouseMotionEventArgs(lParam.ToInt64())); }
                    break;
                case WM_LBUTTONDOWN:
                    if (MouseButton != null) { MouseButton(null, new MouseButtonEventArgs(lParam.ToInt64(), MOUSE_BUTTON.LEFT_BUTTON, ButtonState.Pressed)); }
                    break;
                case WM_LBUTTONUP:
                    if (MouseButton != null) { MouseButton(null, new MouseButtonEventArgs(lParam.ToInt64(), MOUSE_BUTTON.LEFT_BUTTON, ButtonState.Released)); }
                    break;
                case WM_MBUTTONDOWN:
                    if (MouseButton != null) { MouseButton(null, new MouseButtonEventArgs(lParam.ToInt64(), MOUSE_BUTTON.MIDDLE_BUTTON, ButtonState.Pressed)); }
                    break;
                case WM_MBUTTONUP:
                    if (MouseButton != null) { MouseButton(null, new MouseButtonEventArgs(lParam.ToInt64(), MOUSE_BUTTON.MIDDLE_BUTTON, ButtonState.Released)); }
                    break;
                case WM_RBUTTONDOWN:
                    if (MouseButton != null) { MouseButton(null, new MouseButtonEventArgs(lParam.ToInt64(), MOUSE_BUTTON.RIGHT_BUTTON, ButtonState.Pressed)); }
                    break;
                case WM_RBUTTONUP:
                    if (MouseButton != null) { MouseButton(null, new MouseButtonEventArgs(lParam.ToInt64(), MOUSE_BUTTON.RIGHT_BUTTON, ButtonState.Released)); }
                    break;
                case WM_MOUSEWHEEL:
                    if (MouseWheel != null) { MouseWheel(null, new MouseWheelEventArgs(wParam.ToInt32())); }
                    break;
            }

            return returnCode;
        }
    }
}
