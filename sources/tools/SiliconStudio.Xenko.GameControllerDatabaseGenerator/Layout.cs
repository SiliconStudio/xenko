// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.Collections.Generic;
using SiliconStudio.Xenko.Input;

namespace SiliconStudio.Xenko.GameControllerDatabaseGenerator
{
    public class Layout
    {
        public int Index;
        public string Name;
        public string Guid;
        public string Platform;
        public List<string> Mappings = new List<string>();

        public void AddMapping(string key, string value)
        {
            if (value.Length == 0)
                return;

            var indexEnd = value.IndexOf('.');
            if (indexEnd < 0)
                indexEnd = value.Length;
            var index = int.Parse(value.Substring(1, indexEnd - 1));

            GamePadButton button;
            if (GamePadLayout.ButtonKeys.TryGetValue(key, out button))
            {
                if (value[0] == 'b')
                    Mappings.Add($"AddButtonToButton({index}, GamePadButton.{button});");
                else if (value[0] == 'a')
                    Mappings.Add($"AddAxisToButton({index}, GamePadButton.{button});");
            }
            GamePadAxis axis;
            if (GamePadLayout.AxisKeys.TryGetValue(key, out axis))
            {
                if (value[0] == 'b')
                    Mappings.Add($"AddButtonToAxis({index}, GamePadAxis.{axis});");
                else if (value[0] == 'a')
                {
                    if(axis == GamePadAxis.LeftThumbY || axis == GamePadAxis.RightThumbY)
                        Mappings.Add($"AddAxisToAxis({index}, GamePadAxis.{axis}, true);");
                    else if(axis == GamePadAxis.LeftTrigger || axis == GamePadAxis.RightTrigger)
                        Mappings.Add($"AddAxisToAxis({index}, GamePadAxis.{axis}, remap: true);");
                    else
                        Mappings.Add($"AddAxisToAxis({index}, GamePadAxis.{axis});");
                }
            }
        }
    }
}