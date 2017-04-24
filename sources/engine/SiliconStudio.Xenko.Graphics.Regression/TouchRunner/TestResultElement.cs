// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System;

#if XAMCORE_2_0
using UIKit;
#else
using MonoTouch.UIKit;
#endif

using MonoTouch.Dialog;

using NUnit.Framework;
using NUnit.Framework.Internal;

namespace SiliconStudio.Xenko.UnitTesting.UI {
	
	class TestResultElement : StyledMultilineElement {
		
		public TestResultElement (TestResult result) : 
			base (result.Message ?? "Unknown error", result.StackTrace, UITableViewCellStyle.Subtitle)
		{
		}
	}
}
