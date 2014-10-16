// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Windows.Forms;

namespace SiliconStudio.LauncherApp
{
    public class DialogEventArgs : EventArgs
    {
        private readonly string text;
        private readonly string caption;
        private readonly MessageBoxButtons buttons;
        private readonly MessageBoxIcon icon;
        private readonly MessageBoxDefaultButton defaultButton;
        private readonly MessageBoxOptions options;

        //public DialogEventArgs(string text, string caption)
        //    : this(text, caption, MessageBoxButtons.OK, MessageBoxIcon.None, MessageBoxDefaultButton.Button1, 0)
        //{
        //}

        public DialogEventArgs(string text, string caption, MessageBoxButtons buttons, MessageBoxIcon icon, MessageBoxDefaultButton defaultButton, MessageBoxOptions options)
        {
            this.text = text;
            this.caption = caption;
            this.buttons = buttons;
            this.icon = icon;
            this.defaultButton = defaultButton;
            this.options = options;
        }

        public string Text
        {
            get
            {
                return text;
            }
        }

        public string Caption
        {
            get
            {
                return caption;
            }
        }

        public MessageBoxButtons Buttons
        {
            get
            {
                return buttons;
            }
        }

        public MessageBoxIcon Icon
        {
            get
            {
                return icon;
            }
        }

        public MessageBoxDefaultButton DefaultButton
        {
            get
            {
                return defaultButton;
            }
        }

        public MessageBoxOptions Options
        {
            get
            {
                return options;
            }
        }

        public DialogResult Result { get; set; }
    }
}