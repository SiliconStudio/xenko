using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SiliconStudio.Core;
using SiliconStudio.Core.Serialization;
using SiliconStudio.Core.Serialization.Contents;
using SiliconStudio.Paradox.Engine.Design;

namespace SiliconStudio.Paradox.Engine
{
    [DataContract]
    [ContentSerializer(typeof(DataContentSerializer<CodeFileReference>))]
    [DataSerializerGlobal(typeof(CloneSerializer<CodeFileReference>), Profile = "Clone")]
    [DataSerializerGlobal(typeof(ReferenceSerializer<CodeFileReference>), Profile = "Asset")]
    public class CodeFileReference
    {
        public string AbsoluteSourceLocation { get; set; }
    }
}
