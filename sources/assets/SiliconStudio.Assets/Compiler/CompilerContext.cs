// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

namespace SiliconStudio.Assets.Compiler
{
    /// <summary>
    /// The context used when compiling an asset in a Package.
    /// </summary>
    public class CompilerContext
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CompilerContext"/> class.
        /// </summary>
        public CompilerContext()
        {
            Properties = new PropertyCollection();
        }

        /// <summary>
        /// Gets the attributes attached to this context.
        /// </summary>
        /// <value>The attributes.</value>
        public PropertyCollection Properties { get; private set; }

        public CompilerContext Clone()
        {
            var context = (CompilerContext)MemberwiseClone();
            return context;
        }
    }
}