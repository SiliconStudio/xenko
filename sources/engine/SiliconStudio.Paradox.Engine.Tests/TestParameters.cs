// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;

using NUnit.Framework;

using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Paradox.Effects;
using SiliconStudio.Paradox.Graphics;

namespace SiliconStudio.Paradox.Engine.Tests
{
    [TestFixture]
    [Description("Tests on ParameterCollection")]
    class TestParameters
    {
        [TestFixtureSetUp] 
        public void Init()
        {
            // var collection = new ParameterCollection("Test");
        }

        [Test]
        [Description("ParameterKey basic test")]
        public void TestParameterKey()
        {
            var paramView1 = new ParameterKey<Vector3>("View");
            var paramView2 = new ParameterKey<Vector3>("View");
            var paramProj = new ParameterKey<Vector3>("Proj");

            Assert.Throws(typeof (ArgumentNullException), () => new ParameterKey<Vector3>(null));
            Assert.AreEqual(paramView1, paramView2);
            Assert.AreEqual(paramView1.GetHashCode(), paramView2.GetHashCode());
            Assert.AreNotEqual(paramView1, null);
            Assert.AreNotEqual(paramView1, new object());
            Assert.True(paramView1 == paramView2);
            Assert.True(paramView1 != paramProj);
            Assert.True(paramView1 == paramView2);
            Assert.True(ReferenceEquals(paramView1.Name, paramView2.Name));
        }

        [Test]
        [Description("ParameterCollection basic test get set with Vector3")]
        public void TestBasicValues()
        {
            //
            // => Initialize, Set V (1,1,1)
            //
            // ---------------
            // |  V = 1,1,1  | (Test)
            // |             |
            // ---------------
            var collection = new ParameterCollection("Test");

            var paramV = new ParameterKey<Vector3>("View");
            var paramP = new ParameterKey<Vector3>("Proj");
            collection.Set(paramV, new Vector3(1, 1, 1));

            // Verify collection.Count
            Assert.AreEqual(collection.Count, 1);

            // Verify collection.Contains
            Assert.AreEqual(collection.ContainsKey(paramV), true);

            // Verify collection.Keys Enumerator
            int count = 0;
            foreach(var key in collection.Keys)
            {
                Assert.AreEqual(key, paramV);
                count++;
            }
            Assert.AreEqual(count, 1);

            // Verify the Get and returned value
            var value = collection.Get(paramV);
            Assert.AreEqual(value, new Vector3(1,1,1));

            //
            // => Set P (2,2,2)
            //
            // ---------------
            // |  V = 1,1,1  | (Test)
            // |  P = 2,2,2  |
            // ---------------
            collection.Set(paramP, new Vector3(2,2,2));
            Assert.AreEqual(collection.Count, 2);
            Assert.AreEqual(collection.Get(paramP), new Vector3(2, 2, 2));

            //
            // => Remove param V
            //
            // ---------------
            // |             | (Test)
            // |  P = 2,2,2  |
            // ---------------
            collection.Remove(paramV);
            Assert.AreEqual(collection.Count, 1);

            // Check that param
            Assert.AreEqual(collection.Get(paramP), new Vector3(2, 2, 2));

            //
            // => Remove param P
            //
            // ---------------
            // |             | (Test)
            // |             |
            // ---------------
            collection.Remove(paramP);
            Assert.AreEqual(collection.Count, 0);

            //
            // => Set param V
            //
            // ---------------
            // |  V = 2,2,2  | (Test)
            // |             |
            // ---------------
            // Just add same key to test that everything is going fine
            collection.Set(paramV, new Vector3(2, 2, 2));
            Assert.AreEqual(collection.Count, 1);
            Assert.AreEqual(collection.Get(paramV), new Vector3(2, 2, 2));
        }

        /*[Test]
        [Description("ParameterCollection basic test get set on an array of Vector4")]
        public void TestBasicArrayValues()
        {
            //
            // => Initialize, Set V = new Vector4[4] { (0,0,0,0), (1,1,1,1), (2,2,2,2), (3, 3, 3, 3) }
            //
            // --------------------
            // |  V = Vector4[4]  | (Test)
            // |                  |
            // --------------------
            var collection = new ParameterCollection("Test");
            var paramV = new ParameterKey<Vector4>("View", 4);

            collection.Set(paramV, new []
                               {
                                   new Vector4(0, 0, 0, 0), new Vector4(1, 1, 1, 1), new Vector4(2, 2, 2, 2),
                                   new Vector4(3, 3, 3, 3)
                               });

            //
            // => Gets V as a Matrix
            //
            Matrix matrixValueV;
            collection.GetAs(paramV, out matrixValueV);

            Assert.AreEqual(matrixValueV.Row1, new Vector4(0, 0, 0, 0));
            Assert.AreEqual(matrixValueV.Row2, new Vector4(1, 1, 1, 1));
            Assert.AreEqual(matrixValueV.Row3, new Vector4(2, 2, 2, 2));
            Assert.AreEqual(matrixValueV.Row4, new Vector4(3, 3, 3, 3));

            //
            // => Gets single element at index 2 from V 
            //
            Assert.AreEqual(collection.Get(paramV, 2), new Vector4(2, 2, 2, 2));

            //
            // => Gets an array of elements from index 1 to 3 from V and write it to a vector of 5 elements at index 2
            //
            //  Vcopy = new Vector4[5]
            //  Vcopy[2] = V[1]; Vcopy[3] = V[2]; Vcopy[4] = V[3];
            //
            var copyOf = new Vector4[5];
            collection.Get(paramV, 1, copyOf, 2, 3);
            Assert.AreEqual(copyOf[2], new Vector4(1, 1, 1, 1));
            Assert.AreEqual(copyOf[3], new Vector4(2, 2, 2, 2));
            Assert.AreEqual(copyOf[4], new Vector4(3, 3, 3, 3));

            //
            // => Sets a matrix by casting
            //
            matrixValueV.Column1 = new Vector4(3, 2, 1, 0);
            collection.SetAs(paramV, matrixValueV);

            copyOf = new Vector4[4];
            collection.Get(paramV, copyOf );

            Assert.AreEqual(copyOf[0], new Vector4(3, 0, 0, 0));
            Assert.AreEqual(copyOf[1], new Vector4(2, 1, 1, 1));
            Assert.AreEqual(copyOf[2], new Vector4(1, 2, 2, 2));
            Assert.AreEqual(copyOf[3], new Vector4(0, 3, 3, 3));

            //
            // => Sets a single element at inedx1 from V
            //
            collection.Set(paramV, 1, new Vector4(0, 0, 0, 0));
            Assert.AreEqual(collection.Get(paramV, 1), new Vector4(0, 0, 0, 0));

            collection.Release();
        }*/

        [Test, Ignore]
        [Description("ParameterCollection dynamic, exception while setting a dynamic value that is referencing a value not present in the collection")]
        [ExpectedException(typeof(InvalidOperationException), ExpectedMessage = @"Value for dependency \[.*\] not found", MatchType = MessageMatch.Regex)]
        public void TestDynamicValues1()
        {
            //
            // => Trying to initialize VDyn = V + (1,1,1)
            //
            // ------------------------
            // |                      | (Test)
            // |                      |
            // ------------------------
            var collection = new ParameterCollection("Test");
            var paramV = new ParameterKey<Vector3>("View");
            var paramViewDyn = new ParameterKey<Vector3>("ViewDyn");

            // Should throw an InvalidOperationException, as paramV is not present in the collection
            collection.AddDynamic(paramViewDyn,
                            ParameterDynamicValue.New(paramV,
                                                        (ref Vector3 paramVArg, ref Vector3 output) =>
                                                        output = paramVArg + new Vector3(1, 1, 1)));
        }

        [Test, Ignore]
        [Description("ParameterCollection dynamic, get and set a dynamic value. Try to remove a value referenced by a dynamic value")]
        public void TestDynamicValues2()
        {
            //
            // => Initialize
            //
            // ------------------------
            // |  V = 1,2,3           | (Test)
            // |  VDyn = V + (1,1,1)  |
            // ------------------------
            // VDyn = (2,3,4)
            var collection = new ParameterCollection("Test");
            var paramV = new ParameterKey<Vector3>("View");
            var paramViewDyn = new ParameterKey<Vector3>("ViewDyn");

            // Set paramV and paramViewDyn1
            collection.Set(paramV, new Vector3(1,2,3));
            collection.AddDynamic(paramViewDyn, ParameterDynamicValue.New(paramV, (ref Vector3 paramVArg, ref Vector3 output) => output = paramVArg + new Vector3(1, 1, 1)));

            // Check that value is correctly updated
            Assert.AreEqual(collection.Get(paramViewDyn), new Vector3(2, 3, 4));

            //
            // => Set V to (3, 2, 1)
            //
            // ------------------------
            // |  V = 3,2,1           | (Test)
            // |  VDyn = V + (1,1,1)  |
            // ------------------------
            // VDyn = (4,3,2)
            collection.Set(paramV, new Vector3(3, 2, 1));
            Assert.AreEqual(collection.Get(paramViewDyn), new Vector3(4, 3, 2));

            //
            // => Set VDyn = V + (2,2,2)
            //
            // ------------------------
            // |  V = 3,2,1           | (Test)
            // |  VDyn = V + (2,2,2)  |
            // ------------------------
            // VDyn = (5,4,3)
            collection.AddDynamic(paramViewDyn, ParameterDynamicValue.New(paramV, (ref Vector3 paramVArg, ref Vector3 output) => output = paramVArg + new Vector3(2, 2, 2)));

            Assert.AreEqual(collection.Get(paramViewDyn), new Vector3(5, 4, 3));

            // Should throw an InvalidOperationException, as paramV is used by paramViewDyn and
            // cannot be removed
            //Assert.Throws(typeof(InvalidOperationException), () => collection.Remove(paramV));

            // Remove all variables
            collection.Remove(paramViewDyn);
            collection.Remove(paramV);

            //
            // => Remove V and VDyn
            //
            // ------------------------
            // |                      | (Test)
            // |                      |
            // ------------------------
            Assert.AreEqual(collection.Count, 0);
        }

        [Test, Ignore]
        [Description("ParameterCollection dynamic, get and set 2 dynamic values.")]
        public void TestDynamicValues3()
        {
            //
            // => Initialize, set V and P
            //
            // ------------------------
            // |  V = 1,2,3           | (Test)
            // |  P = 0,1,0           |
            // ------------------------
            var collection = new ParameterCollection("Test");
            var paramV = new ParameterKey<Vector3>("View");
            var paramP = new ParameterKey<Vector3>("Proj");

            // Set paramV and paramP
            collection.Set(paramV, new Vector3(1, 2, 3));
            collection.Set(paramP, new Vector3(0, 1, 0));

            //
            // => Set VDyn1 and VDyn2
            //
            // ------------------------
            // |  V = 1,2,3           | (Test)
            // |  P = 0,1,0           |
            // |  VDyn1 = V + P       |
            // |  VDyn2 = V - P       |
            // ------------------------
            // VDyn1 = (1,3,3)
            // VDyn2 = (1,1,3)
            var paramViewProjDyn1 = new ParameterKey<Vector3>("ViewProjDyn1");
            var paramViewProjDyn2 = new ParameterKey<Vector3>("ViewProjDyn2");
            var valueViewProjDyn1 = ParameterDynamicValue.New(paramV, paramP, (ref Vector3 paramVArg, ref Vector3 paramPArg, ref Vector3 output) => output = paramVArg + paramPArg);
            collection.AddDynamic(paramViewProjDyn1, valueViewProjDyn1);

            // paramViewProjDyn2 = paramV - paramP
            collection.AddDynamic(paramViewProjDyn2, ParameterDynamicValue.New(paramP, paramV, (ref Vector3 paramPArg, ref Vector3 paramVArg, ref Vector3 output) => output = paramVArg - paramPArg));

            Assert.AreEqual(collection.Get(paramViewProjDyn1), new Vector3(1, 3, 3));
            Assert.AreEqual(collection.Get(paramViewProjDyn2), new Vector3(1, 1, 3));

            //
            // => Set VDyn1 and VDyn2
            //
            // ------------------------
            // |  V = 1,2,3           | (Test)
            // |  P = 0,1,0           |
            // |  VDyn1 = V + P       |
            // |  VDyn2 = V + P       |
            // ------------------------
            // VDyn1 = (1,3,3)
            // VDyn2 = (1,3,3)
            collection.AddDynamic(paramViewProjDyn2, valueViewProjDyn1);
            Assert.AreEqual(collection.Get(paramViewProjDyn2), new Vector3(1, 3, 3));

            //
            // => Remove all variables
            //
            // ------------------------
            // |                      | (Test)
            // |                      |
            // ------------------------
            collection.Remove(paramViewProjDyn1);
            collection.Remove(paramViewProjDyn2);
            collection.Remove(paramV);
            collection.Remove(paramP);

            Assert.AreEqual(collection.Count, 0);
        }

        [Test, Ignore]
        [Description("ParameterCollection dynamic, get and set 10000 dynamic values using the same definition.")]
        public void TestDynamicValues4()
        {
            //
            // => Set P and V on Root (1,1,1)
            //
            // ---------------
            // |   V = 1,2,3 | (Root)
            // |   P = 0,1,0 |
            // ---------------
            //        |
            // ---------------
            // |   V = ^,^,^ | (Child 0..10000)    
            // |   P = ^,^,^ |
            // |  VP =  V + P|
            // ---------------
            var collection = new ParameterCollection("Root");
            var paramV = new ParameterKey<Vector3>("View");
            var paramP = new ParameterKey<Vector3>("Proj");

            // Set paramV and paramP
            collection.Set(paramV, new Vector3(1, 2, 3));
            collection.Set(paramP, new Vector3(0, 1, 0));

            var paramViewProjDyn1 = new ParameterKey<Vector3>("ViewProjDyn1");
            var valueViewProjDyn1 = ParameterDynamicValue.New(paramV, paramP, (ref Vector3 paramVArg, ref Vector3 paramPArg, ref Vector3 output) => output = paramVArg + paramPArg);

            var childsCollection = new ParameterCollection[10000];
            for (int i = 0; i < childsCollection.Length; i++)
            {
                var childCollection = new ParameterCollection("Child " + i);
                childCollection.AddSources(collection);
                childCollection.AddDynamic(paramViewProjDyn1, valueViewProjDyn1);
                childsCollection[i] = childCollection;
            }

            for (int i = 0; i < childsCollection.Length; i++)
                Assert.AreEqual(childsCollection[i].Get(paramViewProjDyn1), new Vector3(1, 3, 3));

            //
            // => Modify P on Root
            //
            // ---------------
            // |   V = 1,2,3 | (Root)
            // |   P = 0,4,0 |
            // ---------------
            //        |
            // ---------------
            // |   V = ^,^,^ | (Child 0..10000)    
            // |   P = ^,^,^ |
            // |  VP =  V + P|
            // ---------------
            collection.Set(paramP, new Vector3(0, 4, 0));

            for (int i = 0; i < childsCollection.Length; i++)
                Assert.AreEqual(childsCollection[i].Get(paramViewProjDyn1), new Vector3(1, 6, 3));
        }

        [Test, Ignore]
        [Description("ParameterCollection dynamic, get and set dynaimc array values.")]
        public void TestDynamicArrayValues1()
        {
            //
            // => Initialize, set V and P
            //
            // -------------------------------
            // |  VArray = {1,2,3}           | (Test)
            // |  PArray = {0,1,0}           |
            // -------------------------------
            var collection = new ParameterCollection("Test");
            var paramVArray = new ParameterKey<float[]>("VArray",3);
            var paramPArray = new ParameterKey<float[]>("PArray",3);

            // Set paramV and paramP
            collection.SetArray(paramVArray, new float[] { 1, 2, 3 });
            collection.SetArray(paramPArray, new float[] { 0, 1, 0 });

            //
            // => Set VDyn1 and VDyn2
            //
            // -------------------------------
            // |  VArray = {1,2,3}           | (Test)
            // |  PArray = {0,1,0}           |
            // |  VPDynArray = VArray+PArray |
            // -------------------------------
            var paramVPDynArray = new ParameterKey<float[]>("VPDynArray", 3);
            // paramViewProjDyn2 = paramV - paramP
            collection.AddDynamic(paramVPDynArray, ParameterDynamicValue.New(paramVArray, paramPArray, 
                (ref float[] paramVArg, ref float[] paramPArg, ref float[] output) =>
                    {
                        for (int i = 0; i < 3; i++)
                            output[i] = paramVArg[i] + paramPArg[i];
                    }));


            float[] result;
            collection.Get(paramVPDynArray, out result);
            Assert.AreEqual(new Vector3(result), new Vector3(1, 3, 3));

            // Remove value from array
            collection.Remove(paramVPDynArray);
        }

        [Test]
        [Description("ParameterCollection basic test with collection inheritance and values")]
        public void TestCollectionsBasicValues()
        {
            //
            // => Initialize
            //
            // ---------------
            // |             | (Root)
            // |             |
            // ---------------
            //        |
            // ---------------
            // |             | (Child)    
            // |             |
            // ---------------
            var rootCollection = new ParameterCollection("Root");
            var childCollection = new ParameterCollection("Child");
            childCollection.AddSources(rootCollection);

            //
            // => Set P and V on Root (1,1,1)
            //
            // ---------------
            // |   V = 1,1,1 | (Root)
            // |   P = 1,1,1 |
            // ---------------
            //        |
            // ---------------
            // |   V = ^,^,^ | (Child)    
            // |   P = ^,^,^ |
            // ---------------
            var paramV = new ParameterKey<Vector3>("View");
            var paramP = new ParameterKey<Vector3>("Proj");
            rootCollection.Set(paramV, new Vector3(1, 1, 1));
            rootCollection.Set(paramP, new Vector3(1, 1, 1));

            // Verify collection.Count
            Assert.AreEqual(childCollection.Count, 2);

            // Verify collection.Contains
            Assert.AreEqual(childCollection.ContainsKey(paramV), true);
            Assert.AreEqual(childCollection.ContainsKey(paramP), true);

            //
            // => Set V in Root, Get from Child
            //
            // ---------------
            // |   V = 2,2,2 | (Root) 
            // |   P = 1,1,1 |
            // ---------------
            //        |
            // ---------------
            // |   V = ^,^,^ | (Child)   
            // |   P = ^,^,^ |
            // ---------------
            // Verify the Get and returned value
            Assert.AreEqual(childCollection.Get(paramV), new Vector3(1, 1, 1));
            rootCollection.Set(paramV, new Vector3(2,2,2));
            Assert.AreEqual(childCollection.Get(paramV), new Vector3(2, 2, 2));

            //
            // => Remove P from Root
            //
            // ---------------
            // |   V = 2,2,2 | (Root)  
            // |             |
            // ---------------
            //        |
            // ---------------
            // |   V = ^,^,^ | (Child)    
            // ---------------
            rootCollection.Remove(paramP);
            Assert.AreEqual(childCollection.Count, 1);

            //
            // => Overrides V in Child (3,3,3)
            //
            // ---------------
            // |   V = 2,2,2 | (Root)  
            // |             |
            // ---------------
            //        |
            // ---------------
            // |   V = 3,3,3 | (Child)    
            // ---------------            
            childCollection.Set(paramV, new Vector3(3, 3, 3));
            Assert.AreEqual(childCollection.Get(paramV), new Vector3(3, 3, 3));
            Assert.AreEqual(rootCollection.Get(paramV), new Vector3(2, 2, 2));

            //
            // => Reset Key V on Child
            //
            // ---------------
            // |   V = 2,2,2 | (Root)  
            // |             |
            // ---------------
            //        |
            // ---------------
            // |   V = ^,^,^ | (Child)    
            // ---------------            
            childCollection.Reset(paramV);
            Assert.AreEqual(childCollection.Get(paramV), new Vector3(2, 2, 2));
            Assert.AreEqual(rootCollection.Get(paramV), new Vector3(2, 2, 2));

            // Check that we cannot dipose a collction used as a source
            //Assert.Throws(typeof (InvalidOperationException), () => rootCollection.Release() );
            
            //
            // => Remove Root source from Child
            //
            // ---------------
            // |   V = 2,2,2 | (Root)  
            // |             |
            // ---------------
            //
            // ---------------
            // |             | (Child)    
            // ---------------            
            // Remove child using root and verify collection.Count
            childCollection.RemoveSource(rootCollection);
            Assert.AreEqual(childCollection.Count, 0);
        }

        [Test]
        [Description("ParameterCollection test with collection inheritance and parameter overriding")]
        public void TestCollections1()
        {
            //
            // => Initialize
            //
            // ---------------                ---------------
            // |  P = 1,1,1  | (Root1)        |  P = 3,3,3  | (Root2)
            // |  V = 2,2,2  |                |             |
            // ---------------                ---------------
            //        
            // ---------------
            // |             | (Child)    
            // |             |
            // ---------------
            var root1Collection = new ParameterCollection("Root1");
            var root2Collection = new ParameterCollection("Root2");
            var childCollection = new ParameterCollection("Child");

            var paramV = new ParameterKey<Vector3>("View");
            var paramP = new ParameterKey<Vector3>("Proj");
            root1Collection.Set(paramP, new Vector3(1, 1, 1));
            root1Collection.Set(paramV, new Vector3(2, 2, 2));
            root2Collection.Set(paramP, new Vector3(3, 3, 3));

            //
            // => Add source Roo1 and Root2 to Child
            //
            // ---------------                ---------------
            // |  P = 1,1,1  | (Root1)        |  P = 3,3,3  | (Root2)
            // |  V = 2,2,2  |                |             |
            // ---------------                ---------------
            //        |                    / 
            // ---------------           /
            // |  P = ^,^,^  | (Child) / 
            // |  V = ^,^,^  |
            // ---------------
            // P = 3,3,3
            // V = 2,2,2
            childCollection.AddSources(root1Collection, root2Collection);

            Assert.AreEqual(childCollection.Count, 2);
            Assert.AreEqual(childCollection.Get(paramP), new Vector3(3, 3, 3));
            Assert.AreEqual(childCollection.Get(paramV), new Vector3(2, 2, 2));

            //
            // => Add source Roo1 and Root2 to Child
            //
            // ---------------                ---------------
            // |  P = 1,1,1  | (Root1)        |             | (Root2)
            // |  V = 2,2,2  |                |  V = 3,3,3  |
            // ---------------                ---------------
            //        |                    / 
            // ---------------           /
            // |  P = ^,^,^  | (Child) / 
            // |  V = ^,^,^  |
            // ---------------
            // P = 1,1,1
            // V = 3,3,3
            root2Collection.Remove(paramP);
            root2Collection.Set(paramV, new Vector3(3,3,3));
            Assert.AreEqual(childCollection.Count, 2);
            Assert.AreEqual(childCollection.Get(paramP), new Vector3(1, 1, 1));
            Assert.AreEqual(childCollection.Get(paramV), new Vector3(3, 3, 3));

            //
            // => Remove source Root1 for Child
            //
            // ---------------                ---------------
            // |  P = 1,1,1  | (Root1)        |             | (Root2)
            // |  V = 2,2,2  |                |  V = 3,3,3  |
            // ---------------                ---------------
            //                            / 
            // ---------------           /
            // |             | (Child) / 
            // |  V = ^,^,^  |
            // ---------------
            // V = 3,3,3
            childCollection.RemoveSource(root1Collection);
            Assert.AreEqual(childCollection.Count, 1);
            Assert.AreEqual(childCollection.Get(paramV), new Vector3(3, 3, 3));

            //
            // => Inherit Root1 from Child
            //
            // ---------------               ---------------
            // |             | (Child) ------|             | (Root2)
            // |  V = ^,^,^  |               |  V = 3,3,3  |
            // ---------------               ---------------
            //        |                     
            // ---------------           
            // |  P = 1,1,1  | (Root1) 
            // |  V = 2,2,2  |         
            // ---------------         
            root1Collection.AddSources(childCollection);
            Assert.AreEqual(root1Collection.Count, 2);
            Assert.AreEqual(root1Collection.Get(paramP), new Vector3(1, 1, 1));
            Assert.AreEqual(root1Collection.Get(paramV), new Vector3(2, 2, 2));
        }

        [Test, Ignore]
        [Description("ParameterCollection test with collection inheritance and dynamic values")]
        public void TestCollections2()
        {
            //
            // => Initialize
            //
            // ---------------                
            // |  P = 1,1,1  | (Root1)        
            // |  V = 2,2,2  |                
            // |  VP = V+P   |                
            // ---------------                
            //        
            // ---------------           ---------------
            // |  P = 3,3,3  | (Root2)   |  P = 5,5,5  | (Root3)    
            // |             |           |  V = P      |
            // |             |           |             |
            // ---------------           ---------------         
            var root1Collection = new ParameterCollection("Root1");
            var root2Collection = new ParameterCollection("Root2");
            var root3Collection = new ParameterCollection("Root3");

            var paramV = new ParameterKey<Vector3>("View");
            var paramP = new ParameterKey<Vector3>("Proj");
            var paramVP = new ParameterKey<Vector3>("ViewProj");

            root1Collection.Set(paramP, new Vector3(1, 1, 1));
            root1Collection.Set(paramV, new Vector3(2, 2, 2));
            root1Collection.AddDynamic(paramVP, ParameterDynamicValue.New(paramV, paramP, (ref Vector3 paramVArg, ref Vector3 paramPArg, ref Vector3 output) => output = paramVArg + paramPArg));

            root2Collection.Set(paramP, new Vector3(3, 3, 3));

            root3Collection.Set(paramP, new Vector3(5, 5, 5));
            root3Collection.AddDynamic(paramV, ParameterDynamicValue.New(paramP, (ref Vector3 paramPArg, ref Vector3 paramVArg) => paramVArg = paramPArg));

            //
            // => Add Root1 as a source of Root2
            //
            // ---------------                
            // |  P = 1,1,1  | (Root1)        
            // |  V = 2,2,2  |                
            // |  VP = V+P   |                
            // ---------------                
            //        |
            // ---------------           ---------------
            // |  P = 3,3,3  | (Root2)   |  P = 5,5,5  | (Root3)    
            // |  V = ^,^,^  |           |  V = P      |
            // |  VP= ^,^,^  |           |             |
            // ---------------           ---------------
            //
            // Root1: VP = 3,3,3
            // Root2: VP = 5,5,5
            //
            // Number of registered Dynamic Values: 3
            // Root1: 1 => VP = Root1.V + Root1.P 
            // Root2: 1 => VP = Root1.V + Root2.P
            // Root3: 1 => V = Root3.P
            root2Collection.AddSources(root1Collection);

            //
            // => Add Root2 and Root3 as source of child collection
            //
            // ---------------                
            // |  P = 1,1,1  | (Root1)        
            // |  V = 2,2,2  |                
            // |  VP = V+P   |                
            // ---------------                
            //        |
            // ---------------           ---------------
            // |  P = 3,3,3  | (Root2)   |  P = 5,5,5  | (Root3)    
            // |  V = ^,^,^  |         / |  V = P      |
            // |  VP= ^,^,^  |        /  |             |
            // ---------------       /   ---------------
            //        |             /
            // ---------------
            // |  P = ^,^,^  | (Child)    
            // |  V = ^,^,^  |
            // |  VP =^,^,^  |
            // ---------------
            //
            // Root1: VP = 3,3,3
            // Root2: VP = 5,5,5
            // Root3: V = 5,5,5
            // Child: VP = 10,10,10
            //
            // Number of registered Dynamic Values: 4
            // Root1: 1 => VP = Root1.V + Root1.P 
            // Root2: 1 => VP = Root1.V + Root2.P
            // Root3: 1 => V = Root3.P
            // Child: 1 => VP = Root3.V + Root3.P
            var childCollection = new ParameterCollection("Child");
            childCollection.AddSources(root2Collection, root3Collection);

            Assert.AreEqual(childCollection.Get(paramVP), new Vector3(10, 10, 10));

            //
            // => Overrides P in child
            //
            // ---------------                
            // |  P = 1,1,1  | (Root1)        
            // |  V = 2,2,2  |                
            // |  VP = V+P   |                
            // ---------------                
            //        |
            // ---------------           ---------------
            // |  P = 3,3,3  | (Root2)   |  P = 5,5,5  | (Root3)    
            // |  V = ^,^,^  |         / |  V = P      |
            // |  VP= ^,^,^  |        /  |             |
            // ---------------       /   ---------------
            //        |             /
            // ---------------
            // |  P = 4,4,4  | (Child)    
            // |  V = ^,^,^  |
            // |  VP =^,^,^  |
            // ---------------
            //
            // Root1: VP = 3,3,3
            // Root2: VP = 5,5,5
            // Root3: V = 5,5,5
            // Child: VP = 8,8,8
            //
            // Number of registered Dynamic Values: 5
            // Root1: 1 => VP = Root1.V + Root1.P 
            // Root2: 1 => VP = Root1.V + Root2.P
            // Root3: 1 => V = Root3.P
            // Child: 2 => V = Child.P, VP = Child.V + Child.P
            childCollection.Set(paramP, new Vector3(4, 4, 4));
            Assert.AreEqual(childCollection.Get(paramVP), new Vector3(8, 8, 8));

            //
            // => Overrides P in child
            //
            // ---------------                
            // |  P = 1,1,1  | (Root1)        
            // |  V = 2,2,2  |                
            // |  VP = V+P   |                
            // ---------------                
            //        |
            // ---------------           ---------------
            // |  P = 3,3,3  | (Root2)   |  P = 5,5,5  | (Root3)    
            // |  V = ^,^,^  |         / |  V = P      |
            // |  VP= ^,^,^  |        /  |             |
            // ---------------       /   ---------------
            //        |             /
            // ---------------
            // |  P = 4,4,4  | (Child)    
            // |  V = ^,^,^  |
            // |  VP = V-P   |
            // ---------------
            //
            // Root1: VP = 3,3,3
            // Root2: VP = 5,5,5
            // Root3: V = 5,5,5
            // Child: VP = 8,8,8
            //
            // Number of registered Dynamic Values: 5
            // Root1: 1 => VP = Root1.V + Root1.P 
            // Root2: 1 => VP = Root1.V + Root2.P
            // Root3: 1 => V = Root3.P
            // Child: 2 => V = Child.P, VP = Child.V - Child.P
            childCollection.AddDynamic(paramVP, ParameterDynamicValue.New(paramV, paramP, (ref Vector3 paramVArg, ref Vector3 paramPArg, ref Vector3 output) => output = paramVArg - paramPArg));
            Assert.AreEqual(root1Collection.Get(paramVP), new Vector3(3, 3, 3));
            Assert.AreEqual(root2Collection.Get(paramVP), new Vector3(5, 5, 5));
            Assert.AreEqual(root3Collection.Get(paramV), new Vector3(5, 5, 5));
            Assert.AreEqual(childCollection.Get(paramV), new Vector3(4, 4, 4));
            Assert.AreEqual(childCollection.Get(paramVP), new Vector3(0, 0, 0));
        }

        [Test, Ignore]
        [Description("ParameterCollection test with collection multiple inheritance")]
        public void TestCollections3()
        {
            //
            // => Initialize
            //
            // ---------------                
            // |  P = 0,0,0  | (Root0)        
            // |             |                
            // |             |                
            // ---------------                
            //        |
            // ---------------           ---------------            ---------------
            // |  P = 1,1,1  | (Root1)   |  P = 2,2,2  | (Root2)    |  P = 3,3,3  | (Root3)  
            // |             |           |             |            |             |
            // |             |           |             |            |             |
            // ---------------           ---------------            ---------------         
            //        |
            // ---------------           
            // |  P = 4,4,4  | (Root4)     
            // |             |           
            // |             |           
            // ---------------       
            //        |
            // ---------------           
            // |  P = 5,5,5  | (Root5)     
            // |             |           
            // |             |           
            // ---------------       
            //        
            // ---------------           
            // |             | (Root6)     
            // |             |           
            // |             |           
            // ---------------       
            var paramV = new ParameterKey<Vector3>("View");
            var paramP = new ParameterKey<Vector3>("Proj");
            var roots = new ParameterCollection[7];
            for (int i = 0; i < roots.Length; i++)
            {
                roots[i] = new ParameterCollection("Root" + i);
                if (i < 6)
                    roots[i].Set(paramP, new Vector3(i,i,i));
            }

            roots[3].Set(paramV, new Vector3(1,1,1));

            roots[1].AddSources(roots[0]);
            roots[4].AddSources(roots[1]);
            roots[5].AddSources(roots[4]);
            Assert.AreEqual(roots[4].Get(paramP), new Vector3(4, 4, 4));

            // check that adding twice a source if handled
            roots[1].AddSources(roots[0]);
            Assert.AreEqual(roots[1].Sources.Length, 1);

            //
            // => Add T on Root0 to test cascading inheritance and removes it from Root0
            //
            // ---------------                
            // |  P = 0,0,0  | (Root0)        
            // |  T = 6,6,6 |                
            // |             |                
            // ---------------                
            //        |
            // ---------------           ---------------            ---------------
            // |  P = 1,1,1  | (Root1)   |  P = 2,2,2  | (Root2)    |  P = 3,3,3  | (Root3)  
            // |  T = ^,^,^  |           |             |            |             |
            // |             |           |             |            |             |
            // ---------------           ---------------            ---------------         
            //        |
            // ---------------           
            // |  P = 4,4,4  | (Root4)     
            // |  T = ^,^,^  |           
            // |             |           
            // ---------------       
            //        |
            // ---------------           
            // |  P = 5,5,5  | (Root5)     
            // |  T = ^,^,^  |           
            // |             |           
            // ---------------  
            var paramT = new ParameterKey<Vector3>("Temp");
            roots[0].Set(paramT, new Vector3(6,6,6));
            Assert.AreEqual(roots[4].Get(paramT), new Vector3(6, 6, 6));
            roots[0].Remove(paramT);
            Assert.False(roots[5].ContainsKey(paramT));
            
            //
            // => Reset P on Root4
            //
            // ---------------                
            // |  P = 0,0,0  | (Root0)        
            // |             |                
            // |             |                
            // ---------------                
            //        |
            // ---------------           ---------------            ---------------
            // |  P = 1,1,1  | (Root1)   |  P = 2,2,2  | (Root2)    |  P = 3,3,3  | (Root3)  
            // |             |           |             |            |  V = 1,1,1  |
            // |             |           |             |            |             |
            // ---------------           ---------------            ---------------         
            //        |
            // ---------------           
            // |  P = ^,^,^  | (Root4)     
            // |             |           
            // |             |           
            // ---------------       
            //        |
            // ---------------           
            // |  P = 5,5,5  | (Root5)     
            // |             |           
            // |             |           
            // ---------------       
            roots[4].Reset(paramP);
            Assert.AreEqual(roots[4].Get(paramP), new Vector3(1, 1, 1));

            //
            // => Adds Root2 and Root3 as sources of Root4
            //
            // ---------------                
            // |  P = 0,0,0  | (Root0)        
            // |             |                
            // |             |                
            // ---------------                
            //        |
            // ---------------           ---------------            ---------------
            // |  P = 1,1,1  | (Root1)   |  P = 2,2,2  | (Root2)  / |  P = 3,3,3  | (Root3)  
            // |             |           |             |        /   |  V = 1,1,1  |
            // |             |        /  |             |      /     |             |
            // ---------------       /   ---------------    /       ---------------         
            //        |             /                     /
            // ---------------                          /
            // |  P = ^,^,^  | (Root4)   --------------/    
            // |  V = ^,^,^  |           
            // |             |           
            // ---------------       
            //        |
            // ---------------           
            // |  P = 5,5,5  | (Root5)     
            // |  V = ^,^,^  |           
            // |             |           
            // ---------------       
            roots[4].AddSources(roots[2], roots[3]);
            Assert.AreEqual(roots[4].Get(paramP), new Vector3(3, 3, 3));
            Assert.AreEqual(roots[5].Get(paramV), new Vector3(1, 1, 1));

            //
            // => Removes P from Root2
            //
            // ---------------                
            // |  P = 0,0,0  | (Root0)        
            // |             |                
            // |             |                
            // ---------------                
            //        |
            // ---------------           ---------------            ---------------
            // |  P = 1,1,1  | (Root1)   |             | (Root2)  / |  P = 3,3,3  | (Root3)  
            // |             |           |             |        /   |  V = 1,1,1  |
            // |             |        /  |             |      /     |             |
            // ---------------       /   ---------------    /       ---------------         
            //        |             /                     /
            // ---------------                          /
            // |  P = ^,^,^  | (Root4)   --------------/    
            // |  V = ^,^,^  |           
            // |             |           
            // ---------------       
            //        |
            // ---------------           
            // |  P = 5,5,5  | (Root5)     
            // |  V = ^,^,^  |           
            // |             |           
            // ---------------       
            roots[2].Remove(paramP);
            Assert.AreEqual(roots[4].Get(paramP), new Vector3(3, 3, 3));
            Assert.AreEqual(roots[5].Get(paramV), new Vector3(1, 1, 1));

            //
            // => Add dynamic value on Root4. 
            // => Sets V locally. 
            // => Overrides VX in Root5.
            // => Add Root5 as source of Root6
            //
            // ---------------                
            // |  P = 0,0,0  | (Root0)        
            // |             |                
            // |             |                
            // ---------------                
            //        |
            // ---------------           ---------------            ---------------
            // |  P = 1,1,1  | (Root1)   |             | (Root2)  / |  P = 3,3,3  | (Root3)  
            // |             |           |             |        /   |  V = 1,1,1  |
            // |             |        /  |             |      /     |             |
            // ---------------       /   ---------------    /       ---------------         
            //        |             /                     /
            // ---------------                          /
            // |  P = ^,^,^  | (Root4)   --------------/    
            // |  V = ^,^,^  |           
            // |  X = 9,9,9  |           
            // | VX = V + X  |           
            // ---------------       
            //        |
            // ---------------           
            // |  P = 5,5,5  | (Root5)     
            // |  V = ^,^,^  |           
            // |  X = ^,^,^  |           
            // | VX = V - X  |           
            // ---------------       
            //        |
            // ---------------           
            // |  P = ^,^,^  | (Root6)     
            // |  V = ^,^,^  |           
            // |  X = ^,^,^  |           
            // | VX =   ^    |           
            // ---------------       
            var paramX = new ParameterKey<Vector3>("X");
            var paramVX = new ParameterKey<Vector3>("VX");
            roots[4].Set(paramX, new Vector3(9,9,9));
            roots[4].AddDynamic(paramVX, ParameterDynamicValue.New(paramV, paramX, (ref Vector3 paramVArg, ref Vector3 paramXArg, ref Vector3 output) => output = paramVArg + paramXArg));
            roots[5].AddDynamic(paramVX, ParameterDynamicValue.New(paramV, paramX, (ref Vector3 paramVArg, ref Vector3 paramXArg, ref Vector3 output) => output = paramVArg - paramXArg));
            roots[6].AddSources(roots[5]);
            Assert.AreEqual(roots[4].Get(paramVX), new Vector3(10,10,10));
            Assert.AreEqual(roots[5].Get(paramVX), new Vector3(-8, -8, -8));

            //
            // => Removes P from Root5 and removes source from Root5
            //
            // ---------------                
            // |  P = 0,0,0  | (Root0)        
            // |             |                
            // |             |                
            // ---------------                
            //        |
            // ---------------           ---------------            ---------------
            // |  P = 1,1,1  | (Root1)   |             | (Root2)  / |  P = 3,3,3  | (Root3)  
            // |             |           |             |        /   |  V = 1,1,1  |
            // |             |        /  |             |      /     |             |
            // ---------------       /   ---------------    /       ---------------         
            //        |             /                     /
            // ---------------                          /
            // |  P = ^,^,^  | (Root4)   --------------/    
            // |  V = ^,^,^  |           
            // |  X = 9,9,9  |           
            // | VX = V + X  |           
            // ---------------       
            //        
            // ---------------           
            // |  P = 5,5,5  | (Root5)     
            // |             |           
            // |             |           
            // |             |           
            // --------------- 
            //        |
            // ---------------           
            // |  P = ^,^,^  | (Root6)     
            // |             |           
            // |             |           
            // |             |           
            // ---------------       
            Assert.True(roots[5].RemoveSource(roots[4]));

            // try to remove it a 2nd time
            Assert.False(roots[5].RemoveSource(roots[4]));

            Assert.True(roots[5].ContainsKey(paramP));
            Assert.True(roots[6].ContainsKey(paramP));

            Assert.False(roots[5].ContainsKey(paramV));
            Assert.False(roots[5].ContainsKey(paramX));
            Assert.False(roots[6].ContainsKey(paramV));
            Assert.False(roots[6].ContainsKey(paramX));

            Assert.AreEqual(roots[5].Get(paramP), new Vector3(5, 5, 5));
            Assert.AreEqual(roots[6].Get(paramP), new Vector3(5, 5, 5));

            //
            // => Removes source Root5 from Root6
            //
            // ---------------           
            // |             | (Root5)     
            // |  V = 1,1,1  |           
            // |  X = 9,9,9  |           
            // | VX = V - X  |           
            // --------------- 
            //        
            // ---------------           
            // |             | (Root6)     
            // |             |           
            // |             |           
            // |             |           
            // ---------------       
            roots[6].RemoveSource(roots[5]);
            Assert.AreEqual(roots[6].Count, 0);
        }

        [Test, Ignore]
        public void TestCollections4()
        {
            //          Root1 <- Root3
            //            |        |
            // Root0 <- Root2 <- Root4
            var paramV = new ParameterKey<Vector3>("View");
            var roots = new ParameterCollection[5];
            for (int i = 0; i < roots.Length; i++)
            {
                roots[i] = new ParameterCollection("Root" + i);
            }

            roots[0].Set(paramV, new Vector3(1.0f, 1.0f, 1.0f));
            roots[1].Set(paramV, new Vector3(2.0f, 1.0f, 1.0f));

            roots[2].AddSources(roots[1], roots[0]);
            roots[3].AddSources(roots[1]);
            roots[4].AddSources(roots[2], roots[3]);

            Assert.AreEqual(new Vector3(1.0f, 1.0f, 1.0f), roots[4].Get(paramV));
        }

        [Test, Ignore]
        public void TestCollectionsEngine()
        {
            // RPP   <-  EP  <-  EMP
            //  EFPD <-/          | 
            //           E   <-  EM   
            var paramV = new ParameterKey<Vector3>("View");
            var paramV2 = new ParameterKey<Vector3>("ViewProj");
            var dynV2 = ParameterDynamicValue.New(paramV, (ref Vector3 param1, ref Vector3 output) => { output = new Vector3(param1.X + 1.0f, param1.Y, param1.Z); });
            var dynV3 = ParameterDynamicValue.New(paramV, (ref Vector3 param1, ref Vector3 output) => { output = new Vector3(param1.X + 4.0f, param1.Y, param1.Z); });

            var renderPlugin = new ParameterCollection("RenderPlugin");
            var effectPassDefault = new ParameterCollection("EffectPassDefault");
            var effectPass = new ParameterCollection("EffectPass");
            var effectMeshPass = new ParameterCollection("EffectMeshPass");
            var renderMesh = new ParameterCollection("RenderMesh");
            var effect = new ParameterCollection("Effect");

            effectPassDefault.Set(paramV, new Vector3(2.0f, 2.0f, 2.0f));
            effectPassDefault.AddDynamic(paramV2, dynV2);

            effectPass.AddSources(effectPassDefault, renderPlugin);

            renderPlugin.Set(paramV, new Vector3(1.0f, 1.0f, 1.0f));

            Assert.AreEqual(new Vector3(2.0f, 1.0f, 1.0f), effectPass.Get(paramV2));

            renderMesh.AddSources(effect);

            effectMeshPass.AddSources(effectPass, renderMesh);


            Assert.AreEqual(new Vector3(2.0f, 1.0f, 1.0f), effectMeshPass.Get(paramV2));

            effect.AddDynamic(paramV2, dynV3);
            effect.Set(paramV, new Vector3(3.0f, 3.0f, 3.0f));

            Assert.AreEqual(new Vector3(7.0f, 3.0f, 3.0f), effectMeshPass.Get(paramV2));
        }

        [Test, Ignore]
        [Description("ParameterCollection test with collection of graphics resources parameters")]
        public void TestGraphicsResource1()
        {
            //
            // => Removes P from Root5 and removes source from Root5
            //
            // -------------------              -------------------            
            // |  Tx = Texture2  | (Root0)      |  Tx = Texture1  | (Root1)    
            // |                 |              |                 |            
            // |                 |              |                 |            
            // -------------------              -------------------            
            //        |
            // -------------------                
            // |  Tx = ^         | (Root2)        
            // |                 |                
            // |                 |                
            // -------------------                
            //        
            var paramTx = new ParameterKey<MyGraphicsResource>("Tx");
            var roots = new ParameterCollection[4];
            for (int i = 0; i < roots.Length; i++)
            {
                roots[i] = new ParameterCollection("Root" + i);
            }
            roots[2].AddSources(roots[0]);

            var instanceTx2 = new MyGraphicsResource("Tx2");
            roots[0].Set(paramTx, instanceTx2);
            // Check that setting it twice is not going to add reference more than once
            roots[0].Set(paramTx, instanceTx2);

            var instanceTx1 = new MyGraphicsResource("Tx1");
            roots[1].Set(paramTx, instanceTx1);

            Assert.AreEqual(roots[2].Get(paramTx), instanceTx2);


            //
            // => Adds T1 on Root0 and removes it immediately
            //
            // -------------------              -------------------            
            // |  Tx = Texture2  | (Root0)      |  Tx = Texture1  | (Root1)    
            // |                 |              |                 |            
            // |                 |              |                 |            
            // -------------------              -------------------            
            //        |
            // -------------------                
            // |  Tx = ^         | (Root2)        
            // |                 |                
            // |                 |                
            // -------------------                
            //        
            var paramT1 = new ParameterKey<MyGraphicsResource>("T1");
            var instanceT1 = new MyGraphicsResource("T1");
            roots[0].Set(paramT1, instanceT1);
            roots[0].Remove(paramT1);
            Assert.AreEqual(instanceT1.DisposeCounter, 0);

            // Set paramT1 = Tx1 and after paramT2 = Tx2
            roots[0].Set(paramT1, instanceTx1);
            roots[0].Set(paramT1, instanceTx2);
            //
            // => Add source Root2 to Root1
            //
            // -------------------              -------------------            
            // |  Tx = Texture2  | (Root0)      |  Tx = Texture1  | (Root1)    
            // |                 |             /|                 |            
            // |                 |           /  |                 |            
            // -------------------         /    -------------------            
            //        |                  /
            // -------------------                
            // |  Tx = ^         | (Root2)        
            // |                 |                
            // |                 |                
            // -------------------                
            //        
            roots[2].AddSources(roots[1]);
            Assert.AreEqual(roots[1].Get(paramTx), instanceTx1);

            // Assert that graphics resource are disposed
            Assert.AreEqual(instanceTx1.DisposeCounter, 0);
            Assert.AreEqual(instanceTx2.DisposeCounter, 0);
        }

        /// <summary>
        /// Local graphics resource mock
        /// </summary>
        internal class MyGraphicsResource : ComponentBase
        {
            public int DisposeCounter { get; private set; }

            public MyGraphicsResource(string name) : base(name)
            {
                DisposeCounter = 1;
            }

            protected override void Destroy()
            {
                DisposeCounter--;
                base.Destroy();
            }
            public GraphicsDevice GraphicsDevice
            {
                get { return null; }
            }
        }

        [Test, Ignore]
        public void TestDynamicValues5()
        {
            //
            // => Initialize
            //
            // ---------------                
            // |             | (Root1)        
            // |  V = 2,2,2  |                
            // |  VP = V+P   |                
            // ---------------                
            //      |
            // ---------------         
            // |             | (Root2)
            // |             |
            // |             |
            // ---------------
            var root1Collection = new ParameterCollection("Root1");
            var root2Collection = new ParameterCollection("Root2");

            var paramV = new ParameterKey<Vector3>("View");
            var paramP = new ParameterKey<Vector3>("Proj");
            var paramVP = new ParameterKey<Vector3>("ViewProj");

            root1Collection.Set(paramV, new Vector3(2, 2, 2));
            root1Collection.AddDynamic(paramVP, ParameterDynamicValue.New(paramV, paramP, (ref Vector3 paramVArg, ref Vector3 paramPArg, ref Vector3 output) => output = paramVArg + paramPArg));

            root2Collection.AddSources(root1Collection);

            // Add P = 3, 3, 3 into Root2
            root2Collection.Set(paramP, new Vector3(3, 3, 3));

            Assert.AreEqual(new Vector3(5, 5, 5), root2Collection.Get(paramVP));
        }

        /*[Test]
        public void TestValueUpdate()
        {
            var root1Collection = new ParameterCollection("Root1");
            var root2Collection = new ParameterCollection("Root2");
            root2Collection.AddSources(root1Collection);

            var paramV = new ParameterKey<Vector3>("View");
            var paramP = new ParameterKey<Vector3>("Proj");
            var paramVP = new ParameterKey<Vector3>("ViewProj");

            var eventCounter = 0;
            var eventCounterV = 0;
            var eventCounterVP = 0;

            root2Collection.AddEvent(null, (v, o) => eventCounter++);
            root2Collection.AddEvent(paramV, (v, o) => eventCounterV++);
            root2Collection.AddEvent(paramVP, (v, o) => eventCounterVP++);

            root1Collection.Set(paramV, new Vector3(2, 2, 2));
            root2Collection.Set(paramP, new Vector3(3, 3, 3));
            root1Collection.Set(paramVP, ParameterDynamicValue.New(paramV, paramP, (ref Vector3 paramVArg, ref Vector3 paramPArg, ref Vector3 output) => output = paramVArg + paramPArg));

            Assert.AreEqual(3, eventCounter);
            Assert.AreEqual(1, eventCounterV);
            Assert.AreEqual(1, eventCounterVP);

            root1Collection.Set(paramV, new Vector3(3, 3, 3));
            root2Collection.Get(paramVP);

            Assert.AreEqual(5, eventCounter);
            Assert.AreEqual(2, eventCounterV);
            Assert.AreEqual(2, eventCounterVP);
        }*/
    }
}
