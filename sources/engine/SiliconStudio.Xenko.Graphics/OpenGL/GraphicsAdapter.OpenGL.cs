// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

#if SILICONSTUDIO_XENKO_GRAPHICS_API_OPENGL
using System.Linq;
using SiliconStudio.Xenko.Graphics.OpenGL;
#if SILICONSTUDIO_XENKO_GRAPHICS_API_OPENGLES
using OpenTK.Graphics.ES30;
#else
using OpenTK.Graphics.OpenGL;
#endif
namespace SiliconStudio.Xenko.Graphics
{
    /// <summary>
    /// Provides methods to retrieve and manipulate graphics adapters.
    /// </summary>
    public partial class GraphicsAdapter
    {
        private GraphicsProfile supportedGraphicsProfile;

        internal GraphicsAdapter()
        {
            outputs = new [] { new GraphicsOutput() };

            // set default values
            int versionMajor = 1;
            int versionMinor = 0;

            // get real values
            // using glGetIntegerv(GL_MAJOR_VERSION / GL_MINOR_VERSION) only works on opengl (es) > 3.0
            var version = GL.GetString(StringName.Version);
            if (version != null)
            {
                var splitVersion = version.Split(new char[] { '.', ' ' });
                // find first number occurrence because:
                //   - on OpenGL, "<major>.<minor>"
                //   - on OpenGL ES, "OpenGL ES <profile> <major>.<minor>"
                for (var i = 0; i < splitVersion.Length - 1; ++i)
                {
                    if (int.TryParse(splitVersion[i], out versionMajor))
                    {
                        // Note: minor version might have stuff concat, take only until not digits
                        var versionMinorString = splitVersion[i + 1];
                        versionMinorString = new string(versionMinorString.TakeWhile(c => char.IsDigit(c)).ToArray());

                        int.TryParse(versionMinorString, out versionMinor);
                        break;
                    }
                }
            }

            supportedGraphicsProfile = OpenGLUtils.GetFeatureLevel(versionMajor, versionMinor);
        }

        public bool IsProfileSupported(GraphicsProfile graphicsProfile)
        {
            // TODO: Check OpenGL version?
            // TODO: ES specific code?
            return graphicsProfile <= supportedGraphicsProfile;
        }

        /// <summary>
        /// Gets the description of this adapter.
        /// </summary>
        /// <value>The description.</value>
        public string Description
        {
            get { return "Default OpenGL Adapter"; }
        }

        /// <summary>
        /// Determines if this instance of GraphicsAdapter is the default adapter.
        /// </summary>
        public bool IsDefaultAdapter
        {
            get { return true; }
        }

        /// <summary>
        /// Gets or sets the vendor identifier.
        /// </summary>
        /// <value>
        /// The vendor identifier.
        /// </value>
        public int VendorId
        {
            get { return 0; }
        }
    }
}

#endif
