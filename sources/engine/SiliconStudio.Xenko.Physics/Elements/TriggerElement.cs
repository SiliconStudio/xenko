using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SiliconStudio.Core;
using SiliconStudio.Core.Collections;

namespace SiliconStudio.Paradox.Physics
{
    [DataContract("TriggerElement")]
    [Display(40, "Trigger")]
    public class TriggerElement : PhysicsElementBase, IPhysicsElement
    {
        public override Types Type => Types.PhantomCollider;
    }
}
