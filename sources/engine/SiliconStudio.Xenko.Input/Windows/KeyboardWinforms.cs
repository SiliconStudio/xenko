// Copyright (c) 2016-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

#if SILICONSTUDIO_PLATFORM_WINDOWS_DESKTOP && (SILICONSTUDIO_XENKO_UI_WINFORMS || SILICONSTUDIO_XENKO_UI_WPF)
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using SiliconStudio.Xenko.Games;

namespace SiliconStudio.Xenko.Input
{
    internal class KeyboardWinforms : KeyboardDeviceBase, ITextInputDevice, IDisposable
    {
        private readonly Control uiControl;
        private readonly List<TextInputEvent> textEvents = new List<TextInputEvent>();

        // Hack that uses a text box to receive IME text input
        private readonly RichTextBox richTextBox;
        private readonly IntPtr oldWndProc;

        private bool textInputEnabled;
        private Win32Native.WndProc wndProcDelegate;

        public KeyboardWinforms(InputSourceWinforms source, Control uiControl)
        {
            Source = source;
            this.uiControl = uiControl;

            richTextBox = new RichTextBox
            {
                Location = new Point(-100, -100),
                Size = new Size(80, 80)
            };
            
            // Assign custom window procedure to this text box
            wndProcDelegate = WndProc;
            var windowProc = Marshal.GetFunctionPointerForDelegate(wndProcDelegate);
            oldWndProc = Win32Native.SetWindowLong(richTextBox.Handle, Win32Native.WindowLongType.WndProc, windowProc);
        }

        public void Dispose()
        {
            richTextBox?.Dispose();
        }
         
        public override string Name => "Windows Keyboard";

        public override Guid Id => new Guid("027cf994-681f-4ed5-b38f-ce34fc295b8f");

        public override IInputSource Source { get; }

        public override void Update(List<InputEvent> inputEvents)
        {
            base.Update(inputEvents);

            inputEvents.AddRange(textEvents);
            textEvents.Clear();
        }

        internal void HandleKeyDown(System.Windows.Forms.Keys winFormsKey)
        {
            // Translate from windows key enum to Xenko key enum
            Keys xenkoKey;
            if (WinKeys.mapKeys.TryGetValue(winFormsKey, out xenkoKey) && xenkoKey != Keys.None)
            {
                HandleKeyDown(xenkoKey);
            }
        }

        internal void HandleKeyUp(System.Windows.Forms.Keys winFormsKey)
        {
            // Translate from windows key enum to Xenko key enum
            Keys xenkoKey;
            if (WinKeys.mapKeys.TryGetValue(winFormsKey, out xenkoKey) && xenkoKey != Keys.None)
            {
                HandleKeyUp(xenkoKey);
            }
        }

        public void EnabledTextInput()
        {
            if (!textInputEnabled)
            {
                uiControl.Controls.Add(richTextBox);
                textInputEnabled = true;
                richTextBox.TextChanged += RichTextBoxOnTextChanged;
                richTextBox.Focus();
            }
        }

        private void RichTextBoxOnTextChanged(object sender, EventArgs eventArgs)
        {
            // Take all text inserted into the text box and send it as a text event instead
            if (richTextBox.Text.Length > 0)
            {
                // Filter out characters that do not belong in text input
                string inputString = richTextBox.Text;
                inputString = inputString.Replace("\r", "").Replace("\n", "").Replace("\t", "");

                if (inputString.Length > 0)
                {
                    var compEvent = InputEventPool<TextInputEvent>.GetOrCreate(this);
                    compEvent.Type = TextInputEventType.Input;
                    compEvent.Text = inputString;
                    compEvent.CompositionStart = richTextBox.SelectionStart;
                    compEvent.CompositionLength = richTextBox.SelectionLength;
                    textEvents.Add(compEvent);
                }
            }

            richTextBox.Text = "";
        }

        private IntPtr WndProc(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam)
        {
            switch (msg)
            {
                case 0x118: // Cursor blink message
                    return new IntPtr(0);

                case Win32Native.WM_KEYDOWN:
                case Win32Native.WM_SYSKEYDOWN:
                    if (richTextBox.TextLength == 0)
                    {
                        var virtualKey = (System.Windows.Forms.Keys)wParam.ToInt64();
                        if (virtualKey == System.Windows.Forms.Keys.Back || 
                            virtualKey == System.Windows.Forms.Keys.Left ||
                            virtualKey == System.Windows.Forms.Keys.Right ||
                            virtualKey == System.Windows.Forms.Keys.Delete || 
                            virtualKey == System.Windows.Forms.Keys.Home ||
                            virtualKey == System.Windows.Forms.Keys.End ||
                            virtualKey == System.Windows.Forms.Keys.Up ||
                            virtualKey == System.Windows.Forms.Keys.Down)
                            return new IntPtr(0); // Swallow some keys when the text box is empty to prevent ding sound
                    }
                    break;

                case Win32Native.WM_KEYUP:
                case Win32Native.WM_SYSKEYUP:
                    break;

                case Win32Native.WM_IME_COMPOSITION:
                    OnComposition(hWnd, (int)lParam);
                    break;

                case Win32Native.WM_NCPAINT:
                case Win32Native.WM_PAINT:
                    var paintStruct = new Win32Native.PAINTSTRUCT();
                    Win32Native.BeginPaint(hWnd, ref paintStruct);
                    Win32Native.EndPaint(hWnd, ref paintStruct);
                    return new IntPtr(0); // Don't paint the control
            }
            return Win32Native.CallWindowProc(oldWndProc, hWnd, msg, wParam, lParam);
        }
        
        private unsafe string GetCompositionString(IntPtr context, int type)
        {
            int len = Win32Native.ImmGetCompositionString(context, type, IntPtr.Zero, 0);
            byte[] data = new byte[len];

            fixed (byte* dataPtr = data)
            {
                Win32Native.ImmGetCompositionString(context, type, new IntPtr(dataPtr), len);
            }

            return Encoding.Unicode.GetString(data);
        }

        private void OnComposition(IntPtr hWnd, int lParam)
        {
            TextInputEvent compEvent;
            if (lParam == 0)
            {
                // Clear composition
                compEvent = InputEventPool<TextInputEvent>.GetOrCreate(this);
                compEvent.Text = "";
                compEvent.CompositionStart = 0;
                compEvent.CompositionLength = 0;
                compEvent.Type = TextInputEventType.Composition;
                textEvents.Add(compEvent);
                return;
            }

            if ((lParam & Win32Native.GCS_COMPSTR) != 0)
            {
                // Update the composition string
                var context = Win32Native.ImmGetContext(hWnd);

                var compString = GetCompositionString(context, Win32Native.GCS_COMPSTR);

                compEvent = InputEventPool<TextInputEvent>.GetOrCreate(this);
                compEvent.Type = TextInputEventType.Composition;
                compEvent.Text = compString;
                compEvent.CompositionStart = 0;
                compEvent.CompositionLength = 0;
                textEvents.Add(compEvent);

                Win32Native.ImmReleaseContext(hWnd, context);
            }
        }

        public void DisableTextInput()
        {
            if (textInputEnabled)
            {
                richTextBox.TextChanged -= RichTextBoxOnTextChanged;
                uiControl.Focus();
                uiControl.Controls.Remove(richTextBox);
                textInputEnabled = false;
            }
        }
    }
}

#endif