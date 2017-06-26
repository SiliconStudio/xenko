// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SiliconStudio.BuildEngine
{
    public interface IParamFilter
    {
        Action<Command, object> Assigner { get; }
        Type CommandType { get; }
        IEnumerable Filter(object param);
    }

    public abstract class ParamFilter<TIn, TOut, TCommand> : IParamFilter where TCommand : Command
    {
        public Action<Command, object> Assigner { get; protected set; }

        public Type CommandType { get; protected set; }

        protected ParamFilter(Action<TCommand, TOut> assigner)
        {
            CommandType = typeof(TCommand);
            SetAssigner(assigner);
        }

        public IEnumerable Filter(object param)
        {
            return Filter((TIn)param);
        }

        protected void SetAssigner(Action<TCommand, TOut> assigner)
        {
            Assigner = (x, y) => assigner((TCommand)x, (TOut)y);
        }

        public abstract IEnumerable<TOut> Filter(TIn input);
    }

    public class FilePatternFilter<TCommand> : ParamFilter<string, string, TCommand> where TCommand : Command
    {
        public FilePatternFilter(Action<TCommand, string> assigner)
            : base(assigner)
        {

        }

        public override IEnumerable<string> Filter(string pattern)
        {
            if (pattern.Contains("/") || pattern.Contains("\\"))
            {
                string path = pattern.Substring(0, Math.Max(pattern.LastIndexOf('/'), pattern.LastIndexOf('\\')));
                string filePattern = path.Length < pattern.Length ? pattern.Substring(path.Length + 1) : "";
                return Directory.EnumerateFiles(path, filePattern);
            }
            return Directory.EnumerateFiles(pattern);
        }
    }
}
