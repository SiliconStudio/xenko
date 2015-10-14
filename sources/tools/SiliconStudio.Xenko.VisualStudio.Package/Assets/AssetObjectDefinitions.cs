// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under MIT License. See LICENSE.md for details.
//
//  This file is part of YamlDotNet - A .NET library for YAML.
//  Copyright (c) 2008, 2009, 2010, 2011, 2012, 2013 Antoine Aubry

//  Permission is hereby granted, free of charge, to any person obtaining a copy of
//  this software and associated documentation files (the "Software"), to deal in
//  the Software without restriction, including without limitation the rights to
//  use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies
//  of the Software, and to permit persons to whom the Software is furnished to do
//  so, subject to the following conditions:

//  The above copyright notice and this permission notice shall be included in all
//  copies or substantial portions of the Software.

//  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//  IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//  FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//  AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//  LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//  OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
//  SOFTWARE.

using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Utilities;

namespace SiliconStudio.Xenko.VisualStudio.Assets
{
	internal static class AssetObjectDefinitions
	{
        /// <summary>
        /// Content Type
        /// </summary>
        [Export]
        [Name(Constants.ContentType)]
        [BaseDefinition("code")]
        internal static ContentTypeDefinition hidingContentTypeDefinition = null;

        /// <summary>
        /// File extensions
        /// </summary>
		[Export]
		[FileExtension(".xkpkg")]
		[ContentType(Constants.ContentType)]
		internal static FileExtensionToContentTypeDefinition xkpkgFileExtensionDefinition = null;
		
		[Export]
        [FileExtension(".xkfnt")]
        [ContentType(Constants.ContentType)]
        internal static FileExtensionToContentTypeDefinition xkfntFileExtensionDefinition = null;

        [Export]
        [FileExtension(".xkfxlib")]
        [ContentType(Constants.ContentType)]
        internal static FileExtensionToContentTypeDefinition xkfxlibFileExtensionDefinition = null;

        [Export]
        [FileExtension(".xklightconf")]
        [ContentType(Constants.ContentType)]
        internal static FileExtensionToContentTypeDefinition xklightconfFileExtensionDefinition = null;

        [Export]
        [FileExtension(".xktex")]
        [ContentType(Constants.ContentType)]
        internal static FileExtensionToContentTypeDefinition xktexFileExtensionDefinition = null;

        [Export]
        [FileExtension(".xkentity")]
        [ContentType(Constants.ContentType)]
        internal static FileExtensionToContentTypeDefinition xkentityFileExtensionDefinition = null;


        [Export]
        [FileExtension(".xkm3d")]
        [ContentType(Constants.ContentType)]
        internal static FileExtensionToContentTypeDefinition xkm3dFileExtensionDefinition = null;

        [Export]
        [FileExtension(".xkanim")]
        [ContentType(Constants.ContentType)]
        internal static FileExtensionToContentTypeDefinition xkanimFileExtensionDefinition = null;

        [Export]
        [FileExtension(".xksnd")]
        [ContentType(Constants.ContentType)]
        internal static FileExtensionToContentTypeDefinition xksndFileExtensionDefinition = null;

        [Export]
        [FileExtension(".xkmat")]
        [ContentType(Constants.ContentType)]
        internal static FileExtensionToContentTypeDefinition xkmatFileExtensionDefinition = null;

        [Export]
        [FileExtension(".xksprite")]
        [ContentType(Constants.ContentType)]
        internal static FileExtensionToContentTypeDefinition xksprtFileExtensionDefinition = null;

        /// <summary>
        /// Classification type definitions
        /// </summary>
        [Export(typeof(ClassificationTypeDefinition))]
        [Name(AnchorClassificationName)]
		internal static ClassificationTypeDefinition YamlAnchorType = null;
	    public const string AnchorClassificationName = "Xenko.YamlAnchor";

		[Export(typeof(ClassificationTypeDefinition))]
        [Name(AliasClassificationName)]
		internal static ClassificationTypeDefinition YamlAliasType = null;
        public const string AliasClassificationName = "Xenko.YamlAlias";

        [Export(typeof(ClassificationTypeDefinition))]
        [Name(NumberClassificationName)]
        internal static ClassificationTypeDefinition YamlNumberType = null;
        public const string NumberClassificationName = "Xenko.YamlNumber";

        [Export(typeof(ClassificationTypeDefinition))]
        [Name(KeyClassificationName)]
        internal static ClassificationTypeDefinition YamlKeyType = null;
        public const string KeyClassificationName = "Xenko.YamlKey";

        [Export(typeof(ClassificationTypeDefinition))]
        [Name(ErrorClassificationName)]
        internal static ClassificationTypeDefinition YamlErrorType = null;
        public const string ErrorClassificationName = "Xenko.YamlError";
    }
}
