// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using System.Linq;

namespace SiliconStudio.Presentation.Extensions
{
    public static class StringExtensions
    {
        public const int MaxStackTraceLines = 30;

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

            return result;
        }

        public static string FormatExceptionForMessageBox(Exception exception, bool startWithNewLine)
        {
            // Get the innermost exception.
            while (exception.InnerException != null)
            {
                exception = exception.InnerException;
            }
            var stackTrace = ExtractStackTrace(exception, MaxStackTraceLines);
            return string.Format("{0}{1}{2}{3}", startWithNewLine ? Environment.NewLine : "", exception.Message, Environment.NewLine, stackTrace);
        }

        public static string FormatExceptionForReport(Exception exception)
        {
            var message = string.Format("{0}{1}{2}{3}", exception.Message, Environment.NewLine, ExtractStackTrace(exception), Environment.NewLine);
            var aggregateException = exception as AggregateException;
            if (aggregateException != null)
            {
                message = aggregateException.InnerExceptions.Aggregate(message, (current, innerException) => current + FormatExceptionForReport(exception));
            }

            if (exception.InnerException != null)
            {
                message += FormatExceptionForReport(exception.InnerException);
            }
            return message;
        }

        private static string ExtractStackTrace(Exception exception, int maxLines = -1)
        {
            var stackTraceArray = exception.StackTrace.Split("\r\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                return string.Join(Environment.NewLine, maxLines > 0 ? stackTraceArray.Take(maxLines) : stackTraceArray);
        }
    }
}