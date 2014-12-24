// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.Collections.Generic;
using System.Linq;

using SiliconStudio.Core.Reflection;

namespace SiliconStudio.Core.Design.Tests
{
    public class TestMemberPathBase
    {
        protected IMemberDescriptor MemberValue;
        protected IMemberDescriptor MemberSub;
        protected IMemberDescriptor MemberStruct;
        protected IMemberDescriptor MemberSubs;
        protected IMemberDescriptor MemberMaps;
        protected IMemberDescriptor MemberX;
        protected IMemberDescriptor MemberClass;

        protected CollectionDescriptor ListClassDesc;
        protected DictionaryDescriptor MapClassDesc;

        protected TypeDescriptorFactory TypeFactory;

        public struct MyStruct
        {
            public int X { get; set; }

            public MyClass Class { get; set; }
        }

        public class MyClass
        {
            public MyClass()
            {
                Subs = new List<MyClass>();
                Maps = new Dictionary<string, MyClass>();
            }

            public int Value { get; set; }

            public MyClass Sub { get; set; }

            public MyStruct Struct { get; set; }

            public List<MyClass> Subs { get; set; }

            public Dictionary<string, MyClass> Maps { get; set; }
        }

        /// <summary>
        /// Initialize the tests.
        /// </summary>
        public virtual void Initialize()
        {
            TypeFactory = new TypeDescriptorFactory();
            var myClassDesc = TypeFactory.Find(typeof(MyClass));
            var myStructDesc = TypeFactory.Find(typeof(MyStruct));
            ListClassDesc = (CollectionDescriptor)TypeFactory.Find(typeof(List<MyClass>));
            MapClassDesc = (DictionaryDescriptor)TypeFactory.Find(typeof(Dictionary<string, MyClass>));

            MemberValue = myClassDesc.Members.FirstOrDefault(member => member.Name == "Value");
            MemberSub = myClassDesc.Members.FirstOrDefault(member => member.Name == "Sub");
            MemberStruct = myClassDesc.Members.FirstOrDefault(member => member.Name == "Struct");
            MemberSubs = myClassDesc.Members.FirstOrDefault(member => member.Name == "Subs");
            MemberMaps = myClassDesc.Members.FirstOrDefault(member => member.Name == "Maps");
            MemberX = myStructDesc.Members.FirstOrDefault(member => member.Name == "X");
            MemberClass = myStructDesc.Members.FirstOrDefault(member => member.Name == "Class");
        }
         
    }
}