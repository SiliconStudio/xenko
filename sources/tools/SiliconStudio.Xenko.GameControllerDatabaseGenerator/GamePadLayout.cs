// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using SiliconStudio.Xenko.Input;

namespace SiliconStudio.Xenko.GameControllerDatabaseGenerator
{
    public partial class GamePadLayout
    {
        public static readonly Dictionary<string, GamePadButton> ButtonKeys = new Dictionary<string, GamePadButton>
        {
            ["a"] = GamePadButton.A,
            ["b"] = GamePadButton.B,
            ["x"] = GamePadButton.X,
            ["y"] = GamePadButton.Y,
            ["dpleft"] = GamePadButton.PadLeft,
            ["dpright"] = GamePadButton.PadRight,
            ["dpup"] = GamePadButton.PadUp,
            ["dpdown"] = GamePadButton.PadDown,
            ["leftshoulder"] = GamePadButton.LeftShoulder,
            ["rightshoulder"] = GamePadButton.RightShoulder,
            ["start"] = GamePadButton.Start,
            ["back"] = GamePadButton.Back,
            ["leftstick"] = GamePadButton.LeftThumb,
            ["rightstick"] = GamePadButton.RightThumb,
        };

        public static readonly Dictionary<string, GamePadAxis> AxisKeys = new Dictionary<string, GamePadAxis>
        {
            ["leftx"] = GamePadAxis.LeftThumbX,
            ["lefty"] = GamePadAxis.LeftThumbY,
            ["rightx"] = GamePadAxis.RightThumbX,
            ["righty"] = GamePadAxis.RightThumbY,
            ["lefttrigger"] = GamePadAxis.LeftTrigger,
            ["righttrigger"] = GamePadAxis.RightTrigger,
        };

        public List<Layout> Layouts;
        public Dictionary<string, List<Layout>> Platforms;

        public GamePadLayout(string sourceFile)
        {
            // Load the source game controller database file
            using (var stream = File.OpenRead(sourceFile))
            {
                Layouts = EnumerateLayouts(stream).ToList();
            }

            Platforms = new Dictionary<string, List<Layout>>();
            foreach (var layout in Layouts)
            {
                List<Layout> platformLayouts;
                string platformString = MapPlatform(layout.Platform);
                if (!Platforms.TryGetValue(platformString, out platformLayouts))
                {
                    platformLayouts = new List<Layout>();
                    Platforms.Add(platformString, platformLayouts);
                }
                platformLayouts.Add(layout);
            }
        }

        static string MapPlatform(string input)
        {
            if (input == "Windows")
                return "SILICONSTUDIO_PLATFORM_WINDOWS";
            if (input == "Linux")
                return "SILICONSTUDIO_PLATFORM_UNIX";
            if (input == "Mac OS X")
                return "SILICONSTUDIO_PLATFORM_MACOS";
            if (input == "android")
                return "SILICONSTUDIO_PLATFORM_ANDROID";
            return "";
        }

        private static KeyValuePair<string, string> ParseKeyValuePair(string pair)
        {
            string[] parts = pair.Split(':');
            if (parts.Length != 2)
                throw new InvalidOperationException("Not a key value pair");
            return new KeyValuePair<string, string>(parts[0], parts[1]);
        }

        private static IEnumerable<Layout> EnumerateLayouts(FileStream stream)
        {
            using (var reader = new StreamReader(stream))
            {
                int index = 0;
                while (true)
                {
                    string line = reader.ReadLine();
                    if (line == null) break;
                    string[] parts = line.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

                    if (parts.Length <= 2)
                        continue;

                    // Manually construct a Guid since the one in the database is stored in byte order and not Guid.ToString() order
                    byte[] pidBytes = new byte[16];
                    for (int i = 0; i < 16; i++)
                    {
                        string bytePart = parts[0].Substring(i * 2, 2);
                        pidBytes[i] = byte.Parse(bytePart, NumberStyles.HexNumber);
                    }

                    Guid productId = new Guid(pidBytes);
                    string deviceName = parts[1];

                    Layout layout = new Layout();
                    layout.Platform = "all";

                    Dictionary<string, List<string>> options = new Dictionary<string, List<string>>();
                    for (int i = 2; i < parts.Length; i++)
                    {
                        var pair = ParseKeyValuePair(parts[i]);

                        if (pair.Key == "platform")
                        {
                            layout.Platform = pair.Value;
                            continue;
                        }

                        List<string> list;
                        if (!options.TryGetValue(pair.Key, out list))
                        {

                            list = new List<string>();
                            options[pair.Key] = list;
                        }

                        list.Add(pair.Value);
                    }

                    layout.Index = index++;
                    layout.Name = deviceName;
                    layout.Guid = productId.ToString();

                    foreach (var p in options)
                    {
                        foreach (var v in p.Value)
                        {
                            layout.AddMapping(p.Key, v);
                        }
                    }

                    yield return layout;
                }
            }
        }
    }
}