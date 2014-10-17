// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;

using SiliconStudio.Core.Mathematics;

namespace SiliconStudio.Presentation.Legacy
{
	public class ThicknessElementsMapper : IVectorElementsMapper
	{
		public IEnumerable<string> GetVectorElementNames()
		{
			yield return "Left";
			yield return "Top";
			yield return "Right";
			yield return "Bottom";
		}
	}

	public class SizeElementsMapper : IVectorElementsMapper
	{
		public IEnumerable<string> GetVectorElementNames()
		{
			yield return "Width";
			yield return "Height";
		}
	}

	public class Vector3ElementsMapper : IVectorElementsMapper
	{
		public IEnumerable<string> GetVectorElementNames()
		{
			yield return "X";
			yield return "Y";
			yield return "Z";
		}
	}

	public class Vector4ElementsMapper : IFullVectorElementsMapper
	{
		public IEnumerable<string> GetVectorElementNames()
		{
			yield return "X";
			yield return "Y";
			yield return "Z";
			yield return "W";
		}

		public object GetValue(object instance, string memberName)
		{
			if (memberName == "X")
				return ((Vector4)instance).X;
			else if (memberName == "Y")
				return ((Vector4)instance).Y;
			else if (memberName == "Z")
				return ((Vector4)instance).Z;
			else if (memberName == "W")
				return ((Vector4)instance).W;
			throw new ArgumentException(string.Format("Invalid member name"));
		}

		public bool SetValue(object instance, string memberName, object value)
		{
			Vector4 vec = (Vector4)instance;
			bool result = false;

			if (memberName == "X")
			{
				result = !vec.X.Equals(value);
				vec.X = (float)value;
			}
			else if (memberName == "Y")
			{
				result = !vec.Y.Equals(value);
				vec.Y = (float)value;
			}
			else if (memberName == "Z")
			{
				result = !vec.Z.Equals(value);
				vec.Z = (float)value;
			}
			else if (memberName == "W")
			{
				result = !vec.W.Equals(value);
				vec.W = (float)value;
			}
			else
				throw new ArgumentException(string.Format("Invalid member name"));

			return result;
		}
	}

	public class MatrixElementsMapper : IVectorElementsMapper
	{
		private bool isRowMajor;

		public MatrixElementsMapper(bool isRowMajor = true)
		{
			this.isRowMajor = isRowMajor;
		}

		public IEnumerable<string> GetVectorElementNames()
		{
			for (int x = 1; x <= 4; x++)
			{
				for (int y = 1; y <= 4; y++)
				{
					if (isRowMajor)
						yield return string.Format("M{0}{1}", x, y);
					else
						yield return string.Format("M{0}{1}", y, x);
				}
			}
		}
	}
}
