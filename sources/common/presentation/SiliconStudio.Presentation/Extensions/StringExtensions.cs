// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System.Collections.Generic;
using System.Linq;

namespace SiliconStudio.Presentation.Extensions
{
    public static class StringExtensions
    {
        public static List<string> CamelCaseSplit(this string str)
        {
            var result = new List<string>();
            int wordStart = 0;
            int wordLength = 0;
            char prevChar = '\0';

            foreach (char currentChar in str)
            {
                if (prevChar != '\0')
                {
                    // Split white spaces
                    if (char.IsWhiteSpace(currentChar))
                    {
                        var word = str.Substring(wordStart, wordLength);
                        result.Add(word);
                        wordStart += wordLength;
                        wordLength = 0;
                    }

                    // aA -> split between a and A
                    if (char.IsLower(prevChar) && char.IsUpper(currentChar))
                    {
                        var word = str.Substring(wordStart, wordLength);
                        result.Add(word);
                        wordStart += wordLength;
                        wordLength = 0;
                    }
                    // This will manage abbreviation words that does not contain lower case character: ABCDef should split into ABC and Def
                    if (char.IsUpper(prevChar) && char.IsLower(currentChar) && wordLength > 1)
                    {
                        var word = str.Substring(wordStart, wordLength - 1);
                        result.Add(word);
                        wordStart += wordLength - 1;
                        wordLength = 1;
                    }
                }
                ++wordLength;
                prevChar = currentChar;
            }

            result.Add(str.Substring(wordStart, wordLength));

            return result.Select(x => x.Trim()).Where(x => x.Length > 0).ToList();
        }
    }
}
