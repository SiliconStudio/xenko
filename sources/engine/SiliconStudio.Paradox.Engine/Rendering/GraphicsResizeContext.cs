// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Linq;
using System.Collections.Generic;

using SiliconStudio.Paradox.Games;
using SiliconStudio.Core;

namespace SiliconStudio.Paradox.Rendering
{
    /// <summary>
    /// Keep track of actions to execute when GraphicsDevice is resized.
    /// </summary>
    public class GraphicsResizeContext
    {
        private List<ParameterLocation> currentParameterLocations = new List<ParameterLocation>();
        private List<ResizeAction> resizeActions = new List<ResizeAction>();
        private ResizeAction currentAction;

        /// <summary>
        /// Execute the resize action and register it for later reexecution.
        /// </summary>
        /// <param name="action">The action.</param>
        public void SetupResize(Action<GraphicsResizeContext> action)
        {
            // TODO: Unregister SetupResize callbacks
            // Setup this action and its associated registered resources.
            resizeActions.Add(new ResizeAction { Action = action, ParameterLocations = currentParameterLocations });
            currentAction = resizeActions.Last();

            currentParameterLocations = new List<ParameterLocation>();

            // Execute the action
            action(this);

            currentAction = null;
        }

        /// <summary>
        /// First step of the resize process: it sets to null all the resources assigned to a group through SetWithResize.
        /// Later, EndResize() should be called to execute second step of this system.
        /// This two steps system should help avoiding unecessary memory peaks and reduce GPU memory fragmentation.
        /// </summary>
        public void StartResize()
        {
            // Erase all the resources assigned through SetWithResize.
            foreach (var resizeAction in resizeActions)
            {
                if (!resizeAction.ParameterLocations.All(x => (IReferencable)x.ParameterCollection.GetObject(x.ParameterKey) == x.Value))
                {
                    throw new InvalidOperationException("Attempted to resize resource that changed.");
                }

                foreach (var parameterLocation in resizeAction.ParameterLocations)
                {
                    parameterLocation.ParameterCollection.SetObject(parameterLocation.ParameterKey, null);
                }
                resizeAction.ParameterLocations.Clear();
            }
        }

        /// <summary>
        /// Recreate all the resizable resources.
        /// It reexecute all the resize handlers registered through SetupResize to recreate necessary resources.
        /// </summary>
        public void EndResize()
        {
            // Reexecute the resize handler (in the order they were added)
            foreach (var resizeAction in resizeActions)
            {
                currentAction = resizeAction;
                resizeAction.Action(this);
            }
            currentAction = null;
        }

        internal void SetWithResize<T>(ParameterCollection parameterCollection, ParameterKey<T> key, T resourceValue) where T : IReferencable
        {
            parameterCollection.Set(key, resourceValue);
            if (currentAction == null)
                throw new InvalidOperationException("Tried to use SetWithResize outside of a SetupResize callback.");

            currentAction.ParameterLocations.Add(new ParameterLocation { ParameterCollection = parameterCollection, ParameterKey = key, Value = resourceValue });
        }

        private class ResizeAction
        {
            public Action<GraphicsResizeContext> Action;
            public List<ParameterLocation> ParameterLocations;
        }

        private struct ParameterLocation
        {
            public ParameterCollection ParameterCollection;
            public ParameterKey ParameterKey;
            public IReferencable Value;
        }
    }
}