// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using SiliconStudio.Shaders.Parser;

namespace SiliconStudio.Shaders
{
    /// <summary>
    /// C++ preprocessor using D3DPreprocess method from d3dcompiler API.
    /// </summary>
    public partial class PreProcessor
    {
#if !FRAMEWORK_SHADER_USE_SHARPDX
        [Guid("8BA5FB08-5195-40e2-AC58-0D989C3A0102"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        internal interface IBlob
        {
            [PreserveSig]
            System.IntPtr GetBufferPointer();

            [PreserveSig]
            System.IntPtr GetBufferSize();
        }

        [DllImport("d3dcompiler_43.dll", EntryPoint = "D3DPreprocess", PreserveSig = true, CallingConvention = CallingConvention.Winapi, CharSet = CharSet.Ansi)]
        private static extern int D3DPreprocess(IntPtr ptrData, IntPtr size, [MarshalAs(UnmanagedType.LPStr)] string pSourceName, [MarshalAs(UnmanagedType.LPArray)] ShaderMacro[] pDefines, IntPtr pInclude, [Out] out IBlob ppCodeText, [Out] out IBlob ppErrorMsgs);
#endif

        /// <summary>
        /// Preprocesses the provided shader or effect source.
        /// </summary>
        /// <param name="shaderSource">An array of bytes containing the raw source of the shader or effect to preprocess.</param>
        /// <param name="sourceFileName">Name of the source file.</param>
        /// <param name="defines">A set of macros to define during preprocessing.</param>
        /// <param name="includeDirectories">The include directories used by the preprocessor.</param>
        /// <returns>
        /// The preprocessed shader source.
        /// </returns>
        public static string Run(string shaderSource, string sourceFileName, ShaderMacro[] defines = null, params string[] includeDirectories)
        {
            // Use a default include handler
            var defaultHandler = new DefaultIncludeHandler();

            if (includeDirectories != null)
                defaultHandler.AddDirectories(includeDirectories);

            defaultHandler.AddDirectory(Environment.CurrentDirectory);

            var directoryName = Path.GetDirectoryName(sourceFileName);
            if (!string.IsNullOrEmpty(directoryName))
                defaultHandler.AddDirectory(directoryName);

            // Run the processor
            return Run(shaderSource, sourceFileName, defines, defaultHandler);
        }

        /// <summary>
        /// Preprocesses the provided shader or effect source.
        /// </summary>
        /// <param name="shaderSource">An array of bytes containing the raw source of the shader or effect to preprocess.</param>
        /// <param name="sourceFileName">Name of the source file.</param>
        /// <param name="defines">A set of macros to define during preprocessing.</param>
        /// <param name="include">An interface for handling include files.</param>
        /// <returns>
        /// The preprocessed shader source.
        /// </returns>
        public static string Run(string shaderSource, string sourceFileName = null, ShaderMacro[] defines = null, IncludeHandler include = null)
        {
            // Stringify/Concat not supported by D3DCompiler preprocessor.
            shaderSource = ConcatenateTokens(shaderSource, defines);

            try
            {
#if FRAMEWORK_SHADER_USE_SHARPDX
                string compilationErrors;
                shaderSource = SharpDX.D3DCompiler.ShaderBytecode.Preprocess(
                    shaderSource,
                    defines != null ? defines.Select(x => new SharpDX.Direct3D.ShaderMacro(x.Name, x.Definition)).ToArray() : null,
                    new IncludeShadow(include),
                    out compilationErrors, sourceFileName);
#else
                IBlob blobForText = null;
                IBlob blobForErrors = null;

                var shadow = include == null ? null : new IncludeShadow(include);

                var data = Encoding.ASCII.GetBytes(shaderSource);
                int result;
                unsafe
                {
                    fixed (void* pData = data)
                        result = D3DPreprocess((IntPtr)pData, new IntPtr(data.Length), sourceFileName, PrepareMacros(defines), shadow != null ? shadow.NativePointer : IntPtr.Zero, out blobForText, out blobForErrors);
                }

                if (shadow != null)
                    shadow.Dispose();

                if (result < 0)
                    throw new InvalidOperationException(string.Format("Include errors: {0}", blobForErrors == null ? "" : Marshal.PtrToStringAnsi(blobForErrors.GetBufferPointer())));

                shaderSource = Marshal.PtrToStringAnsi(blobForText.GetBufferPointer());
#endif
            } catch (Exception ex)
            {
                Console.WriteLine("Warning, error while preprocessing file [{0}] : {1}", sourceFileName, ex.Message);
            }
            return shaderSource;
        }

        internal static ShaderMacro[] PrepareMacros(ShaderMacro[] macros)
        {
            if (macros == null)
                return null;

            if (macros.Length == 0)
                return null;

            if (macros[macros.Length - 1].Name == null && macros[macros.Length - 1].Definition == null)
                return macros;

            var macroArray = new ShaderMacro[macros.Length + 1];

            Array.Copy(macros, macroArray, macros.Length);

            macroArray[macros.Length] = new ShaderMacro();
            return macroArray;
        }

#if FRAMEWORK_SHADER_USE_SHARPDX
        /// <summary>
        /// Shadow callback for <see cref="IncludeHandler"/>.
        /// </summary>
        internal class IncludeShadow : SharpDX.CallbackBase, SharpDX.D3DCompiler.Include
        {
            private readonly IncludeHandler callback;

            public IncludeShadow(IncludeHandler callback)
            {
                this.callback = callback;
            }

            public Stream Open(SharpDX.D3DCompiler.IncludeType type, string fileName, Stream parentStream)
            {
                return callback.Open((IncludeType)type, fileName, parentStream);
            }

            public void Close(Stream stream)
            {
                callback.Close(stream);
            }
        }
#else
        /// <summary>
        /// Shadow callback for <see cref="IncludeHandler"/>.
        /// </summary>
        internal class IncludeShadow : IDisposable
        {
            private static readonly IncludeVtbl Vtbl = new IncludeVtbl();
            private readonly GCHandle handle;
            private Dictionary<IntPtr, Frame> _frames;

            public IntPtr NativePointer;

            public IncludeHandler Callback { get; set; }

            public IncludeShadow(IncludeHandler callback)
            {
                this.Callback = callback;
                // Allocate ptr to vtbl + ptr to callback together
                NativePointer = Marshal.AllocHGlobal(IntPtr.Size * 2);

                handle = GCHandle.Alloc(this);
                Marshal.WriteIntPtr(NativePointer, Vtbl.Pointer);
                Marshal.WriteIntPtr(NativePointer, IntPtr.Size, GCHandle.ToIntPtr(handle));

                _frames = new Dictionary<IntPtr, Frame>();
            }

            /// <summary>
            /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
            /// </summary>
            public void Dispose()
            {
                Dispose(true);
            }

            protected void Dispose(bool disposing)
            {
                if (NativePointer != IntPtr.Zero)
                {
                    handle.Free();

                    foreach (var frame in _frames)
                        frame.Value.Close();
                    _frames = null;

                    // Free instance
                    Marshal.FreeHGlobal(NativePointer);
                    NativePointer = IntPtr.Zero;
                }
                Callback = null;
            }


            /// <summary>
            ///   Read stream to a byte[] buffer
            /// </summary>
            /// <param name = "stream">input stream</param>
            /// <returns>a byte[] buffer</returns>
            private static byte[] ReadStream(Stream stream)
            {
                int readLength = 0;
                return ReadStream(stream, ref readLength);
            }

            /// <summary>
            ///   Read stream to a byte[] buffer
            /// </summary>
            /// <param name = "stream">input stream</param>
            /// <param name = "readLength">length to read</param>
            /// <returns>a byte[] buffer</returns>
            private static byte[] ReadStream(Stream stream, ref int readLength)
            {
                int num = readLength;
                if (num == 0)
                    readLength = (int)(stream.Length - stream.Position);
                num = readLength;

                System.Diagnostics.Debug.Assert(num >= 0);
                if (num == 0)
                    return new byte[0];

                byte[] buffer = new byte[num];
                int bytesRead = 0;
                if (num > 0)
                {
                    do
                    {
                        bytesRead += stream.Read(buffer, bytesRead, readLength - bytesRead);
                    } while (bytesRead < readLength);
                }
                return buffer;
            }

            private class Frame
            {
                public Frame(Stream stream, GCHandle handle)
                {
                    Stream = stream;
                    Handle = handle;
                }

                public bool IsClosed;
                public Stream Stream;
                public GCHandle Handle;

                public void Close()
                {
                    if (IsClosed)
                        return;
                    Stream = null;
                    Handle.Free();
                    IsClosed = true;
                }
            }

            /// <summary>
            /// Internal Include Callback
            /// </summary>
            private class IncludeVtbl
            {
                public IntPtr Pointer;
                private List<Delegate> methods;

                /// <summary>
                /// Add a method supported by this interface. This method is typically called from inherited constructor.
                /// </summary>
                /// <param name="method">the managed delegate method</param>
                private void AddMethod(Delegate method)
                {
                    int index = methods.Count;
                    methods.Add(method);
                    Marshal.WriteIntPtr(Pointer, index * IntPtr.Size, Marshal.GetFunctionPointerForDelegate(method));
                }

                public IncludeVtbl()
                {
                    // Allocate ptr to vtbl
                    Pointer = Marshal.AllocHGlobal(IntPtr.Size * 2);
                    methods = new List<Delegate>();

                    AddMethod(new OpenDelegate(OpenImpl));
                    AddMethod(new CloseDelegate(CloseImpl));
                }


                private static IncludeShadow ToShadow(IntPtr thisPtr)
                {
                    var handle = GCHandle.FromIntPtr(Marshal.ReadIntPtr(thisPtr, IntPtr.Size));
                    return (IncludeShadow)handle.Target;
                }

                /// <summary>	
                /// A user-implemented method for opening and reading the contents of a shader #include file.	
                /// </summary>
                /// <param name="thisPtr">This pointer</param>
                /// <param name="includeType">A <see cref="SharpDX.D3DCompiler.IncludeType"/>-typed value that indicates the location of the #include file. </param>
                /// <param name="fileNameRef">Name of the #include file.</param>
                /// <param name="pParentData">Pointer to the container that includes the #include file.</param>
                /// <param name="dataRef">Pointer to the buffer that Open returns that contains the include directives. This pointer remains valid until <see cref="SharpDX.D3DCompiler.Include.Close"/> is called.</param>
                /// <param name="bytesRef">Pointer to the number of bytes that Open returns in ppData.</param>
                /// <returns>The user-implemented method should return S_OK. If Open fails when reading the #include file, the application programming interface (API) that caused Open to be called fails. This failure can occur in one of the following situations:The high-level shader language (HLSL) shader fails one of the D3D10CompileShader*** functions.The effect fails one of the D3D10CreateEffect*** functions.</returns>
                [UnmanagedFunctionPointer(CallingConvention.StdCall)]
                private delegate int OpenDelegate(IntPtr thisPtr, IncludeType includeType, IntPtr fileNameRef, IntPtr pParentData, out IntPtr dataRef, out int bytesRef);
                private static int OpenImpl(IntPtr thisPtr, IncludeType includeType, IntPtr fileNameRef, IntPtr pParentData, out IntPtr dataRef, out int bytesRef)
                {
                    dataRef = IntPtr.Zero;
                    bytesRef = 0;
                    try
                    {

                        var shadow = ToShadow(thisPtr);
                        var callback = shadow.Callback;

                        Stream stream = null;
                        Stream parentStream = null;

                        if (shadow._frames.ContainsKey(pParentData))
                            parentStream = shadow._frames[pParentData].Stream;

                        stream = callback.Open(includeType, Marshal.PtrToStringAnsi(fileNameRef), parentStream);
                        if (stream == null)
                            return -1;

                        // Read the stream into a byte array and pin it
                        byte[] data = ReadStream(stream);
                        var handle = GCHandle.Alloc(data, GCHandleType.Pinned);
                        dataRef = handle.AddrOfPinnedObject();
                        bytesRef = data.Length;

                        shadow._frames.Add(dataRef, new Frame(stream, handle));

                        return 0;
                    }
                    catch (Exception)
                    {
                        return -1;
                    }
                }

                /// <summary>	
                /// A user-implemented method for closing a shader #include file.	
                /// </summary>	
                /// <remarks>	
                /// If <see cref="SharpDX.D3DCompiler.Include.Open"/> was successful, Close is guaranteed to be called before the API using the <see cref="SharpDX.D3DCompiler.Include"/> interface returns.	
                /// </remarks>
                /// <param name="thisPtr">This pointer</param>
                /// <param name="pData">Pointer to the buffer that contains the include directives. This is the pointer that was returned by the corresponding <see cref="SharpDX.D3DCompiler.Include.Open"/> call.</param>
                /// <returns>The user-implemented Close method should return S_OK. If Close fails when it closes the #include file, the application programming interface (API) that caused Close to be called fails. This failure can occur in one of the following situations:The high-level shader language (HLSL) shader fails one of the D3D10CompileShader*** functions.The effect fails one of the D3D10CreateEffect*** functions.</returns>
                [UnmanagedFunctionPointer(CallingConvention.StdCall)]
                private delegate int CloseDelegate(IntPtr thisPtr, IntPtr pData);
                private static int CloseImpl(IntPtr thisPtr, IntPtr pData)
                {
                    try
                    {
                        var shadow = ToShadow(thisPtr);
                        var callback = shadow.Callback;

                        Frame frame;
                        if (shadow._frames.TryGetValue(pData, out frame))
                        {
                            callback.Close(frame.Stream);
                        }
                        return 0;
                    }
                    catch (Exception)
                    {
                        return -1;
                    }
                }
            }
        }
#endif

        private readonly static Regex ConcatenateTokensRegex = new Regex(@"(\w*)\s*#(#)?\s*(\w+)", RegexOptions.Compiled);
        private readonly static HashSet<string> PreprocessorKeywords = new HashSet<string>(new[] { "if", "else", "elif", "endif", "define", "undef", "ifdef", "ifndef", "line", "error", "pragma", "include" });

        private static string ConcatenateTokens(string source, IEnumerable<ShaderMacro> macros)
        {
            // TODO: This code is not reliable with comments
            // Avoid using costly regex if there is no # tokens
            if (!source.Contains('#'))
            {
                return source;
            }

            var newSource = new StringBuilder();
            var reader = new StringReader(source);
            string sourceLine;
            while ((sourceLine= reader.ReadLine()) != null)
            {
                string line;
                string comment = null;

                // If source starts by a pragma, skip this line
                if (sourceLine.TrimStart().StartsWith("#"))
                {
                    newSource.AppendLine(sourceLine);
                    continue;
                }

                // If source starts by a comment, separate it from the part to process
                int indexComment = sourceLine.IndexOf("//", StringComparison.InvariantCultureIgnoreCase);
                if (indexComment >= 0)
                {
                    comment = sourceLine.Substring(indexComment, sourceLine.Length - indexComment);
                    line = sourceLine.Substring(0, indexComment);
                }
                else
                {
                    line = sourceLine;
                }

                // Process every A ## B ## C ## ... patterns
                // Find first match
                int position = 0;
                var match = ConcatenateTokensRegex.Match(line, position);

                // Early exit
                if (!match.Success)
                {
                    newSource.AppendLine(sourceLine);
                    continue;
                }

                // Create map of macros.
                Dictionary<string, string> macroMap = null;
                int addLength = 0;
                if (macros != null)
                {
                    macroMap = new Dictionary<string, string>();
                    foreach (var shaderMacro in macros)
                    {
                        macroMap[shaderMacro.Name] = shaderMacro.Definition;

                        if (shaderMacro.Definition != null)
                        {
                            addLength += shaderMacro.Definition.Length;
                        }
                    }
                }

                var stringBuilder = new StringBuilder(line.Length + addLength);

                while (match.Success)
                {
                    // Add what was before regex
                    stringBuilder.Append(line, position, match.Index - position);

                    // Check if # (stringify) or ## (concat)
                    bool stringify = !match.Groups[2].Success;

                    var group = match.Groups[3];
                    var token = group.Value;
                    if (stringify && PreprocessorKeywords.Contains(token))
                    {
                        // Ignore some special preprocessor tokens
                        stringBuilder.Append(match.Groups[0].Value);
                    }
                    else
                    {
                        // Expand and add first macro
                        stringBuilder.Append(TransformToken(match.Groups[1].Value, macroMap));

                        if (stringify) // stringification
                        {
                            stringBuilder.Append('"');
                            // TODO: Escape string
                            stringBuilder.Append(EscapeString(TransformToken(token, macroMap, true)));
                            stringBuilder.Append('"');
                        }
                        else // concatenation
                        {
                            stringBuilder.Append(TransformToken(token, macroMap));
                        }
                    }

                    // Find next match
                    position = group.Index + group.Length;
                    match = ConcatenateTokensRegex.Match(line, position);
                }

                // Add what is after regex
                stringBuilder.Append(line, position, line.Length - position);

                if (comment != null)
                {
                    stringBuilder.Append(comment);
                }

                newSource.AppendLine(stringBuilder.ToString());
            }

            return newSource.ToString();
        }

        private static string TransformToken(string token, Dictionary<string, string> macros, bool emptyIfNotFound = false)
        {
            if (macros == null)
                return token;

            string result;
            return macros.TryGetValue(token, out result) ? result : emptyIfNotFound ? string.Empty : token;
        }

        private static string EscapeString(string s)
        {
            return s.Replace("\\", "\\\\").Replace("\"", "\\\"");
        }
    }
}
