using System.ComponentModel;
using SiliconStudio.Core;

namespace SiliconStudio.Xenko.Assets.Scripts
{
    [DataContract]
    public class Property : Symbol
    {
        public Property()
        {
        }

        public Property(string type, string name) : base(type, name)
        {
        }

        [DataMember(0)]
        [DefaultValue(Accessibility.Public)]
        public Accessibility Accessibility { get; set; } = Accessibility.Public;

        [DataMember(10)]
        [DefaultValue(false)]
        public bool IsStatic { get; set; }
    }
}