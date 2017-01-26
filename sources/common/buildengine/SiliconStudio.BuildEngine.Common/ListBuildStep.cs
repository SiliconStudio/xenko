// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace SiliconStudio.BuildEngine
{
    [Description("Step list")]
    public class ListBuildStep : EnumerableBuildStep, IList<BuildStep>
    {
        private readonly List<BuildStep> children;

        public ListBuildStep()
            : base(new List<BuildStep>())
        {
            children = (List<BuildStep>)Steps;
        }

        public ListBuildStep(IEnumerable<BuildStep> steps)
            : base(new List<BuildStep>(steps))
        {
            children = (List<BuildStep>)Steps;
        }

        public override BuildStep Clone()
        {
            return new ListBuildStep(children.Select(x => x.Clone()));
        }

        /// <inheritdoc/>
        public override string ToString()
        {
 	         return "Build step list (" + Count + " items)";
        }

        /// <inheritdoc/>
        public int Count => children.Count;

        /// <inheritdoc/>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <inheritdoc/>
        public IEnumerator<BuildStep> GetEnumerator()
        {
            return children.GetEnumerator();
        }

        public CommandBuildStep Add(Command command)
        {
            var commandBuildStep = new CommandBuildStep(command);
            Add(commandBuildStep);
            return commandBuildStep;
        }

        public IEnumerable<CommandBuildStep> Add(IEnumerable<Command> commands)
        {
            var commandBuildSteps = commands.Select(x => new CommandBuildStep(x) ).ToArray();
            foreach (var commandBuildStep in commandBuildSteps)
            {
                Add(commandBuildStep);
            }
            return commandBuildSteps;
        }

        /// <inheritdoc/>
        public void Add(BuildStep buildStep)
        {
            if (Status != ResultStatus.NotProcessed)
                throw new InvalidOperationException("Unable to add a build step to an already processed ListBuildStep.");

            buildStep.Parent = this;
            children.Add(buildStep);
        }

        /// <inheritdoc/>
        public void Clear()
        {
            children.Clear();
        }

        /// <inheritdoc/>
        public bool Contains(BuildStep item)
        {
            return children.Contains(item);
        }

        /// <inheritdoc/>
        public void CopyTo(BuildStep[] array, int arrayIndex)
        {
            children.CopyTo(array, arrayIndex);
        }

        /// <inheritdoc/>
        public bool Remove(BuildStep item)
        {
            item.Parent = null;
            return children.Remove(item);
        }

        /// <inheritdoc/>
        public int IndexOf(BuildStep item)
        {
            return children.IndexOf(item);
        }

        /// <inheritdoc/>
        public void Insert(int index, BuildStep item)
        {
            item.Parent = this;
            children.Insert(index, item);
        }

        /// <inheritdoc/>
        public void RemoveAt(int index)
        {
           children[index].Parent = null;
           children.RemoveAt(index);
        }

        /// <inheritdoc/>
        public BuildStep this[int index]
        {
            get { return children[index]; }
            set { children[index] = value; value.Parent = this; }
        }

        /// <inheritdoc/>
        bool ICollection<BuildStep>.IsReadOnly => ((IList<BuildStep>)children).IsReadOnly;
    }
}