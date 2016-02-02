// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;

using NUnit.Framework;

using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Rendering;
using SiliconStudio.Xenko.Graphics;

namespace SiliconStudio.Xenko.Engine.Tests
{
    [TestFixture]
    [Description("Tests on ParameterCollection")]
    class ParametersTest
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
    }
}
