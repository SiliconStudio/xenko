// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SiliconStudio.Core.Serialization.Assets
{
    partial class ContentManager
    {
        // Used internally for Garbage Collection
        // Allocate once and reuse collection for every GC
        private Stack<AssetReference> stack = new Stack<AssetReference>();
        private uint nextCollectIndex;

        /// <summary>
        /// Increments reference count of an <see cref="AssetReference"/>.
        /// </summary>
        /// <param name="assetReference"></param>
        /// <param name="publicReference"></param>
        internal void IncrementReference(AssetReference assetReference, bool publicReference)
        {
            if (publicReference)
            {
                assetReference.PublicReferenceCount++;
            }
            else
            {
                assetReference.PrivateReferenceCount++;
            }
        }

        /// <summary>
        /// Decrements reference count of an <see cref="AssetReference"/>.
        /// </summary>
        /// <param name="assetReference"></param>
        /// <param name="publicReference"></param>
        internal void DecrementReference(AssetReference assetReference, bool publicReference)
        {
            int referenceCount;
            if (publicReference)
            {
                if (assetReference.PublicReferenceCount <= 0)
                    throw new InvalidOperationException("Cannot release an object that doesn't have active public references. Load/Unload pairs must match.");

                referenceCount = --assetReference.PublicReferenceCount + assetReference.PrivateReferenceCount;
            }
            else
            {
                if (assetReference.PrivateReferenceCount <= 0)
                    throw new InvalidOperationException("Cannot release an object that doesn't have active private references. This is either due to non-matching Load/Unload pairs or an engine internal error.");
             
                referenceCount = --assetReference.PrivateReferenceCount + assetReference.PublicReferenceCount;
            }

            if (referenceCount == 0)
            {
                // Free the object itself
                ReleaseAsset(assetReference);

                // Free all its referenced objects
                foreach (var reference in assetReference.References)
                {
                    DecrementReference(reference, false);
                }
            }
            else if (publicReference && assetReference.PublicReferenceCount == 0)
            {
                // If there is no more public reference but object is still alive, let's kick a cycle GC
                CollectUnreferencedCycles();
            }
        }

        /// <summary>
        /// Releases an asset.
        /// </summary>
        /// <param name="assetReference">The asset reference.</param>
        private void ReleaseAsset(AssetReference assetReference)
        {
            var referencable = assetReference.Object as IReferencable;
            if (referencable != null)
            {
                referencable.Release();
            }
            else
            {
                var disposable = assetReference.Object as IDisposable;
                if (disposable != null)
                {
                    disposable.Dispose();
                }
            }

            // Remove AssetReference from loaded assets.
            var oldPrev = assetReference.Prev;
            var oldNext = assetReference.Next;
            if (oldPrev != null)
                oldPrev.Next = oldNext;
            if (oldNext != null)
                oldNext.Prev = oldPrev;

            if (oldPrev == null)
            {
                if (oldNext == null)
                    LoadedAssetUrls.Remove(assetReference.Url);
                else
                    LoadedAssetUrls[assetReference.Url] = oldNext;
            }
            LoadedAssetReferences.Remove(assetReference.Object);

            assetReference.Object = null;
        }

        internal void CollectUnreferencedCycles()
        {
            // Push everything on the stack
            var currentCollectIndex = nextCollectIndex++;
            foreach (var asset in LoadedAssetUrls)
            {
                var currentAsset = asset.Value;
                do
                {
                    if (asset.Value.PublicReferenceCount > 0)
                        stack.Push(asset.Value);
                    currentAsset = currentAsset.Next;
                }
                while (currentAsset != null);
            }

            // Until stack is empty, collect references and push them on the stack
            while (stack.Count > 0)
            {
                var v = stack.Pop();

                // We use CollectIndex to know if object has already been processed during current collection
                var collectIndex = v.CollectIndex;
                if (collectIndex != currentCollectIndex)
                {
                    v.CollectIndex = currentCollectIndex;
                    foreach (var reference in v.References)
                    {
                        if (reference.CollectIndex != currentCollectIndex)
                            stack.Push(reference);
                    }
                }
            }

            // Collect objects that are not referenceable.
            // Reuse stack
            // TODO: Use collections where you can iterate and remove at the same time?
            foreach (var asset in LoadedAssetUrls)
            {
                var currentAsset = asset.Value;
                do
                {
                    if (asset.Value.CollectIndex != currentCollectIndex)
                    {
                        stack.Push(asset.Value);
                    }
                    currentAsset = currentAsset.Next;
                }
                while (currentAsset != null);
            }

            // Release those objects
            // Note: order of release might be unexpected (i.e. if A ref B, B might be released before A)
            // We don't really have a choice if there is cycle anyway, but still user could have reference himself to prevent or enforce this order if it's really important.
            foreach (var assetReference in stack)
            {
                ReleaseAsset(assetReference);
            }
        }
    }
}
