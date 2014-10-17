// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;

namespace SiliconStudio.Presentation.Legacy
{
	public class VectorLayoutDescriptor
	{
		public VectorLayoutDescriptor(Type vectorType, SizeI vectorLayoutSize, IVectorElementsMapper vectorElementsMapper)
		{
			if (vectorType == null)
				throw new ArgumentNullException("vectorType");
			if (vectorLayoutSize.IsEmpty || vectorLayoutSize.X < 0 || vectorLayoutSize.Y < 0)
				throw new ArgumentException("vectorLayoutSize");
			if (vectorElementsMapper == null)
				throw new ArgumentNullException("vectorElementsMapper");

			VectorType = vectorType;
			LayoutSize = vectorLayoutSize;
			ElementsMapper = vectorElementsMapper;
		}

		public Type VectorType { get; private set; }
		public SizeI LayoutSize { get; private set; }
		public IVectorElementsMapper ElementsMapper { get; private set; }
	}
}
