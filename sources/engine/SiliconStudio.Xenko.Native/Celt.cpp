// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

#include "../../../deps/NativePath/NativePath.h"
#include "../../../../opus/include/opus_custom.h"

extern "C" {

	class CeltDecoder
	{
	public:
		CeltDecoder(): mMode(NULL) {
		}

		bool Init()
		{
			mMode = opus_custom_mode_create(44100, 1024, NULL);
			if (mMode == NULL) return false;
			return true;
		}

	private:
		OpusCustomMode* mMode;
	};

	void* InitCeltDecoder()
	{
		auto decoder = new CeltDecoder();
		if(!decoder->Init())
		{
			delete decoder;
			return NULL;
		}
		return decoder;
	}

}
