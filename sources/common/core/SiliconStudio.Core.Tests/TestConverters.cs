// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System.Collections.Generic;
using NUnit.Framework;
using SiliconStudio.Core.Serialization;
using SiliconStudio.Core.Serialization.Contents;
using SiliconStudio.Core.Serialization.Converters;

namespace SiliconStudio.Core.Tests
{
    [TestFixture]
    public class TestConverters
    {
        public class A
        {
            public int I;
        }

        public class AData
        {
            public int I;
        }

        public class ADataConverter : DataConverter<AData, A>
        {
            public override void ConvertFromData(ConverterContext converterContext, AData data, ref A obj)
            {
                obj = new A { I = data.I };
            }

            public override void ConvertToData(ConverterContext converterContext, ref AData data, A obj)
            {
                data = new AData { I = obj.I };
            }

            [ModuleInitializer]
            internal static void Initialize()
            {
                // Register ADataConverter
                ConverterContext.RegisterConverter(new ADataConverter());
            }
        }

        [Test]
        public void Simple()
        {
            var obj = new A { I = 32 };
            var data = (AData)new ConverterContext().ConvertToData<object>(obj);

            Assert.That(data.I, Is.EqualTo(obj.I));

            var obj1 = (A)new ConverterContext().ConvertFromData<object>(data);

            Assert.That(obj1.I, Is.EqualTo(data.I));
        }

        [Test]
        public void ListToData()
        {
            var obj = new List<A> { new A { I = 12 }, new A { I = 13 } };
            var data = (List<AData>)new ConverterContext().ConvertToData<object>(obj);

            Assert.That(data.Count, Is.EqualTo(2));
            Assert.That(data[0].I, Is.EqualTo(obj[0].I));
            Assert.That(data[0].I, Is.EqualTo(obj[0].I));
        }

        [Test]
        public void DataToList()
        {
            var data = new List<AData> { new AData { I = 12 }, new AData { I = 13 } };
            var obj = (List<A>)new ConverterContext().ConvertFromData<object>(data);

            Assert.That(obj.Count, Is.EqualTo(2));
            Assert.That(obj[0].I, Is.EqualTo(data[0].I));
            Assert.That(obj[0].I, Is.EqualTo(data[0].I));
        }

        [Test]
        public void ContentReferences()
        {
            var obj = new A { I = 32 };
            var data = new ConverterContext().ConvertToData<ContentReference<AData>>(obj);

            Assert.That(data.Value.I, Is.EqualTo(obj.I));

            var obj2 = new ConverterContext().ConvertFromData<A>(data);

            Assert.That(obj2.I, Is.EqualTo(obj.I));
        }

        [Test]
        public void Context()
        {
            var context = new ConverterContext();

            var obj = new A { I = 32 };
            var data1 = (AData)context.ConvertToData<object>(obj);
            var data2 = (AData)context.ConvertToData<object>(obj);

            Assert.That(data1, Is.EqualTo(data2));

            var obj1 = (A)context.ConvertFromData<object>(data1);
            var obj2 = (A)context.ConvertFromData<object>(data1);

            Assert.That(obj1, Is.EqualTo(obj2));
        }
    }
}