// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
#if SILICONSTUDIO_PLATFORM_WINDOWS_DESKTOP && (SILICONSTUDIO_XENKO_UI_WINFORMS || SILICONSTUDIO_XENKO_UI_WPF)
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Windows.Interop;

using SharpDX.Multimedia;
using SharpDX.RawInput;
using WinFormsKeys = System.Windows.Forms.Keys;

namespace SiliconStudio.Xenko.Input
{
    internal partial class InputManagerWindows<TK>
    {
        protected internal void BindRawInputKeyboard(Control winformControl)
        {
            if (winformControl.Handle == IntPtr.Zero)
            {
                winformControl.HandleCreated += (sender, args) =>
                    {
                        if (winformControl.Handle != IntPtr.Zero)
                        {
                            BindRawInputKeyboard(winformControl);
                        }
                    };
            }
            else
            {
                SharpDX.RawInput.Device.RegisterDevice(UsagePage.Generic, UsageId.GenericKeyboard, DeviceFlags.None, winformControl.Handle);
                SharpDX.RawInput.Device.KeyboardInput += DeviceOnKeyboardInput;
            }
        }

        protected internal void BindRawInputKeyboard(System.Windows.Window winformControl)
        {
            var interopHelper = new WindowInteropHelper(winformControl);
            interopHelper.EnsureHandle();
            SharpDX.RawInput.Device.RegisterDevice(UsagePage.Generic, UsageId.GenericKeyboard, DeviceFlags.None, interopHelper.Handle);
            SharpDX.RawInput.Device.KeyboardInput += DeviceOnKeyboardInput;
        }

        private void DeviceOnKeyboardInput(object sender, KeyboardInputEventArgs rawKb)
        {
            // Code partially from: http://molecularmusings.wordpress.com/2011/09/05/properly-handling-keyboard-input/
            var key = Keys.None;

            var virtualKey = rawKb.Key;
            var scanCode = rawKb.MakeCode;
            var flags = rawKb.ScanCodeFlags;

            if ((int)virtualKey == 255)
            {
                // discard "fake keys" which are part of an escaped sequence
                return;
            }

            if (virtualKey == WinFormsKeys.ShiftKey)
            {
                // correct left-hand / right-hand SHIFT
                virtualKey = (WinFormsKeys) WinKeys.MapVirtualKey(scanCode, WinKeys.MAPVK_VSC_TO_VK_EX);
            }
            else if (virtualKey == WinFormsKeys.NumLock)
            {
                // correct PAUSE/BREAK and NUM LOCK silliness, and set the extended bit
                scanCode = WinKeys.MapVirtualKey((int)virtualKey, WinKeys.MAPVK_VK_TO_VSC) | 0x100;
            }

            // e0 and e1 are escape sequences used for certain special keys, such as PRINT and PAUSE/BREAK.
            // see http://www.win.tue.nl/~aeb/linux/kbd/scancodes-1.html
            bool isE0 = ((flags & ScanCodeFlags.E0) != 0);
            bool isE1 = ((flags & ScanCodeFlags.E1) != 0);

            if (isE1)
            {
                // for escaped sequences, turn the virtual key into the correct scan code using MapVirtualKey.
                // however, MapVirtualKey is unable to map VK_PAUSE (this is a known bug), hence we map that by hand.
                scanCode = virtualKey == WinFormsKeys.Pause ? 0x45 : WinKeys.MapVirtualKey((int)virtualKey, WinKeys.MAPVK_VK_TO_VSC);
            }

            switch (virtualKey)
            {
                    // right-hand CONTROL and ALT have their e0 bit set
                case WinFormsKeys.ControlKey:
                    virtualKey = isE0 ? WinFormsKeys.RControlKey : WinFormsKeys.LControlKey;
                    break;

                case WinFormsKeys.Menu:
                    virtualKey = isE0 ? WinFormsKeys.RMenu : WinFormsKeys.LMenu;
                    break;

                    // NUMPAD ENTER has its e0 bit set
                case WinFormsKeys.Return:
                    if (isE0)
                        key = Keys.NumPadEnter;
                    break;
            }


            if (key == Keys.None)
            {
                WinKeys.mapKeys.TryGetValue(virtualKey, out key);
            }


            if (key != Keys.None)
            {
                bool isKeyUp = (flags & ScanCodeFlags.Break) != 0;

                if (isKeyUp)
                {
                    lock (KeyboardInputEvents)
                    {
                        KeyboardInputEvents.Add(new KeyboardInputEvent { Key = key, Type = InputEventType.Up });
                    }
                }
                else
                {
                    lock (KeyboardInputEvents)
                    {
                        KeyboardInputEvents.Add(new KeyboardInputEvent { Key = key, Type = InputEventType.Down });
                    }
                }
            }
        }
    }

    /// <summary>
    /// Mapping between <see cref="WinFormsKeys"/> and <see cref="SiliconStudio.Xenko.Input.Keys"/> needed for
    /// translating Winform key events into Xenko ones.
    /// </summary>
    static class WinKeys
    {
        [DllImport("user32.dll")]
        internal static extern int MapVirtualKey(int uCode, uint uMapType);

        /// <summary>
        /// Map between Winform keys and Xenko keys.
        /// </summary>
        internal static readonly Dictionary<WinFormsKeys, Keys> mapKeys = NewMapKeys();

        public const uint MAPVK_VK_TO_VSC = 0x00;
        public const uint MAPVK_VSC_TO_VK = 0x01;
        public const uint MAPVK_VK_TO_CHAR = 0x02;
        public const uint MAPVK_VSC_TO_VK_EX = 0x03;
        public const uint MAPVK_VK_TO_VSC_EX = 0x04;

        /// <summary>
        /// Create a mapping between <see cref="WinFormsKeys"/> and <see cref="SiliconStudio.Xenko.Input.Keys"/>
        /// </summary>
        /// <returns>A new map.</returns>
        private static Dictionary<WinFormsKeys, Keys> NewMapKeys()
        {
            var map = new Dictionary<WinFormsKeys, Keys>(200);
            map[WinFormsKeys.None] = Keys.None;
            map[WinFormsKeys.Cancel] = Keys.Cancel;
            map[WinFormsKeys.Back] = Keys.Back;
            map[WinFormsKeys.Tab] = Keys.Tab;
            map[WinFormsKeys.LineFeed] = Keys.LineFeed;
            map[WinFormsKeys.Clear] = Keys.Clear;
            map[WinFormsKeys.Enter] = Keys.Enter;
            map[WinFormsKeys.Return] = Keys.Return;
            map[WinFormsKeys.Pause] = Keys.Pause;
            map[WinFormsKeys.Capital] = Keys.Capital;
            map[WinFormsKeys.CapsLock] = Keys.CapsLock;
            map[WinFormsKeys.HangulMode] = Keys.HangulMode;
            map[WinFormsKeys.KanaMode] = Keys.KanaMode;
            map[WinFormsKeys.JunjaMode] = Keys.JunjaMode;
            map[WinFormsKeys.FinalMode] = Keys.FinalMode;
            map[WinFormsKeys.HanjaMode] = Keys.HanjaMode;
            map[WinFormsKeys.KanjiMode] = Keys.KanjiMode;
            map[WinFormsKeys.Escape] = Keys.Escape;
            map[WinFormsKeys.IMEConvert] = Keys.ImeConvert;
            map[WinFormsKeys.IMENonconvert] = Keys.ImeNonConvert;
            map[WinFormsKeys.IMEAccept] = Keys.ImeAccept;
            map[WinFormsKeys.IMEModeChange] = Keys.ImeModeChange;
            map[WinFormsKeys.Space] = Keys.Space;
            map[WinFormsKeys.PageUp] = Keys.PageUp;
            map[WinFormsKeys.Prior] = Keys.Prior;
            map[WinFormsKeys.Next] = Keys.Next;
            map[WinFormsKeys.PageDown] = Keys.PageDown;
            map[WinFormsKeys.End] = Keys.End;
            map[WinFormsKeys.Home] = Keys.Home;
            map[WinFormsKeys.Left] = Keys.Left;
            map[WinFormsKeys.Up] = Keys.Up;
            map[WinFormsKeys.Right] = Keys.Right;
            map[WinFormsKeys.Down] = Keys.Down;
            map[WinFormsKeys.Select] = Keys.Select;
            map[WinFormsKeys.Print] = Keys.Print;
            map[WinFormsKeys.Execute] = Keys.Execute;
            map[WinFormsKeys.PrintScreen] = Keys.PrintScreen;
            map[WinFormsKeys.Snapshot] = Keys.Snapshot;
            map[WinFormsKeys.Insert] = Keys.Insert;
            map[WinFormsKeys.Delete] = Keys.Delete;
            map[WinFormsKeys.Help] = Keys.Help;
            map[WinFormsKeys.D0] = Keys.D0;
            map[WinFormsKeys.D1] = Keys.D1;
            map[WinFormsKeys.D2] = Keys.D2;
            map[WinFormsKeys.D3] = Keys.D3;
            map[WinFormsKeys.D4] = Keys.D4;
            map[WinFormsKeys.D5] = Keys.D5;
            map[WinFormsKeys.D6] = Keys.D6;
            map[WinFormsKeys.D7] = Keys.D7;
            map[WinFormsKeys.D8] = Keys.D8;
            map[WinFormsKeys.D9] = Keys.D9;
            map[WinFormsKeys.A] = Keys.A;
            map[WinFormsKeys.B] = Keys.B;
            map[WinFormsKeys.C] = Keys.C;
            map[WinFormsKeys.D] = Keys.D;
            map[WinFormsKeys.E] = Keys.E;
            map[WinFormsKeys.F] = Keys.F;
            map[WinFormsKeys.G] = Keys.G;
            map[WinFormsKeys.H] = Keys.H;
            map[WinFormsKeys.I] = Keys.I;
            map[WinFormsKeys.J] = Keys.J;
            map[WinFormsKeys.K] = Keys.K;
            map[WinFormsKeys.L] = Keys.L;
            map[WinFormsKeys.M] = Keys.M;
            map[WinFormsKeys.N] = Keys.N;
            map[WinFormsKeys.O] = Keys.O;
            map[WinFormsKeys.P] = Keys.P;
            map[WinFormsKeys.Q] = Keys.Q;
            map[WinFormsKeys.R] = Keys.R;
            map[WinFormsKeys.S] = Keys.S;
            map[WinFormsKeys.T] = Keys.T;
            map[WinFormsKeys.U] = Keys.U;
            map[WinFormsKeys.V] = Keys.V;
            map[WinFormsKeys.W] = Keys.W;
            map[WinFormsKeys.X] = Keys.X;
            map[WinFormsKeys.Y] = Keys.Y;
            map[WinFormsKeys.Z] = Keys.Z;
            map[WinFormsKeys.LWin] = Keys.LeftWin;
            map[WinFormsKeys.RWin] = Keys.RightWin;
            map[WinFormsKeys.Apps] = Keys.Apps;
            map[WinFormsKeys.Sleep] = Keys.Sleep;
            map[WinFormsKeys.NumPad0] = Keys.NumPad0;
            map[WinFormsKeys.NumPad1] = Keys.NumPad1;
            map[WinFormsKeys.NumPad2] = Keys.NumPad2;
            map[WinFormsKeys.NumPad3] = Keys.NumPad3;
            map[WinFormsKeys.NumPad4] = Keys.NumPad4;
            map[WinFormsKeys.NumPad5] = Keys.NumPad5;
            map[WinFormsKeys.NumPad6] = Keys.NumPad6;
            map[WinFormsKeys.NumPad7] = Keys.NumPad7;
            map[WinFormsKeys.NumPad8] = Keys.NumPad8;
            map[WinFormsKeys.NumPad9] = Keys.NumPad9;
            map[WinFormsKeys.Multiply] = Keys.Multiply;
            map[WinFormsKeys.Add] = Keys.Add;
            map[WinFormsKeys.Separator] = Keys.Separator;
            map[WinFormsKeys.Subtract] = Keys.Subtract;
            map[WinFormsKeys.Decimal] = Keys.Decimal;
            map[WinFormsKeys.Divide] = Keys.Divide;
            map[WinFormsKeys.F1] = Keys.F1;
            map[WinFormsKeys.F2] = Keys.F2;
            map[WinFormsKeys.F3] = Keys.F3;
            map[WinFormsKeys.F4] = Keys.F4;
            map[WinFormsKeys.F5] = Keys.F5;
            map[WinFormsKeys.F6] = Keys.F6;
            map[WinFormsKeys.F7] = Keys.F7;
            map[WinFormsKeys.F8] = Keys.F8;
            map[WinFormsKeys.F9] = Keys.F9;
            map[WinFormsKeys.F10] = Keys.F10;
            map[WinFormsKeys.F11] = Keys.F11;
            map[WinFormsKeys.F12] = Keys.F12;
            map[WinFormsKeys.F13] = Keys.F13;
            map[WinFormsKeys.F14] = Keys.F14;
            map[WinFormsKeys.F15] = Keys.F15;
            map[WinFormsKeys.F16] = Keys.F16;
            map[WinFormsKeys.F17] = Keys.F17;
            map[WinFormsKeys.F18] = Keys.F18;
            map[WinFormsKeys.F19] = Keys.F19;
            map[WinFormsKeys.F20] = Keys.F20;
            map[WinFormsKeys.F21] = Keys.F21;
            map[WinFormsKeys.F22] = Keys.F22;
            map[WinFormsKeys.F23] = Keys.F23;
            map[WinFormsKeys.F24] = Keys.F24;
            map[WinFormsKeys.NumLock] = Keys.NumLock;
            map[WinFormsKeys.Scroll] = Keys.Scroll;
            map[WinFormsKeys.LShiftKey] = Keys.LeftShift;
            map[WinFormsKeys.RShiftKey] = Keys.RightShift;
            map[WinFormsKeys.LControlKey] = Keys.LeftCtrl;
            map[WinFormsKeys.RControlKey] = Keys.RightCtrl;
            map[WinFormsKeys.LMenu] = Keys.LeftAlt;
            map[WinFormsKeys.RMenu] = Keys.RightAlt;
            map[WinFormsKeys.BrowserBack] = Keys.BrowserBack;
            map[WinFormsKeys.BrowserForward] = Keys.BrowserForward;
            map[WinFormsKeys.BrowserRefresh] = Keys.BrowserRefresh;
            map[WinFormsKeys.BrowserStop] = Keys.BrowserStop;
            map[WinFormsKeys.BrowserSearch] = Keys.BrowserSearch;
            map[WinFormsKeys.BrowserFavorites] = Keys.BrowserFavorites;
            map[WinFormsKeys.BrowserHome] = Keys.BrowserHome;
            map[WinFormsKeys.VolumeMute] = Keys.VolumeMute;
            map[WinFormsKeys.VolumeDown] = Keys.VolumeDown;
            map[WinFormsKeys.VolumeUp] = Keys.VolumeUp;
            map[WinFormsKeys.MediaNextTrack] = Keys.MediaNextTrack;
            map[WinFormsKeys.MediaPreviousTrack] = Keys.MediaPreviousTrack;
            map[WinFormsKeys.MediaStop] = Keys.MediaStop;
            map[WinFormsKeys.MediaPlayPause] = Keys.MediaPlayPause;
            map[WinFormsKeys.LaunchMail] = Keys.LaunchMail;
            map[WinFormsKeys.SelectMedia] = Keys.SelectMedia;
            map[WinFormsKeys.LaunchApplication1] = Keys.LaunchApplication1;
            map[WinFormsKeys.LaunchApplication2] = Keys.LaunchApplication2;
            map[WinFormsKeys.Oem1] = Keys.Oem1;
            map[WinFormsKeys.OemSemicolon] = Keys.OemSemicolon;
            map[WinFormsKeys.Oemplus] = Keys.OemPlus;
            map[WinFormsKeys.Oemcomma] = Keys.OemComma;
            map[WinFormsKeys.OemMinus] = Keys.OemMinus;
            map[WinFormsKeys.OemPeriod] = Keys.OemPeriod;
            map[WinFormsKeys.Oem2] = Keys.Oem2;
            map[WinFormsKeys.OemQuestion] = Keys.OemQuestion;
            map[WinFormsKeys.Oem3] = Keys.Oem3;
            map[WinFormsKeys.Oemtilde] = Keys.OemTilde;
            map[WinFormsKeys.Oem4] = Keys.Oem4;
            map[WinFormsKeys.OemOpenBrackets] = Keys.OemOpenBrackets;
            map[WinFormsKeys.Oem5] = Keys.Oem5;
            map[WinFormsKeys.OemPipe] = Keys.OemPipe;
            map[WinFormsKeys.Oem6] = Keys.Oem6;
            map[WinFormsKeys.OemCloseBrackets] = Keys.OemCloseBrackets;
            map[WinFormsKeys.Oem7] = Keys.Oem7;
            map[WinFormsKeys.OemQuotes] = Keys.OemQuotes;
            map[WinFormsKeys.Oem8] = Keys.Oem8;
            map[WinFormsKeys.Oem102] = Keys.Oem102;
            map[WinFormsKeys.OemBackslash] = Keys.OemBackslash;
            map[WinFormsKeys.Attn] = Keys.Attn;
            map[WinFormsKeys.Crsel] = Keys.CrSel;
            map[WinFormsKeys.Exsel] = Keys.ExSel;
            map[WinFormsKeys.EraseEof] = Keys.EraseEof;
            map[WinFormsKeys.Play] = Keys.Play;
            map[WinFormsKeys.Zoom] = Keys.Zoom;
            map[WinFormsKeys.NoName] = Keys.NoName;
            map[WinFormsKeys.Pa1] = Keys.Pa1;
            map[WinFormsKeys.OemClear] = Keys.OemClear;
            return map;
        }
    }
}
#endif
