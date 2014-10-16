// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System.IO;

namespace SiliconStudio.Shaders.Parser
{
    /// <summary>
    /// Callback interface to handle included file requested by the <see cref="PreProcessor"/>.
    /// </summary>
    public interface IncludeHandler
    {
        /// <summary>
        /// A user-implemented method for opening and reading the contents of a shader #include file.
        /// </summary>
        /// <param name="type">A <see cref="IncludeType"/>-typed value that indicates the location of the #include file.</param>
        /// <param name="fileName">Name of the #include file.</param>
        /// <param name="parentStream">Pointer to the container that includes the #include file.</param>
        /// <returns>Stream that is associated with fileName to be read. This reference remains valid until <see cref="IncludeHandler.Close"/> is called.</returns>
        Stream Open(IncludeType type, string fileName, Stream parentStream);

        /// <summary>	
        /// A user-implemented method for closing a shader #include file.	
        /// </summary>	
        /// <remarks>	
        /// If <see cref="IncludeHandler.Open"/> was successful, Close is guaranteed to be called before the API using the <see cref="IncludeHandler"/> interface returns.	    
        /// </remarks>	
        /// <param name="stream">This is a reference that was returned by the corresponding <see cref="IncludeHandler.Open"/> call.</param>
        void Close(Stream stream);
    }
}