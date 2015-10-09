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
		[FileExtension(".pdxpkg")]
		[ContentType(Constants.ContentType)]
		internal static FileExtensionToContentTypeDefinition pdxpkgFileExtensionDefinition = null;
		
		[Export]
        [FileExtension(".pdxfnt")]
        [ContentType(Constants.ContentType)]
        internal static FileExtensionToContentTypeDefinition pdxfntFileExtensionDefinition = null;

        [Export]
        [FileExtension(".pdxfxlib")]
        [ContentType(Constants.ContentType)]
        internal static FileExtensionToContentTypeDefinition pdxfxlibFileExtensionDefinition = null;

        [Export]
        [FileExtension(".pdxlightconf")]
        [ContentType(Constants.ContentType)]
        internal static FileExtensionToContentTypeDefinition pdxlightconfFileExtensionDefinition = null;

        [Export]
        [FileExtension(".pdxtex")]
        [ContentType(Constants.ContentType)]
        internal static FileExtensionToContentTypeDefinition pdxtexFileExtensionDefinition = null;

        [Export]
        [FileExtension(".pdxentity")]
        [ContentType(Constants.ContentType)]
        internal static FileExtensionToContentTypeDefinition pdxentityFileExtensionDefinition = null;


        [Export]
        [FileExtension(".pdxm3d")]
        [ContentType(Constants.ContentType)]
        internal static FileExtensionToContentTypeDefinition pdxm3dFileExtensionDefinition = null;

        [Export]
        [FileExtension(".pdxanim")]
        [ContentType(Constants.ContentType)]
        internal static FileExtensionToContentTypeDefinition pdxanimFileExtensionDefinition = null;

        [Export]
        [FileExtension(".pdxsnd")]
        [ContentType(Constants.ContentType)]
        internal static FileExtensionToContentTypeDefinition pdxsndFileExtensionDefinition = null;

        [Export]
        [FileExtension(".pdxmat")]
        [ContentType(Constants.ContentType)]
        internal static FileExtensionToContentTypeDefinition pdxmatFileExtensionDefinition = null;

        [Export]
        [FileExtension(".pdxsprite")]
        [ContentType(Constants.ContentType)]
        internal static FileExtensionToContentTypeDefinition pdxsprtFileExtensionDefinition = null;

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
