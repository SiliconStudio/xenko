// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using Android.Content;
using Android.Views;

namespace MonoDroid.Dialog
{
    public class RadioElement : StringElement
    {
        public string Group;
        internal int RadioIdx;

        public RadioElement(string caption, string group)
            : base(caption)
        {
            Group = group;
        }

        public RadioElement(string caption)
            : base(caption)
        {
        }

		public override View GetView(Context context, View convertView, ViewGroup parent)
        {
            if (!(((RootElement)Parent.Parent)._group is RadioGroup))
                throw new Exception("The RootElement's Group is null or is not a RadioGroup");

            return base.GetView(context, convertView, parent);
        }

        public override string Summary()
        {
            return Caption;
        }
    }
}