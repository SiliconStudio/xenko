// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System.Collections.Generic;

namespace SiliconStudio.Presentation.Legacy
{
	public interface IVectorElementsMapper
	{
		IEnumerable<string> GetVectorElementNames();
	}

	public interface IFullVectorElementsMapper : IVectorElementsMapper
	{
		object GetValue(object instance, string memberName);
		bool SetValue(object instance, string memberName, object value);
	}
}
