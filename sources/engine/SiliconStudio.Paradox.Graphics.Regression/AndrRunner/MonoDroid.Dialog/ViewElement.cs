// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections;
using System.Collections.Generic;
using Android.Views;

namespace MonoDroid.Dialog
{
    public class ViewElement : IEnumerable<View>
    {
        public ViewElement(object o, View view, bool b)
        {
            
        }

        public IEnumerator<View> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}