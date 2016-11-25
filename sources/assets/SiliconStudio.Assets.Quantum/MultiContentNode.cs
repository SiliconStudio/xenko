using System;
using System.Collections.Generic;
using SiliconStudio.Quantum.Contents;

namespace SiliconStudio.Assets.Quantum
{
    public sealed class MultiContentNode : AssetNode
    {
        private readonly Dictionary<string, IContent> contents = new Dictionary<string, IContent>();

        public MultiContentNode(string name, IContent content, Guid guid) : base(name, content, guid)
        {
        }

        public bool HasContent(string key)
        {
            return contents.ContainsKey(key);
        }

        public void SetContent(string key, IContent content)
        {
            contents[key] = content;
        }

        public IContent GetContent(string key)
        {
            IContent content;
            contents.TryGetValue(key, out content);
            return content;
        }
    }
}