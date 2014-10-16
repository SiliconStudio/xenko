// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.IO;

namespace SiliconStudio.Paradox
{
    public static class PrettyPrinter
    {
        static string EscapeString(string s)
        {
            return s.Replace("\"", "\\\"");
        }

        static void Print(TextWriter output, string s)
        {
            output.Write(s);
        }
        static void EscapeChar(TextWriter output, char c)
        {
            if (c == '\'')
            {
                output.Write("'\\''");
                return;
            }
            if (c > 32)
            {
                output.Write("'{0}'", c);
                return;
            }
            switch (c)
            {
                case '\a':
                    output.Write("'\\a'");
                    break;

                case '\b':
                    output.Write("'\\b'");
                    break;

                case '\n':
                    output.Write("'\\n'");
                    break;

                case '\v':
                    output.Write("'\\v'");
                    break;

                case '\r':
                    output.Write("'\\r'");
                    break;

                case '\f':
                    output.Write("'\\f'");
                    break;

                case '\t':
                    output.Write("'\\t");
                    break;

                default:
                    output.Write("'\\x{0:x}", (int)c);
                    break;
            }
        }

        public static void PrettyPrint(TextWriter output, object result)
        {
            if (result == null)
            {
                Print(output, "null");
                return;
            }

            if (result is Array)
            {
                Array a = (Array)result;

                Print(output, "{ ");
                int top = a.GetUpperBound(0);
                for (int i = a.GetLowerBound(0); i <= top; i++)
                {
                    PrettyPrint(output, a.GetValue(i));
                    if (i != top)
                        Print(output, ", ");
                }
                Print(output, " }");
            }
            else if (result is bool)
            {
                if ((bool)result)
                    Print(output, "true");
                else
                    Print(output, "false");
            }
            else if (result is string)
            {
                Print(output, String.Format("\"{0}\"", EscapeString((string)result)));
            }
            else if (result is System.Collections.IDictionary)
            {
                var dict = (System.Collections.IDictionary)result;
                int top = dict.Count, count = 0;

                Print(output, "{");
                foreach (System.Collections.DictionaryEntry entry in dict)
                {
                    count++;
                    Print(output, "{ ");
                    PrettyPrint(output, entry.Key);
                    Print(output, ", ");
                    PrettyPrint(output, entry.Value);
                    if (count != top)
                        Print(output, " }, ");
                    else
                        Print(output, " }");
                }
                Print(output, "}");
            }
            else if (result is System.Collections.IEnumerable)
            {
                int i = 0;
                Print(output, "{ ");
                foreach (object item in (System.Collections.IEnumerable)result)
                {
                    if (i++ != 0)
                        Print(output, ", ");

                    PrettyPrint(output, item);
                }
                Print(output, " }");
            }
            else if (result is char)
            {
                EscapeChar(output, (char)result);
            }
            else
            {
                Print(output, result.ToString());
            }
        }
    }
}