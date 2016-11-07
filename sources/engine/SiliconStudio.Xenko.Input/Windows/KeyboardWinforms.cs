// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

#if SILICONSTUDIO_XENKO_UI_WINFORMS
using System;
using System.Collections.Generic;

namespace SiliconStudio.Xenko.Input
{
    public class KeyboardWinforms : KeyboardDeviceBase
    {
        private readonly List<TextInputEvent> textEvents = new List<TextInputEvent>();

        public override string DeviceName => "Windows Keyboard";
        public override Guid Id => new Guid("027cf994-681f-4ed5-b38f-ce34fc295b8f");

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

        internal void HandleChar(char character)
        {
            var textInputEvent = InputEventPool<TextInputEvent>.GetOrCreate(this);
            textInputEvent.Text = new string(character, 1);
            textEvents.Add(textInputEvent);
        }
    }
}
#endif