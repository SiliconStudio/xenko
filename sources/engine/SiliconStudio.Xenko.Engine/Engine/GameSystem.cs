using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SiliconStudio.Core;
using SiliconStudio.Xenko.Games;

namespace SiliconStudio.Xenko.Engine
{
    public abstract class GameSystem : GameSystemBase
    {
        protected GameSystem(IServiceRegistry registry) : base(registry)
        {
        }

        /// <summary>
        /// Gets the <see cref="Game"/> associated with this <see cref="GameSystemBase"/>. This value can be null in a mock environment.
        /// </summary>
        /// <value>The game.</value>
        /// <remarks>This value can be null</remarks>
        public new Game Game => (Game)base.Game;
    }
}
