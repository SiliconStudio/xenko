// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System.Xml.Linq;
namespace SiliconStudio.Xenko.ProjectGenerator
{
    public interface IProjectProcessor
    {
        void Process(ProjectProcessorContext context);
    }
}