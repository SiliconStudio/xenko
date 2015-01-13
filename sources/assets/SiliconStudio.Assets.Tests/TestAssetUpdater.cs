// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.Linq;

using NUnit.Framework;

using SiliconStudio.Assets.Analysis;
using SiliconStudio.Core.Design.Tests;
using SiliconStudio.Core.Reflection;

namespace SiliconStudio.Assets.Tests
{
    /// <summary>
    /// Unit tests for <see cref="AssetUpdater"/>
    /// </summary>
    public class TestAssetUpdater : TestMemberPathBase
    {
        private Package package;
        private AssetUpdater assetUpdater;
        private PackageSession session;
        private AssetDependencyManager dependencyManager;

        private IMemberDescriptor memberMyClass;

        public override void Initialize()
        {
            base.Initialize();

            TypeFactory = new TypeDescriptorFactory();
            var assetDesc = TypeFactory.Find(typeof(TestAssetUpdate));
            memberMyClass = assetDesc.Members.FirstOrDefault(member => member.Name == "MyClass");

            if (session != null)
            {
                session.Dispose();
                dependencyManager.Dispose();
            }

            package = new Package();
            session = new PackageSession(package);
            dependencyManager = new AssetDependencyManager(session);
            assetUpdater = new AssetUpdater(dependencyManager);
        }

        private class TestAssetUpdate : Asset
        {
            public MyClass MyClass;
        }

        /// <summary>
        /// Test the <see cref="AssetUpdater.CanBeModified(SiliconStudio.Assets.Asset,SiliconStudio.Core.Reflection.MemberPath)"/> method on an orphan asset.
        /// </summary>
        [Test]
        public void TestCanBeModifiedOrphan()
        {
            // -------------------------------------------
            // Check that CanBeModified is always true 
            // whatever are the values of the overrides
            //
            // For that set all the possible values of the overrides
            // at two first level of fields and check the reset
            // 
            // Field checked is orphan.MyClass.Value
            //
            // --------------------------------------------

            Initialize();

            var orphan = new TestAssetUpdate { MyClass = new MyClass() };
            var testValues = new[] { OverrideType.Base, OverrideType.Sealed, OverrideType.New, OverrideType.Sealed | OverrideType.New };

            var pathToValue = new MemberPath();
            pathToValue.Push(memberMyClass);
            pathToValue.Push(MemberValue);

            foreach (var value1 in testValues)
            {
                orphan.SetOverride(memberMyClass, value1);

                foreach (var value2 in testValues)
                {
                    orphan.MyClass.SetOverride(MemberValue, value2);

                    Assert.IsTrue(assetUpdater.CanBeModified(orphan, pathToValue));
                }
            }
        }

        /// <summary>
        /// Test the <see cref="AssetUpdater.CanBeModified(SiliconStudio.Assets.Asset,SiliconStudio.Core.Reflection.MemberPath)"/> method on an parent and its child asset.
        /// </summary>
        [Test]
        public void TestCanBeModifiedParentChild()
        {
            Initialize();

            // ----------------------------------------------
            // Case of Asset with only one level of parent.
            //
            // 1. Seal all the parent, play with the child override status.
            // 2. Break path between child and parent, and check that field can be modified in child.
            // 3. Set child to base and play with parent override status.
            //
            // -----------------------------------------------

            var parent = new TestAssetUpdate { MyClass = new MyClass { Maps = { { "0", new MyClass()} } } };
            var child = new TestAssetUpdate { MyClass = new MyClass { Maps = { { "0", new MyClass() } } }, Base = new AssetBase(parent) };

            var pathToMyClass = new MemberPath();
            pathToMyClass.Push(memberMyClass);

            var pathToSub = new MemberPath();
            pathToSub.Push(memberMyClass);
            pathToSub.Push(MemberSub);

            var pathTo0 = new MemberPath();
            pathTo0.Push(memberMyClass);
            pathTo0.Push(MemberMaps);
            pathTo0.Push(MapClassDesc, "0");

            // ## 1 ##

            parent.SetOverride(memberMyClass, OverrideType.Sealed);
            parent.MyClass.SetOverride(MemberSub, OverrideType.Sealed);
            parent.MyClass.SetOverride(MemberMaps, OverrideType.Sealed);
            parent.MyClass.Maps["0"].SetOverride(ThisDescriptor.Default, OverrideType.Sealed);

            Assert.IsFalse(assetUpdater.CanBeModified(child, pathToMyClass));
            Assert.IsFalse(assetUpdater.CanBeModified(child, pathToSub));
            Assert.IsFalse(assetUpdater.CanBeModified(child, pathTo0));

            child.SetOverride(memberMyClass, OverrideType.Sealed);
            child.MyClass.SetOverride(MemberSub, OverrideType.Sealed);
            child.MyClass.SetOverride(MemberMaps, OverrideType.Sealed);
            child.MyClass.Maps["0"].SetOverride(ThisDescriptor.Default, OverrideType.Sealed);

            Assert.IsFalse(assetUpdater.CanBeModified(child, pathToMyClass));
            Assert.IsFalse(assetUpdater.CanBeModified(child, pathToSub));
            Assert.IsFalse(assetUpdater.CanBeModified(child, pathTo0));
            
            child.SetOverride(memberMyClass, OverrideType.New);

            Assert.IsTrue(assetUpdater.CanBeModified(child, pathToMyClass));
            Assert.IsTrue(assetUpdater.CanBeModified(child, pathToSub));
            Assert.IsTrue(assetUpdater.CanBeModified(child, pathTo0));

            child.SetOverride(memberMyClass, OverrideType.Sealed);
            child.MyClass.SetOverride(MemberSub, OverrideType.New);
            child.MyClass.Maps["0"].SetOverride(ThisDescriptor.Default, OverrideType.New);

            Assert.IsFalse(assetUpdater.CanBeModified(child, pathToMyClass));
            Assert.IsTrue(assetUpdater.CanBeModified(child, pathToSub));
            Assert.IsTrue(assetUpdater.CanBeModified(child, pathTo0));
            
            child.SetOverride(memberMyClass, OverrideType.New | OverrideType.Sealed);
            child.MyClass.SetOverride(MemberSub, OverrideType.New | OverrideType.Sealed);
            child.MyClass.Maps["0"].SetOverride(ThisDescriptor.Default, OverrideType.New | OverrideType.Sealed);

            Assert.IsTrue(assetUpdater.CanBeModified(child, pathToMyClass));
            Assert.IsTrue(assetUpdater.CanBeModified(child, pathToSub));
            Assert.IsTrue(assetUpdater.CanBeModified(child, pathTo0));

            // ## 2 ##
            var parent0 = parent.MyClass.Maps["0"];
            parent.MyClass.Maps.Remove("0");

            child.SetOverride(memberMyClass, OverrideType.Base);
            child.MyClass.SetOverride(MemberSub, OverrideType.Base);
            child.MyClass.SetOverride(MemberMaps, OverrideType.Base);
            child.MyClass.Maps["0"].SetOverride(ThisDescriptor.Default, OverrideType.Base);
            
            Assert.IsFalse(assetUpdater.CanBeModified(child, pathToMyClass));
            Assert.IsFalse(assetUpdater.CanBeModified(child, pathToSub));
            Assert.IsTrue(assetUpdater.CanBeModified(child, pathTo0));

            var parentMyClass = parent.MyClass;
            parent.MyClass = null;

            Assert.IsFalse(assetUpdater.CanBeModified(child, pathToMyClass));
            Assert.IsTrue(assetUpdater.CanBeModified(child, pathToSub));
            Assert.IsTrue(assetUpdater.CanBeModified(child, pathTo0));

            // ## 3 ##
            parentMyClass.Maps["0"] = parent0;
            parent.MyClass = parentMyClass;

            parent.SetOverride(memberMyClass, OverrideType.New | OverrideType.Sealed);
            parent.MyClass.SetOverride(MemberSub, OverrideType.New | OverrideType.Sealed);
            parent.MyClass.Maps["0"].SetOverride(ThisDescriptor.Default, OverrideType.New | OverrideType.Sealed);

            Assert.IsFalse(assetUpdater.CanBeModified(child, pathToMyClass));
            Assert.IsFalse(assetUpdater.CanBeModified(child, pathToSub));
            Assert.IsFalse(assetUpdater.CanBeModified(child, pathTo0));

            parent.SetOverride(memberMyClass, OverrideType.Base);

            Assert.IsTrue(assetUpdater.CanBeModified(child, pathToMyClass));
            Assert.IsFalse(assetUpdater.CanBeModified(child, pathToSub));
            Assert.IsFalse(assetUpdater.CanBeModified(child, pathTo0));

            parent.MyClass.SetOverride(MemberSub, OverrideType.Base);

            Assert.IsTrue(assetUpdater.CanBeModified(child, pathToMyClass));
            Assert.IsTrue(assetUpdater.CanBeModified(child, pathToSub));
            Assert.IsFalse(assetUpdater.CanBeModified(child, pathTo0));

            parent.MyClass.Maps["0"].SetOverride(ThisDescriptor.Default, OverrideType.Base);

            Assert.IsTrue(assetUpdater.CanBeModified(child, pathToMyClass));
            Assert.IsTrue(assetUpdater.CanBeModified(child, pathToSub));
            Assert.IsTrue(assetUpdater.CanBeModified(child, pathTo0));
        }

        /// <summary>
        /// Test the <see cref="AssetUpdater.CanBeModified(SiliconStudio.Assets.Asset,SiliconStudio.Core.Reflection.MemberPath)"/> method on an parent and its grand child asset.
        /// </summary>
        [Test]
        public void TestCanBeModifiedParentGrandChild()
        {
            // ----------------------------------------------
            // Case of Asset with only two level of parent.
            //
            // 1. Seal all the parent, check that grand child cannot be modified.
            // 2. Play with new override of child, and check grand child can be modified.
            // 3. Break a path only in child and check that grand child can be modified (even if parent is sealed).
            //
            // -----------------------------------------------

            Initialize();

            var parent = new TestAssetUpdate { MyClass = new MyClass() };
            var child = new TestAssetUpdate { MyClass = new MyClass(), Base = new AssetBase(parent) };
            var grandChild = new TestAssetUpdate { MyClass = new MyClass(), Base = new AssetBase(child) };

            var pathToMyClass = new MemberPath();
            pathToMyClass.Push(memberMyClass);

            var pathToSub = new MemberPath();
            pathToSub.Push(memberMyClass);
            pathToSub.Push(MemberSub);

            // ## 1 ##

            parent.SetOverride(memberMyClass, OverrideType.Sealed);
            parent.MyClass.SetOverride(MemberSub, OverrideType.Sealed);

            Assert.IsFalse(assetUpdater.CanBeModified(grandChild, pathToMyClass));
            Assert.IsFalse(assetUpdater.CanBeModified(grandChild, pathToSub));

            // ## 2 ##

            child.SetOverride(memberMyClass, OverrideType.New);

            Assert.IsTrue(assetUpdater.CanBeModified(grandChild, pathToMyClass));
            Assert.IsTrue(assetUpdater.CanBeModified(grandChild, pathToSub));

            child.SetOverride(memberMyClass, OverrideType.Base);
            child.MyClass.SetOverride(MemberSub, OverrideType.New);

            Assert.IsFalse(assetUpdater.CanBeModified(grandChild, pathToMyClass));
            Assert.IsTrue(assetUpdater.CanBeModified(grandChild, pathToSub));

            // ## 3 ##
            
            child.SetOverride(memberMyClass, OverrideType.Base);
            child.MyClass.SetOverride(MemberSub, OverrideType.Base);

            child.MyClass = null;

            Assert.IsFalse(assetUpdater.CanBeModified(grandChild, pathToMyClass));
            Assert.IsTrue(assetUpdater.CanBeModified(grandChild, pathToSub));
        }
    }
}