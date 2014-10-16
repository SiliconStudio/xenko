// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
namespace MonoDroid.Dialog
{
    /// <summary>
    /// Used by root elements to fetch information when they need to
    /// render a summary (Checkbox count or selected radio group).
    /// </summary>
    public class Group
    {
        public string Key;

        public Group(string key)
        {
            Key = key;
        }
    }

    /// <summary>
    /// Captures the information about mutually exclusive elements in a RootElement
    /// </summary>
    public class RadioGroup : Group
    {
        public int Selected;

        public RadioGroup(string key, int selected)
            : base(key)
        {
            Selected = selected;
        }

        public RadioGroup(int selected)
            : base(null)
        {
            Selected = selected;
        }
    }
}