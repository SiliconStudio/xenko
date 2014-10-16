// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

#include "stdafx.h"

using namespace System;
using namespace System::Runtime::InteropServices;

namespace SiliconStudio { namespace GlslOptimizer {

public ref class GlslOptmizer
{
public:
	static String^ Run(String^ baseShader, bool openGLES, bool es30, bool vertex)
	{
		auto baseShaderPtr = Marshal::StringToHGlobalAnsi(baseShader);
		std::string baseShaderStr = std::string((char*)baseShaderPtr.ToPointer());

		glslopt_ctx* ctx;

		if (openGLES)
		{
			if (es30)
				ctx = glslopt_initialize(kGlslTargetOpenGLES30);
			else
				ctx = glslopt_initialize(kGlslTargetOpenGLES20);

			if (vertex)
			{
				std::string pre;
				pre += "#define gl_Vertex _glesVertex\nattribute highp vec4 _glesVertex;\n";
				pre += "#define gl_Normal _glesNormal\nattribute mediump vec3 _glesNormal;\n";
				pre += "#define gl_MultiTexCoord0 _glesMultiTexCoord0\nattribute highp vec4 _glesMultiTexCoord0;\n";
				pre += "#define gl_MultiTexCoord1 _glesMultiTexCoord1\nattribute highp vec4 _glesMultiTexCoord1;\n";
				pre += "#define gl_Color _glesColor\nattribute lowp vec4 _glesColor;\n";
				baseShaderStr = pre + baseShaderStr;
			}
		}
		else
		{
			ctx = glslopt_initialize(kGlslTargetOpenGL);
		}

		glslopt_shader_type type = vertex ? kGlslOptShaderVertex : kGlslOptShaderFragment;
		glslopt_shader* shader = glslopt_optimize(ctx, type, baseShaderStr.c_str(), 0);

		bool optimizeOk = glslopt_get_status(shader);

		if (optimizeOk)
		{
			std::string optShader = glslopt_get_output(shader);
			return gcnew String(optShader.c_str());
		}
		
		return gcnew String("");
	}
};

}}