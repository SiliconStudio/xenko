using System.Collections.Generic;
using SiliconStudio.Xenko.Graphics;

namespace SiliconStudio.Xenko.Graphics
{
    public class EffectDescriptorSetReflection
    {
        internal List<LayoutEntry> Layouts { get; } = new List<LayoutEntry>();

        public DescriptorSetLayoutBuilder GetLayout(string name)
        {
            foreach (var entry in Layouts)
            {
                if (entry.Name == name)
                    return entry.Layout;
            }

            return null;
        }

        public int GetLayoutIndex(string name)
        {
            for (int index = 0; index < Layouts.Count; index++)
            {
                if (Layouts[index].Name == name)
                    return index;
            }

            return -1;
        }

        public void AddLayout(string descriptorSetName, DescriptorSetLayoutBuilder descriptorSetLayoutBuilder)
        {
            Layouts.Add(new LayoutEntry(descriptorSetName, descriptorSetLayoutBuilder));
        }

        internal struct LayoutEntry
        {
            public string Name;
            public DescriptorSetLayoutBuilder Layout;

            public LayoutEntry(string name, DescriptorSetLayoutBuilder layout)
            {
                Name = name;
                Layout = layout;
            }
        }
    }
}