// Copyright (c) 2011-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System;
using System.Threading.Tasks;
using FirstPersonShooter.Player;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Engine.Events;
using SiliconStudio.Xenko.Physics;

namespace FirstPersonShooter
{
    /// <summary>
    /// Will spawn some visual effects when the gun shoots
    /// </summary>
    public class EffectController : TriggerScript
    {      
        private readonly EventReceiver<WeaponFiredResult> weaponFiredEvent = new EventReceiver<WeaponFiredResult>(WeaponScript.WeaponFired);

        public override async Task Execute()
        {
            while (Game.IsRunning)
            {
                var target = await weaponFiredEvent.ReceiveAsync();

                if (target.DidFire)
                    SpawnEvent("MuzzleFlash", Entity, Matrix.Identity);

                if (target.DidHit)
                    SpawnEvent("BulletImpact", null, Matrix.RotationQuaternion(Quaternion.BetweenDirections(Vector3.UnitY, target.HitResult.Normal)) * Matrix.Translation(target.HitResult.Point));

                var rigidBody = target.HitResult.Collider as RigidbodyComponent;
                if (rigidBody != null)
                {
                    var rand = new Random();
                    SpawnEvent("DamagedTrail", rigidBody.Entity, Matrix.Translation(new Vector3((float)rand.NextDouble() - 0.5f, (float)rand.NextDouble() - 0.5f, (float)rand.NextDouble() - 0.5f)));
                }
            }
        }        
    }
}
