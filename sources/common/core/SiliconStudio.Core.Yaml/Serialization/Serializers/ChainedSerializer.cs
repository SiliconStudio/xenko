// Copyright (c) 2015 SharpYaml - Alexandre Mutel
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
// 
// -------------------------------------------------------------------------------
// SharpYaml is a fork of YamlDotNet https://github.com/aaubry/YamlDotNet
// published with the following license:
// -------------------------------------------------------------------------------
// 
// Copyright (c) 2008, 2009, 2010, 2011, 2012 Antoine Aubry
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of
// this software and associated documentation files (the "Software"), to deal in
// the Software without restriction, including without limitation the rights to
// use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies
// of the Software, and to permit persons to whom the Software is furnished to do
// so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using System;

namespace SiliconStudio.Core.Yaml.Serialization.Serializers
{
    /// <summary>
    /// An implementation of <see cref="IYamlSerializable"/> that will call the <see cref="ReadYaml"/> and <see cref="WriteYaml"/> methods
    /// of another serializer when invoked.
    /// </summary>
    public abstract class ChainedSerializer : IYamlSerializable
    {
        /// <summary>
        /// The chained serializer.
        /// </summary>
        private IYamlSerializable next;

        /// <summary>
        /// Sets the serializer to chain with this instance.
        /// </summary>
        /// <param name="other">The serializer to chain with this instance.</param>
        public void PrependTo(IYamlSerializable other)
        {
            if (next != null)
                throw new InvalidOperationException("This serializer already have a succeeding serializer");

            next = other;
        }

        /// <summary>
        /// Sets the serializer to chain with an instance of <see cref="ChainedSerializer"/>.
        /// </summary>
        /// <param name="chained">The chained serializer.</param>
        /// <param name="serializer">The serializer to chain.</param>
        /// <returns>The chained argument passed in the <paramref name="chained"/> parameter.</returns>
        public static ChainedSerializer Prepend(ChainedSerializer chained, IYamlSerializable serializer)
        {
            chained.PrependTo(serializer);
            return chained;
        }

        /// <inheritdoc/>
        public virtual object ReadYaml(ref ObjectContext objectContext)
        {
            return next.ReadYaml(ref objectContext);
        }

        /// <inheritdoc/>
        public virtual void WriteYaml(ref ObjectContext objectContext)
        {
            next.WriteYaml(ref objectContext);
        }
    }
}
