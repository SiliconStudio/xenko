// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

#if defined(ANDROID) || defined(IOS) || !defined(__clang__)

#if !defined(__clang__)
#define size_t unsigned long //shutup a error on resharper
#endif

#if defined(IOS)
#define NP_STATIC_LINKING
#endif

#include "../../../../deps/NativePath/NativeDynamicLinking.h"
#include "../../../../deps/NativePath/NativePath.h"

#define GVR_NO_CPP_WRAPPER
#include "../../../../deps/GoogleVR/vr/gvr/capi/include/gvr_types.h"
#include "../../../../deps/GoogleVR/vr/gvr/capi/include/gvr.h"

extern "C" {

	void* gGvrLibrary = NULL;
	void* gGvrGLESv2 = NULL;
	gvr_context* gGvrContext = NULL;

#define GL_COLOR_WRITEMASK 0x0C23
#define GL_SCISSOR_TEST 0x0C11
#define GL_BLEND 0x0BE2
#define GL_CULL_FACE 0x0B44
#define GL_DEPTH_TEST 0x0B71
#define GL_BLEND_EQUATION_RGB 0x8009
#define GL_BLEND_EQUATION_ALPHA 0x883D
#define GL_BLEND_DST_RGB 0x80C8
#define GL_BLEND_SRC_RGB 0x80C9
#define GL_BLEND_DST_ALPHA 0x80CA
#define GL_BLEND_SRC_ALPHA 0x80CB

	typedef unsigned char GLboolean;
	typedef unsigned int GLenum;
	typedef unsigned int GLuint;
	typedef int GLint;

	NP_IMPORT(void, glColorMask, GLboolean red, GLboolean green, GLboolean blue, GLboolean alpha);
	NP_IMPORT(void, glDisable, GLenum cap);
	NP_IMPORT(void, glEnable, GLenum cap);
	NP_IMPORT(void, glGetBooleanv, GLenum pname, GLboolean* data);
	NP_IMPORT(void, glGetIntegerv, GLenum pname, GLint* data);
	NP_IMPORT(void, glBlendEquationSeparate, GLenum modeRGB, GLenum modeAlpha);
	NP_IMPORT(void, glBlendFuncSeparate, GLenum sfactorRGB, GLenum dfactorRGB, GLenum sfactorAlpha, GLenum dfactorAlpha);
	NP_IMPORT(void, gvr_initialize_gl, gvr_context* gvr);
	NP_IMPORT(int32_t, gvr_clear_error, gvr_context* gvr);
	NP_IMPORT(int32_t, gvr_get_error, gvr_context* gvr);
	NP_IMPORT(gvr_buffer_viewport_list*, gvr_buffer_viewport_list_create, const gvr_context* gvr);
	NP_IMPORT(void, gvr_get_recommended_buffer_viewports, const gvr_context* gvr, gvr_buffer_viewport_list* viewport_list);
	NP_IMPORT(void, gvr_buffer_viewport_list_get_item, const gvr_buffer_viewport_list* viewport_list, size_t index, gvr_buffer_viewport* viewport);
	NP_IMPORT(gvr_buffer_viewport*, gvr_buffer_viewport_create, gvr_context* gvr);
	NP_IMPORT(gvr_sizei, gvr_get_maximum_effective_render_target_size, const gvr_context* gvr);
	NP_IMPORT(gvr_buffer_spec*, gvr_buffer_spec_create, gvr_context* gvr);
	NP_IMPORT(void, gvr_buffer_spec_destroy, gvr_buffer_spec** spec);
	NP_IMPORT(void, gvr_buffer_spec_set_size, gvr_buffer_spec* spec, gvr_sizei size);
	NP_IMPORT(void, gvr_buffer_spec_set_samples, gvr_buffer_spec* spec, int32_t num_samples);
	NP_IMPORT(gvr_frame*, gvr_swap_chain_acquire_frame, gvr_swap_chain* swap_chain);
	NP_IMPORT(int32_t, gvr_frame_get_framebuffer_object, const gvr_frame* frame, int32_t index);
	NP_IMPORT(void, gvr_swap_chain_resize_buffer, gvr_swap_chain* swap_chain, int32_t index, gvr_sizei size);
	NP_IMPORT(void, gvr_frame_submit, gvr_frame** frame, const gvr_buffer_viewport_list* list, gvr_mat4f head_space_from_start_space);
	NP_IMPORT(gvr_mat4f, gvr_get_head_space_from_start_space_rotation ,const gvr_context* gvr, const gvr_clock_time_point time);
	NP_IMPORT(gvr_clock_time_point, gvr_get_time_point_now);
	NP_IMPORT(gvr_rectf, gvr_buffer_viewport_get_source_uv, const gvr_buffer_viewport* viewport);
	NP_IMPORT(void, gvr_refresh_viewer_profile, gvr_context* gvr);
	NP_IMPORT(void, gvr_frame_bind_buffer, gvr_frame* frame, int32_t index);
	NP_IMPORT(void, gvr_frame_unbind, gvr_frame* frame);
	NP_IMPORT(gvr_context*, gvr_create);
	NP_IMPORT(void, gvr_set_surface_size, gvr_context* gvr, gvr_sizei surface_size_pixels);
	NP_IMPORT(void, gvr_buffer_spec_set_color_format, gvr_buffer_spec* spec, int32_t color_format);
	NP_IMPORT(void, gvr_buffer_spec_set_depth_stencil_format, gvr_buffer_spec* spec, int32_t depth_stencil_format);
	NP_IMPORT(gvr_swap_chain*, gvr_swap_chain_create, gvr_context* gvr, const gvr_buffer_spec** buffers, int32_t count);
	NP_IMPORT(gvr_mat4f, gvr_get_eye_from_head_matrix, const gvr_context* gvr, const int32_t eye);

	gvr_buffer_viewport_list* xnGvr_ViewportsList = NULL;
	gvr_buffer_viewport* xnGvr_LeftVieport = NULL;
	gvr_buffer_viewport* xnGvr_RightVieport = NULL;

	gvr_swap_chain* xnGvr_swap_chain = NULL;

	gvr_sizei xnGvr_size;

	uint64_t kPredictionTimeWithoutVsyncNanos = 50000000;

	int xnGvrStartup(gvr_context* context, int* width, int* height)
	{
		if (!gGvrLibrary)
		{
#if defined(ANDROID)
			gGvrLibrary = LoadDynamicLibrary("libgvr");
			gGvrGLESv2 = LoadDynamicLibrary("libGLESv2");
#else
			gGvrLibrary = LoadDynamicLibrary(NULL);
			gGvrGLESv2 = LoadDynamicLibrary(NULL);
#endif

			if (!gGvrLibrary) return 1;

			NP_LOAD(gGvrGLESv2, glColorMask);
			NP_CHECK(glColorMask, return false);
			NP_LOAD(gGvrGLESv2, glDisable);
			NP_CHECK(glDisable, return false);
			NP_LOAD(gGvrGLESv2, glEnable);
			NP_CHECK(glEnable, return false);
			NP_LOAD(gGvrGLESv2, glGetBooleanv);
			NP_CHECK(glGetBooleanv, return false);
			NP_LOAD(gGvrGLESv2, glGetIntegerv);
			NP_CHECK(glGetIntegerv, return false);
			NP_LOAD(gGvrGLESv2, glBlendEquationSeparate);
			NP_CHECK(glBlendEquationSeparate, return false);
			NP_LOAD(gGvrGLESv2, glBlendFuncSeparate);
			NP_CHECK(glBlendFuncSeparate, return false);

			NP_LOAD(gGvrLibrary, gvr_refresh_viewer_profile);
			NP_CHECK(gvr_refresh_viewer_profile, return 2);
		}

		if(context)
		{
			gGvrContext = context;
		}
		else
		{
			NP_LOAD(gGvrLibrary, gvr_create);
			NP_CHECK(gvr_create, return 3);
			gGvrContext = NP_CALL(gvr_create);

			if (gGvrContext == NULL) return 4;
		}
		
		NP_LOAD(gGvrLibrary, gvr_get_maximum_effective_render_target_size);
		NP_CHECK(gvr_get_maximum_effective_render_target_size, return 5);

		NP_CALL(gvr_refresh_viewer_profile, gGvrContext);

		///       gvr_sizei render_target_size =
		///           gvr_get_maximum_effective_render_target_size(gvr);
		///       // The maximum effective render target size can be very large, most
		///       // applications need to scale down to compensate.
		///       render_target_size.width /= 2;
		///       render_target_size.height /= 2;

		xnGvr_size = NP_CALL(gvr_get_maximum_effective_render_target_size, gGvrContext);
		xnGvr_size.height = (xnGvr_size.height >> 1) - ((xnGvr_size.height >> 1) % 2);
		xnGvr_size.width = (xnGvr_size.width >> 1) - ((xnGvr_size.width >> 1) % 2);
		*width = xnGvr_size.width;
		*height = xnGvr_size.height;

		NP_LOAD(gGvrLibrary, gvr_set_surface_size);
		NP_CHECK(gvr_set_surface_size, return 6);
		NP_CALL(gvr_set_surface_size, gGvrContext, xnGvr_size);

		return 0;
	}

	npBool xnGvrInit()
	{
		NP_LOAD(gGvrLibrary, gvr_initialize_gl);
		NP_CHECK(gvr_initialize_gl, return false);
		NP_LOAD(gGvrLibrary, gvr_clear_error);
		NP_CHECK(gvr_clear_error, return false);
		NP_LOAD(gGvrLibrary, gvr_get_error);
		NP_CHECK(gvr_get_error, return false);
		NP_LOAD(gGvrLibrary, gvr_buffer_viewport_list_create);
		NP_CHECK(gvr_buffer_viewport_list_create, return false);
		NP_LOAD(gGvrLibrary, gvr_get_recommended_buffer_viewports);
		NP_CHECK(gvr_get_recommended_buffer_viewports, return false);
		NP_LOAD(gGvrLibrary, gvr_buffer_viewport_list_get_item);
		NP_CHECK(gvr_buffer_viewport_list_get_item, return false);
		NP_LOAD(gGvrLibrary, gvr_buffer_viewport_create);
		NP_CHECK(gvr_buffer_viewport_create, return false);
		NP_LOAD(gGvrLibrary, gvr_buffer_spec_create);
		NP_CHECK(gvr_buffer_spec_create, return false);
		NP_LOAD(gGvrLibrary, gvr_buffer_spec_destroy);
		NP_CHECK(gvr_buffer_spec_destroy, return false);
		NP_LOAD(gGvrLibrary, gvr_swap_chain_acquire_frame);
		NP_CHECK(gvr_swap_chain_acquire_frame, return false);
		NP_LOAD(gGvrLibrary, gvr_frame_get_framebuffer_object);
		NP_CHECK(gvr_frame_get_framebuffer_object, return false);
		NP_LOAD(gGvrLibrary, gvr_swap_chain_resize_buffer);
		NP_CHECK(gvr_swap_chain_resize_buffer, return false);
		NP_LOAD(gGvrLibrary, gvr_buffer_spec_set_color_format);
		NP_CHECK(gvr_buffer_spec_set_color_format, return false);
		NP_LOAD(gGvrLibrary, gvr_buffer_spec_set_depth_stencil_format);
		NP_CHECK(gvr_buffer_spec_set_depth_stencil_format, return false);
		NP_LOAD(gGvrLibrary, gvr_swap_chain_create);
		NP_CHECK(gvr_swap_chain_create, return false);
		NP_LOAD(gGvrLibrary, gvr_frame_submit);
		NP_CHECK(gvr_frame_submit, return false);
		NP_LOAD(gGvrLibrary, gvr_get_head_space_from_start_space_rotation);
		NP_CHECK(gvr_get_head_space_from_start_space_rotation, return false);
		NP_LOAD(gGvrLibrary, gvr_get_time_point_now);
		NP_CHECK(gvr_get_time_point_now, return false);
		NP_LOAD(gGvrLibrary, gvr_frame_bind_buffer);
		NP_CHECK(gvr_frame_bind_buffer, return false);
		NP_LOAD(gGvrLibrary, gvr_frame_unbind);
		NP_CHECK(gvr_frame_unbind, return false);
		NP_LOAD(gGvrLibrary, gvr_buffer_viewport_get_source_uv);
		NP_CHECK(gvr_buffer_viewport_get_source_uv, return false);
		NP_LOAD(gGvrLibrary, gvr_buffer_spec_set_size);
		NP_CHECK(gvr_buffer_spec_set_size, return false);
		NP_LOAD(gGvrLibrary, gvr_buffer_spec_set_samples);
		NP_CHECK(gvr_buffer_spec_set_samples, return false);
		NP_LOAD(gGvrLibrary, gvr_get_eye_from_head_matrix);
		NP_CHECK(gvr_get_eye_from_head_matrix, return false);

		NP_CALL(gvr_clear_error, gGvrContext);
		NP_CALL(gvr_initialize_gl, gGvrContext);
		if (NP_CALL(gvr_get_error, gGvrContext) != GVR_ERROR_NONE) return false;

		NP_CALL(gvr_clear_error, gGvrContext);
		xnGvr_ViewportsList = NP_CALL(gvr_buffer_viewport_list_create, gGvrContext);
		NP_CALL(gvr_get_recommended_buffer_viewports, gGvrContext, xnGvr_ViewportsList);
		if (NP_CALL(gvr_get_error, gGvrContext) != GVR_ERROR_NONE) return false;

		NP_CALL(gvr_clear_error, gGvrContext);
		xnGvr_LeftVieport = NP_CALL(gvr_buffer_viewport_create, gGvrContext);
		xnGvr_RightVieport = NP_CALL(gvr_buffer_viewport_create, gGvrContext);
		if (NP_CALL(gvr_get_error, gGvrContext) != GVR_ERROR_NONE) return false;

		NP_CALL(gvr_clear_error, gGvrContext);
		NP_CALL(gvr_buffer_viewport_list_get_item, xnGvr_ViewportsList, 0, xnGvr_LeftVieport);
		NP_CALL(gvr_buffer_viewport_list_get_item, xnGvr_ViewportsList, 1, xnGvr_RightVieport);
		if (NP_CALL(gvr_get_error, gGvrContext) != GVR_ERROR_NONE) return false;

		NP_CALL(gvr_clear_error, gGvrContext);
		auto bufferSpec = NP_CALL(gvr_buffer_spec_create, gGvrContext);
		if (NP_CALL(gvr_get_error, gGvrContext) != GVR_ERROR_NONE) return false;

		NP_CALL(gvr_clear_error, gGvrContext);
		NP_CALL(gvr_buffer_spec_set_size, bufferSpec, xnGvr_size);
		if (NP_CALL(gvr_get_error, gGvrContext) != GVR_ERROR_NONE) return false;

		NP_CALL(gvr_clear_error, gGvrContext);
		NP_CALL(gvr_buffer_spec_set_samples, bufferSpec, 1);
		if (NP_CALL(gvr_get_error, gGvrContext) != GVR_ERROR_NONE) return false;

		NP_CALL(gvr_clear_error, gGvrContext);
		NP_CALL(gvr_buffer_spec_set_color_format, bufferSpec, GVR_COLOR_FORMAT_RGBA_8888);
		if (NP_CALL(gvr_get_error, gGvrContext) != GVR_ERROR_NONE) return false;

		NP_CALL(gvr_clear_error, gGvrContext);
		NP_CALL(gvr_buffer_spec_set_depth_stencil_format, bufferSpec, GVR_DEPTH_STENCIL_FORMAT_NONE);
		if (NP_CALL(gvr_get_error, gGvrContext) != GVR_ERROR_NONE) return false;
	
		NP_CALL(gvr_clear_error, gGvrContext);
		const gvr_buffer_spec* specs[] = { bufferSpec };
		xnGvr_swap_chain = NP_CALL(gvr_swap_chain_create, gGvrContext, specs, 1);
		if (NP_CALL(gvr_get_error, gGvrContext) != GVR_ERROR_NONE || xnGvr_swap_chain == NULL) return false;

		NP_CALL(gvr_buffer_spec_destroy, &bufferSpec);

		NP_CALL(gvr_clear_error, gGvrContext);
		NP_CALL(gvr_swap_chain_resize_buffer, xnGvr_swap_chain, 0, xnGvr_size);
		if (NP_CALL(gvr_get_error, gGvrContext)) return false;		

		return true;
	}

	void xnGvrGetHeadMatrix(float* outMatrix)
	{
		auto time = NP_CALL(gvr_get_time_point_now);
		time.monotonic_system_time_nanos += kPredictionTimeWithoutVsyncNanos;
		auto gvrMat4 = reinterpret_cast<gvr_mat4f*>(outMatrix);
		*gvrMat4 = NP_CALL(gvr_get_head_space_from_start_space_rotation, gGvrContext, time);
	}

	void xnGvrGetEyeMatrix(int eyeIndex, float* outMatrix)
	{
		auto gvrMat4 = reinterpret_cast<gvr_mat4f*>(outMatrix);
		*gvrMat4 = NP_CALL(gvr_get_eye_from_head_matrix, gGvrContext, eyeIndex);
	}

	void* xnGvrGetNextFrame()
	{
		NP_CALL(gvr_clear_error, gGvrContext);
		auto frame = NP_CALL(gvr_swap_chain_acquire_frame, xnGvr_swap_chain);
		auto err = NP_CALL(gvr_get_error, gGvrContext);
		return err == GVR_ERROR_NONE ? frame : NULL;
	}

	void xnGvrBindBuffer(gvr_frame* frame, int index)
	{
		NP_CALL(gvr_frame_bind_buffer, frame, index);
	}

	void xnGvrUnbindBuffer(gvr_frame* frame)
	{
		NP_CALL(gvr_frame_unbind, frame);
	}

	npBool xnGvrSubmitFrame(gvr_frame* frame, float* headMatrix)
	{
		GLboolean masks[4];
		NP_CALL(glGetBooleanv, GL_COLOR_WRITEMASK, masks);
		NP_CALL(glColorMask, true, true, true, true); // This was the super major headache and it's needed

		GLboolean scissor;
		NP_CALL(glGetBooleanv, GL_SCISSOR_TEST, &scissor);

		GLboolean blend;
		NP_CALL(glGetBooleanv, GL_BLEND, &blend);

		GLboolean cullFace;
		NP_CALL(glGetBooleanv, GL_CULL_FACE, &cullFace);

		GLboolean depthTest;
		NP_CALL(glGetBooleanv, GL_DEPTH_TEST, &depthTest);

		GLint eqRgb;
		NP_CALL(glGetIntegerv, GL_BLEND_EQUATION_RGB, &eqRgb);
		GLint eqAlpha;
		NP_CALL(glGetIntegerv, GL_BLEND_EQUATION_ALPHA, &eqAlpha);

		GLint dstRgb;
		NP_CALL(glGetIntegerv, GL_BLEND_DST_RGB, &dstRgb);
		GLint dstAlpha;
		NP_CALL(glGetIntegerv, GL_BLEND_DST_ALPHA, &dstAlpha);
		GLint srcRgb;
		NP_CALL(glGetIntegerv, GL_BLEND_SRC_RGB, &srcRgb);
		GLint srcAlpha;
		NP_CALL(glGetIntegerv, GL_BLEND_SRC_ALPHA, &srcAlpha);

		NP_CALL(gvr_clear_error, gGvrContext);
		
		gvr_mat4f* gvrMat4 = reinterpret_cast<gvr_mat4f*>(headMatrix);
		NP_CALL(gvr_frame_submit, &frame, xnGvr_ViewportsList, *gvrMat4);
		auto err = NP_CALL(gvr_get_error, gGvrContext);

		NP_CALL(glColorMask, masks[0], masks[1], masks[2], masks[3]); // This was the super major headache and it's needed
		
		if (scissor)
		{
			NP_CALL(glEnable, GL_SCISSOR_TEST);
		}

		if(!blend)
		{
			NP_CALL(glDisable, GL_BLEND);
		}

		if(cullFace)
		{
			NP_CALL(glEnable, GL_CULL_FACE);
		}

		if(depthTest)
		{
			NP_CALL(glEnable, GL_DEPTH_TEST);
		}

		NP_CALL(glBlendEquationSeparate, eqRgb, eqAlpha);

		NP_CALL(glBlendFuncSeparate, srcRgb, dstRgb, srcAlpha, dstAlpha);

		return err == GVR_ERROR_NONE;
	}
}

#else

#endif