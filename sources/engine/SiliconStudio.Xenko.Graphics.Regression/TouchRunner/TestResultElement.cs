// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;

#if XAMCORE_2_0
using UIKit;
#else
using MonoTouch.UIKit;
#endif

using MonoTouch.Dialog;

using NUnit.Framework;
using NUnit.Framework.Internal;

namespace SiliconStudio.Paradox.UnitTesting.UI {
	
	class TestResultElement : StyledMultilineElement {
		
		public TestResultElement (TestResult result) : 
			base (result.Message ?? "Unknown error", result.StackTrace, UITableViewCellStyle.Subtitle)
		{
		}
	}
}
