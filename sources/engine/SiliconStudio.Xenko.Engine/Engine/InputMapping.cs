using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SiliconStudio.Core;
using SiliconStudio.Core.Serialization;
using SiliconStudio.Core.Serialization.Contents;
using SiliconStudio.Xenko.Input;
using SiliconStudio.Xenko.Input.Data;

namespace SiliconStudio.Xenko.Engine
{
    [DataContract]
    public class InputBinding
    {
        [DataMember(0)]
        public List<IVirtualButtonDesc> DefaultMappings { get; set; }
    }

    [DataContract]
    [DataSerializerGlobal(typeof(ReferenceSerializer<InputMapping>), Profile = "Content")]
    [ContentSerializer(typeof(DataContentSerializer<InputMapping>))]
    public class InputMapping : ComponentBase
    {
        [DataMember(0)]
        public List<InputBinding> Bindings { get; set; }
        
        [DataMemberIgnore]
        internal InputMapper InputMapper;
    }
}
