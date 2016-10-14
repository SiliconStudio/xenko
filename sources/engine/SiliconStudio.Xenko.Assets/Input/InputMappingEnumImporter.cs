// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;
using System.Diagnostics.SymbolStore;
using System.Runtime.InteropServices;
using SiliconStudio.Assets;
using SiliconStudio.Core.IO;
using SiliconStudio.Core.Reflection;
using SiliconStudio.Core.Serialization;
using SiliconStudio.Xenko.Assets.Audio;

namespace SiliconStudio.Xenko.Assets.Input
{
    public class InputMappingEnumImporter : AssetImporterBase
    {
        // Supported file extensions for this importer
        public const string FileExtensions = ".cs";

        private static readonly Guid Uid = new Guid("7b4bb142-1dc1-45ab-b213-ef2a8c6e84e9");
        public override Guid Id => Uid;

        public override string Description => "Importer for input enumeration";

        public override string SupportedFileExtensions => FileExtensions;

        public override IEnumerable<Type> RootAssetTypes
        {
            get { yield return typeof(SoundAsset); }
        }

        public override IEnumerable<AssetItem> Import(UFile rawAssetPath, AssetImporterParameters importParameters)
        {
            var asset = new InputMappingAsset { Source = rawAssetPath };

            string fileName = rawAssetPath.GetFileName();
            AssemblyRegistry.GetTypeFromAlias(fileName);

            var assemblies = AssemblyRegistry.FindAll();
            foreach (var assembly in assemblies)
            {
                ISymbolReader reader = GetSymbolReaderForFile(new SymBinder(), assembly.Location, null);
                ISymbolDocument[] documents = reader.GetDocuments();
                foreach (var doc in documents)
                {
                    if (doc.URL == rawAssetPath)
                    {
                        
                    }
                }
                //ISymbolReader reader = assembly.Location;
            }

            // Creates the url to the texture
            var url = new UFile(rawAssetPath.GetFileName());

            //SymUtil.Get
            //SymBinder binder = new SymBinder();
            //SymDocument doc = new SymDocument(); 

            yield return new AssetItem(url, asset);
        }

        [DllImport("ole32.dll")]
        public static extern int CoCreateInstance(
            [In] ref Guid rclsid,
            [In, MarshalAs(UnmanagedType.IUnknown)] Object pUnkOuter,
            [In] uint dwClsContext,
            [In] ref Guid riid,
            [Out, MarshalAs(UnmanagedType.Interface)] out Object ppv);

        [System.Security.Permissions.SecurityPermission(System.Security.Permissions.SecurityAction.Demand, Flags = System.Security.Permissions.SecurityPermissionFlag.UnmanagedCode)]
        public static ISymbolReader GetSymbolReaderForFile(SymBinder binder, string pathModule, string searchPath)
        {
            // Guids for imported metadata interfaces.
            Guid dispenserClassID = new Guid(0xe5cb7a31, 0x7512, 0x11d2, 0x89,
                0xce, 0x00, 0x80, 0xc7, 0x92, 0xe5, 0xd8); // CLSID_CorMetaDataDispenser
            Guid dispenserIID = new Guid(0x809c652e, 0x7396, 0x11d2, 0x97, 0x71,
                0x00, 0xa0, 0xc9, 0xb4, 0xd5, 0x0c); // IID_IMetaDataDispenser
            Guid importerIID = new Guid(0x7dac8207, 0xd3ae, 0x4c75, 0x9b, 0x67,
                0x92, 0x80, 0x1a, 0x49, 0x7d, 0x44); // IID_IMetaDataImport

            // First create the Metadata dispenser.
            object objDispenser;
            CoCreateInstance(ref dispenserClassID, null, 1,
                ref dispenserIID, out objDispenser);

            // Now open an Importer on the given filename. We'll end up passing this importer 
            // straight through to the Binder.
            object objImporter;
            IMetaDataDispenser dispenser = (IMetaDataDispenser)objDispenser;
            dispenser.OpenScope(pathModule, 0, ref importerIID, out objImporter);

            IntPtr importerPtr = IntPtr.Zero;
            ISymbolReader reader;
            try
            {
                // This will manually AddRef the underlying object, so we need to 
                // be very careful to Release it.
                importerPtr = Marshal.GetComInterfaceForObject(objImporter,
                    typeof(IMetadataImport));

                reader = binder.GetReader(importerPtr, pathModule, searchPath);
            }
            finally
            {
                if (importerPtr != IntPtr.Zero)
                {
                    Marshal.Release(importerPtr);
                }
            }
            return reader;
        }

        [Guid("809c652e-7396-11d2-9771-00a0c9b4d50c"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        [ComVisible(true)]
        interface IMetaDataDispenser
        {
            // We need to be able to call OpenScope, which is the 2nd vtable slot.
            // Thus we need this one placeholder here to occupy the first slot..
            void DefineScope_Placeholder();

            void OpenScope([In, MarshalAs(UnmanagedType.LPWStr)] String szScope,
                [In] Int32 dwOpenFlags, [In] ref Guid riid,
                [Out, MarshalAs(UnmanagedType.IUnknown)] out Object punk);
        }

        [Guid("7DAC8207-D3AE-4c75-9B67-92801A497D44"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        [ComVisible(true)]
        [CLSCompliant(true)]
        public interface IMetadataImport
        {
            // Just need a single placeholder method so that it doesn't complain
            // about an empty interface.
            void Placeholder();
        }
    }
}