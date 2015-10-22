// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
#if SILICONSTUDIO_XENKO_GRAPHICS_API_NULL 
using System;
using System.Collections.ObjectModel;
using System.Drawing;
using SiliconStudio.Core.Mathematics;
using Rectangle = SiliconStudio.Core.Mathematics.Rectangle;

namespace SiliconStudio.Xenko.Graphics
{
    public partial class GraphicsAdapter
    {
        /// <summary>
        /// Tests to see if the adapter supports the requested profile.
        /// </summary>
        /// <param name="graphicsProfile">The graphics profile.</param>
        /// <returns>true if the profile is supported</returns>
        public bool IsProfileSupported(GraphicsProfile graphicsProfile)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets the current display mode.
        /// </summary>
        public DisplayMode CurrentDisplayMode
        {
            get { throw new NotImplementedException(); }
        }

        /// <summary>
        /// Retrieves a string used for presentation to the user.
        /// </summary>
        public string Description
        {
            get { throw new NotImplementedException(); }
        }

        /// <summary>
        /// Retrieves a value that is used to help identify a particular chip set.
        /// </summary>
        public int DeviceId
        {
            get { throw new NotImplementedException(); }
        }

        /// <summary>
        /// Retrieves a string that contains the device name for a Microsoft Windows Graphics Device Interface (GDI).
        /// </summary>
        public string DeviceName
        {
            get { throw new NotImplementedException(); }
        }

        /// <summary>
        /// Retrieves bounds of the desktop coordinates.
        /// </summary>
        public Rectangle DesktopBounds
        {
            get { throw new NotImplementedException(); }
        }

        /// <summary>
        /// Determines if this instance of GraphicsAdapter is the default adapter.
        /// </summary>
        public bool IsDefaultAdapter
        {
            get { throw new NotImplementedException(); }
        }

        /// <summary>
        /// Determines if the graphics adapter is in widescreen mode.
        /// </summary>
        public bool IsWideScreen
        {
            get { throw new NotImplementedException(); }
        }

        /// <summary>
        /// Retrieves the handle of the monitor associated with the Microsoft Direct3D object.
        /// </summary>
        public IntPtr MonitorHandle
        {
            get { throw new NotImplementedException(); }
        }

        /// <summary>
        /// Retrieves a value used to help identify the revision level of a particular chip set.
        /// </summary>
        public int Revision
        {
            get { throw new NotImplementedException(); }
        }

        /// <summary>
        /// Retrieves a value used to identify the subsystem.
        /// </summary>
        public int SubSystemId
        {
            get { throw new NotImplementedException(); }
        }

        /// <summary>
        /// Returns a collection of supported display modes for the current adapter.
        /// </summary>
        public ReadOnlyCollection<DisplayMode> SupportedDisplayModes
        {
            get { throw new NotImplementedException(); }
        }

        /// <summary>
        /// Retrieves a value used to identify the manufacturer.
        /// </summary>
        public int VendorId
        {
            get { throw new NotImplementedException(); }
        }

        /// <summary>
        /// Gets the best graphics profile supported by this adapter.
        /// </summary>
        public GraphicsProfile GraphicsProfile
        {
            get { throw new NotImplementedException(); }
        }
    }
} 
#endif
