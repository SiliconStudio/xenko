// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using NUnit.Framework;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Particles.Updaters.FieldShapes;

namespace SiliconStudio.Xenko.Particles.Tests
{
    class ParticleForcesTest
    {
        [Test]
        public void ForceFieldShapes()
        {
            var unitPos = new Vector3(0, 0, 0);
            var unitRot = new Quaternion(0, 0, 0, 1);
            var unitScl = new Vector3(1, 1, 1);

            Vector3 alongAxis;
            Vector3 aroundAxis;
            Vector3 awayAxis;

            float falloff;

            // Sphere
            var shapeSphere = new Sphere();
            shapeSphere.PreUpdateField(unitPos, unitRot, unitScl);

            falloff = shapeSphere.GetDistanceToCenter(new Vector3(0.1f, 0, 0), new Vector3(0, 1, 0), out alongAxis, out aroundAxis, out awayAxis);
            Assert.That(falloff, Is.EqualTo(0.1f));
            Assert.That(alongAxis, Is.EqualTo(new Vector3(0, 1, 0)));
            Assert.That(aroundAxis, Is.EqualTo(new Vector3(0, 0, -1)));
            Assert.That(awayAxis, Is.EqualTo(new Vector3(1, 0, 0)));

            falloff = shapeSphere.GetDistanceToCenter(new Vector3(0.5f, 0, 0), new Vector3(0, 1, 0), out alongAxis, out aroundAxis, out awayAxis);
            Assert.That(falloff, Is.EqualTo(0.5f));
            Assert.That(alongAxis, Is.EqualTo(new Vector3(0, 1, 0)));
            Assert.That(aroundAxis, Is.EqualTo(new Vector3(0, 0, -1)));
            Assert.That(awayAxis, Is.EqualTo(new Vector3(1, 0, 0)));

            falloff = shapeSphere.GetDistanceToCenter(new Vector3(1, 0, 0), new Vector3(0, 1, 0), out alongAxis, out aroundAxis, out awayAxis);
            Assert.That(falloff, Is.EqualTo(1f));
            Assert.That(alongAxis, Is.EqualTo(new Vector3(0, 1, 0)));
            Assert.That(aroundAxis, Is.EqualTo(new Vector3(0, 0, -1)));
            Assert.That(awayAxis, Is.EqualTo(new Vector3(1, 0, 0)));

            // Box
            var shapeBox = new Cube();
            shapeBox.PreUpdateField(unitPos, unitRot, unitScl);

            falloff = shapeBox.GetDistanceToCenter(new Vector3(0.3f, 0, 0.4f), new Vector3(0, 1, 0), out alongAxis, out aroundAxis, out awayAxis);
            Assert.That(falloff, Is.EqualTo(0.4f)); // Bigger than the two
            Assert.That(alongAxis, Is.EqualTo(new Vector3(0, 1, 0)));
            Assert.That(aroundAxis, Is.EqualTo(new Vector3(0.8f, 0, -0.6f)));
            Assert.That(awayAxis, Is.EqualTo(new Vector3(0.6f, 0, 0.8f)));

            falloff = shapeBox.GetDistanceToCenter(new Vector3(0.5f, 0, 0.4f), new Vector3(0, 1, 0), out alongAxis, out aroundAxis, out awayAxis);
            Assert.That(falloff, Is.EqualTo(0.5f));
            Assert.That(alongAxis, Is.EqualTo(new Vector3(0, 1, 0)));

            falloff = shapeBox.GetDistanceToCenter(new Vector3(1, 0, 0.4f), new Vector3(0, 1, 0), out alongAxis, out aroundAxis, out awayAxis);
            Assert.That(falloff, Is.EqualTo(1f));
            Assert.That(alongAxis, Is.EqualTo(new Vector3(0, 1, 0)));

            // Cylinder
            var shapeCylinder = new Cylinder();
            shapeCylinder.PreUpdateField(unitPos, unitRot, unitScl);

            falloff = shapeCylinder.GetDistanceToCenter(new Vector3(0, 0, 0), new Vector3(0, 1, 0), out alongAxis, out aroundAxis, out awayAxis);
            Assert.That(falloff, Is.EqualTo(0));

            falloff = shapeCylinder.GetDistanceToCenter(new Vector3(0, 0.5f, 0), new Vector3(0, 1, 0), out alongAxis, out aroundAxis, out awayAxis);
            Assert.That(falloff, Is.EqualTo(0)); // No falloff along the Y-axis

            falloff = shapeCylinder.GetDistanceToCenter(new Vector3(0, 1, 0), new Vector3(0, 1, 0), out alongAxis, out aroundAxis, out awayAxis);
            Assert.That(falloff, Is.EqualTo(1));

            falloff = shapeCylinder.GetDistanceToCenter(new Vector3(0.5f, 0, 0), new Vector3(0, 1, 0), out alongAxis, out aroundAxis, out awayAxis);
            Assert.That(falloff, Is.EqualTo(0.5f));

            falloff = shapeCylinder.GetDistanceToCenter(new Vector3(1, 0, 0), new Vector3(0, 1, 0), out alongAxis, out aroundAxis, out awayAxis);
            Assert.That(falloff, Is.EqualTo(1f));

            // Torus
            var shapeTorus = new Torus();
            shapeTorus.PreUpdateField(unitPos, unitRot, unitScl);

            falloff = shapeTorus.GetDistanceToCenter(new Vector3(0, 0, 0), new Vector3(0, 1, 0), out alongAxis, out aroundAxis, out awayAxis);
            Assert.That(falloff, Is.EqualTo(1)); // This is actually outside the torus

            falloff = shapeTorus.GetDistanceToCenter(new Vector3(0.5f, 0, 0), new Vector3(0, 1, 0), out alongAxis, out aroundAxis, out awayAxis);
            Assert.That(falloff, Is.EqualTo(1)); // This is on the torus surface, inner circle
            Assert.That(alongAxis, Is.EqualTo(new Vector3(0, 0, -1)));
            Assert.That(awayAxis, Is.EqualTo(new Vector3(-1, 0, 0)));
            Assert.That(aroundAxis, Is.EqualTo(new Vector3(0, 1, 0)));

            falloff = shapeTorus.GetDistanceToCenter(new Vector3(1, 0, 0), new Vector3(0, 1, 0), out alongAxis, out aroundAxis, out awayAxis);
            Assert.That(falloff, Is.EqualTo(0)); // This is on the torus axis
            Assert.That(alongAxis, Is.EqualTo(new Vector3(0, 0, -1)));

            falloff = shapeTorus.GetDistanceToCenter(new Vector3(1.5f, 0, 0), new Vector3(0, 1, 0), out alongAxis, out aroundAxis, out awayAxis);
            Assert.That(falloff, Is.EqualTo(1)); // This is on the torus surface, outer circle
            Assert.That(alongAxis, Is.EqualTo(new Vector3(0, 0, -1)));
            Assert.That(awayAxis, Is.EqualTo(new Vector3(1, 0, 0)));
            Assert.That(aroundAxis, Is.EqualTo(new Vector3(0, -1, 0)));


        }
    }
}
