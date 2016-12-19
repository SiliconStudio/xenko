// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

// Generated from gamecontrollerdb.txt (https://github.com/gabomdq/SDL_GameControllerDB)

// Simple DirectMedia Layer
// Copyright(C) 1997-2013 Sam Lantinga<slouken@libsdl.org>
// 
// 
// This software is provided 'as-is', without any express or implied
// warranty.  In no event will the authors be held liable for any damages
// arising from the use of this software.
// 
// Permission is granted to anyone to use this software for any purpose,
// including commercial applications, and to alter it and redistribute it
// freely, subject to the following restrictions:
//   
// 1. The origin of this software must not be misrepresented; you must not
//    claim that you wrote the original software.If you use this software
//    in a product, an acknowledgment in the product documentation would be
//    appreciated but is not required.
// 2. Altered source versions must be plainly marked as such, and must not be
//    misrepresented as being the original software.
// 3. This notice may not be removed or altered from any source distribution.

using System;

namespace SiliconStudio.Xenko.Input
{
	#if SILICONSTUDIO_PLATFORM_WINDOWS
	/// <summary>
    /// Acme 
    /// </summary>
	internal class Layout0 : GameControllerDatabaseLayout
	{
		public Layout0() : base(new Guid("00120e8f-0000-0000-0000-504944564944"), "Acme")
		{
			AddButtonToButton(2, GamePadButton.X);
			AddButtonToButton(0, GamePadButton.A);
			AddButtonToButton(1, GamePadButton.B);
			AddButtonToButton(3, GamePadButton.Y);
			AddButtonToButton(8, GamePadButton.Back);
			AddButtonToButton(9, GamePadButton.Start);
			AddButtonToButton(4, GamePadButton.LeftShoulder);
			AddButtonToAxis(5, GamePadAxis.LeftTrigger);
			AddButtonToButton(6, GamePadButton.RightShoulder);
			AddButtonToAxis(7, GamePadAxis.RightTrigger);
			AddButtonToButton(10, GamePadButton.LeftThumb);
			AddButtonToButton(11, GamePadButton.RightThumb);
			AddAxisToAxis(0, GamePadAxis.LeftThumbX);
			AddAxisToAxis(1, GamePadAxis.LeftThumbY, true);
			AddAxisToAxis(3, GamePadAxis.RightThumbX);
			AddAxisToAxis(2, GamePadAxis.RightThumbY, true);
			 
		}
	}

	
	/// <summary>
    /// Afterglow PS3 Controller 
    /// </summary>
	internal class Layout1 : GameControllerDatabaseLayout
	{
		public Layout1() : base(new Guid("08361a34-0000-0000-0000-504944564944"), "Afterglow PS3 Controller")
		{
			AddButtonToButton(1, GamePadButton.A);
			AddButtonToButton(2, GamePadButton.B);
			AddButtonToButton(8, GamePadButton.Back);
			AddButtonToButton(4, GamePadButton.LeftShoulder);
			AddButtonToButton(10, GamePadButton.LeftThumb);
			AddButtonToAxis(6, GamePadAxis.LeftTrigger);
			AddAxisToAxis(0, GamePadAxis.LeftThumbX);
			AddAxisToAxis(1, GamePadAxis.LeftThumbY, true);
			AddButtonToButton(5, GamePadButton.RightShoulder);
			AddButtonToButton(11, GamePadButton.RightThumb);
			AddButtonToAxis(7, GamePadAxis.RightTrigger);
			AddAxisToAxis(2, GamePadAxis.RightThumbX);
			AddAxisToAxis(3, GamePadAxis.RightThumbY, true);
			AddButtonToButton(9, GamePadButton.Start);
			AddButtonToButton(0, GamePadButton.X);
			AddButtonToButton(3, GamePadButton.Y);
			 
		}
	}

	
	/// <summary>
    /// GameStop Gamepad 
    /// </summary>
	internal class Layout2 : GameControllerDatabaseLayout
	{
		public Layout2() : base(new Guid("0000ffff-0000-0000-0000-504944564944"), "GameStop Gamepad")
		{
			AddButtonToButton(0, GamePadButton.A);
			AddButtonToButton(1, GamePadButton.B);
			AddButtonToButton(8, GamePadButton.Back);
			AddButtonToButton(4, GamePadButton.LeftShoulder);
			AddButtonToButton(10, GamePadButton.LeftThumb);
			AddButtonToAxis(6, GamePadAxis.LeftTrigger);
			AddAxisToAxis(0, GamePadAxis.LeftThumbX);
			AddAxisToAxis(1, GamePadAxis.LeftThumbY, true);
			AddButtonToButton(5, GamePadButton.RightShoulder);
			AddButtonToButton(11, GamePadButton.RightThumb);
			AddButtonToAxis(7, GamePadAxis.RightTrigger);
			AddAxisToAxis(2, GamePadAxis.RightThumbX);
			AddAxisToAxis(3, GamePadAxis.RightThumbY, true);
			AddButtonToButton(9, GamePadButton.Start);
			AddButtonToButton(2, GamePadButton.X);
			AddButtonToButton(3, GamePadButton.Y);
			 
		}
	}

	
	/// <summary>
    /// Generic DirectInput Controller 
    /// </summary>
	internal class Layout3 : GameControllerDatabaseLayout
	{
		public Layout3() : base(new Guid("c216046d-0000-0000-0000-504944564944"), "Generic DirectInput Controller")
		{
			AddButtonToButton(1, GamePadButton.A);
			AddButtonToButton(2, GamePadButton.B);
			AddButtonToButton(8, GamePadButton.Back);
			AddButtonToButton(4, GamePadButton.LeftShoulder);
			AddButtonToButton(10, GamePadButton.LeftThumb);
			AddButtonToAxis(6, GamePadAxis.LeftTrigger);
			AddAxisToAxis(0, GamePadAxis.LeftThumbX);
			AddAxisToAxis(1, GamePadAxis.LeftThumbY, true);
			AddButtonToButton(5, GamePadButton.RightShoulder);
			AddButtonToButton(11, GamePadButton.RightThumb);
			AddButtonToAxis(7, GamePadAxis.RightTrigger);
			AddAxisToAxis(2, GamePadAxis.RightThumbX);
			AddAxisToAxis(3, GamePadAxis.RightThumbY, true);
			AddButtonToButton(9, GamePadButton.Start);
			AddButtonToButton(0, GamePadButton.X);
			AddButtonToButton(3, GamePadButton.Y);
			 
		}
	}

	
	/// <summary>
    /// HORIPAD 4 
    /// </summary>
	internal class Layout4 : GameControllerDatabaseLayout
	{
		public Layout4() : base(new Guid("006e0f0d-0000-0000-0000-504944564944"), "HORIPAD 4")
		{
			AddButtonToButton(1, GamePadButton.A);
			AddButtonToButton(2, GamePadButton.B);
			AddButtonToButton(3, GamePadButton.Y);
			AddButtonToButton(0, GamePadButton.X);
			AddButtonToButton(9, GamePadButton.Start);
			AddButtonToButton(8, GamePadButton.Back);
			AddButtonToButton(10, GamePadButton.LeftThumb);
			AddButtonToButton(11, GamePadButton.RightThumb);
			AddButtonToButton(4, GamePadButton.LeftShoulder);
			AddButtonToButton(5, GamePadButton.RightShoulder);
			AddAxisToAxis(0, GamePadAxis.LeftThumbX);
			AddAxisToAxis(1, GamePadAxis.LeftThumbY, true);
			AddAxisToAxis(2, GamePadAxis.RightThumbX);
			AddAxisToAxis(3, GamePadAxis.RightThumbY, true);
			AddButtonToAxis(6, GamePadAxis.LeftTrigger);
			AddButtonToAxis(7, GamePadAxis.RightTrigger);
			 
		}
	}

	
	/// <summary>
    /// Logitech F710 Gamepad 
    /// </summary>
	internal class Layout5 : GameControllerDatabaseLayout
	{
		public Layout5() : base(new Guid("c219046d-0000-0000-0000-504944564944"), "Logitech F710 Gamepad")
		{
			AddButtonToButton(1, GamePadButton.A);
			AddButtonToButton(2, GamePadButton.B);
			AddButtonToButton(8, GamePadButton.Back);
			AddButtonToButton(4, GamePadButton.LeftShoulder);
			AddButtonToButton(10, GamePadButton.LeftThumb);
			AddButtonToAxis(6, GamePadAxis.LeftTrigger);
			AddAxisToAxis(0, GamePadAxis.LeftThumbX);
			AddAxisToAxis(1, GamePadAxis.LeftThumbY, true);
			AddButtonToButton(5, GamePadButton.RightShoulder);
			AddButtonToButton(11, GamePadButton.RightThumb);
			AddButtonToAxis(7, GamePadAxis.RightTrigger);
			AddAxisToAxis(2, GamePadAxis.RightThumbX);
			AddAxisToAxis(3, GamePadAxis.RightThumbY, true);
			AddButtonToButton(9, GamePadButton.Start);
			AddButtonToButton(0, GamePadButton.X);
			AddButtonToButton(3, GamePadButton.Y);
			 
		}
	}

	
	/// <summary>
    /// PS3 Controller 
    /// </summary>
	internal class Layout6 : GameControllerDatabaseLayout
	{
		public Layout6() : base(new Guid("03088888-0000-0000-0000-504944564944"), "PS3 Controller")
		{
			AddButtonToButton(2, GamePadButton.A);
			AddButtonToButton(1, GamePadButton.B);
			AddButtonToButton(8, GamePadButton.Back);
			AddButtonToButton(4, GamePadButton.LeftShoulder);
			AddButtonToButton(9, GamePadButton.LeftThumb);
			AddButtonToAxis(6, GamePadAxis.LeftTrigger);
			AddAxisToAxis(0, GamePadAxis.LeftThumbX);
			AddAxisToAxis(1, GamePadAxis.LeftThumbY, true);
			AddButtonToButton(5, GamePadButton.RightShoulder);
			AddButtonToButton(10, GamePadButton.RightThumb);
			AddButtonToAxis(7, GamePadAxis.RightTrigger);
			AddAxisToAxis(3, GamePadAxis.RightThumbX);
			AddAxisToAxis(4, GamePadAxis.RightThumbY, true);
			AddButtonToButton(11, GamePadButton.Start);
			AddButtonToButton(0, GamePadButton.X);
			AddButtonToButton(3, GamePadButton.Y);
			 
		}
	}

	
	/// <summary>
    /// PS3 Controller 
    /// </summary>
	internal class Layout7 : GameControllerDatabaseLayout
	{
		public Layout7() : base(new Guid("0268054c-0000-0000-0000-504944564944"), "PS3 Controller")
		{
			AddButtonToButton(14, GamePadButton.A);
			AddButtonToButton(13, GamePadButton.B);
			AddButtonToButton(0, GamePadButton.Back);
			AddButtonToButton(6, GamePadButton.PadDown);
			AddButtonToButton(7, GamePadButton.PadLeft);
			AddButtonToButton(5, GamePadButton.PadRight);
			AddButtonToButton(4, GamePadButton.PadUp);
			AddButtonToButton(10, GamePadButton.LeftShoulder);
			AddButtonToButton(1, GamePadButton.LeftThumb);
			AddButtonToAxis(8, GamePadAxis.LeftTrigger);
			AddAxisToAxis(0, GamePadAxis.LeftThumbX);
			AddAxisToAxis(1, GamePadAxis.LeftThumbY, true);
			AddButtonToButton(11, GamePadButton.RightShoulder);
			AddButtonToButton(2, GamePadButton.RightThumb);
			AddButtonToAxis(9, GamePadAxis.RightTrigger);
			AddAxisToAxis(2, GamePadAxis.RightThumbX);
			AddAxisToAxis(3, GamePadAxis.RightThumbY, true);
			AddButtonToButton(3, GamePadButton.Start);
			AddButtonToButton(15, GamePadButton.X);
			AddButtonToButton(12, GamePadButton.Y);
			 
		}
	}

	
	/// <summary>
    /// PS3 DualShock 
    /// </summary>
	internal class Layout8 : GameControllerDatabaseLayout
	{
		public Layout8() : base(new Guid("00050925-0000-0000-0000-504944564944"), "PS3 DualShock")
		{
			AddButtonToButton(2, GamePadButton.A);
			AddButtonToButton(1, GamePadButton.B);
			AddButtonToButton(9, GamePadButton.Back);
			AddButtonToButton(6, GamePadButton.LeftShoulder);
			AddButtonToButton(10, GamePadButton.LeftThumb);
			AddButtonToAxis(4, GamePadAxis.LeftTrigger);
			AddAxisToAxis(0, GamePadAxis.LeftThumbX);
			AddAxisToAxis(1, GamePadAxis.LeftThumbY, true);
			AddButtonToButton(7, GamePadButton.RightShoulder);
			AddButtonToButton(11, GamePadButton.RightThumb);
			AddButtonToAxis(5, GamePadAxis.RightTrigger);
			AddAxisToAxis(2, GamePadAxis.RightThumbX);
			AddAxisToAxis(3, GamePadAxis.RightThumbY, true);
			AddButtonToButton(8, GamePadButton.Start);
			AddButtonToButton(0, GamePadButton.X);
			AddButtonToButton(3, GamePadButton.Y);
			 
		}
	}

	
	/// <summary>
    /// PS4 Controller 
    /// </summary>
	internal class Layout9 : GameControllerDatabaseLayout
	{
		public Layout9() : base(new Guid("05c4054c-0000-0000-0000-504944564944"), "PS4 Controller")
		{
			AddButtonToButton(1, GamePadButton.A);
			AddButtonToButton(2, GamePadButton.B);
			AddButtonToButton(8, GamePadButton.Back);
			AddButtonToButton(4, GamePadButton.LeftShoulder);
			AddButtonToButton(10, GamePadButton.LeftThumb);
			AddAxisToAxis(3, GamePadAxis.LeftTrigger, remap: true);
			AddAxisToAxis(0, GamePadAxis.LeftThumbX);
			AddAxisToAxis(1, GamePadAxis.LeftThumbY, true);
			AddButtonToButton(5, GamePadButton.RightShoulder);
			AddButtonToButton(11, GamePadButton.RightThumb);
			AddAxisToAxis(4, GamePadAxis.RightTrigger, remap: true);
			AddAxisToAxis(2, GamePadAxis.RightThumbX);
			AddAxisToAxis(5, GamePadAxis.RightThumbY, true);
			AddButtonToButton(9, GamePadButton.Start);
			AddButtonToButton(0, GamePadButton.X);
			AddButtonToButton(3, GamePadButton.Y);
			 
		}
	}

	
	/// <summary>
    /// Logitech RumblePad 2 USB 
    /// </summary>
	internal class Layout10 : GameControllerDatabaseLayout
	{
		public Layout10() : base(new Guid("c218046d-0000-0000-0000-504944564944"), "Logitech RumblePad 2 USB")
		{
			AddButtonToButton(0, GamePadButton.X);
			AddButtonToButton(1, GamePadButton.A);
			AddButtonToButton(2, GamePadButton.B);
			AddButtonToButton(3, GamePadButton.Y);
			AddButtonToButton(8, GamePadButton.Back);
			AddButtonToButton(9, GamePadButton.Start);
			AddButtonToButton(4, GamePadButton.LeftShoulder);
			AddButtonToAxis(6, GamePadAxis.LeftTrigger);
			AddButtonToButton(5, GamePadButton.RightShoulder);
			AddButtonToAxis(7, GamePadAxis.RightTrigger);
			AddButtonToButton(10, GamePadButton.LeftThumb);
			AddButtonToButton(11, GamePadButton.RightThumb);
			AddAxisToAxis(0, GamePadAxis.LeftThumbX);
			AddAxisToAxis(1, GamePadAxis.LeftThumbY, true);
			AddAxisToAxis(2, GamePadAxis.RightThumbX);
			AddAxisToAxis(3, GamePadAxis.RightThumbY, true);
			 
		}
	}

	
	/// <summary>
    /// OUYA Controller 
    /// </summary>
	internal class Layout11 : GameControllerDatabaseLayout
	{
		public Layout11() : base(new Guid("00012836-0000-0000-0000-504944564944"), "OUYA Controller")
		{
			AddButtonToButton(0, GamePadButton.A);
			AddButtonToButton(3, GamePadButton.B);
			AddButtonToButton(2, GamePadButton.Y);
			AddButtonToButton(1, GamePadButton.X);
			AddButtonToButton(14, GamePadButton.Start);
			AddButtonToButton(6, GamePadButton.LeftThumb);
			AddButtonToButton(7, GamePadButton.RightThumb);
			AddButtonToButton(4, GamePadButton.LeftShoulder);
			AddButtonToButton(5, GamePadButton.RightShoulder);
			AddButtonToButton(8, GamePadButton.PadUp);
			AddButtonToButton(10, GamePadButton.PadLeft);
			AddButtonToButton(9, GamePadButton.PadDown);
			AddButtonToButton(11, GamePadButton.PadRight);
			AddAxisToAxis(0, GamePadAxis.LeftThumbX);
			AddAxisToAxis(1, GamePadAxis.LeftThumbY, true);
			AddAxisToAxis(3, GamePadAxis.RightThumbX);
			AddAxisToAxis(4, GamePadAxis.RightThumbY, true);
			AddButtonToAxis(12, GamePadAxis.LeftTrigger);
			AddButtonToAxis(13, GamePadAxis.RightTrigger);
			 
		}
	}

	
	/// <summary>
    /// Thrustmaster Firestorm Dual Power 
    /// </summary>
	internal class Layout12 : GameControllerDatabaseLayout
	{
		public Layout12() : base(new Guid("b300044f-0000-0000-0000-504944564944"), "Thrustmaster Firestorm Dual Power")
		{
			AddButtonToButton(0, GamePadButton.A);
			AddButtonToButton(2, GamePadButton.B);
			AddButtonToButton(3, GamePadButton.Y);
			AddButtonToButton(1, GamePadButton.X);
			AddButtonToButton(10, GamePadButton.Start);
			AddButtonToButton(9, GamePadButton.Back);
			AddButtonToButton(11, GamePadButton.LeftThumb);
			AddButtonToButton(12, GamePadButton.RightThumb);
			AddButtonToButton(4, GamePadButton.LeftShoulder);
			AddButtonToButton(6, GamePadButton.RightShoulder);
			AddAxisToAxis(0, GamePadAxis.LeftThumbX);
			AddAxisToAxis(1, GamePadAxis.LeftThumbY, true);
			AddAxisToAxis(2, GamePadAxis.RightThumbX);
			AddAxisToAxis(3, GamePadAxis.RightThumbY, true);
			AddButtonToAxis(5, GamePadAxis.LeftTrigger);
			AddButtonToAxis(7, GamePadAxis.RightTrigger);
			 
		}
	}

	
	/// <summary>
    /// RetroUSB.com RetroPad 
    /// </summary>
	internal class Layout13 : GameControllerDatabaseLayout
	{
		public Layout13() : base(new Guid("0003f000-0000-0000-0000-504944564944"), "RetroUSB.com RetroPad")
		{
			AddButtonToButton(1, GamePadButton.A);
			AddButtonToButton(5, GamePadButton.B);
			AddButtonToButton(0, GamePadButton.X);
			AddButtonToButton(4, GamePadButton.Y);
			AddButtonToButton(2, GamePadButton.Back);
			AddButtonToButton(3, GamePadButton.Start);
			AddButtonToButton(6, GamePadButton.LeftShoulder);
			AddButtonToButton(7, GamePadButton.RightShoulder);
			AddAxisToAxis(0, GamePadAxis.LeftThumbX);
			AddAxisToAxis(1, GamePadAxis.LeftThumbY, true);
			 
		}
	}

	
	/// <summary>
    /// RetroUSB.com Super RetroPort 
    /// </summary>
	internal class Layout14 : GameControllerDatabaseLayout
	{
		public Layout14() : base(new Guid("00f1f000-0000-0000-0000-504944564944"), "RetroUSB.com Super RetroPort")
		{
			AddButtonToButton(1, GamePadButton.A);
			AddButtonToButton(5, GamePadButton.B);
			AddButtonToButton(0, GamePadButton.X);
			AddButtonToButton(4, GamePadButton.Y);
			AddButtonToButton(2, GamePadButton.Back);
			AddButtonToButton(3, GamePadButton.Start);
			AddButtonToButton(6, GamePadButton.LeftShoulder);
			AddButtonToButton(7, GamePadButton.RightShoulder);
			AddAxisToAxis(0, GamePadAxis.LeftThumbX);
			AddAxisToAxis(1, GamePadAxis.LeftThumbY, true);
			 
		}
	}

	
	/// <summary>
    /// GamePad Pro USB 
    /// </summary>
	internal class Layout15 : GameControllerDatabaseLayout
	{
		public Layout15() : base(new Guid("40010428-0000-0000-0000-504944564944"), "GamePad Pro USB")
		{
			AddButtonToButton(1, GamePadButton.A);
			AddButtonToButton(2, GamePadButton.B);
			AddButtonToButton(0, GamePadButton.X);
			AddButtonToButton(3, GamePadButton.Y);
			AddButtonToButton(8, GamePadButton.Back);
			AddButtonToButton(9, GamePadButton.Start);
			AddButtonToButton(4, GamePadButton.LeftShoulder);
			AddButtonToButton(5, GamePadButton.RightShoulder);
			AddAxisToAxis(0, GamePadAxis.LeftThumbX);
			AddAxisToAxis(1, GamePadAxis.LeftThumbY, true);
			AddButtonToAxis(6, GamePadAxis.LeftTrigger);
			AddButtonToAxis(7, GamePadAxis.RightTrigger);
			 
		}
	}

	
	/// <summary>
    /// SVEN X-PAD 
    /// </summary>
	internal class Layout16 : GameControllerDatabaseLayout
	{
		public Layout16() : base(new Guid("333111ff-0000-0000-0000-504944564944"), "SVEN X-PAD")
		{
			AddButtonToButton(2, GamePadButton.A);
			AddButtonToButton(3, GamePadButton.B);
			AddButtonToButton(1, GamePadButton.Y);
			AddButtonToButton(0, GamePadButton.X);
			AddButtonToButton(5, GamePadButton.Start);
			AddButtonToButton(4, GamePadButton.Back);
			AddButtonToButton(6, GamePadButton.LeftShoulder);
			AddButtonToButton(7, GamePadButton.RightShoulder);
			AddAxisToAxis(0, GamePadAxis.LeftThumbX);
			AddAxisToAxis(1, GamePadAxis.LeftThumbY, true);
			AddAxisToAxis(2, GamePadAxis.RightThumbX);
			AddAxisToAxis(4, GamePadAxis.RightThumbY, true);
			AddButtonToAxis(8, GamePadAxis.LeftTrigger);
			AddButtonToAxis(9, GamePadAxis.RightTrigger);
			 
		}
	}

	
	/// <summary>
    /// Piranha xtreme 
    /// </summary>
	internal class Layout17 : GameControllerDatabaseLayout
	{
		public Layout17() : base(new Guid("00030e8f-0000-0000-0000-504944564944"), "Piranha xtreme")
		{
			AddButtonToButton(3, GamePadButton.X);
			AddButtonToButton(2, GamePadButton.A);
			AddButtonToButton(1, GamePadButton.B);
			AddButtonToButton(0, GamePadButton.Y);
			AddButtonToButton(8, GamePadButton.Back);
			AddButtonToButton(9, GamePadButton.Start);
			AddButtonToButton(6, GamePadButton.LeftShoulder);
			AddButtonToAxis(4, GamePadAxis.LeftTrigger);
			AddButtonToButton(7, GamePadButton.RightShoulder);
			AddButtonToAxis(5, GamePadAxis.RightTrigger);
			AddButtonToButton(10, GamePadButton.LeftThumb);
			AddButtonToButton(11, GamePadButton.RightThumb);
			AddAxisToAxis(0, GamePadAxis.LeftThumbX);
			AddAxisToAxis(1, GamePadAxis.LeftThumbY, true);
			AddAxisToAxis(3, GamePadAxis.RightThumbX);
			AddAxisToAxis(2, GamePadAxis.RightThumbY, true);
			 
		}
	}

	
	/// <summary>
    /// Multilaser JS071 USB 
    /// </summary>
	internal class Layout18 : GameControllerDatabaseLayout
	{
		public Layout18() : base(new Guid("310d0e8f-0000-0000-0000-504944564944"), "Multilaser JS071 USB")
		{
			AddButtonToButton(1, GamePadButton.A);
			AddButtonToButton(2, GamePadButton.B);
			AddButtonToButton(3, GamePadButton.Y);
			AddButtonToButton(0, GamePadButton.X);
			AddButtonToButton(9, GamePadButton.Start);
			AddButtonToButton(8, GamePadButton.Back);
			AddButtonToButton(10, GamePadButton.LeftThumb);
			AddButtonToButton(11, GamePadButton.RightThumb);
			AddButtonToButton(4, GamePadButton.LeftShoulder);
			AddButtonToButton(5, GamePadButton.RightShoulder);
			AddAxisToAxis(0, GamePadAxis.LeftThumbX);
			AddAxisToAxis(1, GamePadAxis.LeftThumbY, true);
			AddAxisToAxis(2, GamePadAxis.RightThumbX);
			AddAxisToAxis(3, GamePadAxis.RightThumbY, true);
			AddButtonToAxis(6, GamePadAxis.LeftTrigger);
			AddButtonToAxis(7, GamePadAxis.RightTrigger);
			 
		}
	}

	
	/// <summary>
    /// PS2 USB 
    /// </summary>
	internal class Layout19 : GameControllerDatabaseLayout
	{
		public Layout19() : base(new Guid("00030810-0000-0000-0000-504944564944"), "PS2 USB")
		{
			AddButtonToButton(2, GamePadButton.A);
			AddButtonToButton(1, GamePadButton.B);
			AddButtonToButton(0, GamePadButton.Y);
			AddButtonToButton(3, GamePadButton.X);
			AddButtonToButton(9, GamePadButton.Start);
			AddButtonToButton(8, GamePadButton.Back);
			AddButtonToButton(10, GamePadButton.LeftThumb);
			AddButtonToButton(11, GamePadButton.RightThumb);
			AddButtonToButton(6, GamePadButton.LeftShoulder);
			AddButtonToButton(7, GamePadButton.RightShoulder);
			AddAxisToAxis(0, GamePadAxis.LeftThumbX);
			AddAxisToAxis(1, GamePadAxis.LeftThumbY, true);
			AddAxisToAxis(4, GamePadAxis.RightThumbX);
			AddAxisToAxis(2, GamePadAxis.RightThumbY, true);
			AddButtonToAxis(4, GamePadAxis.LeftTrigger);
			AddButtonToAxis(5, GamePadAxis.RightTrigger);
			 
		}
	}

	
	/// <summary>
    /// G-Shark GS-GP702 
    /// </summary>
	internal class Layout20 : GameControllerDatabaseLayout
	{
		public Layout20() : base(new Guid("00060079-0000-0000-0000-504944564944"), "G-Shark GS-GP702")
		{
			AddButtonToButton(2, GamePadButton.A);
			AddButtonToButton(1, GamePadButton.B);
			AddButtonToButton(3, GamePadButton.X);
			AddButtonToButton(0, GamePadButton.Y);
			AddButtonToButton(8, GamePadButton.Back);
			AddButtonToButton(9, GamePadButton.Start);
			AddButtonToButton(10, GamePadButton.LeftThumb);
			AddButtonToButton(11, GamePadButton.RightThumb);
			AddButtonToButton(4, GamePadButton.LeftShoulder);
			AddButtonToButton(5, GamePadButton.RightShoulder);
			AddAxisToAxis(0, GamePadAxis.LeftThumbX);
			AddAxisToAxis(1, GamePadAxis.LeftThumbY, true);
			AddAxisToAxis(2, GamePadAxis.RightThumbX);
			AddAxisToAxis(4, GamePadAxis.RightThumbY, true);
			AddButtonToAxis(6, GamePadAxis.LeftTrigger);
			AddButtonToAxis(7, GamePadAxis.RightTrigger);
			 
		}
	}

	
	/// <summary>
    /// NYKO AIRFLO 
    /// </summary>
	internal class Layout21 : GameControllerDatabaseLayout
	{
		public Layout21() : base(new Guid("4d01124b-0000-0000-0000-504944564944"), "NYKO AIRFLO")
		{
			AddButtonToButton(0, GamePadButton.A);
			AddButtonToButton(1, GamePadButton.B);
			AddButtonToButton(2, GamePadButton.X);
			AddButtonToButton(3, GamePadButton.Y);
			AddButtonToButton(8, GamePadButton.Back);
			AddButtonToButton(9, GamePadButton.Start);
			AddAxisToButton(0, GamePadButton.LeftThumb);
			AddAxisToButton(2, GamePadButton.RightThumb);
			AddAxisToButton(3, GamePadButton.LeftShoulder);
			AddButtonToButton(5, GamePadButton.RightShoulder);
			AddButtonToAxis(6, GamePadAxis.LeftTrigger);
			AddButtonToAxis(7, GamePadAxis.RightTrigger);
			 
		}
	}

	
	/// <summary>
    /// PowerA Pro Ex 
    /// </summary>
	internal class Layout22 : GameControllerDatabaseLayout
	{
		public Layout22() : base(new Guid("ca6d20d6-0000-0000-0000-504944564944"), "PowerA Pro Ex")
		{
			AddButtonToButton(1, GamePadButton.A);
			AddButtonToButton(2, GamePadButton.B);
			AddButtonToButton(0, GamePadButton.X);
			AddButtonToButton(3, GamePadButton.Y);
			AddButtonToButton(8, GamePadButton.Back);
			AddButtonToButton(9, GamePadButton.Start);
			AddButtonToButton(10, GamePadButton.LeftThumb);
			AddButtonToButton(11, GamePadButton.RightThumb);
			AddButtonToButton(4, GamePadButton.LeftShoulder);
			AddButtonToButton(5, GamePadButton.RightShoulder);
			AddAxisToAxis(0, GamePadAxis.LeftThumbX);
			AddAxisToAxis(1, GamePadAxis.LeftThumbY, true);
			AddAxisToAxis(2, GamePadAxis.RightThumbX);
			AddAxisToAxis(3, GamePadAxis.RightThumbY, true);
			AddButtonToAxis(6, GamePadAxis.LeftTrigger);
			AddButtonToAxis(7, GamePadAxis.RightTrigger);
			 
		}
	}

	
	/// <summary>
    /// Saitek P2500 
    /// </summary>
	internal class Layout23 : GameControllerDatabaseLayout
	{
		public Layout23() : base(new Guid("ff0c06a3-0000-0000-0000-504944564944"), "Saitek P2500")
		{
			AddButtonToButton(2, GamePadButton.A);
			AddButtonToButton(3, GamePadButton.B);
			AddButtonToButton(1, GamePadButton.Y);
			AddButtonToButton(0, GamePadButton.X);
			AddButtonToButton(4, GamePadButton.Start);
			AddButtonToButton(5, GamePadButton.Back);
			AddButtonToButton(8, GamePadButton.LeftThumb);
			AddButtonToButton(9, GamePadButton.RightThumb);
			AddButtonToButton(6, GamePadButton.LeftShoulder);
			AddButtonToButton(7, GamePadButton.RightShoulder);
			AddAxisToAxis(0, GamePadAxis.LeftThumbX);
			AddAxisToAxis(1, GamePadAxis.LeftThumbY, true);
			AddAxisToAxis(2, GamePadAxis.RightThumbX);
			AddAxisToAxis(3, GamePadAxis.RightThumbY, true);
			 
		}
	}

	
	/// <summary>
    /// Thrustmaster Dual Analog 3.2 
    /// </summary>
	internal class Layout24 : GameControllerDatabaseLayout
	{
		public Layout24() : base(new Guid("b315044f-0000-0000-0000-504944564944"), "Thrustmaster Dual Analog 3.2")
		{
			AddButtonToButton(1, GamePadButton.X);
			AddButtonToButton(0, GamePadButton.A);
			AddButtonToButton(2, GamePadButton.B);
			AddButtonToButton(3, GamePadButton.Y);
			AddButtonToButton(8, GamePadButton.Back);
			AddButtonToButton(9, GamePadButton.Start);
			AddButtonToButton(4, GamePadButton.LeftShoulder);
			AddButtonToAxis(5, GamePadAxis.LeftTrigger);
			AddButtonToButton(6, GamePadButton.RightShoulder);
			AddButtonToAxis(7, GamePadAxis.RightTrigger);
			AddButtonToButton(10, GamePadButton.LeftThumb);
			AddButtonToButton(11, GamePadButton.RightThumb);
			AddAxisToAxis(0, GamePadAxis.LeftThumbX);
			AddAxisToAxis(1, GamePadAxis.LeftThumbY, true);
			AddAxisToAxis(2, GamePadAxis.RightThumbX);
			AddAxisToAxis(3, GamePadAxis.RightThumbY, true);
			 
		}
	}

	
	/// <summary>
    /// Rock Candy Gamepad for PS3 
    /// </summary>
	internal class Layout25 : GameControllerDatabaseLayout
	{
		public Layout25() : base(new Guid("011e0e6f-0000-0000-0000-504944564944"), "Rock Candy Gamepad for PS3")
		{
			AddButtonToButton(1, GamePadButton.A);
			AddButtonToButton(2, GamePadButton.B);
			AddButtonToButton(0, GamePadButton.X);
			AddButtonToButton(3, GamePadButton.Y);
			AddButtonToButton(8, GamePadButton.Back);
			AddButtonToButton(9, GamePadButton.Start);
			AddButtonToButton(4, GamePadButton.LeftShoulder);
			AddButtonToButton(5, GamePadButton.RightShoulder);
			AddButtonToButton(10, GamePadButton.LeftThumb);
			AddButtonToButton(11, GamePadButton.RightThumb);
			AddAxisToAxis(0, GamePadAxis.LeftThumbX);
			AddAxisToAxis(1, GamePadAxis.LeftThumbY, true);
			AddAxisToAxis(2, GamePadAxis.RightThumbX);
			AddAxisToAxis(3, GamePadAxis.RightThumbY, true);
			AddButtonToAxis(6, GamePadAxis.LeftTrigger);
			AddButtonToAxis(7, GamePadAxis.RightTrigger);
			 
		}
	}

	
	/// <summary>
    /// iBuffalo USB 2-axis 8-button Gamepad 
    /// </summary>
	internal class Layout26 : GameControllerDatabaseLayout
	{
		public Layout26() : base(new Guid("20600583-0000-0000-0000-504944564944"), "iBuffalo USB 2-axis 8-button Gamepad")
		{
			AddButtonToButton(1, GamePadButton.A);
			AddButtonToButton(0, GamePadButton.B);
			AddButtonToButton(2, GamePadButton.Y);
			AddButtonToButton(3, GamePadButton.X);
			AddButtonToButton(7, GamePadButton.Start);
			AddButtonToButton(6, GamePadButton.Back);
			AddButtonToButton(4, GamePadButton.LeftShoulder);
			AddButtonToButton(5, GamePadButton.RightShoulder);
			AddAxisToAxis(0, GamePadAxis.LeftThumbX);
			AddAxisToAxis(1, GamePadAxis.LeftThumbY, true);
			 
		}
	}

	
	/// <summary>
    /// PS1 USB 
    /// </summary>
	internal class Layout27 : GameControllerDatabaseLayout
	{
		public Layout27() : base(new Guid("00010810-0000-0000-0000-504944564944"), "PS1 USB")
		{
			AddButtonToButton(2, GamePadButton.A);
			AddButtonToButton(1, GamePadButton.B);
			AddButtonToButton(3, GamePadButton.X);
			AddButtonToButton(0, GamePadButton.Y);
			AddButtonToButton(8, GamePadButton.Back);
			AddButtonToButton(9, GamePadButton.Start);
			AddButtonToButton(6, GamePadButton.LeftShoulder);
			AddButtonToButton(7, GamePadButton.RightShoulder);
			AddButtonToButton(10, GamePadButton.LeftThumb);
			AddButtonToButton(11, GamePadButton.RightThumb);
			AddAxisToAxis(0, GamePadAxis.LeftThumbX);
			AddAxisToAxis(1, GamePadAxis.LeftThumbY, true);
			AddAxisToAxis(3, GamePadAxis.RightThumbX);
			AddAxisToAxis(2, GamePadAxis.RightThumbY, true);
			AddButtonToAxis(4, GamePadAxis.LeftTrigger);
			AddButtonToAxis(5, GamePadAxis.RightTrigger);
			 
		}
	}

	
	/// <summary>
    /// Ipega PG-9023 
    /// </summary>
	internal class Layout28 : GameControllerDatabaseLayout
	{
		public Layout28() : base(new Guid("04021949-0000-0000-0000-504944564944"), "Ipega PG-9023")
		{
			AddButtonToButton(0, GamePadButton.A);
			AddButtonToButton(1, GamePadButton.B);
			AddButtonToButton(3, GamePadButton.X);
			AddButtonToButton(4, GamePadButton.Y);
			AddButtonToButton(10, GamePadButton.Back);
			AddButtonToButton(11, GamePadButton.Start);
			AddButtonToButton(13, GamePadButton.LeftThumb);
			AddButtonToButton(14, GamePadButton.RightThumb);
			AddButtonToButton(6, GamePadButton.LeftShoulder);
			AddButtonToButton(7, GamePadButton.RightShoulder);
			AddAxisToAxis(0, GamePadAxis.LeftThumbX);
			AddAxisToAxis(1, GamePadAxis.LeftThumbY, true);
			AddAxisToAxis(3, GamePadAxis.RightThumbX);
			AddAxisToAxis(4, GamePadAxis.RightThumbY, true);
			AddButtonToAxis(8, GamePadAxis.LeftTrigger);
			AddButtonToAxis(9, GamePadAxis.RightTrigger);
			 
		}
	}

	
	/// <summary>
    /// Dual Trigger 3-in-1 
    /// </summary>
	internal class Layout29 : GameControllerDatabaseLayout
	{
		public Layout29() : base(new Guid("b323044f-0000-0000-0000-504944564944"), "Dual Trigger 3-in-1")
		{
			AddButtonToButton(1, GamePadButton.A);
			AddButtonToButton(2, GamePadButton.B);
			AddButtonToButton(0, GamePadButton.X);
			AddButtonToButton(3, GamePadButton.Y);
			AddButtonToButton(8, GamePadButton.Back);
			AddButtonToButton(9, GamePadButton.Start);
			AddButtonToButton(10, GamePadButton.LeftThumb);
			AddButtonToButton(11, GamePadButton.RightThumb);
			AddButtonToButton(4, GamePadButton.LeftShoulder);
			AddButtonToButton(5, GamePadButton.RightShoulder);
			AddAxisToAxis(0, GamePadAxis.LeftThumbX);
			AddAxisToAxis(1, GamePadAxis.LeftThumbY, true);
			AddAxisToAxis(2, GamePadAxis.RightThumbX);
			AddAxisToAxis(5, GamePadAxis.RightThumbY, true);
			AddButtonToAxis(6, GamePadAxis.LeftTrigger);
			AddButtonToAxis(7, GamePadAxis.RightTrigger);
			 
		}
	}

	
	/// <summary>
    /// Hatsune Miku Sho Controller 
    /// </summary>
	internal class Layout30 : GameControllerDatabaseLayout
	{
		public Layout30() : base(new Guid("00490f0d-0000-0000-0000-504944564944"), "Hatsune Miku Sho Controller")
		{
			AddButtonToButton(1, GamePadButton.A);
			AddButtonToButton(2, GamePadButton.B);
			AddButtonToButton(0, GamePadButton.X);
			AddButtonToButton(3, GamePadButton.Y);
			AddButtonToButton(8, GamePadButton.Back);
			AddButtonToButton(9, GamePadButton.Start);
			AddButtonToButton(10, GamePadButton.LeftThumb);
			AddButtonToButton(11, GamePadButton.RightThumb);
			AddButtonToButton(4, GamePadButton.LeftShoulder);
			AddButtonToButton(5, GamePadButton.RightShoulder);
			AddAxisToAxis(0, GamePadAxis.LeftThumbX);
			AddAxisToAxis(1, GamePadAxis.LeftThumbY, true);
			AddAxisToAxis(2, GamePadAxis.RightThumbX);
			AddAxisToAxis(3, GamePadAxis.RightThumbY, true);
			AddButtonToAxis(6, GamePadAxis.LeftTrigger);
			AddButtonToAxis(7, GamePadAxis.RightTrigger);
			 
		}
	}

	
	/// <summary>
    /// Mayflash GameCube Controller Adapter 
    /// </summary>
	internal class Layout31 : GameControllerDatabaseLayout
	{
		public Layout31() : base(new Guid("18430079-0000-0000-0000-504944564944"), "Mayflash GameCube Controller Adapter")
		{
			AddButtonToButton(1, GamePadButton.A);
			AddButtonToButton(2, GamePadButton.B);
			AddButtonToButton(0, GamePadButton.X);
			AddButtonToButton(3, GamePadButton.Y);
			AddButtonToButton(0, GamePadButton.Back);
			AddButtonToButton(9, GamePadButton.Start);
			AddButtonToButton(4, GamePadButton.LeftShoulder);
			AddButtonToButton(7, GamePadButton.RightShoulder);
			AddButtonToButton(0, GamePadButton.LeftThumb);
			AddButtonToButton(0, GamePadButton.RightThumb);
			AddAxisToAxis(0, GamePadAxis.LeftThumbX);
			AddAxisToAxis(1, GamePadAxis.LeftThumbY, true);
			AddAxisToAxis(5, GamePadAxis.RightThumbX);
			AddAxisToAxis(2, GamePadAxis.RightThumbY, true);
			AddAxisToAxis(3, GamePadAxis.LeftTrigger, remap: true);
			AddAxisToAxis(4, GamePadAxis.RightTrigger, remap: true);
			 
		}
	}

	
	/// <summary>
    /// Mayflash WiiU Pro Game Controller Adapter (DInput) 
    /// </summary>
	internal class Layout32 : GameControllerDatabaseLayout
	{
		public Layout32() : base(new Guid("18000079-0000-0000-0000-504944564944"), "Mayflash WiiU Pro Game Controller Adapter (DInput)")
		{
			AddButtonToButton(1, GamePadButton.A);
			AddButtonToButton(2, GamePadButton.B);
			AddButtonToButton(0, GamePadButton.X);
			AddButtonToButton(3, GamePadButton.Y);
			AddButtonToButton(8, GamePadButton.Back);
			AddButtonToButton(9, GamePadButton.Start);
			AddButtonToButton(10, GamePadButton.LeftThumb);
			AddButtonToButton(11, GamePadButton.RightThumb);
			AddButtonToButton(4, GamePadButton.LeftShoulder);
			AddButtonToButton(5, GamePadButton.RightShoulder);
			AddAxisToAxis(0, GamePadAxis.LeftThumbX);
			AddAxisToAxis(1, GamePadAxis.LeftThumbY, true);
			AddAxisToAxis(2, GamePadAxis.RightThumbX);
			AddAxisToAxis(3, GamePadAxis.RightThumbY, true);
			AddButtonToAxis(6, GamePadAxis.LeftTrigger);
			AddButtonToAxis(7, GamePadAxis.RightTrigger);
			 
		}
	}

	
	/// <summary>
    /// Mayflash Wii Classic Controller 
    /// </summary>
	internal class Layout33 : GameControllerDatabaseLayout
	{
		public Layout33() : base(new Guid("03e80925-0000-0000-0000-504944564944"), "Mayflash Wii Classic Controller")
		{
			AddButtonToButton(1, GamePadButton.A);
			AddButtonToButton(0, GamePadButton.B);
			AddButtonToButton(3, GamePadButton.X);
			AddButtonToButton(2, GamePadButton.Y);
			AddButtonToButton(8, GamePadButton.Back);
			AddButtonToButton(9, GamePadButton.Start);
			AddButtonToButton(4, GamePadButton.LeftShoulder);
			AddButtonToButton(5, GamePadButton.RightShoulder);
			AddButtonToButton(11, GamePadButton.PadUp);
			AddButtonToButton(13, GamePadButton.PadDown);
			AddButtonToButton(12, GamePadButton.PadLeft);
			AddButtonToButton(14, GamePadButton.PadRight);
			AddAxisToAxis(0, GamePadAxis.LeftThumbX);
			AddAxisToAxis(1, GamePadAxis.LeftThumbY, true);
			AddAxisToAxis(2, GamePadAxis.RightThumbX);
			AddAxisToAxis(3, GamePadAxis.RightThumbY, true);
			AddButtonToAxis(6, GamePadAxis.LeftTrigger);
			AddButtonToAxis(7, GamePadAxis.RightTrigger);
			 
		}
	}

	
	/// <summary>
    /// Saitek P480 Rumble Pad 
    /// </summary>
	internal class Layout34 : GameControllerDatabaseLayout
	{
		public Layout34() : base(new Guid("01100f30-0000-0000-0000-504944564944"), "Saitek P480 Rumble Pad")
		{
			AddButtonToButton(2, GamePadButton.A);
			AddButtonToButton(3, GamePadButton.B);
			AddButtonToButton(0, GamePadButton.X);
			AddButtonToButton(1, GamePadButton.Y);
			AddButtonToButton(8, GamePadButton.Back);
			AddButtonToButton(9, GamePadButton.Start);
			AddButtonToButton(10, GamePadButton.LeftThumb);
			AddButtonToButton(11, GamePadButton.RightThumb);
			AddButtonToButton(4, GamePadButton.LeftShoulder);
			AddButtonToButton(6, GamePadButton.RightShoulder);
			AddAxisToAxis(0, GamePadAxis.LeftThumbX);
			AddAxisToAxis(1, GamePadAxis.LeftThumbY, true);
			AddAxisToAxis(3, GamePadAxis.RightThumbX);
			AddAxisToAxis(2, GamePadAxis.RightThumbY, true);
			AddButtonToAxis(5, GamePadAxis.LeftTrigger);
			AddButtonToAxis(7, GamePadAxis.RightTrigger);
			 
		}
	}

	
	/// <summary>
    /// 8Bitdo SFC30 GamePad 
    /// </summary>
	internal class Layout35 : GameControllerDatabaseLayout
	{
		public Layout35() : base(new Guid("00092810-0000-0000-0000-504944564944"), "8Bitdo SFC30 GamePad")
		{
			AddButtonToButton(1, GamePadButton.A);
			AddButtonToButton(0, GamePadButton.B);
			AddButtonToButton(3, GamePadButton.Y);
			AddButtonToButton(4, GamePadButton.X);
			AddButtonToButton(11, GamePadButton.Start);
			AddButtonToButton(10, GamePadButton.Back);
			AddButtonToButton(6, GamePadButton.LeftShoulder);
			AddAxisToAxis(0, GamePadAxis.LeftThumbX);
			AddAxisToAxis(1, GamePadAxis.LeftThumbY, true);
			AddButtonToButton(7, GamePadButton.RightShoulder);
			 
		}
	}

	
	/// <summary>
    /// USB Vibration Joystick (BM) 
    /// </summary>
	internal class Layout36 : GameControllerDatabaseLayout
	{
		public Layout36() : base(new Guid("05232563-0000-0000-0000-504944564944"), "USB Vibration Joystick (BM)")
		{
			AddButtonToButton(3, GamePadButton.X);
			AddButtonToButton(2, GamePadButton.A);
			AddButtonToButton(1, GamePadButton.B);
			AddButtonToButton(0, GamePadButton.Y);
			AddButtonToButton(8, GamePadButton.Back);
			AddButtonToButton(9, GamePadButton.Start);
			AddButtonToButton(4, GamePadButton.LeftShoulder);
			AddButtonToAxis(6, GamePadAxis.LeftTrigger);
			AddButtonToButton(5, GamePadButton.RightShoulder);
			AddButtonToAxis(7, GamePadAxis.RightTrigger);
			AddButtonToButton(10, GamePadButton.LeftThumb);
			AddButtonToButton(11, GamePadButton.RightThumb);
			AddAxisToAxis(0, GamePadAxis.LeftThumbX);
			AddAxisToAxis(1, GamePadAxis.LeftThumbY, true);
			AddAxisToAxis(2, GamePadAxis.RightThumbX);
			AddAxisToAxis(3, GamePadAxis.RightThumbY, true);
			 
		}
	}

	
	/// <summary>
    /// 8Bitdo NES30 PRO Wireless 
    /// </summary>
	internal class Layout37 : GameControllerDatabaseLayout
	{
		public Layout37() : base(new Guid("00093820-0000-0000-0000-504944564944"), "8Bitdo NES30 PRO Wireless")
		{
			AddButtonToButton(0, GamePadButton.A);
			AddButtonToButton(1, GamePadButton.B);
			AddButtonToButton(3, GamePadButton.X);
			AddButtonToButton(4, GamePadButton.Y);
			AddButtonToButton(6, GamePadButton.LeftShoulder);
			AddButtonToButton(7, GamePadButton.RightShoulder);
			AddButtonToAxis(8, GamePadAxis.LeftTrigger);
			AddButtonToAxis(9, GamePadAxis.RightTrigger);
			AddButtonToButton(10, GamePadButton.Back);
			AddButtonToButton(11, GamePadButton.Start);
			AddButtonToButton(13, GamePadButton.LeftThumb);
			AddButtonToButton(14, GamePadButton.RightThumb);
			AddAxisToAxis(0, GamePadAxis.LeftThumbX);
			AddAxisToAxis(1, GamePadAxis.LeftThumbY, true);
			AddAxisToAxis(3, GamePadAxis.RightThumbX);
			AddAxisToAxis(4, GamePadAxis.RightThumbY, true);
			 
		}
	}

	
	/// <summary>
    /// 8Bitdo NES30 PRO USB 
    /// </summary>
	internal class Layout38 : GameControllerDatabaseLayout
	{
		public Layout38() : base(new Guid("90002002-0000-0000-0000-504944564944"), "8Bitdo NES30 PRO USB")
		{
			AddButtonToButton(0, GamePadButton.A);
			AddButtonToButton(1, GamePadButton.B);
			AddButtonToButton(3, GamePadButton.X);
			AddButtonToButton(4, GamePadButton.Y);
			AddButtonToButton(6, GamePadButton.LeftShoulder);
			AddButtonToButton(7, GamePadButton.RightShoulder);
			AddButtonToAxis(8, GamePadAxis.LeftTrigger);
			AddButtonToAxis(9, GamePadAxis.RightTrigger);
			AddButtonToButton(10, GamePadButton.Back);
			AddButtonToButton(11, GamePadButton.Start);
			AddButtonToButton(13, GamePadButton.LeftThumb);
			AddButtonToButton(14, GamePadButton.RightThumb);
			AddAxisToAxis(0, GamePadAxis.LeftThumbX);
			AddAxisToAxis(1, GamePadAxis.LeftThumbY, true);
			AddAxisToAxis(3, GamePadAxis.RightThumbX);
			AddAxisToAxis(4, GamePadAxis.RightThumbY, true);
			 
		}
	}

	
	/// <summary>
    /// Gembird JPD-DualForce 
    /// </summary>
	internal class Layout39 : GameControllerDatabaseLayout
	{
		public Layout39() : base(new Guid("333111ff-0000-0000-0000-504944564944"), "Gembird JPD-DualForce")
		{
			AddButtonToButton(2, GamePadButton.A);
			AddButtonToButton(3, GamePadButton.B);
			AddButtonToButton(0, GamePadButton.X);
			AddButtonToButton(1, GamePadButton.Y);
			AddButtonToButton(9, GamePadButton.Start);
			AddButtonToButton(8, GamePadButton.Back);
			AddButtonToButton(4, GamePadButton.LeftShoulder);
			AddButtonToButton(5, GamePadButton.RightShoulder);
			AddAxisToAxis(0, GamePadAxis.LeftThumbX);
			AddAxisToAxis(1, GamePadAxis.LeftThumbY, true);
			AddAxisToAxis(2, GamePadAxis.RightThumbX);
			AddAxisToAxis(4, GamePadAxis.RightThumbY, true);
			AddButtonToAxis(6, GamePadAxis.LeftTrigger);
			AddButtonToAxis(7, GamePadAxis.RightTrigger);
			AddButtonToButton(10, GamePadButton.LeftThumb);
			AddButtonToButton(11, GamePadButton.RightThumb);
			 
		}
	}

	
	/// <summary>
    /// EXEQ RF USB Gamepad 8206 
    /// </summary>
	internal class Layout40 : GameControllerDatabaseLayout
	{
		public Layout40() : base(new Guid("08011a34-0000-0000-0000-504944564944"), "EXEQ RF USB Gamepad 8206")
		{
			AddButtonToButton(0, GamePadButton.A);
			AddButtonToButton(1, GamePadButton.B);
			AddButtonToButton(2, GamePadButton.X);
			AddButtonToButton(3, GamePadButton.Y);
			AddButtonToButton(4, GamePadButton.LeftShoulder);
			AddButtonToButton(5, GamePadButton.RightShoulder);
			AddButtonToButton(8, GamePadButton.LeftThumb);
			AddButtonToButton(7, GamePadButton.RightThumb);
			AddButtonToButton(8, GamePadButton.Back);
			AddButtonToButton(9, GamePadButton.Start);
			AddAxisToAxis(0, GamePadAxis.LeftThumbX);
			AddAxisToAxis(1, GamePadAxis.LeftThumbY, true);
			AddAxisToAxis(2, GamePadAxis.RightThumbX);
			AddAxisToAxis(3, GamePadAxis.RightThumbY, true);
			 
		}
	}

	
	/// <summary>
    /// Battalife Joystick 
    /// </summary>
	internal class Layout41 : GameControllerDatabaseLayout
	{
		public Layout41() : base(new Guid("521311c0-0000-0000-0000-504944564944"), "Battalife Joystick")
		{
			AddButtonToButton(4, GamePadButton.X);
			AddButtonToButton(6, GamePadButton.A);
			AddButtonToButton(7, GamePadButton.B);
			AddButtonToButton(5, GamePadButton.Y);
			AddButtonToButton(2, GamePadButton.Back);
			AddButtonToButton(3, GamePadButton.Start);
			AddButtonToButton(0, GamePadButton.LeftShoulder);
			AddButtonToButton(1, GamePadButton.RightShoulder);
			AddAxisToAxis(0, GamePadAxis.LeftThumbX);
			AddAxisToAxis(1, GamePadAxis.LeftThumbY, true);
			 
		}
	}

	#endif
	#if SILICONSTUDIO_PLATFORM_MACOS
	/// <summary>
    /// GameStop Gamepad 
    /// </summary>
	internal class Layout42 : GameControllerDatabaseLayout
	{
		public Layout42() : base(new Guid("00000005-5347-4720-616d-657061640000"), "GameStop Gamepad")
		{
			AddButtonToButton(0, GamePadButton.A);
			AddButtonToButton(1, GamePadButton.B);
			AddButtonToButton(8, GamePadButton.Back);
			AddButtonToButton(4, GamePadButton.LeftShoulder);
			AddButtonToButton(10, GamePadButton.LeftThumb);
			AddButtonToAxis(6, GamePadAxis.LeftTrigger);
			AddAxisToAxis(0, GamePadAxis.LeftThumbX);
			AddAxisToAxis(1, GamePadAxis.LeftThumbY, true);
			AddButtonToButton(5, GamePadButton.RightShoulder);
			AddButtonToButton(11, GamePadButton.RightThumb);
			AddButtonToAxis(7, GamePadAxis.RightTrigger);
			AddAxisToAxis(2, GamePadAxis.RightThumbX);
			AddAxisToAxis(3, GamePadAxis.RightThumbY, true);
			AddButtonToButton(9, GamePadButton.Start);
			AddButtonToButton(2, GamePadButton.X);
			AddButtonToButton(3, GamePadButton.Y);
			 
		}
	}

	
	/// <summary>
    /// Logitech F310 Gamepad (DInput) 
    /// </summary>
	internal class Layout43 : GameControllerDatabaseLayout
	{
		public Layout43() : base(new Guid("0000046d-0000-0000-16c2-000000000000"), "Logitech F310 Gamepad (DInput)")
		{
			AddButtonToButton(1, GamePadButton.A);
			AddButtonToButton(2, GamePadButton.B);
			AddButtonToButton(8, GamePadButton.Back);
			AddButtonToButton(4, GamePadButton.LeftShoulder);
			AddButtonToButton(10, GamePadButton.LeftThumb);
			AddButtonToAxis(6, GamePadAxis.LeftTrigger);
			AddAxisToAxis(0, GamePadAxis.LeftThumbX);
			AddAxisToAxis(1, GamePadAxis.LeftThumbY, true);
			AddButtonToButton(5, GamePadButton.RightShoulder);
			AddButtonToButton(11, GamePadButton.RightThumb);
			AddButtonToAxis(7, GamePadAxis.RightTrigger);
			AddAxisToAxis(2, GamePadAxis.RightThumbX);
			AddAxisToAxis(3, GamePadAxis.RightThumbY, true);
			AddButtonToButton(9, GamePadButton.Start);
			AddButtonToButton(0, GamePadButton.X);
			AddButtonToButton(3, GamePadButton.Y);
			 
		}
	}

	
	/// <summary>
    /// Logitech F510 Gamepad (DInput) 
    /// </summary>
	internal class Layout44 : GameControllerDatabaseLayout
	{
		public Layout44() : base(new Guid("0000046d-0000-0000-18c2-000000000000"), "Logitech F510 Gamepad (DInput)")
		{
			AddButtonToButton(1, GamePadButton.A);
			AddButtonToButton(2, GamePadButton.B);
			AddButtonToButton(8, GamePadButton.Back);
			AddButtonToButton(4, GamePadButton.LeftShoulder);
			AddButtonToButton(10, GamePadButton.LeftThumb);
			AddButtonToAxis(6, GamePadAxis.LeftTrigger);
			AddAxisToAxis(0, GamePadAxis.LeftThumbX);
			AddAxisToAxis(1, GamePadAxis.LeftThumbY, true);
			AddButtonToButton(5, GamePadButton.RightShoulder);
			AddButtonToButton(11, GamePadButton.RightThumb);
			AddButtonToAxis(7, GamePadAxis.RightTrigger);
			AddAxisToAxis(2, GamePadAxis.RightThumbX);
			AddAxisToAxis(3, GamePadAxis.RightThumbY, true);
			AddButtonToButton(9, GamePadButton.Start);
			AddButtonToButton(0, GamePadButton.X);
			AddButtonToButton(3, GamePadButton.Y);
			 
		}
	}

	
	/// <summary>
    /// Logitech F710 Gamepad (XInput) 
    /// </summary>
	internal class Layout45 : GameControllerDatabaseLayout
	{
		public Layout45() : base(new Guid("0000046d-0000-0000-1fc2-000000000000"), "Logitech F710 Gamepad (XInput)")
		{
			AddButtonToButton(0, GamePadButton.A);
			AddButtonToButton(1, GamePadButton.B);
			AddButtonToButton(9, GamePadButton.Back);
			AddButtonToButton(12, GamePadButton.PadDown);
			AddButtonToButton(13, GamePadButton.PadLeft);
			AddButtonToButton(14, GamePadButton.PadRight);
			AddButtonToButton(11, GamePadButton.PadUp);
			AddButtonToButton(4, GamePadButton.LeftShoulder);
			AddButtonToButton(6, GamePadButton.LeftThumb);
			AddAxisToAxis(2, GamePadAxis.LeftTrigger, remap: true);
			AddAxisToAxis(0, GamePadAxis.LeftThumbX);
			AddAxisToAxis(1, GamePadAxis.LeftThumbY, true);
			AddButtonToButton(5, GamePadButton.RightShoulder);
			AddButtonToButton(7, GamePadButton.RightThumb);
			AddAxisToAxis(5, GamePadAxis.RightTrigger, remap: true);
			AddAxisToAxis(3, GamePadAxis.RightThumbX);
			AddAxisToAxis(4, GamePadAxis.RightThumbY, true);
			AddButtonToButton(8, GamePadButton.Start);
			AddButtonToButton(2, GamePadButton.X);
			AddButtonToButton(3, GamePadButton.Y);
			 
		}
	}

	
	/// <summary>
    /// Logitech Wireless Gamepad (DInput) 
    /// </summary>
	internal class Layout46 : GameControllerDatabaseLayout
	{
		public Layout46() : base(new Guid("0000046d-0000-0000-19c2-000000000000"), "Logitech Wireless Gamepad (DInput)")
		{
			AddButtonToButton(1, GamePadButton.A);
			AddButtonToButton(2, GamePadButton.B);
			AddButtonToButton(8, GamePadButton.Back);
			AddButtonToButton(4, GamePadButton.LeftShoulder);
			AddButtonToButton(10, GamePadButton.LeftThumb);
			AddButtonToAxis(6, GamePadAxis.LeftTrigger);
			AddAxisToAxis(0, GamePadAxis.LeftThumbX);
			AddAxisToAxis(1, GamePadAxis.LeftThumbY, true);
			AddButtonToButton(5, GamePadButton.RightShoulder);
			AddButtonToButton(11, GamePadButton.RightThumb);
			AddButtonToAxis(7, GamePadAxis.RightTrigger);
			AddAxisToAxis(2, GamePadAxis.RightThumbX);
			AddAxisToAxis(3, GamePadAxis.RightThumbY, true);
			AddButtonToButton(9, GamePadButton.Start);
			AddButtonToButton(0, GamePadButton.X);
			AddButtonToButton(3, GamePadButton.Y);
			 
		}
	}

	
	/// <summary>
    /// PS3 Controller 
    /// </summary>
	internal class Layout47 : GameControllerDatabaseLayout
	{
		public Layout47() : base(new Guid("0000054c-0000-0000-6802-000000000000"), "PS3 Controller")
		{
			AddButtonToButton(14, GamePadButton.A);
			AddButtonToButton(13, GamePadButton.B);
			AddButtonToButton(0, GamePadButton.Back);
			AddButtonToButton(6, GamePadButton.PadDown);
			AddButtonToButton(7, GamePadButton.PadLeft);
			AddButtonToButton(5, GamePadButton.PadRight);
			AddButtonToButton(4, GamePadButton.PadUp);
			AddButtonToButton(10, GamePadButton.LeftShoulder);
			AddButtonToButton(1, GamePadButton.LeftThumb);
			AddButtonToAxis(8, GamePadAxis.LeftTrigger);
			AddAxisToAxis(0, GamePadAxis.LeftThumbX);
			AddAxisToAxis(1, GamePadAxis.LeftThumbY, true);
			AddButtonToButton(11, GamePadButton.RightShoulder);
			AddButtonToButton(2, GamePadButton.RightThumb);
			AddButtonToAxis(9, GamePadAxis.RightTrigger);
			AddAxisToAxis(2, GamePadAxis.RightThumbX);
			AddAxisToAxis(3, GamePadAxis.RightThumbY, true);
			AddButtonToButton(3, GamePadButton.Start);
			AddButtonToButton(15, GamePadButton.X);
			AddButtonToButton(12, GamePadButton.Y);
			 
		}
	}

	
	/// <summary>
    /// PS4 Controller 
    /// </summary>
	internal class Layout48 : GameControllerDatabaseLayout
	{
		public Layout48() : base(new Guid("0000054c-0000-0000-c405-000000000000"), "PS4 Controller")
		{
			AddButtonToButton(1, GamePadButton.A);
			AddButtonToButton(2, GamePadButton.B);
			AddButtonToButton(8, GamePadButton.Back);
			AddButtonToButton(4, GamePadButton.LeftShoulder);
			AddButtonToButton(10, GamePadButton.LeftThumb);
			AddAxisToAxis(3, GamePadAxis.LeftTrigger, remap: true);
			AddAxisToAxis(0, GamePadAxis.LeftThumbX);
			AddAxisToAxis(1, GamePadAxis.LeftThumbY, true);
			AddButtonToButton(5, GamePadButton.RightShoulder);
			AddButtonToButton(11, GamePadButton.RightThumb);
			AddAxisToAxis(4, GamePadAxis.RightTrigger, remap: true);
			AddAxisToAxis(2, GamePadAxis.RightThumbX);
			AddAxisToAxis(5, GamePadAxis.RightThumbY, true);
			AddButtonToButton(9, GamePadButton.Start);
			AddButtonToButton(0, GamePadButton.X);
			AddButtonToButton(3, GamePadButton.Y);
			 
		}
	}

	
	/// <summary>
    /// X360 Controller 
    /// </summary>
	internal class Layout49 : GameControllerDatabaseLayout
	{
		public Layout49() : base(new Guid("0000045e-0000-0000-8e02-000000000000"), "X360 Controller")
		{
			AddButtonToButton(0, GamePadButton.A);
			AddButtonToButton(1, GamePadButton.B);
			AddButtonToButton(9, GamePadButton.Back);
			AddButtonToButton(12, GamePadButton.PadDown);
			AddButtonToButton(13, GamePadButton.PadLeft);
			AddButtonToButton(14, GamePadButton.PadRight);
			AddButtonToButton(11, GamePadButton.PadUp);
			AddButtonToButton(4, GamePadButton.LeftShoulder);
			AddButtonToButton(6, GamePadButton.LeftThumb);
			AddAxisToAxis(2, GamePadAxis.LeftTrigger, remap: true);
			AddAxisToAxis(0, GamePadAxis.LeftThumbX);
			AddAxisToAxis(1, GamePadAxis.LeftThumbY, true);
			AddButtonToButton(5, GamePadButton.RightShoulder);
			AddButtonToButton(7, GamePadButton.RightThumb);
			AddAxisToAxis(5, GamePadAxis.RightTrigger, remap: true);
			AddAxisToAxis(3, GamePadAxis.RightThumbX);
			AddAxisToAxis(4, GamePadAxis.RightThumbY, true);
			AddButtonToButton(8, GamePadButton.Start);
			AddButtonToButton(2, GamePadButton.X);
			AddButtonToButton(3, GamePadButton.Y);
			 
		}
	}

	
	/// <summary>
    /// Razer Onza Tournament 
    /// </summary>
	internal class Layout50 : GameControllerDatabaseLayout
	{
		public Layout50() : base(new Guid("00001689-0000-0000-00fd-000000000000"), "Razer Onza Tournament")
		{
			AddButtonToButton(0, GamePadButton.A);
			AddButtonToButton(1, GamePadButton.B);
			AddButtonToButton(3, GamePadButton.Y);
			AddButtonToButton(2, GamePadButton.X);
			AddButtonToButton(8, GamePadButton.Start);
			AddButtonToButton(9, GamePadButton.Back);
			AddButtonToButton(6, GamePadButton.LeftThumb);
			AddButtonToButton(7, GamePadButton.RightThumb);
			AddButtonToButton(4, GamePadButton.LeftShoulder);
			AddButtonToButton(5, GamePadButton.RightShoulder);
			AddButtonToButton(11, GamePadButton.PadUp);
			AddButtonToButton(13, GamePadButton.PadLeft);
			AddButtonToButton(12, GamePadButton.PadDown);
			AddButtonToButton(14, GamePadButton.PadRight);
			AddAxisToAxis(0, GamePadAxis.LeftThumbX);
			AddAxisToAxis(1, GamePadAxis.LeftThumbY, true);
			AddAxisToAxis(3, GamePadAxis.RightThumbX);
			AddAxisToAxis(4, GamePadAxis.RightThumbY, true);
			AddAxisToAxis(2, GamePadAxis.LeftTrigger, remap: true);
			AddAxisToAxis(5, GamePadAxis.RightTrigger, remap: true);
			 
		}
	}

	
	/// <summary>
    /// Thrustmaster Firestorm Dual Power 
    /// </summary>
	internal class Layout51 : GameControllerDatabaseLayout
	{
		public Layout51() : base(new Guid("0000044f-0000-0000-00b3-000000000000"), "Thrustmaster Firestorm Dual Power")
		{
			AddButtonToButton(0, GamePadButton.A);
			AddButtonToButton(2, GamePadButton.B);
			AddButtonToButton(3, GamePadButton.Y);
			AddButtonToButton(1, GamePadButton.X);
			AddButtonToButton(10, GamePadButton.Start);
			AddButtonToButton(9, GamePadButton.Back);
			AddButtonToButton(11, GamePadButton.LeftThumb);
			AddButtonToButton(4, GamePadButton.LeftShoulder);
			AddButtonToButton(6, GamePadButton.RightShoulder);
			AddAxisToAxis(0, GamePadAxis.LeftThumbX);
			AddAxisToAxis(1, GamePadAxis.LeftThumbY, true);
			AddAxisToAxis(2, GamePadAxis.RightThumbX);
			AddAxisToAxis(3, GamePadAxis.RightThumbY, true);
			AddButtonToAxis(5, GamePadAxis.LeftTrigger);
			AddButtonToAxis(7, GamePadAxis.RightTrigger);
			 
		}
	}

	
	/// <summary>
    /// Piranha xtreme 
    /// </summary>
	internal class Layout52 : GameControllerDatabaseLayout
	{
		public Layout52() : base(new Guid("00000e8f-0000-0000-0300-000000000000"), "Piranha xtreme")
		{
			AddButtonToButton(3, GamePadButton.X);
			AddButtonToButton(2, GamePadButton.A);
			AddButtonToButton(1, GamePadButton.B);
			AddButtonToButton(0, GamePadButton.Y);
			AddButtonToButton(8, GamePadButton.Back);
			AddButtonToButton(9, GamePadButton.Start);
			AddButtonToButton(6, GamePadButton.LeftShoulder);
			AddButtonToAxis(4, GamePadAxis.LeftTrigger);
			AddButtonToButton(7, GamePadButton.RightShoulder);
			AddButtonToAxis(5, GamePadAxis.RightTrigger);
			AddButtonToButton(10, GamePadButton.LeftThumb);
			AddButtonToButton(11, GamePadButton.RightThumb);
			AddAxisToAxis(0, GamePadAxis.LeftThumbX);
			AddAxisToAxis(1, GamePadAxis.LeftThumbY, true);
			AddAxisToAxis(3, GamePadAxis.RightThumbX);
			AddAxisToAxis(2, GamePadAxis.RightThumbY, true);
			 
		}
	}

	
	/// <summary>
    /// HORI Gem Pad 3 
    /// </summary>
	internal class Layout53 : GameControllerDatabaseLayout
	{
		public Layout53() : base(new Guid("00000f0d-0000-0000-4d00-000000000000"), "HORI Gem Pad 3")
		{
			AddButtonToButton(1, GamePadButton.A);
			AddButtonToButton(2, GamePadButton.B);
			AddButtonToButton(3, GamePadButton.Y);
			AddButtonToButton(0, GamePadButton.X);
			AddButtonToButton(9, GamePadButton.Start);
			AddButtonToButton(8, GamePadButton.Back);
			AddButtonToButton(10, GamePadButton.LeftThumb);
			AddButtonToButton(11, GamePadButton.RightThumb);
			AddButtonToButton(4, GamePadButton.LeftShoulder);
			AddButtonToButton(5, GamePadButton.RightShoulder);
			AddAxisToAxis(0, GamePadAxis.LeftThumbX);
			AddAxisToAxis(1, GamePadAxis.LeftThumbY, true);
			AddAxisToAxis(2, GamePadAxis.RightThumbX);
			AddAxisToAxis(3, GamePadAxis.RightThumbY, true);
			AddButtonToAxis(6, GamePadAxis.LeftTrigger);
			AddButtonToAxis(7, GamePadAxis.RightTrigger);
			 
		}
	}

	
	/// <summary>
    /// G-Shark GP-702 
    /// </summary>
	internal class Layout54 : GameControllerDatabaseLayout
	{
		public Layout54() : base(new Guid("00000079-0000-0000-0600-000000000000"), "G-Shark GP-702")
		{
			AddButtonToButton(2, GamePadButton.A);
			AddButtonToButton(1, GamePadButton.B);
			AddButtonToButton(3, GamePadButton.X);
			AddButtonToButton(0, GamePadButton.Y);
			AddButtonToButton(8, GamePadButton.Back);
			AddButtonToButton(9, GamePadButton.Start);
			AddButtonToButton(10, GamePadButton.LeftThumb);
			AddButtonToButton(11, GamePadButton.RightThumb);
			AddButtonToButton(4, GamePadButton.LeftShoulder);
			AddButtonToButton(5, GamePadButton.RightShoulder);
			AddAxisToAxis(0, GamePadAxis.LeftThumbX);
			AddAxisToAxis(1, GamePadAxis.LeftThumbY, true);
			AddAxisToAxis(3, GamePadAxis.RightThumbX);
			AddAxisToAxis(4, GamePadAxis.RightThumbY, true);
			AddButtonToAxis(6, GamePadAxis.LeftTrigger);
			AddButtonToAxis(7, GamePadAxis.RightTrigger);
			 
		}
	}

	
	/// <summary>
    /// Thrustmaster Dual Analog 3.2 
    /// </summary>
	internal class Layout55 : GameControllerDatabaseLayout
	{
		public Layout55() : base(new Guid("0000044f-0000-0000-15b3-000000000000"), "Thrustmaster Dual Analog 3.2")
		{
			AddButtonToButton(1, GamePadButton.X);
			AddButtonToButton(0, GamePadButton.A);
			AddButtonToButton(2, GamePadButton.B);
			AddButtonToButton(3, GamePadButton.Y);
			AddButtonToButton(8, GamePadButton.Back);
			AddButtonToButton(9, GamePadButton.Start);
			AddButtonToButton(4, GamePadButton.LeftShoulder);
			AddButtonToAxis(5, GamePadAxis.LeftTrigger);
			AddButtonToButton(6, GamePadButton.RightShoulder);
			AddButtonToAxis(7, GamePadAxis.RightTrigger);
			AddButtonToButton(10, GamePadButton.LeftThumb);
			AddButtonToButton(11, GamePadButton.RightThumb);
			AddAxisToAxis(0, GamePadAxis.LeftThumbX);
			AddAxisToAxis(1, GamePadAxis.LeftThumbY, true);
			AddAxisToAxis(2, GamePadAxis.RightThumbX);
			AddAxisToAxis(3, GamePadAxis.RightThumbY, true);
			 
		}
	}

	
	/// <summary>
    /// Gamestop BB-070 X360 Controller 
    /// </summary>
	internal class Layout56 : GameControllerDatabaseLayout
	{
		public Layout56() : base(new Guid("00001bad-0000-0000-01f9-000000000000"), "Gamestop BB-070 X360 Controller")
		{
			AddButtonToButton(0, GamePadButton.A);
			AddButtonToButton(1, GamePadButton.B);
			AddButtonToButton(9, GamePadButton.Back);
			AddButtonToButton(12, GamePadButton.PadDown);
			AddButtonToButton(13, GamePadButton.PadLeft);
			AddButtonToButton(14, GamePadButton.PadRight);
			AddButtonToButton(11, GamePadButton.PadUp);
			AddButtonToButton(4, GamePadButton.LeftShoulder);
			AddButtonToButton(6, GamePadButton.LeftThumb);
			AddAxisToAxis(2, GamePadAxis.LeftTrigger, remap: true);
			AddAxisToAxis(0, GamePadAxis.LeftThumbX);
			AddAxisToAxis(1, GamePadAxis.LeftThumbY, true);
			AddButtonToButton(5, GamePadButton.RightShoulder);
			AddButtonToButton(7, GamePadButton.RightThumb);
			AddAxisToAxis(5, GamePadAxis.RightTrigger, remap: true);
			AddAxisToAxis(3, GamePadAxis.RightThumbX);
			AddAxisToAxis(4, GamePadAxis.RightThumbY, true);
			AddButtonToButton(8, GamePadButton.Start);
			AddButtonToButton(2, GamePadButton.X);
			AddButtonToButton(3, GamePadButton.Y);
			 
		}
	}

	
	/// <summary>
    /// Wii Remote 
    /// </summary>
	internal class Layout57 : GameControllerDatabaseLayout
	{
		public Layout57() : base(new Guid("00000005-6957-6d69-6f74-652028303000"), "Wii Remote")
		{
			AddButtonToButton(4, GamePadButton.A);
			AddButtonToButton(5, GamePadButton.B);
			AddButtonToButton(9, GamePadButton.Y);
			AddButtonToButton(10, GamePadButton.X);
			AddButtonToButton(6, GamePadButton.Start);
			AddButtonToButton(7, GamePadButton.Back);
			AddButtonToButton(2, GamePadButton.PadUp);
			AddButtonToButton(0, GamePadButton.PadLeft);
			AddButtonToButton(3, GamePadButton.PadDown);
			AddButtonToButton(1, GamePadButton.PadRight);
			AddAxisToAxis(0, GamePadAxis.LeftThumbX);
			AddAxisToAxis(1, GamePadAxis.LeftThumbY, true);
			AddButtonToAxis(12, GamePadAxis.LeftTrigger);
			AddButtonToButton(11, GamePadButton.LeftShoulder);
			 
		}
	}

	
	/// <summary>
    /// iBuffalo USB 2-axis 8-button Gamepad 
    /// </summary>
	internal class Layout58 : GameControllerDatabaseLayout
	{
		public Layout58() : base(new Guid("00000583-0000-0000-6020-000000000000"), "iBuffalo USB 2-axis 8-button Gamepad")
		{
			AddButtonToButton(1, GamePadButton.A);
			AddButtonToButton(0, GamePadButton.B);
			AddButtonToButton(3, GamePadButton.X);
			AddButtonToButton(2, GamePadButton.Y);
			AddButtonToButton(6, GamePadButton.Back);
			AddButtonToButton(7, GamePadButton.Start);
			AddButtonToButton(4, GamePadButton.LeftShoulder);
			AddButtonToButton(5, GamePadButton.RightShoulder);
			AddAxisToAxis(0, GamePadAxis.LeftThumbX);
			AddAxisToAxis(1, GamePadAxis.LeftThumbY, true);
			 
		}
	}

	
	/// <summary>
    /// Xbox One Wired Controller 
    /// </summary>
	internal class Layout59 : GameControllerDatabaseLayout
	{
		public Layout59() : base(new Guid("0000045e-0000-0000-dd02-000000000000"), "Xbox One Wired Controller")
		{
			AddButtonToButton(2, GamePadButton.X);
			AddButtonToButton(0, GamePadButton.A);
			AddButtonToButton(1, GamePadButton.B);
			AddButtonToButton(3, GamePadButton.Y);
			AddButtonToButton(9, GamePadButton.Back);
			AddButtonToButton(8, GamePadButton.Start);
			AddButtonToButton(13, GamePadButton.PadLeft);
			AddButtonToButton(12, GamePadButton.PadDown);
			AddButtonToButton(14, GamePadButton.PadRight);
			AddButtonToButton(11, GamePadButton.PadUp);
			AddButtonToButton(4, GamePadButton.LeftShoulder);
			AddAxisToAxis(2, GamePadAxis.LeftTrigger, remap: true);
			AddButtonToButton(5, GamePadButton.RightShoulder);
			AddAxisToAxis(5, GamePadAxis.RightTrigger, remap: true);
			AddButtonToButton(6, GamePadButton.LeftThumb);
			AddButtonToButton(7, GamePadButton.RightThumb);
			AddAxisToAxis(0, GamePadAxis.LeftThumbX);
			AddAxisToAxis(1, GamePadAxis.LeftThumbY, true);
			AddAxisToAxis(3, GamePadAxis.RightThumbX);
			AddAxisToAxis(4, GamePadAxis.RightThumbY, true);
			 
		}
	}

	
	/// <summary>
    /// Wii U Pro Controller 
    /// </summary>
	internal class Layout60 : GameControllerDatabaseLayout
	{
		public Layout60() : base(new Guid("00000005-6957-6d69-6f74-652028313800"), "Wii U Pro Controller")
		{
			AddButtonToButton(16, GamePadButton.A);
			AddButtonToButton(15, GamePadButton.B);
			AddButtonToButton(18, GamePadButton.X);
			AddButtonToButton(17, GamePadButton.Y);
			AddButtonToButton(7, GamePadButton.Back);
			AddButtonToButton(6, GamePadButton.Start);
			AddButtonToButton(23, GamePadButton.LeftThumb);
			AddButtonToButton(24, GamePadButton.RightThumb);
			AddButtonToButton(19, GamePadButton.LeftShoulder);
			AddButtonToButton(20, GamePadButton.RightShoulder);
			AddButtonToButton(11, GamePadButton.PadUp);
			AddButtonToButton(12, GamePadButton.PadDown);
			AddButtonToButton(13, GamePadButton.PadLeft);
			AddButtonToButton(14, GamePadButton.PadRight);
			AddAxisToAxis(0, GamePadAxis.LeftThumbX);
			AddAxisToAxis(1, GamePadAxis.LeftThumbY, true);
			AddAxisToAxis(2, GamePadAxis.RightThumbX);
			AddAxisToAxis(3, GamePadAxis.RightThumbY, true);
			AddButtonToAxis(21, GamePadAxis.LeftTrigger);
			AddButtonToAxis(22, GamePadAxis.RightTrigger);
			 
		}
	}

	
	/// <summary>
    /// Mayflash WiiU Pro Game Controller Adapter (DInput) 
    /// </summary>
	internal class Layout61 : GameControllerDatabaseLayout
	{
		public Layout61() : base(new Guid("00000079-0000-0000-0018-000000000000"), "Mayflash WiiU Pro Game Controller Adapter (DInput)")
		{
			AddButtonToButton(4, GamePadButton.A);
			AddButtonToButton(8, GamePadButton.B);
			AddButtonToButton(0, GamePadButton.X);
			AddButtonToButton(12, GamePadButton.Y);
			AddButtonToButton(32, GamePadButton.Back);
			AddButtonToButton(36, GamePadButton.Start);
			AddButtonToButton(40, GamePadButton.LeftThumb);
			AddButtonToButton(44, GamePadButton.RightThumb);
			AddButtonToButton(16, GamePadButton.LeftShoulder);
			AddButtonToButton(20, GamePadButton.RightShoulder);
			AddAxisToAxis(0, GamePadAxis.LeftThumbX);
			AddAxisToAxis(4, GamePadAxis.LeftThumbY, true);
			AddAxisToAxis(8, GamePadAxis.RightThumbX);
			AddAxisToAxis(12, GamePadAxis.RightThumbY, true);
			AddButtonToAxis(24, GamePadAxis.LeftTrigger);
			AddButtonToAxis(28, GamePadAxis.RightTrigger);
			 
		}
	}

	
	/// <summary>
    /// Mayflash Wii Classic Controller 
    /// </summary>
	internal class Layout62 : GameControllerDatabaseLayout
	{
		public Layout62() : base(new Guid("00000925-0000-0000-e803-000000000000"), "Mayflash Wii Classic Controller")
		{
			AddButtonToButton(1, GamePadButton.A);
			AddButtonToButton(0, GamePadButton.B);
			AddButtonToButton(3, GamePadButton.X);
			AddButtonToButton(2, GamePadButton.Y);
			AddButtonToButton(8, GamePadButton.Back);
			AddButtonToButton(9, GamePadButton.Start);
			AddButtonToButton(4, GamePadButton.LeftShoulder);
			AddButtonToButton(5, GamePadButton.RightShoulder);
			AddButtonToButton(11, GamePadButton.PadUp);
			AddButtonToButton(13, GamePadButton.PadDown);
			AddButtonToButton(12, GamePadButton.PadLeft);
			AddButtonToButton(14, GamePadButton.PadRight);
			AddAxisToAxis(0, GamePadAxis.LeftThumbX);
			AddAxisToAxis(1, GamePadAxis.LeftThumbY, true);
			AddAxisToAxis(2, GamePadAxis.RightThumbX);
			AddAxisToAxis(3, GamePadAxis.RightThumbY, true);
			AddButtonToAxis(6, GamePadAxis.LeftTrigger);
			AddButtonToAxis(7, GamePadAxis.RightTrigger);
			 
		}
	}

	
	/// <summary>
    /// SFC30 Joystick 
    /// </summary>
	internal class Layout63 : GameControllerDatabaseLayout
	{
		public Layout63() : base(new Guid("00001235-0000-0000-21ab-000000000000"), "SFC30 Joystick")
		{
			AddButtonToButton(1, GamePadButton.A);
			AddButtonToButton(0, GamePadButton.B);
			AddButtonToButton(4, GamePadButton.X);
			AddButtonToButton(3, GamePadButton.Y);
			AddButtonToButton(10, GamePadButton.Back);
			AddButtonToButton(11, GamePadButton.Start);
			AddButtonToButton(6, GamePadButton.LeftShoulder);
			AddButtonToButton(7, GamePadButton.RightShoulder);
			AddAxisToAxis(0, GamePadAxis.LeftThumbX);
			AddAxisToAxis(1, GamePadAxis.LeftThumbY, true);
			 
		}
	}

	
	/// <summary>
    /// Sega Saturn USB Gamepad 
    /// </summary>
	internal class Layout64 : GameControllerDatabaseLayout
	{
		public Layout64() : base(new Guid("000004b4-0000-0000-0a01-000000000000"), "Sega Saturn USB Gamepad")
		{
			AddButtonToButton(0, GamePadButton.A);
			AddButtonToButton(1, GamePadButton.B);
			AddButtonToButton(3, GamePadButton.X);
			AddButtonToButton(4, GamePadButton.Y);
			AddButtonToButton(5, GamePadButton.Back);
			AddButtonToButton(8, GamePadButton.Start);
			AddButtonToButton(6, GamePadButton.LeftShoulder);
			AddButtonToButton(7, GamePadButton.RightShoulder);
			AddAxisToAxis(0, GamePadAxis.LeftThumbX);
			AddAxisToAxis(1, GamePadAxis.LeftThumbY, true);
			 
		}
	}

	
	/// <summary>
    /// 8Bitdo SFC30 GamePad 
    /// </summary>
	internal class Layout65 : GameControllerDatabaseLayout
	{
		public Layout65() : base(new Guid("00002810-0000-0000-0900-000000000000"), "8Bitdo SFC30 GamePad")
		{
			AddButtonToButton(1, GamePadButton.A);
			AddButtonToButton(0, GamePadButton.B);
			AddButtonToButton(4, GamePadButton.X);
			AddButtonToButton(3, GamePadButton.Y);
			AddButtonToButton(10, GamePadButton.Back);
			AddButtonToButton(11, GamePadButton.Start);
			AddButtonToButton(6, GamePadButton.LeftShoulder);
			AddButtonToButton(7, GamePadButton.RightShoulder);
			AddAxisToAxis(0, GamePadAxis.LeftThumbX);
			AddAxisToAxis(1, GamePadAxis.LeftThumbY, true);
			 
		}
	}

	
	/// <summary>
    /// MC Cthulhu 
    /// </summary>
	internal class Layout66 : GameControllerDatabaseLayout
	{
		public Layout66() : base(new Guid("000014d8-0000-0000-cecf-000000000000"), "MC Cthulhu")
		{
			AddButtonToAxis(6, GamePadAxis.LeftTrigger);
			AddButtonToButton(1, GamePadButton.A);
			AddButtonToButton(2, GamePadButton.B);
			AddButtonToButton(3, GamePadButton.Y);
			AddButtonToButton(0, GamePadButton.X);
			AddButtonToButton(9, GamePadButton.Start);
			AddButtonToButton(8, GamePadButton.Back);
			AddButtonToButton(4, GamePadButton.LeftShoulder);
			AddButtonToButton(5, GamePadButton.RightShoulder);
			AddButtonToAxis(7, GamePadAxis.RightTrigger);
			 
		}
	}

	
	/// <summary>
    /// HORIPAD FPS PLUS 4 
    /// </summary>
	internal class Layout67 : GameControllerDatabaseLayout
	{
		public Layout67() : base(new Guid("00000f0d-0000-0000-6600-000000000000"), "HORIPAD FPS PLUS 4")
		{
			AddButtonToButton(1, GamePadButton.A);
			AddButtonToButton(2, GamePadButton.B);
			AddButtonToButton(3, GamePadButton.Y);
			AddButtonToButton(0, GamePadButton.X);
			AddButtonToButton(9, GamePadButton.Start);
			AddButtonToButton(8, GamePadButton.Back);
			AddButtonToButton(10, GamePadButton.LeftThumb);
			AddButtonToButton(11, GamePadButton.RightThumb);
			AddButtonToButton(4, GamePadButton.LeftShoulder);
			AddButtonToButton(5, GamePadButton.RightShoulder);
			AddAxisToAxis(0, GamePadAxis.LeftThumbX);
			AddAxisToAxis(1, GamePadAxis.LeftThumbY, true);
			AddAxisToAxis(2, GamePadAxis.RightThumbX);
			AddAxisToAxis(5, GamePadAxis.RightThumbY, true);
			AddButtonToAxis(6, GamePadAxis.LeftTrigger);
			AddAxisToAxis(4, GamePadAxis.RightTrigger, remap: true);
			 
		}
	}

	#endif
	#if SILICONSTUDIO_PLATFORM_UNIX
	/// <summary>
    /// GameStop Gamepad 
    /// </summary>
	internal class Layout68 : GameControllerDatabaseLayout
	{
		public Layout68() : base(new Guid("00000005-5347-4720-616d-657061640000"), "GameStop Gamepad")
		{
			AddButtonToButton(0, GamePadButton.A);
			AddButtonToButton(1, GamePadButton.B);
			AddButtonToButton(8, GamePadButton.Back);
			AddButtonToButton(4, GamePadButton.LeftShoulder);
			AddButtonToButton(10, GamePadButton.LeftThumb);
			AddButtonToAxis(6, GamePadAxis.LeftTrigger);
			AddAxisToAxis(0, GamePadAxis.LeftThumbX);
			AddAxisToAxis(1, GamePadAxis.LeftThumbY, true);
			AddButtonToButton(5, GamePadButton.RightShoulder);
			AddButtonToButton(11, GamePadButton.RightThumb);
			AddButtonToAxis(7, GamePadAxis.RightTrigger);
			AddAxisToAxis(2, GamePadAxis.RightThumbX);
			AddAxisToAxis(3, GamePadAxis.RightThumbY, true);
			AddButtonToButton(9, GamePadButton.Start);
			AddButtonToButton(2, GamePadButton.X);
			AddButtonToButton(3, GamePadButton.Y);
			 
		}
	}

	
	/// <summary>
    /// Jess Technology USB Game Controller 
    /// </summary>
	internal class Layout69 : GameControllerDatabaseLayout
	{
		public Layout69() : base(new Guid("00000003-22ba-0000-2010-000001010000"), "Jess Technology USB Game Controller")
		{
			AddButtonToButton(2, GamePadButton.A);
			AddButtonToButton(1, GamePadButton.B);
			AddButtonToButton(8, GamePadButton.Back);
			AddButtonToButton(4, GamePadButton.LeftShoulder);
			AddButtonToAxis(6, GamePadAxis.LeftTrigger);
			AddAxisToAxis(0, GamePadAxis.LeftThumbX);
			AddAxisToAxis(1, GamePadAxis.LeftThumbY, true);
			AddButtonToButton(5, GamePadButton.RightShoulder);
			AddButtonToAxis(7, GamePadAxis.RightTrigger);
			AddAxisToAxis(3, GamePadAxis.RightThumbX);
			AddAxisToAxis(2, GamePadAxis.RightThumbY, true);
			AddButtonToButton(9, GamePadButton.Start);
			AddButtonToButton(3, GamePadButton.X);
			AddButtonToButton(0, GamePadButton.Y);
			 
		}
	}

	
	/// <summary>
    /// Logitech Cordless RumblePad 2 
    /// </summary>
	internal class Layout70 : GameControllerDatabaseLayout
	{
		public Layout70() : base(new Guid("00000003-046d-0000-19c2-000010010000"), "Logitech Cordless RumblePad 2")
		{
			AddButtonToButton(1, GamePadButton.A);
			AddButtonToButton(2, GamePadButton.B);
			AddButtonToButton(8, GamePadButton.Back);
			AddButtonToButton(4, GamePadButton.LeftShoulder);
			AddButtonToButton(10, GamePadButton.LeftThumb);
			AddButtonToAxis(6, GamePadAxis.LeftTrigger);
			AddAxisToAxis(0, GamePadAxis.LeftThumbX);
			AddAxisToAxis(1, GamePadAxis.LeftThumbY, true);
			AddButtonToButton(5, GamePadButton.RightShoulder);
			AddButtonToButton(11, GamePadButton.RightThumb);
			AddButtonToAxis(7, GamePadAxis.RightTrigger);
			AddAxisToAxis(2, GamePadAxis.RightThumbX);
			AddAxisToAxis(3, GamePadAxis.RightThumbY, true);
			AddButtonToButton(9, GamePadButton.Start);
			AddButtonToButton(0, GamePadButton.X);
			AddButtonToButton(3, GamePadButton.Y);
			 
		}
	}

	
	/// <summary>
    /// Logitech F310 Gamepad (XInput) 
    /// </summary>
	internal class Layout71 : GameControllerDatabaseLayout
	{
		public Layout71() : base(new Guid("00000003-046d-0000-1dc2-000014400000"), "Logitech F310 Gamepad (XInput)")
		{
			AddButtonToButton(0, GamePadButton.A);
			AddButtonToButton(1, GamePadButton.B);
			AddButtonToButton(6, GamePadButton.Back);
			AddButtonToButton(4, GamePadButton.LeftShoulder);
			AddButtonToButton(9, GamePadButton.LeftThumb);
			AddAxisToAxis(2, GamePadAxis.LeftTrigger, remap: true);
			AddAxisToAxis(0, GamePadAxis.LeftThumbX);
			AddAxisToAxis(1, GamePadAxis.LeftThumbY, true);
			AddButtonToButton(5, GamePadButton.RightShoulder);
			AddButtonToButton(10, GamePadButton.RightThumb);
			AddAxisToAxis(5, GamePadAxis.RightTrigger, remap: true);
			AddAxisToAxis(3, GamePadAxis.RightThumbX);
			AddAxisToAxis(4, GamePadAxis.RightThumbY, true);
			AddButtonToButton(7, GamePadButton.Start);
			AddButtonToButton(2, GamePadButton.X);
			AddButtonToButton(3, GamePadButton.Y);
			 
		}
	}

	
	/// <summary>
    /// Logitech F510 Gamepad (XInput) 
    /// </summary>
	internal class Layout72 : GameControllerDatabaseLayout
	{
		public Layout72() : base(new Guid("00000003-046d-0000-1ec2-000020200000"), "Logitech F510 Gamepad (XInput)")
		{
			AddButtonToButton(0, GamePadButton.A);
			AddButtonToButton(1, GamePadButton.B);
			AddButtonToButton(6, GamePadButton.Back);
			AddButtonToButton(4, GamePadButton.LeftShoulder);
			AddButtonToButton(9, GamePadButton.LeftThumb);
			AddAxisToAxis(2, GamePadAxis.LeftTrigger, remap: true);
			AddAxisToAxis(0, GamePadAxis.LeftThumbX);
			AddAxisToAxis(1, GamePadAxis.LeftThumbY, true);
			AddButtonToButton(5, GamePadButton.RightShoulder);
			AddButtonToButton(10, GamePadButton.RightThumb);
			AddAxisToAxis(5, GamePadAxis.RightTrigger, remap: true);
			AddAxisToAxis(3, GamePadAxis.RightThumbX);
			AddAxisToAxis(4, GamePadAxis.RightThumbY, true);
			AddButtonToButton(7, GamePadButton.Start);
			AddButtonToButton(2, GamePadButton.X);
			AddButtonToButton(3, GamePadButton.Y);
			 
		}
	}

	
	/// <summary>
    /// Logitech F710 Gamepad (DInput) 
    /// </summary>
	internal class Layout73 : GameControllerDatabaseLayout
	{
		public Layout73() : base(new Guid("00000003-046d-0000-19c2-000011010000"), "Logitech F710 Gamepad (DInput)")
		{
			AddButtonToButton(1, GamePadButton.A);
			AddButtonToButton(2, GamePadButton.B);
			AddButtonToButton(8, GamePadButton.Back);
			AddButtonToButton(4, GamePadButton.LeftShoulder);
			AddButtonToButton(10, GamePadButton.LeftThumb);
			AddButtonToAxis(6, GamePadAxis.LeftTrigger);
			AddAxisToAxis(0, GamePadAxis.LeftThumbX);
			AddAxisToAxis(1, GamePadAxis.LeftThumbY, true);
			AddButtonToButton(5, GamePadButton.RightShoulder);
			AddButtonToButton(11, GamePadButton.RightThumb);
			AddButtonToAxis(7, GamePadAxis.RightTrigger);
			AddAxisToAxis(2, GamePadAxis.RightThumbX);
			AddAxisToAxis(3, GamePadAxis.RightThumbY, true);
			AddButtonToButton(9, GamePadButton.Start);
			AddButtonToButton(0, GamePadButton.X);
			AddButtonToButton(3, GamePadButton.Y);
			 
		}
	}

	
	/// <summary>
    /// Logitech F710 Gamepad (XInput) 
    /// </summary>
	internal class Layout74 : GameControllerDatabaseLayout
	{
		public Layout74() : base(new Guid("00000003-046d-0000-1fc2-000005030000"), "Logitech F710 Gamepad (XInput)")
		{
			AddButtonToButton(0, GamePadButton.A);
			AddButtonToButton(1, GamePadButton.B);
			AddButtonToButton(6, GamePadButton.Back);
			AddButtonToButton(4, GamePadButton.LeftShoulder);
			AddButtonToButton(9, GamePadButton.LeftThumb);
			AddAxisToAxis(2, GamePadAxis.LeftTrigger, remap: true);
			AddAxisToAxis(0, GamePadAxis.LeftThumbX);
			AddAxisToAxis(1, GamePadAxis.LeftThumbY, true);
			AddButtonToButton(5, GamePadButton.RightShoulder);
			AddButtonToButton(10, GamePadButton.RightThumb);
			AddAxisToAxis(5, GamePadAxis.RightTrigger, remap: true);
			AddAxisToAxis(3, GamePadAxis.RightThumbX);
			AddAxisToAxis(4, GamePadAxis.RightThumbY, true);
			AddButtonToButton(7, GamePadButton.Start);
			AddButtonToButton(2, GamePadButton.X);
			AddButtonToButton(3, GamePadButton.Y);
			 
		}
	}

	
	/// <summary>
    /// PS3 Controller 
    /// </summary>
	internal class Layout75 : GameControllerDatabaseLayout
	{
		public Layout75() : base(new Guid("00000003-054c-0000-6802-000011010000"), "PS3 Controller")
		{
			AddButtonToButton(14, GamePadButton.A);
			AddButtonToButton(13, GamePadButton.B);
			AddButtonToButton(0, GamePadButton.Back);
			AddButtonToButton(6, GamePadButton.PadDown);
			AddButtonToButton(7, GamePadButton.PadLeft);
			AddButtonToButton(5, GamePadButton.PadRight);
			AddButtonToButton(4, GamePadButton.PadUp);
			AddButtonToButton(10, GamePadButton.LeftShoulder);
			AddButtonToButton(1, GamePadButton.LeftThumb);
			AddButtonToAxis(8, GamePadAxis.LeftTrigger);
			AddAxisToAxis(0, GamePadAxis.LeftThumbX);
			AddAxisToAxis(1, GamePadAxis.LeftThumbY, true);
			AddButtonToButton(11, GamePadButton.RightShoulder);
			AddButtonToButton(2, GamePadButton.RightThumb);
			AddButtonToAxis(9, GamePadAxis.RightTrigger);
			AddAxisToAxis(2, GamePadAxis.RightThumbX);
			AddAxisToAxis(3, GamePadAxis.RightThumbY, true);
			AddButtonToButton(3, GamePadButton.Start);
			AddButtonToButton(15, GamePadButton.X);
			AddButtonToButton(12, GamePadButton.Y);
			 
		}
	}

	
	/// <summary>
    /// Sony DualShock 4 
    /// </summary>
	internal class Layout76 : GameControllerDatabaseLayout
	{
		public Layout76() : base(new Guid("00000003-054c-0000-c405-000011010000"), "Sony DualShock 4")
		{
			AddButtonToButton(1, GamePadButton.A);
			AddButtonToButton(2, GamePadButton.B);
			AddButtonToButton(3, GamePadButton.Y);
			AddButtonToButton(0, GamePadButton.X);
			AddButtonToButton(9, GamePadButton.Start);
			AddButtonToButton(8, GamePadButton.Back);
			AddButtonToButton(10, GamePadButton.LeftThumb);
			AddButtonToButton(11, GamePadButton.RightThumb);
			AddButtonToButton(4, GamePadButton.LeftShoulder);
			AddButtonToButton(5, GamePadButton.RightShoulder);
			AddAxisToAxis(0, GamePadAxis.LeftThumbX);
			AddAxisToAxis(1, GamePadAxis.LeftThumbY, true);
			AddAxisToAxis(2, GamePadAxis.RightThumbX);
			AddAxisToAxis(5, GamePadAxis.RightThumbY, true);
			AddButtonToAxis(6, GamePadAxis.LeftTrigger);
			AddButtonToAxis(7, GamePadAxis.RightTrigger);
			 
		}
	}

	
	/// <summary>
    /// EA Sports PS3 Controller 
    /// </summary>
	internal class Layout77 : GameControllerDatabaseLayout
	{
		public Layout77() : base(new Guid("00000003-0e6f-0000-3001-000001010000"), "EA Sports PS3 Controller")
		{
			AddButtonToButton(1, GamePadButton.A);
			AddButtonToButton(2, GamePadButton.B);
			AddButtonToButton(3, GamePadButton.Y);
			AddButtonToButton(0, GamePadButton.X);
			AddButtonToButton(9, GamePadButton.Start);
			AddButtonToButton(8, GamePadButton.Back);
			AddButtonToButton(10, GamePadButton.LeftThumb);
			AddButtonToButton(11, GamePadButton.RightThumb);
			AddButtonToButton(4, GamePadButton.LeftShoulder);
			AddButtonToButton(5, GamePadButton.RightShoulder);
			AddAxisToAxis(0, GamePadAxis.LeftThumbX);
			AddAxisToAxis(1, GamePadAxis.LeftThumbY, true);
			AddAxisToAxis(2, GamePadAxis.RightThumbX);
			AddAxisToAxis(3, GamePadAxis.RightThumbY, true);
			AddButtonToAxis(6, GamePadAxis.LeftTrigger);
			AddButtonToAxis(7, GamePadAxis.RightTrigger);
			 
		}
	}

	
	/// <summary>
    /// Valve Streaming Gamepad 
    /// </summary>
	internal class Layout78 : GameControllerDatabaseLayout
	{
		public Layout78() : base(new Guid("00000003-28de-0000-ff11-000001000000"), "Valve Streaming Gamepad")
		{
			AddButtonToButton(0, GamePadButton.A);
			AddButtonToButton(1, GamePadButton.B);
			AddButtonToButton(6, GamePadButton.Back);
			AddButtonToButton(4, GamePadButton.LeftShoulder);
			AddButtonToButton(9, GamePadButton.LeftThumb);
			AddAxisToAxis(2, GamePadAxis.LeftTrigger, remap: true);
			AddAxisToAxis(0, GamePadAxis.LeftThumbX);
			AddAxisToAxis(1, GamePadAxis.LeftThumbY, true);
			AddButtonToButton(5, GamePadButton.RightShoulder);
			AddButtonToButton(10, GamePadButton.RightThumb);
			AddAxisToAxis(5, GamePadAxis.RightTrigger, remap: true);
			AddAxisToAxis(3, GamePadAxis.RightThumbX);
			AddAxisToAxis(4, GamePadAxis.RightThumbY, true);
			AddButtonToButton(7, GamePadButton.Start);
			AddButtonToButton(2, GamePadButton.X);
			AddButtonToButton(3, GamePadButton.Y);
			 
		}
	}

	
	/// <summary>
    /// X360 Controller 
    /// </summary>
	internal class Layout79 : GameControllerDatabaseLayout
	{
		public Layout79() : base(new Guid("00000003-045e-0000-8e02-000014010000"), "X360 Controller")
		{
			AddButtonToButton(0, GamePadButton.A);
			AddButtonToButton(1, GamePadButton.B);
			AddButtonToButton(6, GamePadButton.Back);
			AddButtonToButton(4, GamePadButton.LeftShoulder);
			AddButtonToButton(9, GamePadButton.LeftThumb);
			AddAxisToAxis(2, GamePadAxis.LeftTrigger, remap: true);
			AddAxisToAxis(0, GamePadAxis.LeftThumbX);
			AddAxisToAxis(1, GamePadAxis.LeftThumbY, true);
			AddButtonToButton(5, GamePadButton.RightShoulder);
			AddButtonToButton(10, GamePadButton.RightThumb);
			AddAxisToAxis(5, GamePadAxis.RightTrigger, remap: true);
			AddAxisToAxis(3, GamePadAxis.RightThumbX);
			AddAxisToAxis(4, GamePadAxis.RightThumbY, true);
			AddButtonToButton(7, GamePadButton.Start);
			AddButtonToButton(2, GamePadButton.X);
			AddButtonToButton(3, GamePadButton.Y);
			 
		}
	}

	
	/// <summary>
    /// X360 Controller 
    /// </summary>
	internal class Layout80 : GameControllerDatabaseLayout
	{
		public Layout80() : base(new Guid("00000003-045e-0000-8e02-000010010000"), "X360 Controller")
		{
			AddButtonToButton(0, GamePadButton.A);
			AddButtonToButton(1, GamePadButton.B);
			AddButtonToButton(6, GamePadButton.Back);
			AddButtonToButton(4, GamePadButton.LeftShoulder);
			AddButtonToButton(9, GamePadButton.LeftThumb);
			AddAxisToAxis(2, GamePadAxis.LeftTrigger, remap: true);
			AddAxisToAxis(0, GamePadAxis.LeftThumbX);
			AddAxisToAxis(1, GamePadAxis.LeftThumbY, true);
			AddButtonToButton(5, GamePadButton.RightShoulder);
			AddButtonToButton(10, GamePadButton.RightThumb);
			AddAxisToAxis(5, GamePadAxis.RightTrigger, remap: true);
			AddAxisToAxis(3, GamePadAxis.RightThumbX);
			AddAxisToAxis(4, GamePadAxis.RightThumbY, true);
			AddButtonToButton(7, GamePadButton.Start);
			AddButtonToButton(2, GamePadButton.X);
			AddButtonToButton(3, GamePadButton.Y);
			 
		}
	}

	
	/// <summary>
    /// X360 Wireless Controller 
    /// </summary>
	internal class Layout81 : GameControllerDatabaseLayout
	{
		public Layout81() : base(new Guid("00000003-045e-0000-1907-000000010000"), "X360 Wireless Controller")
		{
			AddButtonToButton(0, GamePadButton.A);
			AddButtonToButton(1, GamePadButton.B);
			AddButtonToButton(6, GamePadButton.Back);
			AddButtonToButton(14, GamePadButton.PadDown);
			AddButtonToButton(11, GamePadButton.PadLeft);
			AddButtonToButton(12, GamePadButton.PadRight);
			AddButtonToButton(13, GamePadButton.PadUp);
			AddButtonToButton(4, GamePadButton.LeftShoulder);
			AddButtonToButton(9, GamePadButton.LeftThumb);
			AddAxisToAxis(2, GamePadAxis.LeftTrigger, remap: true);
			AddAxisToAxis(0, GamePadAxis.LeftThumbX);
			AddAxisToAxis(1, GamePadAxis.LeftThumbY, true);
			AddButtonToButton(5, GamePadButton.RightShoulder);
			AddButtonToButton(10, GamePadButton.RightThumb);
			AddAxisToAxis(5, GamePadAxis.RightTrigger, remap: true);
			AddAxisToAxis(3, GamePadAxis.RightThumbX);
			AddAxisToAxis(4, GamePadAxis.RightThumbY, true);
			AddButtonToButton(7, GamePadButton.Start);
			AddButtonToButton(2, GamePadButton.X);
			AddButtonToButton(3, GamePadButton.Y);
			 
		}
	}

	
	/// <summary>
    /// Twin USB PS2 Adapter 
    /// </summary>
	internal class Layout82 : GameControllerDatabaseLayout
	{
		public Layout82() : base(new Guid("00000003-0810-0000-0100-000010010000"), "Twin USB PS2 Adapter")
		{
			AddButtonToButton(2, GamePadButton.A);
			AddButtonToButton(1, GamePadButton.B);
			AddButtonToButton(0, GamePadButton.Y);
			AddButtonToButton(3, GamePadButton.X);
			AddButtonToButton(9, GamePadButton.Start);
			AddButtonToButton(8, GamePadButton.Back);
			AddButtonToButton(10, GamePadButton.LeftThumb);
			AddButtonToButton(11, GamePadButton.RightThumb);
			AddButtonToButton(6, GamePadButton.LeftShoulder);
			AddButtonToButton(7, GamePadButton.RightShoulder);
			AddAxisToAxis(0, GamePadAxis.LeftThumbX);
			AddAxisToAxis(1, GamePadAxis.LeftThumbY, true);
			AddAxisToAxis(3, GamePadAxis.RightThumbX);
			AddAxisToAxis(2, GamePadAxis.RightThumbY, true);
			AddButtonToAxis(4, GamePadAxis.LeftTrigger);
			AddButtonToAxis(5, GamePadAxis.RightTrigger);
			 
		}
	}

	
	/// <summary>
    /// Saitek Cyborg V.1 Game Pad 
    /// </summary>
	internal class Layout83 : GameControllerDatabaseLayout
	{
		public Layout83() : base(new Guid("00000003-06a3-0000-23f6-000011010000"), "Saitek Cyborg V.1 Game Pad")
		{
			AddButtonToButton(1, GamePadButton.A);
			AddButtonToButton(2, GamePadButton.B);
			AddButtonToButton(3, GamePadButton.Y);
			AddButtonToButton(0, GamePadButton.X);
			AddButtonToButton(9, GamePadButton.Start);
			AddButtonToButton(8, GamePadButton.Back);
			AddButtonToButton(10, GamePadButton.LeftThumb);
			AddButtonToButton(11, GamePadButton.RightThumb);
			AddButtonToButton(4, GamePadButton.LeftShoulder);
			AddButtonToButton(5, GamePadButton.RightShoulder);
			AddAxisToAxis(0, GamePadAxis.LeftThumbX);
			AddAxisToAxis(1, GamePadAxis.LeftThumbY, true);
			AddAxisToAxis(2, GamePadAxis.RightThumbX);
			AddAxisToAxis(4, GamePadAxis.RightThumbY, true);
			AddButtonToAxis(6, GamePadAxis.LeftTrigger);
			AddButtonToAxis(7, GamePadAxis.RightTrigger);
			 
		}
	}

	
	/// <summary>
    /// Thrustmaster 2 in 1 DT 
    /// </summary>
	internal class Layout84 : GameControllerDatabaseLayout
	{
		public Layout84() : base(new Guid("00000003-044f-0000-20b3-000010010000"), "Thrustmaster 2 in 1 DT")
		{
			AddButtonToButton(0, GamePadButton.A);
			AddButtonToButton(2, GamePadButton.B);
			AddButtonToButton(3, GamePadButton.Y);
			AddButtonToButton(1, GamePadButton.X);
			AddButtonToButton(9, GamePadButton.Start);
			AddButtonToButton(8, GamePadButton.Back);
			AddButtonToButton(10, GamePadButton.LeftThumb);
			AddButtonToButton(11, GamePadButton.RightThumb);
			AddButtonToButton(4, GamePadButton.LeftShoulder);
			AddButtonToButton(6, GamePadButton.RightShoulder);
			AddAxisToAxis(0, GamePadAxis.LeftThumbX);
			AddAxisToAxis(1, GamePadAxis.LeftThumbY, true);
			AddAxisToAxis(2, GamePadAxis.RightThumbX);
			AddAxisToAxis(3, GamePadAxis.RightThumbY, true);
			AddButtonToAxis(5, GamePadAxis.LeftTrigger);
			AddButtonToAxis(7, GamePadAxis.RightTrigger);
			 
		}
	}

	
	/// <summary>
    /// Thrustmaster Dual Trigger 3-in-1 
    /// </summary>
	internal class Layout85 : GameControllerDatabaseLayout
	{
		public Layout85() : base(new Guid("00000003-044f-0000-23b3-000000010000"), "Thrustmaster Dual Trigger 3-in-1")
		{
			AddButtonToButton(0, GamePadButton.X);
			AddButtonToButton(1, GamePadButton.A);
			AddButtonToButton(2, GamePadButton.B);
			AddButtonToButton(3, GamePadButton.Y);
			AddButtonToButton(8, GamePadButton.Back);
			AddButtonToButton(9, GamePadButton.Start);
			AddButtonToButton(4, GamePadButton.LeftShoulder);
			AddButtonToAxis(6, GamePadAxis.LeftTrigger);
			AddButtonToButton(5, GamePadButton.RightShoulder);
			AddButtonToAxis(7, GamePadAxis.RightTrigger);
			AddButtonToButton(10, GamePadButton.LeftThumb);
			AddButtonToButton(11, GamePadButton.RightThumb);
			AddAxisToAxis(0, GamePadAxis.LeftThumbX);
			AddAxisToAxis(1, GamePadAxis.LeftThumbY, true);
			AddAxisToAxis(2, GamePadAxis.RightThumbX);
			AddAxisToAxis(5, GamePadAxis.RightThumbY, true);
			 
		}
	}

	
	/// <summary>
    /// GreenAsia Inc.    USB Joystick      
    /// </summary>
	internal class Layout86 : GameControllerDatabaseLayout
	{
		public Layout86() : base(new Guid("00000003-0e8f-0000-0300-000010010000"), "GreenAsia Inc.    USB Joystick     ")
		{
			AddButtonToButton(3, GamePadButton.X);
			AddButtonToButton(2, GamePadButton.A);
			AddButtonToButton(1, GamePadButton.B);
			AddButtonToButton(0, GamePadButton.Y);
			AddButtonToButton(8, GamePadButton.Back);
			AddButtonToButton(9, GamePadButton.Start);
			AddButtonToButton(6, GamePadButton.LeftShoulder);
			AddButtonToAxis(4, GamePadAxis.LeftTrigger);
			AddButtonToButton(7, GamePadButton.RightShoulder);
			AddButtonToAxis(5, GamePadAxis.RightTrigger);
			AddButtonToButton(10, GamePadButton.LeftThumb);
			AddButtonToButton(11, GamePadButton.RightThumb);
			AddAxisToAxis(0, GamePadAxis.LeftThumbX);
			AddAxisToAxis(1, GamePadAxis.LeftThumbY, true);
			AddAxisToAxis(3, GamePadAxis.RightThumbX);
			AddAxisToAxis(2, GamePadAxis.RightThumbY, true);
			 
		}
	}

	
	/// <summary>
    /// GreenAsia Inc.      USB  Joystick   
    /// </summary>
	internal class Layout87 : GameControllerDatabaseLayout
	{
		public Layout87() : base(new Guid("00000003-0e8f-0000-1200-000010010000"), "GreenAsia Inc.      USB  Joystick  ")
		{
			AddButtonToButton(2, GamePadButton.X);
			AddButtonToButton(0, GamePadButton.A);
			AddButtonToButton(1, GamePadButton.B);
			AddButtonToButton(3, GamePadButton.Y);
			AddButtonToButton(8, GamePadButton.Back);
			AddButtonToButton(9, GamePadButton.Start);
			AddButtonToButton(4, GamePadButton.LeftShoulder);
			AddButtonToAxis(5, GamePadAxis.LeftTrigger);
			AddButtonToButton(6, GamePadButton.RightShoulder);
			AddButtonToAxis(7, GamePadAxis.RightTrigger);
			AddButtonToButton(10, GamePadButton.LeftThumb);
			AddButtonToButton(11, GamePadButton.RightThumb);
			AddAxisToAxis(0, GamePadAxis.LeftThumbX);
			AddAxisToAxis(1, GamePadAxis.LeftThumbY, true);
			AddAxisToAxis(3, GamePadAxis.RightThumbX);
			AddAxisToAxis(2, GamePadAxis.RightThumbY, true);
			 
		}
	}

	
	/// <summary>
    /// X360 Wireless Controller 
    /// </summary>
	internal class Layout88 : GameControllerDatabaseLayout
	{
		public Layout88() : base(new Guid("00000003-045e-0000-9102-000007010000"), "X360 Wireless Controller")
		{
			AddButtonToButton(0, GamePadButton.A);
			AddButtonToButton(1, GamePadButton.B);
			AddButtonToButton(3, GamePadButton.Y);
			AddButtonToButton(2, GamePadButton.X);
			AddButtonToButton(7, GamePadButton.Start);
			AddButtonToButton(6, GamePadButton.Back);
			AddButtonToButton(9, GamePadButton.LeftThumb);
			AddButtonToButton(10, GamePadButton.RightThumb);
			AddButtonToButton(4, GamePadButton.LeftShoulder);
			AddButtonToButton(5, GamePadButton.RightShoulder);
			AddButtonToButton(13, GamePadButton.PadUp);
			AddButtonToButton(11, GamePadButton.PadLeft);
			AddButtonToButton(14, GamePadButton.PadDown);
			AddButtonToButton(12, GamePadButton.PadRight);
			AddAxisToAxis(0, GamePadAxis.LeftThumbX);
			AddAxisToAxis(1, GamePadAxis.LeftThumbY, true);
			AddAxisToAxis(3, GamePadAxis.RightThumbX);
			AddAxisToAxis(4, GamePadAxis.RightThumbY, true);
			AddAxisToAxis(2, GamePadAxis.LeftTrigger, remap: true);
			AddAxisToAxis(5, GamePadAxis.RightTrigger, remap: true);
			 
		}
	}

	
	/// <summary>
    /// Logitech Logitech Dual Action 
    /// </summary>
	internal class Layout89 : GameControllerDatabaseLayout
	{
		public Layout89() : base(new Guid("00000003-046d-0000-16c2-000010010000"), "Logitech Logitech Dual Action")
		{
			AddButtonToButton(0, GamePadButton.X);
			AddButtonToButton(1, GamePadButton.A);
			AddButtonToButton(2, GamePadButton.B);
			AddButtonToButton(3, GamePadButton.Y);
			AddButtonToButton(8, GamePadButton.Back);
			AddButtonToButton(9, GamePadButton.Start);
			AddButtonToButton(4, GamePadButton.LeftShoulder);
			AddButtonToAxis(6, GamePadAxis.LeftTrigger);
			AddButtonToButton(5, GamePadButton.RightShoulder);
			AddButtonToAxis(7, GamePadAxis.RightTrigger);
			AddButtonToButton(10, GamePadButton.LeftThumb);
			AddButtonToButton(11, GamePadButton.RightThumb);
			AddAxisToAxis(0, GamePadAxis.LeftThumbX);
			AddAxisToAxis(1, GamePadAxis.LeftThumbY, true);
			AddAxisToAxis(2, GamePadAxis.RightThumbX);
			AddAxisToAxis(3, GamePadAxis.RightThumbY, true);
			 
		}
	}

	
	/// <summary>
    /// GameCube {WiseGroup USB box} 
    /// </summary>
	internal class Layout90 : GameControllerDatabaseLayout
	{
		public Layout90() : base(new Guid("00000003-0926-0000-8888-000000010000"), "GameCube {WiseGroup USB box}")
		{
			AddButtonToButton(0, GamePadButton.A);
			AddButtonToButton(2, GamePadButton.B);
			AddButtonToButton(3, GamePadButton.Y);
			AddButtonToButton(1, GamePadButton.X);
			AddButtonToButton(7, GamePadButton.Start);
			AddButtonToButton(6, GamePadButton.RightShoulder);
			AddAxisToAxis(0, GamePadAxis.LeftThumbX);
			AddAxisToAxis(1, GamePadAxis.LeftThumbY, true);
			AddAxisToAxis(2, GamePadAxis.RightThumbX);
			AddAxisToAxis(3, GamePadAxis.RightThumbY, true);
			AddAxisToAxis(4, GamePadAxis.LeftTrigger, remap: true);
			AddAxisToAxis(5, GamePadAxis.RightTrigger, remap: true);
			 
		}
	}

	
	/// <summary>
    /// Logitech WingMan Cordless RumblePad 
    /// </summary>
	internal class Layout91 : GameControllerDatabaseLayout
	{
		public Layout91() : base(new Guid("00000003-046d-0000-11c2-000010010000"), "Logitech WingMan Cordless RumblePad")
		{
			AddButtonToButton(0, GamePadButton.A);
			AddButtonToButton(1, GamePadButton.B);
			AddButtonToButton(4, GamePadButton.Y);
			AddButtonToButton(3, GamePadButton.X);
			AddButtonToButton(8, GamePadButton.Start);
			AddButtonToButton(2, GamePadButton.Back);
			AddButtonToButton(6, GamePadButton.LeftShoulder);
			AddButtonToButton(7, GamePadButton.RightShoulder);
			AddAxisToAxis(0, GamePadAxis.LeftThumbX);
			AddAxisToAxis(1, GamePadAxis.LeftThumbY, true);
			AddAxisToAxis(3, GamePadAxis.RightThumbX);
			AddAxisToAxis(4, GamePadAxis.RightThumbY, true);
			AddButtonToAxis(9, GamePadAxis.LeftTrigger);
			AddButtonToAxis(10, GamePadAxis.RightTrigger);
			 
		}
	}

	
	/// <summary>
    /// Logitech Logitech RumblePad 2 USB 
    /// </summary>
	internal class Layout92 : GameControllerDatabaseLayout
	{
		public Layout92() : base(new Guid("00000003-046d-0000-18c2-000010010000"), "Logitech Logitech RumblePad 2 USB")
		{
			AddButtonToButton(0, GamePadButton.X);
			AddButtonToButton(1, GamePadButton.A);
			AddButtonToButton(2, GamePadButton.B);
			AddButtonToButton(3, GamePadButton.Y);
			AddButtonToButton(8, GamePadButton.Back);
			AddButtonToButton(9, GamePadButton.Start);
			AddButtonToButton(4, GamePadButton.LeftShoulder);
			AddButtonToAxis(6, GamePadAxis.LeftTrigger);
			AddButtonToButton(5, GamePadButton.RightShoulder);
			AddButtonToAxis(7, GamePadAxis.RightTrigger);
			AddButtonToButton(10, GamePadButton.LeftThumb);
			AddButtonToButton(11, GamePadButton.RightThumb);
			AddAxisToAxis(0, GamePadAxis.LeftThumbX);
			AddAxisToAxis(1, GamePadAxis.LeftThumbY, true);
			AddAxisToAxis(2, GamePadAxis.RightThumbX);
			AddAxisToAxis(3, GamePadAxis.RightThumbY, true);
			 
		}
	}

	
	/// <summary>
    /// Moga Pro 
    /// </summary>
	internal class Layout93 : GameControllerDatabaseLayout
	{
		public Layout93() : base(new Guid("00000005-20d6-0000-ad0d-000001000000"), "Moga Pro")
		{
			AddButtonToButton(0, GamePadButton.A);
			AddButtonToButton(1, GamePadButton.B);
			AddButtonToButton(3, GamePadButton.Y);
			AddButtonToButton(2, GamePadButton.X);
			AddButtonToButton(6, GamePadButton.Start);
			AddButtonToButton(7, GamePadButton.LeftThumb);
			AddButtonToButton(8, GamePadButton.RightThumb);
			AddButtonToButton(4, GamePadButton.LeftShoulder);
			AddButtonToButton(5, GamePadButton.RightShoulder);
			AddAxisToAxis(0, GamePadAxis.LeftThumbX);
			AddAxisToAxis(1, GamePadAxis.LeftThumbY, true);
			AddAxisToAxis(2, GamePadAxis.RightThumbX);
			AddAxisToAxis(3, GamePadAxis.RightThumbY, true);
			AddAxisToAxis(5, GamePadAxis.LeftTrigger, remap: true);
			AddAxisToAxis(4, GamePadAxis.RightTrigger, remap: true);
			 
		}
	}

	
	/// <summary>
    /// Thrustmaster Run N Drive Wireless PS3 
    /// </summary>
	internal class Layout94 : GameControllerDatabaseLayout
	{
		public Layout94() : base(new Guid("00000003-044f-0000-09d0-000000010000"), "Thrustmaster Run N Drive Wireless PS3")
		{
			AddButtonToButton(1, GamePadButton.A);
			AddButtonToButton(2, GamePadButton.B);
			AddButtonToButton(0, GamePadButton.X);
			AddButtonToButton(3, GamePadButton.Y);
			AddButtonToButton(9, GamePadButton.Start);
			AddButtonToButton(8, GamePadButton.Back);
			AddButtonToButton(10, GamePadButton.LeftThumb);
			AddButtonToButton(11, GamePadButton.RightThumb);
			AddButtonToButton(4, GamePadButton.LeftShoulder);
			AddButtonToButton(5, GamePadButton.RightShoulder);
			AddAxisToAxis(0, GamePadAxis.LeftThumbX);
			AddAxisToAxis(1, GamePadAxis.LeftThumbY, true);
			AddAxisToAxis(2, GamePadAxis.RightThumbX);
			AddAxisToAxis(3, GamePadAxis.RightThumbY, true);
			AddButtonToAxis(6, GamePadAxis.LeftTrigger);
			AddButtonToAxis(7, GamePadAxis.RightTrigger);
			 
		}
	}

	
	/// <summary>
    /// Thrustmaster Run N Drive  Wireless 
    /// </summary>
	internal class Layout95 : GameControllerDatabaseLayout
	{
		public Layout95() : base(new Guid("00000003-044f-0000-08d0-000000010000"), "Thrustmaster Run N Drive  Wireless")
		{
			AddButtonToButton(1, GamePadButton.A);
			AddButtonToButton(2, GamePadButton.B);
			AddButtonToButton(0, GamePadButton.X);
			AddButtonToButton(3, GamePadButton.Y);
			AddButtonToButton(9, GamePadButton.Start);
			AddButtonToButton(8, GamePadButton.Back);
			AddButtonToButton(10, GamePadButton.LeftThumb);
			AddButtonToButton(11, GamePadButton.RightThumb);
			AddButtonToButton(4, GamePadButton.LeftShoulder);
			AddButtonToButton(5, GamePadButton.RightShoulder);
			AddAxisToAxis(0, GamePadAxis.LeftThumbX);
			AddAxisToAxis(1, GamePadAxis.LeftThumbY, true);
			AddAxisToAxis(2, GamePadAxis.RightThumbX);
			AddAxisToAxis(5, GamePadAxis.RightThumbY, true);
			AddButtonToAxis(6, GamePadAxis.LeftTrigger);
			AddButtonToAxis(7, GamePadAxis.RightTrigger);
			 
		}
	}

	
	/// <summary>
    /// RetroUSB.com RetroPad 
    /// </summary>
	internal class Layout96 : GameControllerDatabaseLayout
	{
		public Layout96() : base(new Guid("00000003-f000-0000-0300-000000010000"), "RetroUSB.com RetroPad")
		{
			AddButtonToButton(1, GamePadButton.A);
			AddButtonToButton(5, GamePadButton.B);
			AddButtonToButton(0, GamePadButton.X);
			AddButtonToButton(4, GamePadButton.Y);
			AddButtonToButton(2, GamePadButton.Back);
			AddButtonToButton(3, GamePadButton.Start);
			AddButtonToButton(6, GamePadButton.LeftShoulder);
			AddButtonToButton(7, GamePadButton.RightShoulder);
			AddAxisToAxis(0, GamePadAxis.LeftThumbX);
			AddAxisToAxis(1, GamePadAxis.LeftThumbY, true);
			 
		}
	}

	
	/// <summary>
    /// RetroUSB.com Super RetroPort 
    /// </summary>
	internal class Layout97 : GameControllerDatabaseLayout
	{
		public Layout97() : base(new Guid("00000003-f000-0000-f100-000000010000"), "RetroUSB.com Super RetroPort")
		{
			AddButtonToButton(1, GamePadButton.A);
			AddButtonToButton(5, GamePadButton.B);
			AddButtonToButton(0, GamePadButton.X);
			AddButtonToButton(4, GamePadButton.Y);
			AddButtonToButton(2, GamePadButton.Back);
			AddButtonToButton(3, GamePadButton.Start);
			AddButtonToButton(6, GamePadButton.LeftShoulder);
			AddButtonToButton(7, GamePadButton.RightShoulder);
			AddAxisToAxis(0, GamePadAxis.LeftThumbX);
			AddAxisToAxis(1, GamePadAxis.LeftThumbY, true);
			 
		}
	}

	
	/// <summary>
    /// Generic X-Box pad 
    /// </summary>
	internal class Layout98 : GameControllerDatabaseLayout
	{
		public Layout98() : base(new Guid("00000003-0e6f-0000-1f01-000000010000"), "Generic X-Box pad")
		{
			AddButtonToButton(2, GamePadButton.X);
			AddButtonToButton(0, GamePadButton.A);
			AddButtonToButton(1, GamePadButton.B);
			AddButtonToButton(3, GamePadButton.Y);
			AddButtonToButton(6, GamePadButton.Back);
			AddButtonToButton(7, GamePadButton.Start);
			AddButtonToButton(4, GamePadButton.LeftShoulder);
			AddAxisToAxis(2, GamePadAxis.LeftTrigger, remap: true);
			AddButtonToButton(5, GamePadButton.RightShoulder);
			AddAxisToAxis(5, GamePadAxis.RightTrigger, remap: true);
			AddButtonToButton(9, GamePadButton.LeftThumb);
			AddButtonToButton(10, GamePadButton.RightThumb);
			AddAxisToAxis(0, GamePadAxis.LeftThumbX);
			AddAxisToAxis(1, GamePadAxis.LeftThumbY, true);
			AddAxisToAxis(3, GamePadAxis.RightThumbX);
			AddAxisToAxis(4, GamePadAxis.RightThumbY, true);
			 
		}
	}

	
	/// <summary>
    /// Gravis GamePad Pro USB  
    /// </summary>
	internal class Layout99 : GameControllerDatabaseLayout
	{
		public Layout99() : base(new Guid("00000003-0428-0000-0140-000000010000"), "Gravis GamePad Pro USB ")
		{
			AddButtonToButton(0, GamePadButton.X);
			AddButtonToButton(1, GamePadButton.A);
			AddButtonToButton(2, GamePadButton.B);
			AddButtonToButton(3, GamePadButton.Y);
			AddButtonToButton(8, GamePadButton.Back);
			AddButtonToButton(9, GamePadButton.Start);
			AddButtonToButton(4, GamePadButton.LeftShoulder);
			AddButtonToAxis(6, GamePadAxis.LeftTrigger);
			AddButtonToButton(5, GamePadButton.RightShoulder);
			AddButtonToAxis(7, GamePadAxis.RightTrigger);
			AddAxisToAxis(0, GamePadAxis.LeftThumbX);
			AddAxisToAxis(1, GamePadAxis.LeftThumbY, true);
			 
		}
	}

	
	/// <summary>
    /// Microsoft X-Box pad v2 (US) 
    /// </summary>
	internal class Layout100 : GameControllerDatabaseLayout
	{
		public Layout100() : base(new Guid("00000003-045e-0000-8902-000021010000"), "Microsoft X-Box pad v2 (US)")
		{
			AddButtonToButton(3, GamePadButton.X);
			AddButtonToButton(0, GamePadButton.A);
			AddButtonToButton(1, GamePadButton.B);
			AddButtonToButton(4, GamePadButton.Y);
			AddButtonToButton(6, GamePadButton.Back);
			AddButtonToButton(7, GamePadButton.Start);
			AddButtonToButton(5, GamePadButton.LeftShoulder);
			AddAxisToAxis(2, GamePadAxis.LeftTrigger, remap: true);
			AddButtonToButton(2, GamePadButton.RightShoulder);
			AddAxisToAxis(5, GamePadAxis.RightTrigger, remap: true);
			AddButtonToButton(8, GamePadButton.LeftThumb);
			AddButtonToButton(9, GamePadButton.RightThumb);
			AddAxisToAxis(0, GamePadAxis.LeftThumbX);
			AddAxisToAxis(1, GamePadAxis.LeftThumbY, true);
			AddAxisToAxis(3, GamePadAxis.RightThumbX);
			AddAxisToAxis(4, GamePadAxis.RightThumbY, true);
			 
		}
	}

	
	/// <summary>
    /// Microsoft X-Box pad (Japan) 
    /// </summary>
	internal class Layout101 : GameControllerDatabaseLayout
	{
		public Layout101() : base(new Guid("00000003-045e-0000-8502-000000010000"), "Microsoft X-Box pad (Japan)")
		{
			AddButtonToButton(3, GamePadButton.X);
			AddButtonToButton(0, GamePadButton.A);
			AddButtonToButton(1, GamePadButton.B);
			AddButtonToButton(4, GamePadButton.Y);
			AddButtonToButton(6, GamePadButton.Back);
			AddButtonToButton(7, GamePadButton.Start);
			AddButtonToButton(5, GamePadButton.LeftShoulder);
			AddAxisToAxis(2, GamePadAxis.LeftTrigger, remap: true);
			AddButtonToButton(2, GamePadButton.RightShoulder);
			AddAxisToAxis(5, GamePadAxis.RightTrigger, remap: true);
			AddButtonToButton(8, GamePadButton.LeftThumb);
			AddButtonToButton(9, GamePadButton.RightThumb);
			AddAxisToAxis(0, GamePadAxis.LeftThumbX);
			AddAxisToAxis(1, GamePadAxis.LeftThumbY, true);
			AddAxisToAxis(3, GamePadAxis.RightThumbX);
			AddAxisToAxis(4, GamePadAxis.RightThumbY, true);
			 
		}
	}

	
	/// <summary>
    /// Rock Candy Gamepad for PS3 
    /// </summary>
	internal class Layout102 : GameControllerDatabaseLayout
	{
		public Layout102() : base(new Guid("00000003-0e6f-0000-1e01-000011010000"), "Rock Candy Gamepad for PS3")
		{
			AddButtonToButton(1, GamePadButton.A);
			AddButtonToButton(2, GamePadButton.B);
			AddButtonToButton(0, GamePadButton.X);
			AddButtonToButton(3, GamePadButton.Y);
			AddButtonToButton(8, GamePadButton.Back);
			AddButtonToButton(9, GamePadButton.Start);
			AddButtonToButton(4, GamePadButton.LeftShoulder);
			AddButtonToButton(5, GamePadButton.RightShoulder);
			AddButtonToButton(10, GamePadButton.LeftThumb);
			AddButtonToButton(11, GamePadButton.RightThumb);
			AddAxisToAxis(0, GamePadAxis.LeftThumbX);
			AddAxisToAxis(1, GamePadAxis.LeftThumbY, true);
			AddAxisToAxis(2, GamePadAxis.RightThumbX);
			AddAxisToAxis(3, GamePadAxis.RightThumbY, true);
			AddButtonToAxis(6, GamePadAxis.LeftTrigger);
			AddButtonToAxis(7, GamePadAxis.RightTrigger);
			 
		}
	}

	
	/// <summary>
    /// Sony PS2 pad with SmartJoy adapter 
    /// </summary>
	internal class Layout103 : GameControllerDatabaseLayout
	{
		public Layout103() : base(new Guid("00000003-0925-0000-0500-000000010000"), "Sony PS2 pad with SmartJoy adapter")
		{
			AddButtonToButton(2, GamePadButton.A);
			AddButtonToButton(1, GamePadButton.B);
			AddButtonToButton(0, GamePadButton.Y);
			AddButtonToButton(3, GamePadButton.X);
			AddButtonToButton(8, GamePadButton.Start);
			AddButtonToButton(9, GamePadButton.Back);
			AddButtonToButton(10, GamePadButton.LeftThumb);
			AddButtonToButton(11, GamePadButton.RightThumb);
			AddButtonToButton(6, GamePadButton.LeftShoulder);
			AddButtonToButton(7, GamePadButton.RightShoulder);
			AddAxisToAxis(0, GamePadAxis.LeftThumbX);
			AddAxisToAxis(1, GamePadAxis.LeftThumbY, true);
			AddAxisToAxis(2, GamePadAxis.RightThumbX);
			AddAxisToAxis(3, GamePadAxis.RightThumbY, true);
			AddButtonToAxis(4, GamePadAxis.LeftTrigger);
			AddButtonToAxis(5, GamePadAxis.RightTrigger);
			 
		}
	}

	
	/// <summary>
    /// Razer Onza Tournament 
    /// </summary>
	internal class Layout104 : GameControllerDatabaseLayout
	{
		public Layout104() : base(new Guid("00000003-1689-0000-00fd-000024010000"), "Razer Onza Tournament")
		{
			AddButtonToButton(0, GamePadButton.A);
			AddButtonToButton(1, GamePadButton.B);
			AddButtonToButton(3, GamePadButton.Y);
			AddButtonToButton(2, GamePadButton.X);
			AddButtonToButton(7, GamePadButton.Start);
			AddButtonToButton(6, GamePadButton.Back);
			AddButtonToButton(9, GamePadButton.LeftThumb);
			AddButtonToButton(10, GamePadButton.RightThumb);
			AddButtonToButton(4, GamePadButton.LeftShoulder);
			AddButtonToButton(5, GamePadButton.RightShoulder);
			AddButtonToButton(13, GamePadButton.PadUp);
			AddButtonToButton(11, GamePadButton.PadLeft);
			AddButtonToButton(14, GamePadButton.PadDown);
			AddButtonToButton(12, GamePadButton.PadRight);
			AddAxisToAxis(0, GamePadAxis.LeftThumbX);
			AddAxisToAxis(1, GamePadAxis.LeftThumbY, true);
			AddAxisToAxis(3, GamePadAxis.RightThumbX);
			AddAxisToAxis(4, GamePadAxis.RightThumbY, true);
			AddAxisToAxis(2, GamePadAxis.LeftTrigger, remap: true);
			AddAxisToAxis(5, GamePadAxis.RightTrigger, remap: true);
			 
		}
	}

	
	/// <summary>
    /// Thrustmaster Firestorm Dual Power 
    /// </summary>
	internal class Layout105 : GameControllerDatabaseLayout
	{
		public Layout105() : base(new Guid("00000003-044f-0000-00b3-000010010000"), "Thrustmaster Firestorm Dual Power")
		{
			AddButtonToButton(0, GamePadButton.A);
			AddButtonToButton(2, GamePadButton.B);
			AddButtonToButton(3, GamePadButton.Y);
			AddButtonToButton(1, GamePadButton.X);
			AddButtonToButton(10, GamePadButton.Start);
			AddButtonToButton(9, GamePadButton.Back);
			AddButtonToButton(11, GamePadButton.LeftThumb);
			AddButtonToButton(12, GamePadButton.RightThumb);
			AddButtonToButton(4, GamePadButton.LeftShoulder);
			AddButtonToButton(6, GamePadButton.RightShoulder);
			AddAxisToAxis(0, GamePadAxis.LeftThumbX);
			AddAxisToAxis(1, GamePadAxis.LeftThumbY, true);
			AddAxisToAxis(2, GamePadAxis.RightThumbX);
			AddAxisToAxis(3, GamePadAxis.RightThumbY, true);
			AddButtonToAxis(5, GamePadAxis.LeftTrigger);
			AddButtonToAxis(7, GamePadAxis.RightTrigger);
			 
		}
	}

	
	/// <summary>
    /// Hori Pad EX Turbo 2 
    /// </summary>
	internal class Layout106 : GameControllerDatabaseLayout
	{
		public Layout106() : base(new Guid("00000003-1bad-0000-01f5-000033050000"), "Hori Pad EX Turbo 2")
		{
			AddButtonToButton(0, GamePadButton.A);
			AddButtonToButton(1, GamePadButton.B);
			AddButtonToButton(3, GamePadButton.Y);
			AddButtonToButton(2, GamePadButton.X);
			AddButtonToButton(7, GamePadButton.Start);
			AddButtonToButton(6, GamePadButton.Back);
			AddButtonToButton(9, GamePadButton.LeftThumb);
			AddButtonToButton(10, GamePadButton.RightThumb);
			AddButtonToButton(4, GamePadButton.LeftShoulder);
			AddButtonToButton(5, GamePadButton.RightShoulder);
			AddAxisToAxis(0, GamePadAxis.LeftThumbX);
			AddAxisToAxis(1, GamePadAxis.LeftThumbY, true);
			AddAxisToAxis(3, GamePadAxis.RightThumbX);
			AddAxisToAxis(4, GamePadAxis.RightThumbY, true);
			AddAxisToAxis(2, GamePadAxis.LeftTrigger, remap: true);
			AddAxisToAxis(5, GamePadAxis.RightTrigger, remap: true);
			 
		}
	}

	
	/// <summary>
    /// PS4 Controller (Bluetooth) 
    /// </summary>
	internal class Layout107 : GameControllerDatabaseLayout
	{
		public Layout107() : base(new Guid("00000005-054c-0000-c405-000000010000"), "PS4 Controller (Bluetooth)")
		{
			AddButtonToButton(1, GamePadButton.A);
			AddButtonToButton(2, GamePadButton.B);
			AddButtonToButton(8, GamePadButton.Back);
			AddButtonToButton(4, GamePadButton.LeftShoulder);
			AddButtonToButton(10, GamePadButton.LeftThumb);
			AddAxisToAxis(3, GamePadAxis.LeftTrigger, remap: true);
			AddAxisToAxis(0, GamePadAxis.LeftThumbX);
			AddAxisToAxis(1, GamePadAxis.LeftThumbY, true);
			AddButtonToButton(5, GamePadButton.RightShoulder);
			AddButtonToButton(11, GamePadButton.RightThumb);
			AddAxisToAxis(4, GamePadAxis.RightTrigger, remap: true);
			AddAxisToAxis(2, GamePadAxis.RightThumbX);
			AddAxisToAxis(5, GamePadAxis.RightThumbY, true);
			AddButtonToButton(9, GamePadButton.Start);
			AddButtonToButton(0, GamePadButton.X);
			AddButtonToButton(3, GamePadButton.Y);
			 
		}
	}

	
	/// <summary>
    /// PS3 Controller (Bluetooth) 
    /// </summary>
	internal class Layout108 : GameControllerDatabaseLayout
	{
		public Layout108() : base(new Guid("00000006-054c-0000-6802-000000010000"), "PS3 Controller (Bluetooth)")
		{
			AddButtonToButton(14, GamePadButton.A);
			AddButtonToButton(13, GamePadButton.B);
			AddButtonToButton(12, GamePadButton.Y);
			AddButtonToButton(15, GamePadButton.X);
			AddButtonToButton(3, GamePadButton.Start);
			AddButtonToButton(0, GamePadButton.Back);
			AddButtonToButton(1, GamePadButton.LeftThumb);
			AddButtonToButton(2, GamePadButton.RightThumb);
			AddButtonToButton(10, GamePadButton.LeftShoulder);
			AddButtonToButton(11, GamePadButton.RightShoulder);
			AddButtonToButton(4, GamePadButton.PadUp);
			AddButtonToButton(7, GamePadButton.PadLeft);
			AddButtonToButton(6, GamePadButton.PadDown);
			AddButtonToButton(5, GamePadButton.PadRight);
			AddAxisToAxis(0, GamePadAxis.LeftThumbX);
			AddAxisToAxis(1, GamePadAxis.LeftThumbY, true);
			AddAxisToAxis(2, GamePadAxis.RightThumbX);
			AddAxisToAxis(3, GamePadAxis.RightThumbY, true);
			AddButtonToAxis(8, GamePadAxis.LeftTrigger);
			AddButtonToAxis(9, GamePadAxis.RightTrigger);
			 
		}
	}

	
	/// <summary>
    /// PS3 Controller (Bluetooth) 
    /// </summary>
	internal class Layout109 : GameControllerDatabaseLayout
	{
		public Layout109() : base(new Guid("00000005-054c-0000-6802-000000010000"), "PS3 Controller (Bluetooth)")
		{
			AddButtonToButton(14, GamePadButton.A);
			AddButtonToButton(13, GamePadButton.B);
			AddButtonToButton(12, GamePadButton.Y);
			AddButtonToButton(15, GamePadButton.X);
			AddButtonToButton(3, GamePadButton.Start);
			AddButtonToButton(0, GamePadButton.Back);
			AddButtonToButton(1, GamePadButton.LeftThumb);
			AddButtonToButton(2, GamePadButton.RightThumb);
			AddButtonToButton(10, GamePadButton.LeftShoulder);
			AddButtonToButton(11, GamePadButton.RightShoulder);
			AddButtonToButton(4, GamePadButton.PadUp);
			AddButtonToButton(7, GamePadButton.PadLeft);
			AddButtonToButton(6, GamePadButton.PadDown);
			AddButtonToButton(5, GamePadButton.PadRight);
			AddAxisToAxis(0, GamePadAxis.LeftThumbX);
			AddAxisToAxis(1, GamePadAxis.LeftThumbY, true);
			AddAxisToAxis(2, GamePadAxis.RightThumbX);
			AddAxisToAxis(3, GamePadAxis.RightThumbY, true);
			AddButtonToAxis(8, GamePadAxis.LeftTrigger);
			AddButtonToAxis(9, GamePadAxis.RightTrigger);
			 
		}
	}

	
	/// <summary>
    /// DragonRise Inc.   Generic   USB  Joystick   
    /// </summary>
	internal class Layout110 : GameControllerDatabaseLayout
	{
		public Layout110() : base(new Guid("00000003-0079-0000-0600-000010010000"), "DragonRise Inc.   Generic   USB  Joystick  ")
		{
			AddButtonToButton(3, GamePadButton.X);
			AddButtonToButton(2, GamePadButton.A);
			AddButtonToButton(1, GamePadButton.B);
			AddButtonToButton(0, GamePadButton.Y);
			AddButtonToButton(8, GamePadButton.Back);
			AddButtonToButton(9, GamePadButton.Start);
			AddButtonToButton(4, GamePadButton.LeftShoulder);
			AddButtonToAxis(6, GamePadAxis.LeftTrigger);
			AddButtonToButton(5, GamePadButton.RightShoulder);
			AddButtonToAxis(7, GamePadAxis.RightTrigger);
			AddButtonToButton(10, GamePadButton.LeftThumb);
			AddButtonToButton(11, GamePadButton.RightThumb);
			AddAxisToAxis(0, GamePadAxis.LeftThumbX);
			AddAxisToAxis(1, GamePadAxis.LeftThumbY, true);
			AddAxisToAxis(3, GamePadAxis.RightThumbX);
			AddAxisToAxis(4, GamePadAxis.RightThumbY, true);
			 
		}
	}

	
	/// <summary>
    /// Super Joy Box 5 Pro 
    /// </summary>
	internal class Layout111 : GameControllerDatabaseLayout
	{
		public Layout111() : base(new Guid("00000003-6666-0000-0488-000000010000"), "Super Joy Box 5 Pro")
		{
			AddButtonToButton(2, GamePadButton.A);
			AddButtonToButton(1, GamePadButton.B);
			AddButtonToButton(3, GamePadButton.X);
			AddButtonToButton(0, GamePadButton.Y);
			AddButtonToButton(9, GamePadButton.Back);
			AddButtonToButton(8, GamePadButton.Start);
			AddButtonToButton(6, GamePadButton.LeftShoulder);
			AddButtonToButton(7, GamePadButton.RightShoulder);
			AddButtonToButton(10, GamePadButton.LeftThumb);
			AddButtonToButton(11, GamePadButton.RightThumb);
			AddAxisToAxis(0, GamePadAxis.LeftThumbX);
			AddAxisToAxis(1, GamePadAxis.LeftThumbY, true);
			AddAxisToAxis(2, GamePadAxis.RightThumbX);
			AddAxisToAxis(3, GamePadAxis.RightThumbY, true);
			AddButtonToAxis(4, GamePadAxis.LeftTrigger);
			AddButtonToAxis(5, GamePadAxis.RightTrigger);
			AddButtonToButton(12, GamePadButton.PadUp);
			AddButtonToButton(15, GamePadButton.PadLeft);
			AddButtonToButton(14, GamePadButton.PadDown);
			AddButtonToButton(13, GamePadButton.PadRight);
			 
		}
	}

	
	/// <summary>
    /// OUYA Game Controller 
    /// </summary>
	internal class Layout112 : GameControllerDatabaseLayout
	{
		public Layout112() : base(new Guid("00000005-2836-0000-0100-000002010000"), "OUYA Game Controller")
		{
			AddButtonToButton(0, GamePadButton.A);
			AddButtonToButton(3, GamePadButton.B);
			AddButtonToButton(9, GamePadButton.PadDown);
			AddButtonToButton(10, GamePadButton.PadLeft);
			AddButtonToButton(11, GamePadButton.PadRight);
			AddButtonToButton(8, GamePadButton.PadUp);
			AddButtonToButton(4, GamePadButton.LeftShoulder);
			AddButtonToButton(6, GamePadButton.LeftThumb);
			AddAxisToAxis(2, GamePadAxis.LeftTrigger, remap: true);
			AddAxisToAxis(0, GamePadAxis.LeftThumbX);
			AddAxisToAxis(1, GamePadAxis.LeftThumbY, true);
			AddButtonToButton(5, GamePadButton.RightShoulder);
			AddButtonToButton(7, GamePadButton.RightThumb);
			AddAxisToAxis(5, GamePadAxis.RightTrigger, remap: true);
			AddAxisToAxis(3, GamePadAxis.RightThumbX);
			AddAxisToAxis(4, GamePadAxis.RightThumbY, true);
			AddButtonToButton(1, GamePadButton.X);
			AddButtonToButton(2, GamePadButton.Y);
			 
		}
	}

	
	/// <summary>
    /// OUYA Game Controller 
    /// </summary>
	internal class Layout113 : GameControllerDatabaseLayout
	{
		public Layout113() : base(new Guid("00000005-2836-0000-0100-000003010000"), "OUYA Game Controller")
		{
			AddButtonToButton(0, GamePadButton.A);
			AddButtonToButton(3, GamePadButton.B);
			AddButtonToButton(9, GamePadButton.PadDown);
			AddButtonToButton(10, GamePadButton.PadLeft);
			AddButtonToButton(11, GamePadButton.PadRight);
			AddButtonToButton(8, GamePadButton.PadUp);
			AddButtonToButton(4, GamePadButton.LeftShoulder);
			AddButtonToButton(6, GamePadButton.LeftThumb);
			AddAxisToAxis(2, GamePadAxis.LeftTrigger, remap: true);
			AddAxisToAxis(0, GamePadAxis.LeftThumbX);
			AddAxisToAxis(1, GamePadAxis.LeftThumbY, true);
			AddButtonToButton(5, GamePadButton.RightShoulder);
			AddButtonToButton(7, GamePadButton.RightThumb);
			AddAxisToAxis(5, GamePadAxis.RightTrigger, remap: true);
			AddAxisToAxis(3, GamePadAxis.RightThumbX);
			AddAxisToAxis(4, GamePadAxis.RightThumbY, true);
			AddButtonToButton(1, GamePadButton.X);
			AddButtonToButton(2, GamePadButton.Y);
			 
		}
	}

	
	/// <summary>
    /// Razer Onza Classic Edition 
    /// </summary>
	internal class Layout114 : GameControllerDatabaseLayout
	{
		public Layout114() : base(new Guid("00000003-1689-0000-01fd-000024010000"), "Razer Onza Classic Edition")
		{
			AddButtonToButton(2, GamePadButton.X);
			AddButtonToButton(0, GamePadButton.A);
			AddButtonToButton(1, GamePadButton.B);
			AddButtonToButton(3, GamePadButton.Y);
			AddButtonToButton(6, GamePadButton.Back);
			AddButtonToButton(7, GamePadButton.Start);
			AddButtonToButton(11, GamePadButton.PadLeft);
			AddButtonToButton(14, GamePadButton.PadDown);
			AddButtonToButton(12, GamePadButton.PadRight);
			AddButtonToButton(13, GamePadButton.PadUp);
			AddButtonToButton(4, GamePadButton.LeftShoulder);
			AddAxisToAxis(2, GamePadAxis.LeftTrigger, remap: true);
			AddButtonToButton(5, GamePadButton.RightShoulder);
			AddAxisToAxis(5, GamePadAxis.RightTrigger, remap: true);
			AddButtonToButton(9, GamePadButton.LeftThumb);
			AddButtonToButton(10, GamePadButton.RightThumb);
			AddAxisToAxis(0, GamePadAxis.LeftThumbX);
			AddAxisToAxis(1, GamePadAxis.LeftThumbY, true);
			AddAxisToAxis(3, GamePadAxis.RightThumbX);
			AddAxisToAxis(4, GamePadAxis.RightThumbY, true);
			 
		}
	}

	
	/// <summary>
    /// Microsoft X-Box One pad 
    /// </summary>
	internal class Layout115 : GameControllerDatabaseLayout
	{
		public Layout115() : base(new Guid("00000003-045e-0000-d102-000001010000"), "Microsoft X-Box One pad")
		{
			AddButtonToButton(2, GamePadButton.X);
			AddButtonToButton(0, GamePadButton.A);
			AddButtonToButton(1, GamePadButton.B);
			AddButtonToButton(3, GamePadButton.Y);
			AddButtonToButton(6, GamePadButton.Back);
			AddButtonToButton(7, GamePadButton.Start);
			AddButtonToButton(4, GamePadButton.LeftShoulder);
			AddAxisToAxis(2, GamePadAxis.LeftTrigger, remap: true);
			AddButtonToButton(5, GamePadButton.RightShoulder);
			AddAxisToAxis(5, GamePadAxis.RightTrigger, remap: true);
			AddButtonToButton(9, GamePadButton.LeftThumb);
			AddButtonToButton(10, GamePadButton.RightThumb);
			AddAxisToAxis(0, GamePadAxis.LeftThumbX);
			AddAxisToAxis(1, GamePadAxis.LeftThumbY, true);
			AddAxisToAxis(3, GamePadAxis.RightThumbX);
			AddAxisToAxis(4, GamePadAxis.RightThumbY, true);
			 
		}
	}

	
	/// <summary>
    /// Microsoft X-Box One pad v2 
    /// </summary>
	internal class Layout116 : GameControllerDatabaseLayout
	{
		public Layout116() : base(new Guid("00000003-045e-0000-dd02-000003020000"), "Microsoft X-Box One pad v2")
		{
			AddButtonToButton(2, GamePadButton.X);
			AddButtonToButton(0, GamePadButton.A);
			AddButtonToButton(1, GamePadButton.B);
			AddButtonToButton(3, GamePadButton.Y);
			AddButtonToButton(6, GamePadButton.Back);
			AddButtonToButton(7, GamePadButton.Start);
			AddButtonToButton(4, GamePadButton.LeftShoulder);
			AddAxisToAxis(2, GamePadAxis.LeftTrigger, remap: true);
			AddButtonToButton(5, GamePadButton.RightShoulder);
			AddAxisToAxis(5, GamePadAxis.RightTrigger, remap: true);
			AddButtonToButton(9, GamePadButton.LeftThumb);
			AddButtonToButton(10, GamePadButton.RightThumb);
			AddAxisToAxis(0, GamePadAxis.LeftThumbX);
			AddAxisToAxis(1, GamePadAxis.LeftThumbY, true);
			AddAxisToAxis(3, GamePadAxis.RightThumbX);
			AddAxisToAxis(4, GamePadAxis.RightThumbY, true);
			 
		}
	}

	
	/// <summary>
    /// RetroLink Saturn Classic Controller 
    /// </summary>
	internal class Layout117 : GameControllerDatabaseLayout
	{
		public Layout117() : base(new Guid("00000003-0079-0000-1100-000010010000"), "RetroLink Saturn Classic Controller")
		{
			AddButtonToButton(3, GamePadButton.X);
			AddButtonToButton(0, GamePadButton.A);
			AddButtonToButton(1, GamePadButton.B);
			AddButtonToButton(4, GamePadButton.Y);
			AddButtonToButton(5, GamePadButton.Back);
			AddButtonToButton(8, GamePadButton.Start);
			AddButtonToButton(6, GamePadButton.LeftShoulder);
			AddButtonToButton(7, GamePadButton.RightShoulder);
			AddAxisToAxis(0, GamePadAxis.LeftThumbX);
			AddAxisToAxis(1, GamePadAxis.LeftThumbY, true);
			 
		}
	}

	
	/// <summary>
    /// Nintendo Wii U Pro Controller 
    /// </summary>
	internal class Layout118 : GameControllerDatabaseLayout
	{
		public Layout118() : base(new Guid("00000005-057e-0000-3003-000001000000"), "Nintendo Wii U Pro Controller")
		{
			AddButtonToButton(0, GamePadButton.A);
			AddButtonToButton(1, GamePadButton.B);
			AddButtonToButton(3, GamePadButton.X);
			AddButtonToButton(2, GamePadButton.Y);
			AddButtonToButton(8, GamePadButton.Back);
			AddButtonToButton(9, GamePadButton.Start);
			AddButtonToButton(4, GamePadButton.LeftShoulder);
			AddButtonToButton(5, GamePadButton.RightShoulder);
			AddButtonToButton(11, GamePadButton.LeftThumb);
			AddButtonToButton(12, GamePadButton.RightThumb);
			AddAxisToAxis(0, GamePadAxis.LeftThumbX);
			AddAxisToAxis(1, GamePadAxis.LeftThumbY, true);
			AddAxisToAxis(2, GamePadAxis.RightThumbX);
			AddAxisToAxis(3, GamePadAxis.RightThumbY, true);
			AddButtonToAxis(6, GamePadAxis.LeftTrigger);
			AddButtonToAxis(7, GamePadAxis.RightTrigger);
			AddButtonToButton(13, GamePadButton.PadUp);
			AddButtonToButton(15, GamePadButton.PadLeft);
			AddButtonToButton(14, GamePadButton.PadDown);
			AddButtonToButton(16, GamePadButton.PadRight);
			 
		}
	}

	
	/// <summary>
    /// Microsoft X-Box 360 pad 
    /// </summary>
	internal class Layout119 : GameControllerDatabaseLayout
	{
		public Layout119() : base(new Guid("00000003-045e-0000-8e02-000004010000"), "Microsoft X-Box 360 pad")
		{
			AddButtonToButton(0, GamePadButton.A);
			AddButtonToButton(1, GamePadButton.B);
			AddButtonToButton(2, GamePadButton.X);
			AddButtonToButton(3, GamePadButton.Y);
			AddButtonToButton(6, GamePadButton.Back);
			AddButtonToButton(7, GamePadButton.Start);
			AddButtonToButton(4, GamePadButton.LeftShoulder);
			AddButtonToButton(5, GamePadButton.RightShoulder);
			AddButtonToButton(9, GamePadButton.LeftThumb);
			AddButtonToButton(10, GamePadButton.RightThumb);
			AddAxisToAxis(0, GamePadAxis.LeftThumbX);
			AddAxisToAxis(1, GamePadAxis.LeftThumbY, true);
			AddAxisToAxis(3, GamePadAxis.RightThumbX);
			AddAxisToAxis(4, GamePadAxis.RightThumbY, true);
			AddAxisToAxis(2, GamePadAxis.LeftTrigger, remap: true);
			AddAxisToAxis(5, GamePadAxis.RightTrigger, remap: true);
			 
		}
	}

	
	/// <summary>
    /// HORI CO. LTD. REAL ARCADE Pro.V3 
    /// </summary>
	internal class Layout120 : GameControllerDatabaseLayout
	{
		public Layout120() : base(new Guid("00000003-0f0d-0000-2200-000011010000"), "HORI CO. LTD. REAL ARCADE Pro.V3")
		{
			AddButtonToButton(0, GamePadButton.X);
			AddButtonToButton(1, GamePadButton.A);
			AddButtonToButton(2, GamePadButton.B);
			AddButtonToButton(3, GamePadButton.Y);
			AddButtonToButton(8, GamePadButton.Back);
			AddButtonToButton(9, GamePadButton.Start);
			AddButtonToButton(4, GamePadButton.LeftShoulder);
			AddButtonToAxis(6, GamePadAxis.LeftTrigger);
			AddButtonToButton(5, GamePadButton.RightShoulder);
			AddButtonToAxis(7, GamePadAxis.RightTrigger);
			 
		}
	}

	
	/// <summary>
    /// HORI CO. LTD. FIGHTING STICK 3 
    /// </summary>
	internal class Layout121 : GameControllerDatabaseLayout
	{
		public Layout121() : base(new Guid("00000003-0f0d-0000-1000-000011010000"), "HORI CO. LTD. FIGHTING STICK 3")
		{
			AddButtonToButton(0, GamePadButton.X);
			AddButtonToButton(1, GamePadButton.A);
			AddButtonToButton(2, GamePadButton.B);
			AddButtonToButton(3, GamePadButton.Y);
			AddButtonToButton(8, GamePadButton.Back);
			AddButtonToButton(9, GamePadButton.Start);
			AddButtonToButton(4, GamePadButton.LeftShoulder);
			AddButtonToAxis(6, GamePadAxis.LeftTrigger);
			AddButtonToButton(5, GamePadButton.RightShoulder);
			AddButtonToAxis(7, GamePadAxis.RightTrigger);
			 
		}
	}

	
	/// <summary>
    /// Goodbetterbest Ltd USB Controller 
    /// </summary>
	internal class Layout122 : GameControllerDatabaseLayout
	{
		public Layout122() : base(new Guid("00000003-25f0-0000-c183-000010010000"), "Goodbetterbest Ltd USB Controller")
		{
			AddButtonToButton(0, GamePadButton.X);
			AddButtonToButton(1, GamePadButton.A);
			AddButtonToButton(2, GamePadButton.B);
			AddButtonToButton(3, GamePadButton.Y);
			AddButtonToButton(8, GamePadButton.Back);
			AddButtonToButton(9, GamePadButton.Start);
			AddButtonToButton(4, GamePadButton.LeftShoulder);
			AddButtonToAxis(6, GamePadAxis.LeftTrigger);
			AddButtonToButton(5, GamePadButton.RightShoulder);
			AddButtonToAxis(7, GamePadAxis.RightTrigger);
			AddButtonToButton(10, GamePadButton.LeftThumb);
			AddButtonToButton(11, GamePadButton.RightThumb);
			AddAxisToAxis(0, GamePadAxis.LeftThumbX);
			AddAxisToAxis(1, GamePadAxis.LeftThumbY, true);
			AddAxisToAxis(2, GamePadAxis.RightThumbX);
			AddAxisToAxis(3, GamePadAxis.RightThumbY, true);
			 
		}
	}

	
	/// <summary>
    /// Xbox Gamepad (userspace driver) 
    /// </summary>
	internal class Layout123 : GameControllerDatabaseLayout
	{
		public Layout123() : base(new Guid("00000000-6258-786f-2047-616d65706100"), "Xbox Gamepad (userspace driver)")
		{
			AddButtonToButton(0, GamePadButton.A);
			AddButtonToButton(1, GamePadButton.B);
			AddButtonToButton(2, GamePadButton.X);
			AddButtonToButton(3, GamePadButton.Y);
			AddButtonToButton(7, GamePadButton.Start);
			AddButtonToButton(6, GamePadButton.Back);
			AddButtonToButton(4, GamePadButton.LeftShoulder);
			AddButtonToButton(5, GamePadButton.RightShoulder);
			AddAxisToAxis(5, GamePadAxis.LeftTrigger, remap: true);
			AddAxisToAxis(4, GamePadAxis.RightTrigger, remap: true);
			AddButtonToButton(9, GamePadButton.LeftThumb);
			AddButtonToButton(10, GamePadButton.RightThumb);
			AddAxisToAxis(0, GamePadAxis.LeftThumbX);
			AddAxisToAxis(1, GamePadAxis.LeftThumbY, true);
			AddAxisToAxis(2, GamePadAxis.RightThumbX);
			AddAxisToAxis(3, GamePadAxis.RightThumbY, true);
			 
		}
	}

	
	/// <summary>
    /// PC Game Controller 
    /// </summary>
	internal class Layout124 : GameControllerDatabaseLayout
	{
		public Layout124() : base(new Guid("00000003-11ff-0000-3133-000010010000"), "PC Game Controller")
		{
			AddButtonToButton(2, GamePadButton.A);
			AddButtonToButton(1, GamePadButton.B);
			AddButtonToButton(0, GamePadButton.Y);
			AddButtonToButton(3, GamePadButton.X);
			AddButtonToButton(9, GamePadButton.Start);
			AddButtonToButton(8, GamePadButton.Back);
			AddButtonToButton(10, GamePadButton.LeftThumb);
			AddButtonToButton(11, GamePadButton.RightThumb);
			AddButtonToButton(4, GamePadButton.LeftShoulder);
			AddButtonToButton(5, GamePadButton.RightShoulder);
			AddAxisToAxis(0, GamePadAxis.LeftThumbX);
			AddAxisToAxis(1, GamePadAxis.LeftThumbY, true);
			AddAxisToAxis(2, GamePadAxis.RightThumbX);
			AddAxisToAxis(3, GamePadAxis.RightThumbY, true);
			AddButtonToAxis(6, GamePadAxis.LeftTrigger);
			AddButtonToAxis(7, GamePadAxis.RightTrigger);
			 
		}
	}

	
	/// <summary>
    /// SpeedLink XEOX Pro Analog Gamepad pad 
    /// </summary>
	internal class Layout125 : GameControllerDatabaseLayout
	{
		public Layout125() : base(new Guid("00000003-045e-0000-8e02-000020200000"), "SpeedLink XEOX Pro Analog Gamepad pad")
		{
			AddButtonToButton(2, GamePadButton.X);
			AddButtonToButton(0, GamePadButton.A);
			AddButtonToButton(1, GamePadButton.B);
			AddButtonToButton(3, GamePadButton.Y);
			AddButtonToButton(6, GamePadButton.Back);
			AddButtonToButton(7, GamePadButton.Start);
			AddButtonToButton(4, GamePadButton.LeftShoulder);
			AddAxisToAxis(2, GamePadAxis.LeftTrigger, remap: true);
			AddButtonToButton(5, GamePadButton.RightShoulder);
			AddAxisToAxis(5, GamePadAxis.RightTrigger, remap: true);
			AddButtonToButton(9, GamePadButton.LeftThumb);
			AddButtonToButton(10, GamePadButton.RightThumb);
			AddAxisToAxis(0, GamePadAxis.LeftThumbX);
			AddAxisToAxis(1, GamePadAxis.LeftThumbY, true);
			AddAxisToAxis(3, GamePadAxis.RightThumbX);
			AddAxisToAxis(4, GamePadAxis.RightThumbY, true);
			 
		}
	}

	
	/// <summary>
    /// Generic X-Box pad 
    /// </summary>
	internal class Layout126 : GameControllerDatabaseLayout
	{
		public Layout126() : base(new Guid("00000003-0e6f-0000-1304-000000010000"), "Generic X-Box pad")
		{
			AddButtonToButton(2, GamePadButton.X);
			AddButtonToButton(0, GamePadButton.A);
			AddButtonToButton(1, GamePadButton.B);
			AddButtonToButton(3, GamePadButton.Y);
			AddButtonToButton(6, GamePadButton.Back);
			AddButtonToButton(7, GamePadButton.Start);
			AddButtonToButton(4, GamePadButton.LeftShoulder);
			AddAxisToAxis(2, GamePadAxis.LeftTrigger, remap: true);
			AddButtonToButton(5, GamePadButton.RightShoulder);
			AddAxisToAxis(5, GamePadAxis.RightTrigger, remap: true);
			AddAxisToButton(0, GamePadButton.LeftThumb);
			AddButtonToButton(9, GamePadButton.LeftThumb);
			AddAxisToButton(3, GamePadButton.RightThumb);
			AddButtonToButton(10, GamePadButton.RightThumb);
			AddAxisToAxis(0, GamePadAxis.LeftThumbX);
			AddAxisToAxis(1, GamePadAxis.LeftThumbY, true);
			AddAxisToAxis(3, GamePadAxis.RightThumbX);
			AddAxisToAxis(4, GamePadAxis.RightThumbY, true);
			 
		}
	}

	
	/// <summary>
    /// Saitek PLC Saitek P3200 Rumble Pad 
    /// </summary>
	internal class Layout127 : GameControllerDatabaseLayout
	{
		public Layout127() : base(new Guid("00000003-06a3-0000-18f5-000010010000"), "Saitek PLC Saitek P3200 Rumble Pad")
		{
			AddButtonToButton(0, GamePadButton.X);
			AddButtonToButton(1, GamePadButton.A);
			AddButtonToButton(2, GamePadButton.B);
			AddButtonToButton(3, GamePadButton.Y);
			AddButtonToButton(8, GamePadButton.Back);
			AddButtonToButton(9, GamePadButton.Start);
			AddButtonToButton(4, GamePadButton.LeftShoulder);
			AddAxisToAxis(2, GamePadAxis.LeftTrigger, remap: true);
			AddButtonToButton(6, GamePadButton.RightShoulder);
			AddButtonToButton(5, GamePadButton.RightShoulder);
			AddButtonToAxis(7, GamePadAxis.RightTrigger);
			AddButtonToButton(10, GamePadButton.LeftThumb);
			AddButtonToButton(11, GamePadButton.RightThumb);
			AddAxisToAxis(0, GamePadAxis.LeftThumbX);
			AddAxisToAxis(1, GamePadAxis.LeftThumbY, true);
			AddAxisToAxis(3, GamePadAxis.RightThumbX);
			AddAxisToAxis(4, GamePadAxis.RightThumbY, true);
			 
		}
	}

	
	/// <summary>
    /// iBuffalo USB 2-axis 8-button Gamepad 
    /// </summary>
	internal class Layout128 : GameControllerDatabaseLayout
	{
		public Layout128() : base(new Guid("00000003-0583-0000-6020-000010010000"), "iBuffalo USB 2-axis 8-button Gamepad")
		{
			AddButtonToButton(1, GamePadButton.A);
			AddButtonToButton(0, GamePadButton.B);
			AddButtonToButton(3, GamePadButton.X);
			AddButtonToButton(2, GamePadButton.Y);
			AddButtonToButton(6, GamePadButton.Back);
			AddButtonToButton(7, GamePadButton.Start);
			AddButtonToButton(4, GamePadButton.LeftShoulder);
			AddButtonToButton(5, GamePadButton.RightShoulder);
			AddAxisToAxis(0, GamePadAxis.LeftThumbX);
			AddAxisToAxis(1, GamePadAxis.LeftThumbY, true);
			 
		}
	}

	
	/// <summary>
    /// HJC Game GAMEPAD 
    /// </summary>
	internal class Layout129 : GameControllerDatabaseLayout
	{
		public Layout129() : base(new Guid("00000003-11c9-0000-f055-000011010000"), "HJC Game GAMEPAD")
		{
			AddAxisToAxis(0, GamePadAxis.LeftThumbX);
			AddAxisToAxis(1, GamePadAxis.LeftThumbY, true);
			AddButtonToButton(11, GamePadButton.RightThumb);
			AddButtonToButton(5, GamePadButton.RightShoulder);
			AddAxisToAxis(2, GamePadAxis.RightThumbX);
			AddButtonToButton(9, GamePadButton.Start);
			AddAxisToAxis(3, GamePadAxis.RightThumbY, true);
			AddButtonToAxis(6, GamePadAxis.LeftTrigger);
			AddButtonToButton(2, GamePadButton.X);
			AddButtonToButton(8, GamePadButton.Back);
			AddButtonToButton(10, GamePadButton.LeftThumb);
			AddButtonToButton(4, GamePadButton.LeftShoulder);
			AddButtonToButton(3, GamePadButton.Y);
			AddButtonToButton(0, GamePadButton.A);
			AddButtonToAxis(7, GamePadAxis.RightTrigger);
			AddButtonToButton(1, GamePadButton.B);
			 
		}
	}

	
	/// <summary>
    /// Saitek P2900 Wireless Pad 
    /// </summary>
	internal class Layout130 : GameControllerDatabaseLayout
	{
		public Layout130() : base(new Guid("00000003-06a3-0000-0c04-000011010000"), "Saitek P2900 Wireless Pad")
		{
			AddButtonToButton(1, GamePadButton.A);
			AddButtonToButton(2, GamePadButton.B);
			AddButtonToButton(3, GamePadButton.Y);
			AddButtonToButton(0, GamePadButton.X);
			AddButtonToButton(12, GamePadButton.Start);
			AddButtonToButton(8, GamePadButton.Back);
			AddButtonToButton(10, GamePadButton.LeftThumb);
			AddButtonToButton(11, GamePadButton.RightThumb);
			AddButtonToButton(6, GamePadButton.LeftShoulder);
			AddButtonToButton(7, GamePadButton.RightShoulder);
			AddAxisToAxis(0, GamePadAxis.LeftThumbX);
			AddAxisToAxis(1, GamePadAxis.LeftThumbY, true);
			AddAxisToAxis(3, GamePadAxis.RightThumbX);
			AddAxisToAxis(2, GamePadAxis.RightThumbY, true);
			AddButtonToAxis(4, GamePadAxis.LeftTrigger);
			AddButtonToAxis(5, GamePadAxis.RightTrigger);
			 
		}
	}

	
	/// <summary>
    /// GameCube {HuiJia USB box} 
    /// </summary>
	internal class Layout131 : GameControllerDatabaseLayout
	{
		public Layout131() : base(new Guid("00000003-1a34-0000-05f7-000010010000"), "GameCube {HuiJia USB box}")
		{
			AddButtonToButton(1, GamePadButton.A);
			AddButtonToButton(2, GamePadButton.B);
			AddButtonToButton(3, GamePadButton.Y);
			AddButtonToButton(0, GamePadButton.X);
			AddButtonToButton(9, GamePadButton.Start);
			AddButtonToButton(15, GamePadButton.PadLeft);
			AddButtonToButton(14, GamePadButton.PadDown);
			AddButtonToButton(13, GamePadButton.PadRight);
			AddAxisToAxis(0, GamePadAxis.LeftThumbX);
			AddAxisToAxis(1, GamePadAxis.LeftThumbY, true);
			AddAxisToAxis(5, GamePadAxis.RightThumbX);
			AddAxisToAxis(2, GamePadAxis.RightThumbY, true);
			AddAxisToAxis(3, GamePadAxis.LeftTrigger, remap: true);
			AddAxisToAxis(4, GamePadAxis.RightTrigger, remap: true);
			AddButtonToButton(7, GamePadButton.RightShoulder);
			AddButtonToButton(12, GamePadButton.PadUp);
			 
		}
	}

	
	/// <summary>
    /// JC-U3613M - DirectInput Mode 
    /// </summary>
	internal class Layout132 : GameControllerDatabaseLayout
	{
		public Layout132() : base(new Guid("00000003-056e-0000-0320-000010010000"), "JC-U3613M - DirectInput Mode")
		{
			AddButtonToButton(0, GamePadButton.X);
			AddButtonToButton(2, GamePadButton.A);
			AddButtonToButton(3, GamePadButton.B);
			AddButtonToButton(1, GamePadButton.Y);
			AddButtonToButton(10, GamePadButton.Back);
			AddButtonToButton(11, GamePadButton.Start);
			AddButtonToButton(4, GamePadButton.LeftShoulder);
			AddButtonToAxis(6, GamePadAxis.LeftTrigger);
			AddButtonToButton(5, GamePadButton.RightShoulder);
			AddButtonToAxis(7, GamePadAxis.RightTrigger);
			AddButtonToButton(8, GamePadButton.LeftThumb);
			AddButtonToButton(9, GamePadButton.RightThumb);
			AddAxisToAxis(0, GamePadAxis.LeftThumbX);
			AddAxisToAxis(1, GamePadAxis.LeftThumbY, true);
			AddAxisToAxis(2, GamePadAxis.RightThumbX);
			AddAxisToAxis(3, GamePadAxis.RightThumbY, true);
			 
		}
	}

	
	/// <summary>
    /// Rock Candy Wired Controller for Xbox One 
    /// </summary>
	internal class Layout133 : GameControllerDatabaseLayout
	{
		public Layout133() : base(new Guid("00000003-0e6f-0000-4601-000001010000"), "Rock Candy Wired Controller for Xbox One")
		{
			AddButtonToButton(0, GamePadButton.A);
			AddButtonToButton(1, GamePadButton.B);
			AddButtonToButton(2, GamePadButton.X);
			AddButtonToButton(3, GamePadButton.Y);
			AddButtonToButton(4, GamePadButton.LeftShoulder);
			AddButtonToButton(5, GamePadButton.RightShoulder);
			AddButtonToButton(6, GamePadButton.Back);
			AddButtonToButton(7, GamePadButton.Start);
			AddButtonToButton(9, GamePadButton.LeftThumb);
			AddButtonToButton(10, GamePadButton.RightThumb);
			AddAxisToAxis(2, GamePadAxis.LeftTrigger, remap: true);
			AddAxisToAxis(5, GamePadAxis.RightTrigger, remap: true);
			AddAxisToAxis(0, GamePadAxis.LeftThumbX);
			AddAxisToAxis(1, GamePadAxis.LeftThumbY, true);
			AddAxisToAxis(3, GamePadAxis.RightThumbX);
			AddAxisToAxis(4, GamePadAxis.RightThumbY, true);
			 
		}
	}

	
	/// <summary>
    /// Mad Catz Wired Xbox 360 Controller 
    /// </summary>
	internal class Layout134 : GameControllerDatabaseLayout
	{
		public Layout134() : base(new Guid("00000003-0738-0000-1647-000010040000"), "Mad Catz Wired Xbox 360 Controller")
		{
			AddButtonToButton(2, GamePadButton.X);
			AddButtonToButton(0, GamePadButton.A);
			AddButtonToButton(1, GamePadButton.B);
			AddButtonToButton(3, GamePadButton.Y);
			AddButtonToButton(6, GamePadButton.Back);
			AddButtonToButton(7, GamePadButton.Start);
			AddButtonToButton(4, GamePadButton.LeftShoulder);
			AddAxisToAxis(2, GamePadAxis.LeftTrigger, remap: true);
			AddButtonToButton(5, GamePadButton.RightShoulder);
			AddAxisToAxis(5, GamePadAxis.RightTrigger, remap: true);
			AddButtonToButton(9, GamePadButton.LeftThumb);
			AddButtonToButton(10, GamePadButton.RightThumb);
			AddAxisToAxis(0, GamePadAxis.LeftThumbX);
			AddAxisToAxis(1, GamePadAxis.LeftThumbY, true);
			AddAxisToAxis(3, GamePadAxis.RightThumbX);
			AddAxisToAxis(4, GamePadAxis.RightThumbY, true);
			 
		}
	}

	
	/// <summary>
    /// Afterglow Wired Controller for Xbox One 
    /// </summary>
	internal class Layout135 : GameControllerDatabaseLayout
	{
		public Layout135() : base(new Guid("00000003-0e6f-0000-3901-000020060000"), "Afterglow Wired Controller for Xbox One")
		{
			AddButtonToButton(2, GamePadButton.X);
			AddButtonToButton(0, GamePadButton.A);
			AddButtonToButton(1, GamePadButton.B);
			AddButtonToButton(3, GamePadButton.Y);
			AddButtonToButton(6, GamePadButton.Back);
			AddButtonToButton(7, GamePadButton.Start);
			AddButtonToButton(4, GamePadButton.LeftShoulder);
			AddAxisToAxis(2, GamePadAxis.LeftTrigger, remap: true);
			AddButtonToButton(5, GamePadButton.RightShoulder);
			AddAxisToAxis(5, GamePadAxis.RightTrigger, remap: true);
			AddButtonToButton(9, GamePadButton.LeftThumb);
			AddButtonToButton(10, GamePadButton.RightThumb);
			AddAxisToAxis(0, GamePadAxis.LeftThumbX);
			AddAxisToAxis(1, GamePadAxis.LeftThumbY, true);
			AddAxisToAxis(3, GamePadAxis.RightThumbX);
			AddAxisToAxis(4, GamePadAxis.RightThumbY, true);
			 
		}
	}

	
	/// <summary>
    /// Thrustmaster Dual Analog 4 
    /// </summary>
	internal class Layout136 : GameControllerDatabaseLayout
	{
		public Layout136() : base(new Guid("00000003-044f-0000-15b3-000010010000"), "Thrustmaster Dual Analog 4")
		{
			AddButtonToButton(0, GamePadButton.A);
			AddButtonToButton(2, GamePadButton.B);
			AddButtonToButton(1, GamePadButton.X);
			AddButtonToButton(3, GamePadButton.Y);
			AddButtonToButton(9, GamePadButton.Start);
			AddButtonToButton(8, GamePadButton.Back);
			AddButtonToButton(10, GamePadButton.LeftThumb);
			AddButtonToButton(11, GamePadButton.RightThumb);
			AddButtonToButton(4, GamePadButton.LeftShoulder);
			AddButtonToButton(6, GamePadButton.RightShoulder);
			AddAxisToAxis(0, GamePadAxis.LeftThumbX);
			AddAxisToAxis(1, GamePadAxis.LeftThumbY, true);
			AddAxisToAxis(2, GamePadAxis.RightThumbX);
			AddAxisToAxis(3, GamePadAxis.RightThumbY, true);
			AddButtonToAxis(5, GamePadAxis.LeftTrigger);
			AddButtonToAxis(7, GamePadAxis.RightTrigger);
			 
		}
	}

	
	/// <summary>
    /// 8Bitdo SFC30 GamePad 
    /// </summary>
	internal class Layout137 : GameControllerDatabaseLayout
	{
		public Layout137() : base(new Guid("00000005-2810-0000-0900-000000010000"), "8Bitdo SFC30 GamePad")
		{
			AddButtonToButton(4, GamePadButton.X);
			AddButtonToButton(1, GamePadButton.A);
			AddButtonToButton(0, GamePadButton.B);
			AddButtonToButton(3, GamePadButton.Y);
			AddButtonToButton(10, GamePadButton.Back);
			AddButtonToButton(11, GamePadButton.Start);
			AddButtonToButton(6, GamePadButton.LeftShoulder);
			AddButtonToButton(7, GamePadButton.RightShoulder);
			AddAxisToAxis(0, GamePadAxis.LeftThumbX);
			AddAxisToAxis(1, GamePadAxis.LeftThumbY, true);
			 
		}
	}

	
	/// <summary>
    /// HitBox (PS3/PC) Analog Mode 
    /// </summary>
	internal class Layout138 : GameControllerDatabaseLayout
	{
		public Layout138() : base(new Guid("00000003-14d8-0000-0862-000011010000"), "HitBox (PS3/PC) Analog Mode")
		{
			AddButtonToButton(1, GamePadButton.A);
			AddButtonToButton(2, GamePadButton.B);
			AddButtonToButton(3, GamePadButton.Y);
			AddButtonToButton(0, GamePadButton.X);
			AddButtonToButton(12, GamePadButton.Start);
			AddButtonToButton(8, GamePadButton.Back);
			AddButtonToButton(4, GamePadButton.LeftShoulder);
			AddButtonToButton(5, GamePadButton.RightShoulder);
			AddButtonToAxis(6, GamePadAxis.LeftTrigger);
			AddButtonToAxis(7, GamePadAxis.RightTrigger);
			AddAxisToAxis(0, GamePadAxis.LeftThumbX);
			AddAxisToAxis(1, GamePadAxis.LeftThumbY, true);
			 
		}
	}

	
	/// <summary>
    /// hori 
    /// </summary>
	internal class Layout139 : GameControllerDatabaseLayout
	{
		public Layout139() : base(new Guid("00000003-0f0d-0000-0d00-000000010000"), "hori")
		{
			AddButtonToButton(0, GamePadButton.A);
			AddButtonToButton(6, GamePadButton.B);
			AddButtonToButton(2, GamePadButton.Y);
			AddButtonToButton(1, GamePadButton.X);
			AddButtonToButton(9, GamePadButton.Start);
			AddButtonToButton(8, GamePadButton.Back);
			AddButtonToButton(3, GamePadButton.LeftShoulder);
			AddButtonToButton(7, GamePadButton.RightShoulder);
			AddButtonToAxis(4, GamePadAxis.LeftThumbX);
			AddButtonToAxis(5, GamePadAxis.LeftThumbY);
			 
		}
	}

	
	/// <summary>
    /// Mad Catz Xbox 360 Controller 
    /// </summary>
	internal class Layout140 : GameControllerDatabaseLayout
	{
		public Layout140() : base(new Guid("00000003-1bad-0000-16f0-000090040000"), "Mad Catz Xbox 360 Controller")
		{
			AddButtonToButton(0, GamePadButton.A);
			AddButtonToButton(1, GamePadButton.B);
			AddButtonToButton(3, GamePadButton.Y);
			AddButtonToButton(2, GamePadButton.X);
			AddButtonToButton(7, GamePadButton.Start);
			AddButtonToButton(6, GamePadButton.Back);
			AddButtonToButton(9, GamePadButton.LeftThumb);
			AddButtonToButton(10, GamePadButton.RightThumb);
			AddButtonToButton(4, GamePadButton.LeftShoulder);
			AddButtonToButton(5, GamePadButton.RightShoulder);
			AddAxisToAxis(0, GamePadAxis.LeftThumbX);
			AddAxisToAxis(1, GamePadAxis.LeftThumbY, true);
			AddAxisToAxis(3, GamePadAxis.RightThumbX);
			AddAxisToAxis(4, GamePadAxis.RightThumbY, true);
			AddAxisToAxis(2, GamePadAxis.LeftTrigger, remap: true);
			AddAxisToAxis(5, GamePadAxis.RightTrigger, remap: true);
			 
		}
	}

	
	/// <summary>
    /// Toodles 2008 Chimp PC/PS3 
    /// </summary>
	internal class Layout141 : GameControllerDatabaseLayout
	{
		public Layout141() : base(new Guid("00000003-14d8-0000-07cd-000011010000"), "Toodles 2008 Chimp PC/PS3")
		{
			AddButtonToButton(0, GamePadButton.A);
			AddButtonToButton(1, GamePadButton.B);
			AddButtonToButton(2, GamePadButton.Y);
			AddButtonToButton(3, GamePadButton.X);
			AddButtonToButton(9, GamePadButton.Start);
			AddButtonToButton(8, GamePadButton.Back);
			AddButtonToButton(4, GamePadButton.LeftShoulder);
			AddButtonToButton(5, GamePadButton.RightShoulder);
			AddAxisToAxis(0, GamePadAxis.LeftThumbX);
			AddAxisToAxis(1, GamePadAxis.LeftThumbY, true);
			AddButtonToAxis(6, GamePadAxis.LeftTrigger);
			AddButtonToAxis(7, GamePadAxis.RightTrigger);
			 
		}
	}

	
	/// <summary>
    /// InterAct GoPad I-73000 (Fighting Game Layout) 
    /// </summary>
	internal class Layout142 : GameControllerDatabaseLayout
	{
		public Layout142() : base(new Guid("00000003-05fd-0000-0030-000000010000"), "InterAct GoPad I-73000 (Fighting Game Layout)")
		{
			AddButtonToButton(3, GamePadButton.A);
			AddButtonToButton(4, GamePadButton.B);
			AddButtonToButton(1, GamePadButton.Y);
			AddButtonToButton(0, GamePadButton.X);
			AddButtonToButton(7, GamePadButton.Start);
			AddButtonToButton(6, GamePadButton.Back);
			AddAxisToAxis(0, GamePadAxis.LeftThumbX);
			AddAxisToAxis(1, GamePadAxis.LeftThumbY, true);
			AddButtonToButton(2, GamePadButton.RightShoulder);
			AddButtonToAxis(5, GamePadAxis.RightTrigger);
			 
		}
	}

	
	/// <summary>
    /// Nintendo Wiimote 
    /// </summary>
	internal class Layout143 : GameControllerDatabaseLayout
	{
		public Layout143() : base(new Guid("00000005-0001-0000-0100-000003000000"), "Nintendo Wiimote")
		{
			AddButtonToButton(0, GamePadButton.A);
			AddButtonToButton(1, GamePadButton.B);
			AddButtonToButton(3, GamePadButton.Y);
			AddButtonToButton(2, GamePadButton.X);
			AddButtonToButton(9, GamePadButton.Start);
			AddButtonToButton(8, GamePadButton.Back);
			AddButtonToButton(11, GamePadButton.LeftThumb);
			AddButtonToButton(12, GamePadButton.RightThumb);
			AddButtonToButton(4, GamePadButton.LeftShoulder);
			AddButtonToButton(5, GamePadButton.RightShoulder);
			AddAxisToAxis(0, GamePadAxis.LeftThumbX);
			AddAxisToAxis(1, GamePadAxis.LeftThumbY, true);
			AddAxisToAxis(2, GamePadAxis.RightThumbX);
			AddAxisToAxis(3, GamePadAxis.RightThumbY, true);
			AddButtonToAxis(6, GamePadAxis.LeftTrigger);
			AddButtonToAxis(7, GamePadAxis.RightTrigger);
			 
		}
	}

	
	/// <summary>
    /// Microsoft X-Box 360 pad 
    /// </summary>
	internal class Layout144 : GameControllerDatabaseLayout
	{
		public Layout144() : base(new Guid("00000003-045e-0000-8e02-000062230000"), "Microsoft X-Box 360 pad")
		{
			AddButtonToButton(2, GamePadButton.X);
			AddButtonToButton(0, GamePadButton.A);
			AddButtonToButton(1, GamePadButton.B);
			AddButtonToButton(3, GamePadButton.Y);
			AddButtonToButton(6, GamePadButton.Back);
			AddButtonToButton(7, GamePadButton.Start);
			AddButtonToButton(4, GamePadButton.LeftShoulder);
			AddAxisToAxis(2, GamePadAxis.LeftTrigger, remap: true);
			AddButtonToButton(5, GamePadButton.RightShoulder);
			AddAxisToAxis(5, GamePadAxis.RightTrigger, remap: true);
			AddButtonToButton(9, GamePadButton.LeftThumb);
			AddButtonToButton(10, GamePadButton.RightThumb);
			AddAxisToAxis(0, GamePadAxis.LeftThumbX);
			AddAxisToAxis(1, GamePadAxis.LeftThumbY, true);
			AddAxisToAxis(3, GamePadAxis.RightThumbX);
			AddAxisToAxis(4, GamePadAxis.RightThumbY, true);
			 
		}
	}

	
	/// <summary>
    /// Saitek P880 
    /// </summary>
	internal class Layout145 : GameControllerDatabaseLayout
	{
		public Layout145() : base(new Guid("00000003-06a3-0000-0901-000000010000"), "Saitek P880")
		{
			AddButtonToButton(2, GamePadButton.A);
			AddButtonToButton(3, GamePadButton.B);
			AddButtonToButton(1, GamePadButton.Y);
			AddButtonToButton(0, GamePadButton.X);
			AddButtonToButton(8, GamePadButton.LeftThumb);
			AddButtonToButton(9, GamePadButton.RightThumb);
			AddButtonToButton(4, GamePadButton.LeftShoulder);
			AddButtonToButton(5, GamePadButton.RightShoulder);
			AddAxisToAxis(0, GamePadAxis.LeftThumbX);
			AddAxisToAxis(1, GamePadAxis.LeftThumbY, true);
			AddAxisToAxis(3, GamePadAxis.RightThumbX);
			AddAxisToAxis(2, GamePadAxis.RightThumbY, true);
			AddButtonToAxis(6, GamePadAxis.LeftTrigger);
			AddButtonToAxis(7, GamePadAxis.RightTrigger);
			 
		}
	}

	
	/// <summary>
    /// Logic3 Controller 
    /// </summary>
	internal class Layout146 : GameControllerDatabaseLayout
	{
		public Layout146() : base(new Guid("00000003-0e6f-0000-0103-000000020000"), "Logic3 Controller")
		{
			AddButtonToButton(2, GamePadButton.X);
			AddButtonToButton(0, GamePadButton.A);
			AddButtonToButton(1, GamePadButton.B);
			AddButtonToButton(3, GamePadButton.Y);
			AddButtonToButton(6, GamePadButton.Back);
			AddButtonToButton(7, GamePadButton.Start);
			AddButtonToButton(4, GamePadButton.LeftShoulder);
			AddAxisToAxis(2, GamePadAxis.LeftTrigger, remap: true);
			AddButtonToButton(5, GamePadButton.RightShoulder);
			AddAxisToAxis(5, GamePadAxis.RightTrigger, remap: true);
			AddButtonToButton(9, GamePadButton.LeftThumb);
			AddButtonToButton(10, GamePadButton.RightThumb);
			AddAxisToAxis(0, GamePadAxis.LeftThumbX);
			AddAxisToAxis(1, GamePadAxis.LeftThumbY, true);
			AddAxisToAxis(3, GamePadAxis.RightThumbX);
			AddAxisToAxis(4, GamePadAxis.RightThumbY, true);
			 
		}
	}

	
	/// <summary>
    /// Mad Catz C.T.R.L.R  
    /// </summary>
	internal class Layout147 : GameControllerDatabaseLayout
	{
		public Layout147() : base(new Guid("00000005-0738-0000-6652-000025010000"), "Mad Catz C.T.R.L.R ")
		{
			AddButtonToButton(0, GamePadButton.X);
			AddButtonToButton(1, GamePadButton.A);
			AddButtonToButton(2, GamePadButton.B);
			AddButtonToButton(3, GamePadButton.Y);
			AddButtonToButton(8, GamePadButton.Back);
			AddButtonToButton(9, GamePadButton.Start);
			AddButtonToButton(4, GamePadButton.LeftShoulder);
			AddButtonToAxis(6, GamePadAxis.LeftTrigger);
			AddButtonToButton(5, GamePadButton.RightShoulder);
			AddButtonToAxis(7, GamePadAxis.RightTrigger);
			AddButtonToButton(10, GamePadButton.LeftThumb);
			AddButtonToButton(11, GamePadButton.RightThumb);
			AddAxisToAxis(0, GamePadAxis.LeftThumbX);
			AddAxisToAxis(1, GamePadAxis.LeftThumbY, true);
			AddAxisToAxis(2, GamePadAxis.RightThumbX);
			AddAxisToAxis(3, GamePadAxis.RightThumbY, true);
			 
		}
	}

	#endif
	
	internal static class GameControllerDatabase
	{
		public static void RegisterLayouts()
		{
		#if SILICONSTUDIO_PLATFORM_WINDOWS
					GamePadLayouts.AddLayout(new Layout0());
					GamePadLayouts.AddLayout(new Layout1());
					GamePadLayouts.AddLayout(new Layout2());
					GamePadLayouts.AddLayout(new Layout3());
					GamePadLayouts.AddLayout(new Layout4());
					GamePadLayouts.AddLayout(new Layout5());
					GamePadLayouts.AddLayout(new Layout6());
					GamePadLayouts.AddLayout(new Layout7());
					GamePadLayouts.AddLayout(new Layout8());
					GamePadLayouts.AddLayout(new Layout9());
					GamePadLayouts.AddLayout(new Layout10());
					GamePadLayouts.AddLayout(new Layout11());
					GamePadLayouts.AddLayout(new Layout12());
					GamePadLayouts.AddLayout(new Layout13());
					GamePadLayouts.AddLayout(new Layout14());
					GamePadLayouts.AddLayout(new Layout15());
					GamePadLayouts.AddLayout(new Layout16());
					GamePadLayouts.AddLayout(new Layout17());
					GamePadLayouts.AddLayout(new Layout18());
					GamePadLayouts.AddLayout(new Layout19());
					GamePadLayouts.AddLayout(new Layout20());
					GamePadLayouts.AddLayout(new Layout21());
					GamePadLayouts.AddLayout(new Layout22());
					GamePadLayouts.AddLayout(new Layout23());
					GamePadLayouts.AddLayout(new Layout24());
					GamePadLayouts.AddLayout(new Layout25());
					GamePadLayouts.AddLayout(new Layout26());
					GamePadLayouts.AddLayout(new Layout27());
					GamePadLayouts.AddLayout(new Layout28());
					GamePadLayouts.AddLayout(new Layout29());
					GamePadLayouts.AddLayout(new Layout30());
					GamePadLayouts.AddLayout(new Layout31());
					GamePadLayouts.AddLayout(new Layout32());
					GamePadLayouts.AddLayout(new Layout33());
					GamePadLayouts.AddLayout(new Layout34());
					GamePadLayouts.AddLayout(new Layout35());
					GamePadLayouts.AddLayout(new Layout36());
					GamePadLayouts.AddLayout(new Layout37());
					GamePadLayouts.AddLayout(new Layout38());
					GamePadLayouts.AddLayout(new Layout39());
					GamePadLayouts.AddLayout(new Layout40());
					GamePadLayouts.AddLayout(new Layout41());
		#endif
		#if SILICONSTUDIO_PLATFORM_MACOS
					GamePadLayouts.AddLayout(new Layout42());
					GamePadLayouts.AddLayout(new Layout43());
					GamePadLayouts.AddLayout(new Layout44());
					GamePadLayouts.AddLayout(new Layout45());
					GamePadLayouts.AddLayout(new Layout46());
					GamePadLayouts.AddLayout(new Layout47());
					GamePadLayouts.AddLayout(new Layout48());
					GamePadLayouts.AddLayout(new Layout49());
					GamePadLayouts.AddLayout(new Layout50());
					GamePadLayouts.AddLayout(new Layout51());
					GamePadLayouts.AddLayout(new Layout52());
					GamePadLayouts.AddLayout(new Layout53());
					GamePadLayouts.AddLayout(new Layout54());
					GamePadLayouts.AddLayout(new Layout55());
					GamePadLayouts.AddLayout(new Layout56());
					GamePadLayouts.AddLayout(new Layout57());
					GamePadLayouts.AddLayout(new Layout58());
					GamePadLayouts.AddLayout(new Layout59());
					GamePadLayouts.AddLayout(new Layout60());
					GamePadLayouts.AddLayout(new Layout61());
					GamePadLayouts.AddLayout(new Layout62());
					GamePadLayouts.AddLayout(new Layout63());
					GamePadLayouts.AddLayout(new Layout64());
					GamePadLayouts.AddLayout(new Layout65());
					GamePadLayouts.AddLayout(new Layout66());
					GamePadLayouts.AddLayout(new Layout67());
		#endif
		#if SILICONSTUDIO_PLATFORM_UNIX
					GamePadLayouts.AddLayout(new Layout68());
					GamePadLayouts.AddLayout(new Layout69());
					GamePadLayouts.AddLayout(new Layout70());
					GamePadLayouts.AddLayout(new Layout71());
					GamePadLayouts.AddLayout(new Layout72());
					GamePadLayouts.AddLayout(new Layout73());
					GamePadLayouts.AddLayout(new Layout74());
					GamePadLayouts.AddLayout(new Layout75());
					GamePadLayouts.AddLayout(new Layout76());
					GamePadLayouts.AddLayout(new Layout77());
					GamePadLayouts.AddLayout(new Layout78());
					GamePadLayouts.AddLayout(new Layout79());
					GamePadLayouts.AddLayout(new Layout80());
					GamePadLayouts.AddLayout(new Layout81());
					GamePadLayouts.AddLayout(new Layout82());
					GamePadLayouts.AddLayout(new Layout83());
					GamePadLayouts.AddLayout(new Layout84());
					GamePadLayouts.AddLayout(new Layout85());
					GamePadLayouts.AddLayout(new Layout86());
					GamePadLayouts.AddLayout(new Layout87());
					GamePadLayouts.AddLayout(new Layout88());
					GamePadLayouts.AddLayout(new Layout89());
					GamePadLayouts.AddLayout(new Layout90());
					GamePadLayouts.AddLayout(new Layout91());
					GamePadLayouts.AddLayout(new Layout92());
					GamePadLayouts.AddLayout(new Layout93());
					GamePadLayouts.AddLayout(new Layout94());
					GamePadLayouts.AddLayout(new Layout95());
					GamePadLayouts.AddLayout(new Layout96());
					GamePadLayouts.AddLayout(new Layout97());
					GamePadLayouts.AddLayout(new Layout98());
					GamePadLayouts.AddLayout(new Layout99());
					GamePadLayouts.AddLayout(new Layout100());
					GamePadLayouts.AddLayout(new Layout101());
					GamePadLayouts.AddLayout(new Layout102());
					GamePadLayouts.AddLayout(new Layout103());
					GamePadLayouts.AddLayout(new Layout104());
					GamePadLayouts.AddLayout(new Layout105());
					GamePadLayouts.AddLayout(new Layout106());
					GamePadLayouts.AddLayout(new Layout107());
					GamePadLayouts.AddLayout(new Layout108());
					GamePadLayouts.AddLayout(new Layout109());
					GamePadLayouts.AddLayout(new Layout110());
					GamePadLayouts.AddLayout(new Layout111());
					GamePadLayouts.AddLayout(new Layout112());
					GamePadLayouts.AddLayout(new Layout113());
					GamePadLayouts.AddLayout(new Layout114());
					GamePadLayouts.AddLayout(new Layout115());
					GamePadLayouts.AddLayout(new Layout116());
					GamePadLayouts.AddLayout(new Layout117());
					GamePadLayouts.AddLayout(new Layout118());
					GamePadLayouts.AddLayout(new Layout119());
					GamePadLayouts.AddLayout(new Layout120());
					GamePadLayouts.AddLayout(new Layout121());
					GamePadLayouts.AddLayout(new Layout122());
					GamePadLayouts.AddLayout(new Layout123());
					GamePadLayouts.AddLayout(new Layout124());
					GamePadLayouts.AddLayout(new Layout125());
					GamePadLayouts.AddLayout(new Layout126());
					GamePadLayouts.AddLayout(new Layout127());
					GamePadLayouts.AddLayout(new Layout128());
					GamePadLayouts.AddLayout(new Layout129());
					GamePadLayouts.AddLayout(new Layout130());
					GamePadLayouts.AddLayout(new Layout131());
					GamePadLayouts.AddLayout(new Layout132());
					GamePadLayouts.AddLayout(new Layout133());
					GamePadLayouts.AddLayout(new Layout134());
					GamePadLayouts.AddLayout(new Layout135());
					GamePadLayouts.AddLayout(new Layout136());
					GamePadLayouts.AddLayout(new Layout137());
					GamePadLayouts.AddLayout(new Layout138());
					GamePadLayouts.AddLayout(new Layout139());
					GamePadLayouts.AddLayout(new Layout140());
					GamePadLayouts.AddLayout(new Layout141());
					GamePadLayouts.AddLayout(new Layout142());
					GamePadLayouts.AddLayout(new Layout143());
					GamePadLayouts.AddLayout(new Layout144());
					GamePadLayouts.AddLayout(new Layout145());
					GamePadLayouts.AddLayout(new Layout146());
					GamePadLayouts.AddLayout(new Layout147());
		#endif
		
		}
	}
}