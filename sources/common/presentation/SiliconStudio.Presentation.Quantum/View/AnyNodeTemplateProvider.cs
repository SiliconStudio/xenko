// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
namespace SiliconStudio.Presentation.Quantum.View
{
    /// <summary>
    /// An implementation of the <see cref="ObservableNodeTemplateProvider"/> that matches every <see cref="IObservableNode"/>.
    /// </summary>
    public class AnyNodeTemplateProvider : ObservableNodeTemplateProvider
    {
        /// <inheritdoc/>
        public override string Name { get { return "AnyNodeTemplateProvider"; } }

        /// <inheritdoc/>
        public override bool MatchNode(IObservableNode node)
        {
            return true;
        }
    }
}