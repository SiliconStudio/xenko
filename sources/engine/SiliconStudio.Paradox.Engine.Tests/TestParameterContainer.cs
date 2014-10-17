using System;
using System.Collections.Generic;
using System.IO;

using Paradox.Effects;
using Paradox.EntityModel;
using Paradox.Framework.Serialization;
using Paradox.Framework.Serialization.Contents;
using Paradox.Framework.Serialization.Serializers;
using Paradox.Framework.VirtualFileSystem;

using NUnit.Framework;

namespace Paradox.Starter
{
    [TestFixture]
    public class TestParameterContainer
    {
        [Test]
        public void TestParameterContainer1()
        {
            var c1 = new Entity { Name = "c1" };
            c1.Set(ComponentTest1.Key, new ComponentTest1 { Test = 32, Test2 = 40, RefTest = new ComponentTest1 { Test = 64 } });

            var c2 = (Entity)ParameterContainerExtensions.Instantiate(c1);
            c2.Name = "c2";
            c2.Get(ComponentTest1.Key).Test += 3;
            c2.Get(ComponentTest1.Key).RefTest.Test += 3;

            Assert.AreEqual(35, c2.Get(ComponentTest1.Key).Test);
            Assert.AreEqual(40, c2.Get(ComponentTest1.Key).Test2);

            c1.Set(ComponentTest1.Key, new ComponentTest1 { Test = 132, Test2 = 140, RefTest = new ComponentTest1 { Test = 164 } });

            Assert.AreEqual(35, c2.Get(ComponentTest1.Key).Test);
            Assert.AreEqual(140, c2.Get(ComponentTest1.Key).Test2);
        }

        [Test]
        public void TestSerialization()
        {
            var c1 = new Entity { Name = "c1" };
            c1.Set(ComponentTest2.Key, new ComponentTest2 { Test3 = 123 });
            c1.Set(ComponentTest1.Key, new ComponentTest1 { Test = 32, Test2 = 40, RefTest2 = c1.Get(ComponentTest2.Key) });

            // Convert to data format
            //var convertedEntities = ParameterContainerExtensions.Convert(new[] { c1 });

            // Save to VFS and reload it
            var vfs = new VirtualFileStorage();
            File.Delete("package_scene.dat");

            var packageManager = new PackageManager(vfs);
            //packageManager.CreatePackage("/data/package_scene.dat");
            //convertedEntities.Location = "/data/package_scene.dat/guid_" + Guid.NewGuid();

            var contentManager = new ContentManager(new ContentSerializerContextGenerator(vfs));
            contentManager.RegisterConverter(new EntitySceneConverter());

            var convertedEntities = contentManager.Convert<EntitySceneData, IList<Entity>>(new[] { c1 }, "/data/package_scene.dat/entityscene.hotei");
            contentManager.Save(convertedEntities);

            contentManager = new ContentManager(new ContentSerializerContextGenerator(vfs));
            contentManager.RegisterConverter(new EntitySceneConverter());
            convertedEntities = contentManager.Load<EntitySceneData>(convertedEntities.Location.Url);
            // Convert back to runtime entities
            //var entities = ParameterContainerExtensions.Convert(convertedEntities);
            var entities = contentManager.Convert<IList<Entity>, EntitySceneData>(convertedEntities);
            var c1Copy = entities[0];

            Assert.AreEqual(c1.Get(ComponentTest1.Key).Test, c1Copy.Get(ComponentTest1.Key).Test);
        }

        public class ComponentTest1 : EntityComponent
        {
            public static ParameterResourceKey<ComponentTest1> Key = new ParameterResourceKey<ComponentTest1>("ComponentTest1Key");
            public static ParameterValueKey<int> TestProperty = new ParameterValueKey<int>("Test");
            public static ParameterValueKey<int> Test2Property = new ParameterValueKey<int>("Test2");
            public static ParameterResourceKey<ComponentTest1> RefTestProperty = new ParameterResourceKey<ComponentTest1>("RefTest");
            public static ParameterResourceKey<ComponentTest2> RefTest2Property = new ParameterResourceKey<ComponentTest2>("RefTest2");

            public int Test
            {
                get { return Parameters.Get(TestProperty); }
                set { Parameters.Set(TestProperty, value); }
            }

            public int Test2
            {
                get { return Parameters.Get(Test2Property); }
                set { Parameters.Set(Test2Property, value); }
            }

            public ComponentTest1 RefTest
            {
                get { return Parameters.Get(RefTestProperty); }
                set { Parameters.Set(RefTestProperty, value); }
            }

            public ComponentTest2 RefTest2
            {
                get { return Parameters.Get(RefTest2Property); }
                set { Parameters.Set(RefTest2Property, value); }
            }
        }

        public class ComponentTest2 : EntityComponent
        {
            public static ParameterResourceKey<ComponentTest2> Key = new ParameterResourceKey<ComponentTest2>("ComponentTest2Key");
            public static ParameterValueKey<int> Test3Property = new ParameterValueKey<int>("Test3");

            public int Test3
            {
                get { return Parameters.Get(Test3Property); }
                set { Parameters.Set(Test3Property, value); }
            }
        }
    }
}