/*
Copyright (c) 2015 Giovanni Petrantoni

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in
all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.  IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
THE SOFTWARE.
*/

//
//  NativePath.h
//  NativePath
//
//  Created by Giovanni Petrantoni on 11/16/15.
//  Copyright © 2015 Giovanni Petrantoni. All rights reserved.
//

#ifndef nativepath_h
#define nativepath_h

//from clang lib/Headers/stdint.h

#ifdef __INT64_TYPE__
typedef __INT64_TYPE__ int64_t;
typedef __UINT64_TYPE__ uint64_t;
#endif /* __INT64_TYPE__ */

#ifdef __INT32_TYPE__
typedef __INT32_TYPE__ int32_t;
typedef __UINT32_TYPE__ uint32_t;
#endif /* __INT32_TYPE__ */

#ifdef __INT16_TYPE__
typedef __INT16_TYPE__ int16_t;
typedef __UINT16_TYPE__ uint16_t;
#endif /* __INT16_TYPE__ */

#ifdef __INT8_TYPE__
typedef __INT8_TYPE__ int8_t;
typedef __UINT8_TYPE__ uint8_t;
#endif /* __INT8_TYPE__ */

//type safeguard, if type sizes are not what we expect it the compiler will throw error
static union
{
	char int_incorrect[sizeof(int) == 4 ? 1 : -1];
	char int64_incorrect[sizeof(int64_t) == 8 ? 1 : -1];
	char int32_incorrect[sizeof(int32_t) == 4 ? 1 : -1];
	char int16_incorrect[sizeof(int16_t) == 2 ? 1 : -1];
	char int8_incorrect[sizeof(int8_t) == 1 ? 1 : -1];
	char float_incorrect[sizeof(float) == 4 ? 1 : -1];
	char double_incorrect[sizeof(double) == 8 ? 1 : -1];
} __types_safeguard;

//Vectors

/*
 The following operators are supported on vectors:
 
 unary +, -
 ++, --
 +, -, *, /, %
 &, |, ^, ~
 >>, <<
 !, &&, ||
 ==, !=, >, <, >=, <=
 =
 
 // identity operation - return 4-element vector v1.
 __builtin_shufflevector(v1, v1, 0, 1, 2, 3)
 
 // "Splat" element 0 of v1 into a 4-element result.
 __builtin_shufflevector(v1, v1, 0, 0, 0, 0)
 
 // Reverse 4-element vector v1.
 __builtin_shufflevector(v1, v1, 3, 2, 1, 0)
 
 // Concatenate every other element of 4-element vectors v1 and v2.
 __builtin_shufflevector(v1, v2, 0, 2, 4, 6)
 
 // Concatenate every other element of 8-element vectors v1 and v2.
 __builtin_shufflevector(v1, v2, 0, 2, 4, 6, 8, 10, 12, 14)
 
 // Shuffle v1 with some elements being undefined
 __builtin_shufflevector(v1, v1, 3, -1, 1, -1)
 
 C-style casts can be used to convert one vector type to another without modifying the underlying bits. __builtin_convertvector can be used to convert from one type to another provided both types have the same number of elements, truncating when converting from floating-point to integer.
 */

//http://clang.llvm.org/docs/LanguageExtensions.html#vectors-and-extended-vectors
//https://developer.chrome.com/native-client/reference/pnacl-c-cpp-language-support

#define VECTOR_BYTES 16
#define VECTOR_ALIGN 4

typedef float float4 __attribute__((vector_size(VECTOR_BYTES), aligned(VECTOR_ALIGN)));
typedef int32_t int4 __attribute__((vector_size(VECTOR_BYTES), aligned(VECTOR_ALIGN)));
typedef uint32_t uint4 __attribute__((vector_size(VECTOR_BYTES), aligned(VECTOR_ALIGN)));

//CLANG usable builtins

#if !__has_builtin(__builtin_atan2)
	#error "atan2 clang built-in not available"
#else
	#define atan2 __builtin_atan2
#endif

#if !__has_builtin(__builtin_atan2f)
	#error "atan2f clang built-in not available"
#else
	#define atan2f __builtin_atan2f
#endif

#if !__has_builtin(__builtin_atan2l)
	#error "atan2l clang built-in not available"
#else
	#define atan2l __builtin_atan2l
#endif

#if !__has_builtin(__builtin_abs)
	#error "abs clang built-in not available"
#else
	#define abs __builtin_abs
#endif

#if !__has_builtin(__builtin_copysign)
	#error "copysign clang built-in not available"
#else
	#define copysign __builtin_copysign
#endif

#if !__has_builtin(__builtin_copysignf)
	#error "copysignf clang built-in not available"
#else
	#define copysignf __builtin_copysignf
#endif

#if !__has_builtin(__builtin_copysignl)
	#error "copysignl clang built-in not available"
#else
	#define copysignl __builtin_copysignl
#endif

#if !__has_builtin(__builtin_fabs)
	#error "fabs clang built-in not available"
#else
	#define fabs __builtin_fabs
#endif

#if !__has_builtin(__builtin_fabsf)
	#error "fabsf clang built-in not available"
#else
	#define fabsf __builtin_fabsf
#endif

#if !__has_builtin(__builtin_fabsl)
	#error "fabsl clang built-in not available"
#else
	#define fabsl __builtin_fabsl
#endif

#if !__has_builtin(__builtin_fmod)
	#error "fmod clang built-in not available"
#else
	#define fmod __builtin_fmod
#endif

#if !__has_builtin(__builtin_fmodf)
	#error "fmodf clang built-in not available"
#else
	#define fmodf __builtin_fmodf
#endif

#if !__has_builtin(__builtin_fmodl)
	#error "fmodl clang built-in not available"
#else
	#define fmodl __builtin_fmodl
#endif

#if !__has_builtin(__builtin_frexp)
	#error "frexp clang built-in not available"
#else
	#define frexp __builtin_frexp
#endif

#if !__has_builtin(__builtin_frexpf)
	#error "frexpf clang built-in not available"
#else
	#define frexpf __builtin_frexpf
#endif

#if !__has_builtin(__builtin_frexpl)
	#error "frexpl clang built-in not available"
#else
	#define frexpl __builtin_frexpl
#endif

#if !__has_builtin(__builtin_huge_val)
	#error "huge_val clang built-in not available"
#else
	#define huge_val __builtin_huge_val
#endif

#if !__has_builtin(__builtin_huge_valf)
	#error "huge_valf clang built-in not available"
#else
	#define huge_valf __builtin_huge_valf
#endif

#if !__has_builtin(__builtin_huge_vall)
	#error "huge_vall clang built-in not available"
#else
	#define huge_vall __builtin_huge_vall
#endif

#if !__has_builtin(__builtin_inf)
	#error "inf clang built-in not available"
#else
	#define inf __builtin_inf
#endif

#if !__has_builtin(__builtin_inff)
	#error "inff clang built-in not available"
#else
	#define inff __builtin_inff
#endif

#if !__has_builtin(__builtin_infl)
	#error "infl clang built-in not available"
#else
	#define infl __builtin_infl
#endif

#if !__has_builtin(__builtin_labs)
	#error "labs clang built-in not available"
#else
	#define labs __builtin_labs
#endif

#if !__has_builtin(__builtin_llabs)
	#error "llabs clang built-in not available"
#else
	#define llabs __builtin_llabs
#endif

#if !__has_builtin(__builtin_ldexp)
	#error "ldexp clang built-in not available"
#else
	#define ldexp __builtin_ldexp
#endif

#if !__has_builtin(__builtin_ldexpf)
	#error "ldexpf clang built-in not available"
#else
	#define ldexpf __builtin_ldexpf
#endif

#if !__has_builtin(__builtin_ldexpl)
	#error "ldexpl clang built-in not available"
#else
	#define ldexpl __builtin_ldexpl
#endif

#if !__has_builtin(__builtin_modf)
	#error "modf clang built-in not available"
#else
	#define modf __builtin_modf
#endif

#if !__has_builtin(__builtin_modff)
	#error "modff clang built-in not available"
#else
	#define modff __builtin_modff
#endif

#if !__has_builtin(__builtin_modfl)
	#error "modfl clang built-in not available"
#else
	#define modfl __builtin_modfl
#endif

#if !__has_builtin(__builtin_nan)
	#error "nan clang built-in not available"
#else
	#define nan __builtin_nan
#endif

#if !__has_builtin(__builtin_nanf)
	#error "nanf clang built-in not available"
#else
	#define nanf __builtin_nanf
#endif

#if !__has_builtin(__builtin_nanl)
	#error "nanl clang built-in not available"
#else
	#define nanl __builtin_nanl
#endif

#if !__has_builtin(__builtin_nans)
	#error "nans clang built-in not available"
#else
	#define nans __builtin_nans
#endif

#if !__has_builtin(__builtin_nansf)
	#error "nansf clang built-in not available"
#else
	#define nansf __builtin_nansf
#endif

#if !__has_builtin(__builtin_nansl)
	#error "nansl clang built-in not available"
#else
	#define nansl __builtin_nansl
#endif

#if !__has_builtin(__builtin_powi)
	#error "powi clang built-in not available"
#else
	#define powi __builtin_powi
#endif

#if !__has_builtin(__builtin_powif)
	#error "powif clang built-in not available"
#else
	#define powif __builtin_powif
#endif

#if !__has_builtin(__builtin_powil)
	#error "powil clang built-in not available"
#else
	#define powil __builtin_powil
#endif

#if !__has_builtin(__builtin_pow)
	#error "pow clang built-in not available"
#else
	#define pow __builtin_pow
#endif

#if !__has_builtin(__builtin_powf)
	#error "powf clang built-in not available"
#else
	#define powf __builtin_powf
#endif

#if !__has_builtin(__builtin_powl)
	#error "powl clang built-in not available"
#else
	#define powl __builtin_powl
#endif

#if !__has_builtin(__builtin_acos)
	#error "acos clang built-in not available"
#else
	#define acos __builtin_acos
#endif

#if !__has_builtin(__builtin_acosf)
	#error "acosf clang built-in not available"
#else
	#define acosf __builtin_acosf
#endif

#if !__has_builtin(__builtin_acosl)
	#error "acosl clang built-in not available"
#else
	#define acosl __builtin_acosl
#endif

#if !__has_builtin(__builtin_acosh)
	#error "acosh clang built-in not available"
#else
	#define acosh __builtin_acosh
#endif

#if !__has_builtin(__builtin_acoshf)
	#error "acoshf clang built-in not available"
#else
	#define acoshf __builtin_acoshf
#endif

#if !__has_builtin(__builtin_acoshl)
	#error "acoshl clang built-in not available"
#else
	#define acoshl __builtin_acoshl
#endif

#if !__has_builtin(__builtin_asin)
	#error "asin clang built-in not available"
#else
	#define asin __builtin_asin
#endif

#if !__has_builtin(__builtin_asinf)
	#error "asinf clang built-in not available"
#else
	#define asinf __builtin_asinf
#endif

#if !__has_builtin(__builtin_asinl)
	#error "asinl clang built-in not available"
#else
	#define asinl __builtin_asinl
#endif

#if !__has_builtin(__builtin_asinh)
	#error "asinh clang built-in not available"
#else
	#define asinh __builtin_asinh
#endif

#if !__has_builtin(__builtin_asinhf)
	#error "asinhf clang built-in not available"
#else
	#define asinhf __builtin_asinhf
#endif

#if !__has_builtin(__builtin_asinhl)
	#error "asinhl clang built-in not available"
#else
	#define asinhl __builtin_asinhl
#endif

#if !__has_builtin(__builtin_atan)
	#error "atan clang built-in not available"
#else
	#define atan __builtin_atan
#endif

#if !__has_builtin(__builtin_atanf)
	#error "atanf clang built-in not available"
#else
	#define atanf __builtin_atanf
#endif

#if !__has_builtin(__builtin_atanl)
	#error "atanl clang built-in not available"
#else
	#define atanl __builtin_atanl
#endif

#if !__has_builtin(__builtin_atanh)
	#error "atanh clang built-in not available"
#else
	#define atanh __builtin_atanh
#endif

#if !__has_builtin(__builtin_atanhf)
	#error "atanhf clang built-in not available"
#else
	#define atanhf __builtin_atanhf
#endif

#if !__has_builtin(__builtin_atanhl)
	#error "atanhl clang built-in not available"
#else
	#define atanhl __builtin_atanhl
#endif

#if !__has_builtin(__builtin_cbrt)
	#error "cbrt clang built-in not available"
#else
	#define cbrt __builtin_cbrt
#endif

#if !__has_builtin(__builtin_cbrtf)
	#error "cbrtf clang built-in not available"
#else
	#define cbrtf __builtin_cbrtf
#endif

#if !__has_builtin(__builtin_cbrtl)
	#error "cbrtl clang built-in not available"
#else
	#define cbrtl __builtin_cbrtl
#endif

#if !__has_builtin(__builtin_ceil)
	#error "ceil clang built-in not available"
#else
	#define ceil __builtin_ceil
#endif

#if !__has_builtin(__builtin_ceilf)
	#error "ceilf clang built-in not available"
#else
	#define ceilf __builtin_ceilf
#endif

#if !__has_builtin(__builtin_ceill)
	#error "ceill clang built-in not available"
#else
	#define ceill __builtin_ceill
#endif

#if !__has_builtin(__builtin_cos)
	#error "cos clang built-in not available"
#else
	#define cos __builtin_cos
#endif

#if !__has_builtin(__builtin_cosf)
	#error "cosf clang built-in not available"
#else
	#define cosf __builtin_cosf
#endif

#if !__has_builtin(__builtin_cosh)
	#error "cosh clang built-in not available"
#else
	#define cosh __builtin_cosh
#endif

#if !__has_builtin(__builtin_coshf)
	#error "coshf clang built-in not available"
#else
	#define coshf __builtin_coshf
#endif

#if !__has_builtin(__builtin_coshl)
	#error "coshl clang built-in not available"
#else
	#define coshl __builtin_coshl
#endif

#if !__has_builtin(__builtin_cosl)
	#error "cosl clang built-in not available"
#else
	#define cosl __builtin_cosl
#endif

#if !__has_builtin(__builtin_erf)
	#error "erf clang built-in not available"
#else
	#define erf __builtin_erf
#endif

#if !__has_builtin(__builtin_erff)
	#error "erff clang built-in not available"
#else
	#define erff __builtin_erff
#endif

#if !__has_builtin(__builtin_erfl)
	#error "erfl clang built-in not available"
#else
	#define erfl __builtin_erfl
#endif

#if !__has_builtin(__builtin_erfc)
	#error "erfc clang built-in not available"
#else
	#define erfc __builtin_erfc
#endif

#if !__has_builtin(__builtin_erfcf)
	#error "erfcf clang built-in not available"
#else
	#define erfcf __builtin_erfcf
#endif

#if !__has_builtin(__builtin_erfcl)
	#error "erfcl clang built-in not available"
#else
	#define erfcl __builtin_erfcl
#endif

#if !__has_builtin(__builtin_exp)
	#error "exp clang built-in not available"
#else
	#define exp __builtin_exp
#endif

#if !__has_builtin(__builtin_expf)
	#error "expf clang built-in not available"
#else
	#define expf __builtin_expf
#endif

#if !__has_builtin(__builtin_expl)
	#error "expl clang built-in not available"
#else
	#define expl __builtin_expl
#endif

#if !__has_builtin(__builtin_exp2)
	#error "exp2 clang built-in not available"
#else
	#define exp2 __builtin_exp2
#endif

#if !__has_builtin(__builtin_exp2f)
	#error "exp2f clang built-in not available"
#else
	#define exp2f __builtin_exp2f
#endif

#if !__has_builtin(__builtin_exp2l)
	#error "exp2l clang built-in not available"
#else
	#define exp2l __builtin_exp2l
#endif

#if !__has_builtin(__builtin_expm1)
	#error "expm1 clang built-in not available"
#else
	#define expm1 __builtin_expm1
#endif

#if !__has_builtin(__builtin_expm1f)
	#error "expm1f clang built-in not available"
#else
	#define expm1f __builtin_expm1f
#endif

#if !__has_builtin(__builtin_expm1l)
	#error "expm1l clang built-in not available"
#else
	#define expm1l __builtin_expm1l
#endif

#if !__has_builtin(__builtin_fdim)
	#error "fdim clang built-in not available"
#else
	#define fdim __builtin_fdim
#endif

#if !__has_builtin(__builtin_fdimf)
	#error "fdimf clang built-in not available"
#else
	#define fdimf __builtin_fdimf
#endif

#if !__has_builtin(__builtin_fdiml)
	#error "fdiml clang built-in not available"
#else
	#define fdiml __builtin_fdiml
#endif

#if !__has_builtin(__builtin_floor)
	#error "floor clang built-in not available"
#else
	#define floor __builtin_floor
#endif

#if !__has_builtin(__builtin_floorf)
	#error "floorf clang built-in not available"
#else
	#define floorf __builtin_floorf
#endif

#if !__has_builtin(__builtin_floorl)
	#error "floorl clang built-in not available"
#else
	#define floorl __builtin_floorl
#endif

#if !__has_builtin(__builtin_fma)
	#error "fma clang built-in not available"
#else
	#define fma __builtin_fma
#endif

#if !__has_builtin(__builtin_fmaf)
	#error "fmaf clang built-in not available"
#else
	#define fmaf __builtin_fmaf
#endif

#if !__has_builtin(__builtin_fmal)
	#error "fmal clang built-in not available"
#else
	#define fmal __builtin_fmal
#endif

#if !__has_builtin(__builtin_fmax)
	#error "fmax clang built-in not available"
#else
	#define fmax __builtin_fmax
#endif

#if !__has_builtin(__builtin_fmaxf)
	#error "fmaxf clang built-in not available"
#else
	#define fmaxf __builtin_fmaxf
#endif

#if !__has_builtin(__builtin_fmaxl)
	#error "fmaxl clang built-in not available"
#else
	#define fmaxl __builtin_fmaxl
#endif

#if !__has_builtin(__builtin_fmin)
	#error "fmin clang built-in not available"
#else
	#define fmin __builtin_fmin
#endif

#if !__has_builtin(__builtin_fminf)
	#error "fminf clang built-in not available"
#else
	#define fminf __builtin_fminf
#endif

#if !__has_builtin(__builtin_fminl)
	#error "fminl clang built-in not available"
#else
	#define fminl __builtin_fminl
#endif

#if !__has_builtin(__builtin_hypot)
	#error "hypot clang built-in not available"
#else
	#define hypot __builtin_hypot
#endif

#if !__has_builtin(__builtin_hypotf)
	#error "hypotf clang built-in not available"
#else
	#define hypotf __builtin_hypotf
#endif

#if !__has_builtin(__builtin_hypotl)
	#error "hypotl clang built-in not available"
#else
	#define hypotl __builtin_hypotl
#endif

#if !__has_builtin(__builtin_ilogb)
	#error "ilogb clang built-in not available"
#else
	#define ilogb __builtin_ilogb
#endif

#if !__has_builtin(__builtin_ilogbf)
	#error "ilogbf clang built-in not available"
#else
	#define ilogbf __builtin_ilogbf
#endif

#if !__has_builtin(__builtin_ilogbl)
	#error "ilogbl clang built-in not available"
#else
	#define ilogbl __builtin_ilogbl
#endif

#if !__has_builtin(__builtin_lgamma)
	#error "lgamma clang built-in not available"
#else
	#define lgamma __builtin_lgamma
#endif

#if !__has_builtin(__builtin_lgammaf)
	#error "lgammaf clang built-in not available"
#else
	#define lgammaf __builtin_lgammaf
#endif

#if !__has_builtin(__builtin_lgammal)
	#error "lgammal clang built-in not available"
#else
	#define lgammal __builtin_lgammal
#endif

#if !__has_builtin(__builtin_llrint)
	#error "llrint clang built-in not available"
#else
	#define llrint __builtin_llrint
#endif

#if !__has_builtin(__builtin_llrintf)
	#error "llrintf clang built-in not available"
#else
	#define llrintf __builtin_llrintf
#endif

#if !__has_builtin(__builtin_llrintl)
	#error "llrintl clang built-in not available"
#else
	#define llrintl __builtin_llrintl
#endif

#if !__has_builtin(__builtin_llround)
	#error "llround clang built-in not available"
#else
	#define llround __builtin_llround
#endif

#if !__has_builtin(__builtin_llroundf)
	#error "llroundf clang built-in not available"
#else
	#define llroundf __builtin_llroundf
#endif

#if !__has_builtin(__builtin_llroundl)
	#error "llroundl clang built-in not available"
#else
	#define llroundl __builtin_llroundl
#endif

#if !__has_builtin(__builtin_log)
	#error "log clang built-in not available"
#else
	#define log __builtin_log
#endif

#if !__has_builtin(__builtin_log10)
	#error "log10 clang built-in not available"
#else
	#define log10 __builtin_log10
#endif

#if !__has_builtin(__builtin_log10f)
	#error "log10f clang built-in not available"
#else
	#define log10f __builtin_log10f
#endif

#if !__has_builtin(__builtin_log10l)
	#error "log10l clang built-in not available"
#else
	#define log10l __builtin_log10l
#endif

#if !__has_builtin(__builtin_log1p)
	#error "log1p clang built-in not available"
#else
	#define log1p __builtin_log1p
#endif

#if !__has_builtin(__builtin_log1pf)
	#error "log1pf clang built-in not available"
#else
	#define log1pf __builtin_log1pf
#endif

#if !__has_builtin(__builtin_log1pl)
	#error "log1pl clang built-in not available"
#else
	#define log1pl __builtin_log1pl
#endif

#if !__has_builtin(__builtin_log2)
	#error "log2 clang built-in not available"
#else
	#define log2 __builtin_log2
#endif

#if !__has_builtin(__builtin_log2f)
	#error "log2f clang built-in not available"
#else
	#define log2f __builtin_log2f
#endif

#if !__has_builtin(__builtin_log2l)
	#error "log2l clang built-in not available"
#else
	#define log2l __builtin_log2l
#endif

#if !__has_builtin(__builtin_logb)
	#error "logb clang built-in not available"
#else
	#define logb __builtin_logb
#endif

#if !__has_builtin(__builtin_logbf)
	#error "logbf clang built-in not available"
#else
	#define logbf __builtin_logbf
#endif

#if !__has_builtin(__builtin_logbl)
	#error "logbl clang built-in not available"
#else
	#define logbl __builtin_logbl
#endif

#if !__has_builtin(__builtin_logf)
	#error "logf clang built-in not available"
#else
	#define logf __builtin_logf
#endif

#if !__has_builtin(__builtin_logl)
	#error "logl clang built-in not available"
#else
	#define logl __builtin_logl
#endif

#if !__has_builtin(__builtin_lrint)
	#error "lrint clang built-in not available"
#else
	#define lrint __builtin_lrint
#endif

#if !__has_builtin(__builtin_lrintf)
	#error "lrintf clang built-in not available"
#else
	#define lrintf __builtin_lrintf
#endif

#if !__has_builtin(__builtin_lrintl)
	#error "lrintl clang built-in not available"
#else
	#define lrintl __builtin_lrintl
#endif

#if !__has_builtin(__builtin_lround)
	#error "lround clang built-in not available"
#else
	#define lround __builtin_lround
#endif

#if !__has_builtin(__builtin_lroundf)
	#error "lroundf clang built-in not available"
#else
	#define lroundf __builtin_lroundf
#endif

#if !__has_builtin(__builtin_lroundl)
	#error "lroundl clang built-in not available"
#else
	#define lroundl __builtin_lroundl
#endif

#if !__has_builtin(__builtin_nearbyint)
	#error "nearbyint clang built-in not available"
#else
	#define nearbyint __builtin_nearbyint
#endif

#if !__has_builtin(__builtin_nearbyintf)
	#error "nearbyintf clang built-in not available"
#else
	#define nearbyintf __builtin_nearbyintf
#endif

#if !__has_builtin(__builtin_nearbyintl)
	#error "nearbyintl clang built-in not available"
#else
	#define nearbyintl __builtin_nearbyintl
#endif

#if !__has_builtin(__builtin_nextafter)
	#error "nextafter clang built-in not available"
#else
	#define nextafter __builtin_nextafter
#endif

#if !__has_builtin(__builtin_nextafterf)
	#error "nextafterf clang built-in not available"
#else
	#define nextafterf __builtin_nextafterf
#endif

#if !__has_builtin(__builtin_nextafterl)
	#error "nextafterl clang built-in not available"
#else
	#define nextafterl __builtin_nextafterl
#endif

#if !__has_builtin(__builtin_nexttoward)
	#error "nexttoward clang built-in not available"
#else
	#define nexttoward __builtin_nexttoward
#endif

#if !__has_builtin(__builtin_nexttowardf)
	#error "nexttowardf clang built-in not available"
#else
	#define nexttowardf __builtin_nexttowardf
#endif

#if !__has_builtin(__builtin_nexttowardl)
	#error "nexttowardl clang built-in not available"
#else
	#define nexttowardl __builtin_nexttowardl
#endif

#if !__has_builtin(__builtin_remainder)
	#error "remainder clang built-in not available"
#else
	#define remainder __builtin_remainder
#endif

#if !__has_builtin(__builtin_remainderf)
	#error "remainderf clang built-in not available"
#else
	#define remainderf __builtin_remainderf
#endif

#if !__has_builtin(__builtin_remainderl)
	#error "remainderl clang built-in not available"
#else
	#define remainderl __builtin_remainderl
#endif

#if !__has_builtin(__builtin_remquo)
	#error "remquo clang built-in not available"
#else
	#define remquo __builtin_remquo
#endif

#if !__has_builtin(__builtin_remquof)
	#error "remquof clang built-in not available"
#else
	#define remquof __builtin_remquof
#endif

#if !__has_builtin(__builtin_remquol)
	#error "remquol clang built-in not available"
#else
	#define remquol __builtin_remquol
#endif

#if !__has_builtin(__builtin_rint)
	#error "rint clang built-in not available"
#else
	#define rint __builtin_rint
#endif

#if !__has_builtin(__builtin_rintf)
	#error "rintf clang built-in not available"
#else
	#define rintf __builtin_rintf
#endif

#if !__has_builtin(__builtin_rintl)
	#error "rintl clang built-in not available"
#else
	#define rintl __builtin_rintl
#endif

#if !__has_builtin(__builtin_round)
	#error "round clang built-in not available"
#else
	#define round __builtin_round
#endif

#if !__has_builtin(__builtin_roundf)
	#error "roundf clang built-in not available"
#else
	#define roundf __builtin_roundf
#endif

#if !__has_builtin(__builtin_roundl)
	#error "roundl clang built-in not available"
#else
	#define roundl __builtin_roundl
#endif

#if !__has_builtin(__builtin_scalbln)
	#error "scalbln clang built-in not available"
#else
	#define scalbln __builtin_scalbln
#endif

#if !__has_builtin(__builtin_scalblnf)
	#error "scalblnf clang built-in not available"
#else
	#define scalblnf __builtin_scalblnf
#endif

#if !__has_builtin(__builtin_scalblnl)
	#error "scalblnl clang built-in not available"
#else
	#define scalblnl __builtin_scalblnl
#endif

#if !__has_builtin(__builtin_scalbn)
	#error "scalbn clang built-in not available"
#else
	#define scalbn __builtin_scalbn
#endif

#if !__has_builtin(__builtin_scalbnf)
	#error "scalbnf clang built-in not available"
#else
	#define scalbnf __builtin_scalbnf
#endif

#if !__has_builtin(__builtin_scalbnl)
	#error "scalbnl clang built-in not available"
#else
	#define scalbnl __builtin_scalbnl
#endif

#if !__has_builtin(__builtin_sin)
	#error "sin clang built-in not available"
#else
	#define sin __builtin_sin
#endif

#if !__has_builtin(__builtin_sinf)
	#error "sinf clang built-in not available"
#else
	#define sinf __builtin_sinf
#endif

#if !__has_builtin(__builtin_sinh)
	#error "sinh clang built-in not available"
#else
	#define sinh __builtin_sinh
#endif

#if !__has_builtin(__builtin_sinhf)
	#error "sinhf clang built-in not available"
#else
	#define sinhf __builtin_sinhf
#endif

#if !__has_builtin(__builtin_sinhl)
	#error "sinhl clang built-in not available"
#else
	#define sinhl __builtin_sinhl
#endif

#if !__has_builtin(__builtin_sinl)
	#error "sinl clang built-in not available"
#else
	#define sinl __builtin_sinl
#endif

#if !__has_builtin(__builtin_sqrt)
	#error "sqrt clang built-in not available"
#else
	#define sqrt __builtin_sqrt
#endif

#if !__has_builtin(__builtin_sqrtf)
	#error "sqrtf clang built-in not available"
#else
	#define sqrtf __builtin_sqrtf
#endif

#if !__has_builtin(__builtin_sqrtl)
	#error "sqrtl clang built-in not available"
#else
	#define sqrtl __builtin_sqrtl
#endif

#if !__has_builtin(__builtin_tan)
	#error "tan clang built-in not available"
#else
	#define tan __builtin_tan
#endif

#if !__has_builtin(__builtin_tanf)
	#error "tanf clang built-in not available"
#else
	#define tanf __builtin_tanf
#endif

#if !__has_builtin(__builtin_tanh)
	#error "tanh clang built-in not available"
#else
	#define tanh __builtin_tanh
#endif

#if !__has_builtin(__builtin_tanhf)
	#error "tanhf clang built-in not available"
#else
	#define tanhf __builtin_tanhf
#endif

#if !__has_builtin(__builtin_tanhl)
	#error "tanhl clang built-in not available"
#else
	#define tanhl __builtin_tanhl
#endif

#if !__has_builtin(__builtin_tanl)
	#error "tanl clang built-in not available"
#else
	#define tanl __builtin_tanl
#endif

#if !__has_builtin(__builtin_tgamma)
	#error "tgamma clang built-in not available"
#else
	#define tgamma __builtin_tgamma
#endif

#if !__has_builtin(__builtin_tgammaf)
	#error "tgammaf clang built-in not available"
#else
	#define tgammaf __builtin_tgammaf
#endif

#if !__has_builtin(__builtin_tgammal)
	#error "tgammal clang built-in not available"
#else
	#define tgammal __builtin_tgammal
#endif

#if !__has_builtin(__builtin_trunc)
	#error "trunc clang built-in not available"
#else
	#define trunc __builtin_trunc
#endif

#if !__has_builtin(__builtin_truncf)
	#error "truncf clang built-in not available"
#else
	#define truncf __builtin_truncf
#endif

#if !__has_builtin(__builtin_truncl)
	#error "truncl clang built-in not available"
#else
	#define truncl __builtin_truncl
#endif

#if !__has_builtin(__builtin_cabs)
	#error "cabs clang built-in not available"
#else
	#define cabs __builtin_cabs
#endif

#if !__has_builtin(__builtin_cabsf)
	#error "cabsf clang built-in not available"
#else
	#define cabsf __builtin_cabsf
#endif

#if !__has_builtin(__builtin_cabsl)
	#error "cabsl clang built-in not available"
#else
	#define cabsl __builtin_cabsl
#endif

#if !__has_builtin(__builtin_cacos)
	#error "cacos clang built-in not available"
#else
	#define cacos __builtin_cacos
#endif

#if !__has_builtin(__builtin_cacosf)
	#error "cacosf clang built-in not available"
#else
	#define cacosf __builtin_cacosf
#endif

#if !__has_builtin(__builtin_cacosh)
	#error "cacosh clang built-in not available"
#else
	#define cacosh __builtin_cacosh
#endif

#if !__has_builtin(__builtin_cacoshf)
	#error "cacoshf clang built-in not available"
#else
	#define cacoshf __builtin_cacoshf
#endif

#if !__has_builtin(__builtin_cacoshl)
	#error "cacoshl clang built-in not available"
#else
	#define cacoshl __builtin_cacoshl
#endif

#if !__has_builtin(__builtin_cacosl)
	#error "cacosl clang built-in not available"
#else
	#define cacosl __builtin_cacosl
#endif

#if !__has_builtin(__builtin_carg)
	#error "carg clang built-in not available"
#else
	#define carg __builtin_carg
#endif

#if !__has_builtin(__builtin_cargf)
	#error "cargf clang built-in not available"
#else
	#define cargf __builtin_cargf
#endif

#if !__has_builtin(__builtin_cargl)
	#error "cargl clang built-in not available"
#else
	#define cargl __builtin_cargl
#endif

#if !__has_builtin(__builtin_casin)
	#error "casin clang built-in not available"
#else
	#define casin __builtin_casin
#endif

#if !__has_builtin(__builtin_casinf)
	#error "casinf clang built-in not available"
#else
	#define casinf __builtin_casinf
#endif

#if !__has_builtin(__builtin_casinh)
	#error "casinh clang built-in not available"
#else
	#define casinh __builtin_casinh
#endif

#if !__has_builtin(__builtin_casinhf)
	#error "casinhf clang built-in not available"
#else
	#define casinhf __builtin_casinhf
#endif

#if !__has_builtin(__builtin_casinhl)
	#error "casinhl clang built-in not available"
#else
	#define casinhl __builtin_casinhl
#endif

#if !__has_builtin(__builtin_casinl)
	#error "casinl clang built-in not available"
#else
	#define casinl __builtin_casinl
#endif

#if !__has_builtin(__builtin_catan)
	#error "catan clang built-in not available"
#else
	#define catan __builtin_catan
#endif

#if !__has_builtin(__builtin_catanf)
	#error "catanf clang built-in not available"
#else
	#define catanf __builtin_catanf
#endif

#if !__has_builtin(__builtin_catanh)
	#error "catanh clang built-in not available"
#else
	#define catanh __builtin_catanh
#endif

#if !__has_builtin(__builtin_catanhf)
	#error "catanhf clang built-in not available"
#else
	#define catanhf __builtin_catanhf
#endif

#if !__has_builtin(__builtin_catanhl)
	#error "catanhl clang built-in not available"
#else
	#define catanhl __builtin_catanhl
#endif

#if !__has_builtin(__builtin_catanl)
	#error "catanl clang built-in not available"
#else
	#define catanl __builtin_catanl
#endif

#if !__has_builtin(__builtin_ccos)
	#error "ccos clang built-in not available"
#else
	#define ccos __builtin_ccos
#endif

#if !__has_builtin(__builtin_ccosf)
	#error "ccosf clang built-in not available"
#else
	#define ccosf __builtin_ccosf
#endif

#if !__has_builtin(__builtin_ccosl)
	#error "ccosl clang built-in not available"
#else
	#define ccosl __builtin_ccosl
#endif

#if !__has_builtin(__builtin_ccosh)
	#error "ccosh clang built-in not available"
#else
	#define ccosh __builtin_ccosh
#endif

#if !__has_builtin(__builtin_ccoshf)
	#error "ccoshf clang built-in not available"
#else
	#define ccoshf __builtin_ccoshf
#endif

#if !__has_builtin(__builtin_ccoshl)
	#error "ccoshl clang built-in not available"
#else
	#define ccoshl __builtin_ccoshl
#endif

#if !__has_builtin(__builtin_cexp)
	#error "cexp clang built-in not available"
#else
	#define cexp __builtin_cexp
#endif

#if !__has_builtin(__builtin_cexpf)
	#error "cexpf clang built-in not available"
#else
	#define cexpf __builtin_cexpf
#endif

#if !__has_builtin(__builtin_cexpl)
	#error "cexpl clang built-in not available"
#else
	#define cexpl __builtin_cexpl
#endif

#if !__has_builtin(__builtin_cimag)
	#error "cimag clang built-in not available"
#else
	#define cimag __builtin_cimag
#endif

#if !__has_builtin(__builtin_cimagf)
	#error "cimagf clang built-in not available"
#else
	#define cimagf __builtin_cimagf
#endif

#if !__has_builtin(__builtin_cimagl)
	#error "cimagl clang built-in not available"
#else
	#define cimagl __builtin_cimagl
#endif

#if !__has_builtin(__builtin_conj)
	#error "conj clang built-in not available"
#else
	#define conj __builtin_conj
#endif

#if !__has_builtin(__builtin_conjf)
	#error "conjf clang built-in not available"
#else
	#define conjf __builtin_conjf
#endif

#if !__has_builtin(__builtin_conjl)
	#error "conjl clang built-in not available"
#else
	#define conjl __builtin_conjl
#endif

#if !__has_builtin(__builtin_clog)
	#error "clog clang built-in not available"
#else
	#define clog __builtin_clog
#endif

#if !__has_builtin(__builtin_clogf)
	#error "clogf clang built-in not available"
#else
	#define clogf __builtin_clogf
#endif

#if !__has_builtin(__builtin_clogl)
	#error "clogl clang built-in not available"
#else
	#define clogl __builtin_clogl
#endif

#if !__has_builtin(__builtin_cproj)
	#error "cproj clang built-in not available"
#else
	#define cproj __builtin_cproj
#endif

#if !__has_builtin(__builtin_cprojf)
	#error "cprojf clang built-in not available"
#else
	#define cprojf __builtin_cprojf
#endif

#if !__has_builtin(__builtin_cprojl)
	#error "cprojl clang built-in not available"
#else
	#define cprojl __builtin_cprojl
#endif

#if !__has_builtin(__builtin_cpow)
	#error "cpow clang built-in not available"
#else
	#define cpow __builtin_cpow
#endif

#if !__has_builtin(__builtin_cpowf)
	#error "cpowf clang built-in not available"
#else
	#define cpowf __builtin_cpowf
#endif

#if !__has_builtin(__builtin_cpowl)
	#error "cpowl clang built-in not available"
#else
	#define cpowl __builtin_cpowl
#endif

#if !__has_builtin(__builtin_creal)
	#error "creal clang built-in not available"
#else
	#define creal __builtin_creal
#endif

#if !__has_builtin(__builtin_crealf)
	#error "crealf clang built-in not available"
#else
	#define crealf __builtin_crealf
#endif

#if !__has_builtin(__builtin_creall)
	#error "creall clang built-in not available"
#else
	#define creall __builtin_creall
#endif

#if !__has_builtin(__builtin_csin)
	#error "csin clang built-in not available"
#else
	#define csin __builtin_csin
#endif

#if !__has_builtin(__builtin_csinf)
	#error "csinf clang built-in not available"
#else
	#define csinf __builtin_csinf
#endif

#if !__has_builtin(__builtin_csinl)
	#error "csinl clang built-in not available"
#else
	#define csinl __builtin_csinl
#endif

#if !__has_builtin(__builtin_csinh)
	#error "csinh clang built-in not available"
#else
	#define csinh __builtin_csinh
#endif

#if !__has_builtin(__builtin_csinhf)
	#error "csinhf clang built-in not available"
#else
	#define csinhf __builtin_csinhf
#endif

#if !__has_builtin(__builtin_csinhl)
	#error "csinhl clang built-in not available"
#else
	#define csinhl __builtin_csinhl
#endif

#if !__has_builtin(__builtin_csqrt)
	#error "csqrt clang built-in not available"
#else
	#define csqrt __builtin_csqrt
#endif

#if !__has_builtin(__builtin_csqrtf)
	#error "csqrtf clang built-in not available"
#else
	#define csqrtf __builtin_csqrtf
#endif

#if !__has_builtin(__builtin_csqrtl)
	#error "csqrtl clang built-in not available"
#else
	#define csqrtl __builtin_csqrtl
#endif

#if !__has_builtin(__builtin_ctan)
	#error "ctan clang built-in not available"
#else
	#define ctan __builtin_ctan
#endif

#if !__has_builtin(__builtin_ctanf)
	#error "ctanf clang built-in not available"
#else
	#define ctanf __builtin_ctanf
#endif

#if !__has_builtin(__builtin_ctanl)
	#error "ctanl clang built-in not available"
#else
	#define ctanl __builtin_ctanl
#endif

#if !__has_builtin(__builtin_ctanh)
	#error "ctanh clang built-in not available"
#else
	#define ctanh __builtin_ctanh
#endif

#if !__has_builtin(__builtin_ctanhf)
	#error "ctanhf clang built-in not available"
#else
	#define ctanhf __builtin_ctanhf
#endif

#if !__has_builtin(__builtin_ctanhl)
	#error "ctanhl clang built-in not available"
#else
	#define ctanhl __builtin_ctanhl
#endif

#if !__has_builtin(__builtin_isgreater)
	#error "isgreater clang built-in not available"
#else
	#define isgreater __builtin_isgreater
#endif

#if !__has_builtin(__builtin_isgreaterequal)
	#error "isgreaterequal clang built-in not available"
#else
	#define isgreaterequal __builtin_isgreaterequal
#endif

#if !__has_builtin(__builtin_isless)
	#error "isless clang built-in not available"
#else
	#define isless __builtin_isless
#endif

#if !__has_builtin(__builtin_islessequal)
	#error "islessequal clang built-in not available"
#else
	#define islessequal __builtin_islessequal
#endif

#if !__has_builtin(__builtin_islessgreater)
	#error "islessgreater clang built-in not available"
#else
	#define islessgreater __builtin_islessgreater
#endif

#if !__has_builtin(__builtin_isunordered)
	#error "isunordered clang built-in not available"
#else
	#define isunordered __builtin_isunordered
#endif

#if !__has_builtin(__builtin_fpclassify)
	#error "fpclassify clang built-in not available"
#else
	#define fpclassify __builtin_fpclassify
#endif

#if !__has_builtin(__builtin_isfinite)
	#error "isfinite clang built-in not available"
#else
	#define isfinite __builtin_isfinite
#endif

#if !__has_builtin(__builtin_isinf)
	#error "isinf clang built-in not available"
#else
	#define isinf __builtin_isinf
#endif

#if !__has_builtin(__builtin_isinf_sign)
	#error "isinf_sign clang built-in not available"
#else
	#define isinf_sign __builtin_isinf_sign
#endif

#if !__has_builtin(__builtin_isnan)
	#error "isnan clang built-in not available"
#else
	#define isnan __builtin_isnan
#endif

#if !__has_builtin(__builtin_isnormal)
	#error "isnormal clang built-in not available"
#else
	#define isnormal __builtin_isnormal
#endif

#if !__has_builtin(__builtin_signbit)
	#error "signbit clang built-in not available"
#else
	#define signbit __builtin_signbit
#endif

#if !__has_builtin(__builtin_signbitf)
	#error "signbitf clang built-in not available"
#else
	#define signbitf __builtin_signbitf
#endif

#if !__has_builtin(__builtin_signbitl)
	#error "signbitl clang built-in not available"
#else
	#define signbitl __builtin_signbitl
#endif

#if !__has_builtin(__builtin_clzs)
	#error "clzs clang built-in not available"
#else
	#define clzs __builtin_clzs
#endif

#if !__has_builtin(__builtin_clz)
	#error "clz clang built-in not available"
#else
	#define clz __builtin_clz
#endif

#if !__has_builtin(__builtin_clzl)
	#error "clzl clang built-in not available"
#else
	#define clzl __builtin_clzl
#endif

#if !__has_builtin(__builtin_clzll)
	#error "clzll clang built-in not available"
#else
	#define clzll __builtin_clzll
#endif

#if !__has_builtin(__builtin_ctzs)
	#error "ctzs clang built-in not available"
#else
	#define ctzs __builtin_ctzs
#endif

#if !__has_builtin(__builtin_ctz)
	#error "ctz clang built-in not available"
#else
	#define ctz __builtin_ctz
#endif

#if !__has_builtin(__builtin_ctzl)
	#error "ctzl clang built-in not available"
#else
	#define ctzl __builtin_ctzl
#endif

#if !__has_builtin(__builtin_ctzll)
	#error "ctzll clang built-in not available"
#else
	#define ctzll __builtin_ctzll
#endif

#if !__has_builtin(__builtin_ffs)
	#error "ffs clang built-in not available"
#else
	#define ffs __builtin_ffs
#endif

#if !__has_builtin(__builtin_ffsl)
	#error "ffsl clang built-in not available"
#else
	#define ffsl __builtin_ffsl
#endif

#if !__has_builtin(__builtin_ffsll)
	#error "ffsll clang built-in not available"
#else
	#define ffsll __builtin_ffsll
#endif

#if !__has_builtin(__builtin_parity)
	#error "parity clang built-in not available"
#else
	#define parity __builtin_parity
#endif

#if !__has_builtin(__builtin_parityl)
	#error "parityl clang built-in not available"
#else
	#define parityl __builtin_parityl
#endif

#if !__has_builtin(__builtin_parityll)
	#error "parityll clang built-in not available"
#else
	#define parityll __builtin_parityll
#endif

#if !__has_builtin(__builtin_popcount)
	#error "popcount clang built-in not available"
#else
	#define popcount __builtin_popcount
#endif

#if !__has_builtin(__builtin_popcountl)
	#error "popcountl clang built-in not available"
#else
	#define popcountl __builtin_popcountl
#endif

#if !__has_builtin(__builtin_popcountll)
	#error "popcountll clang built-in not available"
#else
	#define popcountll __builtin_popcountll
#endif

#if !__has_builtin(__builtin_bswap16)
	#error "bswap16 clang built-in not available"
#else
	#define bswap16 __builtin_bswap16
#endif

#if !__has_builtin(__builtin_bswap32)
	#error "bswap32 clang built-in not available"
#else
	#define bswap32 __builtin_bswap32
#endif

#if !__has_builtin(__builtin_bswap64)
	#error "bswap64 clang built-in not available"
#else
	#define bswap64 __builtin_bswap64
#endif

#if !__has_builtin(__builtin_constant_p)
	#error "constant_p clang built-in not available"
#else
	#define constant_p __builtin_constant_p
#endif

#if !__has_builtin(__builtin_classify_type)
	#error "classify_type clang built-in not available"
#else
	#define classify_type __builtin_classify_type
#endif

#if !__has_builtin(__builtin___CFStringMakeConstantString)
	#error "__CFStringMakeConstantString clang built-in not available"
#else
	#define __CFStringMakeConstantString __builtin___CFStringMakeConstantString
#endif

#if !__has_builtin(__builtin___NSStringMakeConstantString)
	#error "__NSStringMakeConstantString clang built-in not available"
#else
	#define __NSStringMakeConstantString __builtin___NSStringMakeConstantString
#endif

#if !__has_builtin(__builtin_va_start)
	#error "va_start clang built-in not available"
#else
	#define va_start __builtin_va_start
#endif

#if !__has_builtin(__builtin_va_end)
	#error "va_end clang built-in not available"
#else
	#define va_end __builtin_va_end
#endif

#if !__has_builtin(__builtin_va_copy)
	#error "va_copy clang built-in not available"
#else
	#define va_copy __builtin_va_copy
#endif

#if !__has_builtin(__builtin_stdarg_start)
	#error "stdarg_start clang built-in not available"
#else
	#define stdarg_start __builtin_stdarg_start
#endif

#if !__has_builtin(__builtin_assume_aligned)
	#error "assume_aligned clang built-in not available"
#else
	#define assume_aligned __builtin_assume_aligned
#endif

#if !__has_builtin(__builtin_bcmp)
	#error "bcmp clang built-in not available"
#else
	#define bcmp __builtin_bcmp
#endif

#if !__has_builtin(__builtin_bcopy)
	#error "bcopy clang built-in not available"
#else
	#define bcopy __builtin_bcopy
#endif

#if !__has_builtin(__builtin_bzero)
	#error "bzero clang built-in not available"
#else
	#define bzero __builtin_bzero
#endif

#if !__has_builtin(__builtin_fprintf)
	#error "fprintf clang built-in not available"
#else
	#define fprintf __builtin_fprintf
#endif

#if !__has_builtin(__builtin_memchr)
	#error "memchr clang built-in not available"
#else
	#define memchr __builtin_memchr
#endif

#if !__has_builtin(__builtin_memcmp)
	#error "memcmp clang built-in not available"
#else
	#define memcmp __builtin_memcmp
#endif

#if !__has_builtin(__builtin_memcpy)
	#error "memcpy clang built-in not available"
#else
	#define memcpy __builtin_memcpy
#endif

#if !__has_builtin(__builtin_memmove)
	#error "memmove clang built-in not available"
#else
	#define memmove __builtin_memmove
#endif

#if !__has_builtin(__builtin_mempcpy)
	#error "mempcpy clang built-in not available"
#else
	#define mempcpy __builtin_mempcpy
#endif

#if !__has_builtin(__builtin_memset)
	#error "memset clang built-in not available"
#else
	#define memset __builtin_memset
#endif

#if !__has_builtin(__builtin_printf)
	#error "printf clang built-in not available"
#else
	#define printf __builtin_printf
#endif

#if !__has_builtin(__builtin_stpcpy)
	#error "stpcpy clang built-in not available"
#else
	#define stpcpy __builtin_stpcpy
#endif

#if !__has_builtin(__builtin_stpncpy)
	#error "stpncpy clang built-in not available"
#else
	#define stpncpy __builtin_stpncpy
#endif

#if !__has_builtin(__builtin_strcasecmp)
	#error "strcasecmp clang built-in not available"
#else
	#define strcasecmp __builtin_strcasecmp
#endif

#if !__has_builtin(__builtin_strcat)
	#error "strcat clang built-in not available"
#else
	#define strcat __builtin_strcat
#endif

#if !__has_builtin(__builtin_strchr)
	#error "strchr clang built-in not available"
#else
	#define strchr __builtin_strchr
#endif

#if !__has_builtin(__builtin_strcmp)
	#error "strcmp clang built-in not available"
#else
	#define strcmp __builtin_strcmp
#endif

#if !__has_builtin(__builtin_strcpy)
	#error "strcpy clang built-in not available"
#else
	#define strcpy __builtin_strcpy
#endif

#if !__has_builtin(__builtin_strcspn)
	#error "strcspn clang built-in not available"
#else
	#define strcspn __builtin_strcspn
#endif

#if !__has_builtin(__builtin_strdup)
	#error "strdup clang built-in not available"
#else
	#define strdup __builtin_strdup
#endif

#if !__has_builtin(__builtin_strlen)
	#error "strlen clang built-in not available"
#else
	#define strlen __builtin_strlen
#endif

#if !__has_builtin(__builtin_strncasecmp)
	#error "strncasecmp clang built-in not available"
#else
	#define strncasecmp __builtin_strncasecmp
#endif

#if !__has_builtin(__builtin_strncat)
	#error "strncat clang built-in not available"
#else
	#define strncat __builtin_strncat
#endif

#if !__has_builtin(__builtin_strncmp)
	#error "strncmp clang built-in not available"
#else
	#define strncmp __builtin_strncmp
#endif

#if !__has_builtin(__builtin_strncpy)
	#error "strncpy clang built-in not available"
#else
	#define strncpy __builtin_strncpy
#endif

#if !__has_builtin(__builtin_strndup)
	#error "strndup clang built-in not available"
#else
	#define strndup __builtin_strndup
#endif

#if !__has_builtin(__builtin_strpbrk)
	#error "strpbrk clang built-in not available"
#else
	#define strpbrk __builtin_strpbrk
#endif

#if !__has_builtin(__builtin_strrchr)
	#error "strrchr clang built-in not available"
#else
	#define strrchr __builtin_strrchr
#endif

#if !__has_builtin(__builtin_strspn)
	#error "strspn clang built-in not available"
#else
	#define strspn __builtin_strspn
#endif

#if !__has_builtin(__builtin_strstr)
	#error "strstr clang built-in not available"
#else
	#define strstr __builtin_strstr
#endif

#if !__has_builtin(__builtin_return_address)
	#error "return_address clang built-in not available"
#else
	#define return_address __builtin_return_address
#endif

#if !__has_builtin(__builtin_extract_return_addr)
	#error "extract_return_addr clang built-in not available"
#else
	#define extract_return_addr __builtin_extract_return_addr
#endif

#if !__has_builtin(__builtin_frame_address)
	#error "frame_address clang built-in not available"
#else
	#define frame_address __builtin_frame_address
#endif

#if !__has_builtin(__builtin___clear_cache)
	#error "__clear_cache clang built-in not available"
#else
	#define __clear_cache __builtin___clear_cache
#endif

#if !__has_builtin(__builtin_flt_rounds)
	#error "flt_rounds clang built-in not available"
#else
	#define flt_rounds __builtin_flt_rounds
#endif

#if !__has_builtin(__builtin_setjmp)
	#error "setjmp clang built-in not available"
#else
	#define setjmp __builtin_setjmp
#endif

#if !__has_builtin(__builtin_longjmp)
	#error "longjmp clang built-in not available"
#else
	#define longjmp __builtin_longjmp
#endif

#if !__has_builtin(__builtin_eh_return_data_regno)
	#error "eh_return_data_regno clang built-in not available"
#else
	#define eh_return_data_regno __builtin_eh_return_data_regno
#endif

#if !__has_builtin(__builtin_snprintf)
	#error "snprintf clang built-in not available"
#else
	#define snprintf __builtin_snprintf
#endif

#if !__has_builtin(__builtin_vsprintf)
	#error "vsprintf clang built-in not available"
#else
	#define vsprintf __builtin_vsprintf
#endif

#if !__has_builtin(__builtin_vsnprintf)
	#error "vsnprintf clang built-in not available"
#else
	#define vsnprintf __builtin_vsnprintf
#endif

#if !__has_builtin(__builtin_eh_return)
	#error "eh_return clang built-in not available"
#else
	#define eh_return __builtin_eh_return
#endif

#if !__has_builtin(__builtin_frob_return_addr)
	#error "frob_return_addr clang built-in not available"
#else
	#define frob_return_addr __builtin_frob_return_addr
#endif

#if !__has_builtin(__builtin_dwarf_cfa)
	#error "dwarf_cfa clang built-in not available"
#else
	#define dwarf_cfa __builtin_dwarf_cfa
#endif

#if !__has_builtin(__builtin_init_dwarf_reg_size_table)
	#error "init_dwarf_reg_size_table clang built-in not available"
#else
	#define init_dwarf_reg_size_table __builtin_init_dwarf_reg_size_table
#endif

#if !__has_builtin(__builtin_dwarf_sp_column)
	#error "dwarf_sp_column clang built-in not available"
#else
	#define dwarf_sp_column __builtin_dwarf_sp_column
#endif

#if !__has_builtin(__builtin_extend_pointer)
	#error "extend_pointer clang built-in not available"
#else
	#define extend_pointer __builtin_extend_pointer
#endif

#if !__has_builtin(__builtin_object_size)
	#error "object_size clang built-in not available"
#else
	#define object_size __builtin_object_size
#endif

#if !__has_builtin(__builtin___memcpy_chk)
	#error "__memcpy_chk clang built-in not available"
#else
	#define __memcpy_chk __builtin___memcpy_chk
#endif

#if !__has_builtin(__builtin___memccpy_chk)
	#error "__memccpy_chk clang built-in not available"
#else
	#define __memccpy_chk __builtin___memccpy_chk
#endif

#if !__has_builtin(__builtin___memmove_chk)
	#error "__memmove_chk clang built-in not available"
#else
	#define __memmove_chk __builtin___memmove_chk
#endif

#if !__has_builtin(__builtin___mempcpy_chk)
	#error "__mempcpy_chk clang built-in not available"
#else
	#define __mempcpy_chk __builtin___mempcpy_chk
#endif

#if !__has_builtin(__builtin___memset_chk)
	#error "__memset_chk clang built-in not available"
#else
	#define __memset_chk __builtin___memset_chk
#endif

#if !__has_builtin(__builtin___stpcpy_chk)
	#error "__stpcpy_chk clang built-in not available"
#else
	#define __stpcpy_chk __builtin___stpcpy_chk
#endif

#if !__has_builtin(__builtin___strcat_chk)
	#error "__strcat_chk clang built-in not available"
#else
	#define __strcat_chk __builtin___strcat_chk
#endif

#if !__has_builtin(__builtin___strcpy_chk)
	#error "__strcpy_chk clang built-in not available"
#else
	#define __strcpy_chk __builtin___strcpy_chk
#endif

#if !__has_builtin(__builtin___strlcat_chk)
	#error "__strlcat_chk clang built-in not available"
#else
	#define __strlcat_chk __builtin___strlcat_chk
#endif

#if !__has_builtin(__builtin___strlcpy_chk)
	#error "__strlcpy_chk clang built-in not available"
#else
	#define __strlcpy_chk __builtin___strlcpy_chk
#endif

#if !__has_builtin(__builtin___strncat_chk)
	#error "__strncat_chk clang built-in not available"
#else
	#define __strncat_chk __builtin___strncat_chk
#endif

#if !__has_builtin(__builtin___strncpy_chk)
	#error "__strncpy_chk clang built-in not available"
#else
	#define __strncpy_chk __builtin___strncpy_chk
#endif

#if !__has_builtin(__builtin___stpncpy_chk)
	#error "__stpncpy_chk clang built-in not available"
#else
	#define __stpncpy_chk __builtin___stpncpy_chk
#endif

#if !__has_builtin(__builtin___snprintf_chk)
	#error "__snprintf_chk clang built-in not available"
#else
	#define __snprintf_chk __builtin___snprintf_chk
#endif

#if !__has_builtin(__builtin___sprintf_chk)
	#error "__sprintf_chk clang built-in not available"
#else
	#define __sprintf_chk __builtin___sprintf_chk
#endif

#if !__has_builtin(__builtin___vsnprintf_chk)
	#error "__vsnprintf_chk clang built-in not available"
#else
	#define __vsnprintf_chk __builtin___vsnprintf_chk
#endif

#if !__has_builtin(__builtin___vsprintf_chk)
	#error "__vsprintf_chk clang built-in not available"
#else
	#define __vsprintf_chk __builtin___vsprintf_chk
#endif

#if !__has_builtin(__builtin___fprintf_chk)
	#error "__fprintf_chk clang built-in not available"
#else
	#define __fprintf_chk __builtin___fprintf_chk
#endif

#if !__has_builtin(__builtin___printf_chk)
	#error "__printf_chk clang built-in not available"
#else
	#define __printf_chk __builtin___printf_chk
#endif

#if !__has_builtin(__builtin___vfprintf_chk)
	#error "__vfprintf_chk clang built-in not available"
#else
	#define __vfprintf_chk __builtin___vfprintf_chk
#endif

#if !__has_builtin(__builtin___vprintf_chk)
	#error "__vprintf_chk clang built-in not available"
#else
	#define __vprintf_chk __builtin___vprintf_chk
#endif

#if !__has_builtin(__builtin_expect)
	#error "expect clang built-in not available"
#else
	#define expect __builtin_expect
#endif

#if !__has_builtin(__builtin_prefetch)
	#error "prefetch clang built-in not available"
#else
	#define prefetch __builtin_prefetch
#endif

#if !__has_builtin(__builtin_readcyclecounter)
	#error "readcyclecounter clang built-in not available"
#else
	#define readcyclecounter __builtin_readcyclecounter
#endif

#if !__has_builtin(__builtin_trap)
	#error "trap clang built-in not available"
#else
	#define trap __builtin_trap
#endif

#if !__has_builtin(__builtin_debugtrap)
	#error "debugtrap clang built-in not available"
#else
	#define debugtrap __builtin_debugtrap
#endif

#if !__has_builtin(__builtin_unreachable)
	#error "unreachable clang built-in not available"
#else
	#define unreachable __builtin_unreachable
#endif

#if !__has_builtin(__builtin_shufflevector)
	#error "shufflevector clang built-in not available"
#else
	#define shufflevector __builtin_shufflevector
#endif

#if !__has_builtin(__builtin_convertvector)
	#error "convertvector clang built-in not available"
#else
	#define convertvector __builtin_convertvector
#endif

#if !__has_builtin(__builtin_alloca)
	#error "alloca clang built-in not available"
#else
	#define alloca __builtin_alloca
#endif

#if !__has_builtin(__builtin_call_with_static_chain)
	#error "call_with_static_chain clang built-in not available"
#else
	#define call_with_static_chain __builtin_call_with_static_chain
#endif

#if !__has_builtin(__sync_fetch_and_add)
	#error "__sync_fetch_and_add clang built-in not available"
#endif

#if !__has_builtin(__sync_fetch_and_add_1)
	#error "__sync_fetch_and_add_1 clang built-in not available"
#endif

#if !__has_builtin(__sync_fetch_and_add_2)
	#error "__sync_fetch_and_add_2 clang built-in not available"
#endif

#if !__has_builtin(__sync_fetch_and_add_4)
	#error "__sync_fetch_and_add_4 clang built-in not available"
#endif

#if !__has_builtin(__sync_fetch_and_add_8)
	#error "__sync_fetch_and_add_8 clang built-in not available"
#endif

#if !__has_builtin(__sync_fetch_and_add_16)
	#error "__sync_fetch_and_add_16 clang built-in not available"
#endif

#if !__has_builtin(__sync_fetch_and_sub)
	#error "__sync_fetch_and_sub clang built-in not available"
#endif

#if !__has_builtin(__sync_fetch_and_sub_1)
	#error "__sync_fetch_and_sub_1 clang built-in not available"
#endif

#if !__has_builtin(__sync_fetch_and_sub_2)
	#error "__sync_fetch_and_sub_2 clang built-in not available"
#endif

#if !__has_builtin(__sync_fetch_and_sub_4)
	#error "__sync_fetch_and_sub_4 clang built-in not available"
#endif

#if !__has_builtin(__sync_fetch_and_sub_8)
	#error "__sync_fetch_and_sub_8 clang built-in not available"
#endif

#if !__has_builtin(__sync_fetch_and_sub_16)
	#error "__sync_fetch_and_sub_16 clang built-in not available"
#endif

#if !__has_builtin(__sync_fetch_and_or)
	#error "__sync_fetch_and_or clang built-in not available"
#endif

#if !__has_builtin(__sync_fetch_and_or_1)
	#error "__sync_fetch_and_or_1 clang built-in not available"
#endif

#if !__has_builtin(__sync_fetch_and_or_2)
	#error "__sync_fetch_and_or_2 clang built-in not available"
#endif

#if !__has_builtin(__sync_fetch_and_or_4)
	#error "__sync_fetch_and_or_4 clang built-in not available"
#endif

#if !__has_builtin(__sync_fetch_and_or_8)
	#error "__sync_fetch_and_or_8 clang built-in not available"
#endif

#if !__has_builtin(__sync_fetch_and_or_16)
	#error "__sync_fetch_and_or_16 clang built-in not available"
#endif

#if !__has_builtin(__sync_fetch_and_and)
	#error "__sync_fetch_and_and clang built-in not available"
#endif

#if !__has_builtin(__sync_fetch_and_and_1)
	#error "__sync_fetch_and_and_1 clang built-in not available"
#endif

#if !__has_builtin(__sync_fetch_and_and_2)
	#error "__sync_fetch_and_and_2 clang built-in not available"
#endif

#if !__has_builtin(__sync_fetch_and_and_4)
	#error "__sync_fetch_and_and_4 clang built-in not available"
#endif

#if !__has_builtin(__sync_fetch_and_and_8)
	#error "__sync_fetch_and_and_8 clang built-in not available"
#endif

#if !__has_builtin(__sync_fetch_and_and_16)
	#error "__sync_fetch_and_and_16 clang built-in not available"
#endif

#if !__has_builtin(__sync_fetch_and_xor)
	#error "__sync_fetch_and_xor clang built-in not available"
#endif

#if !__has_builtin(__sync_fetch_and_xor_1)
	#error "__sync_fetch_and_xor_1 clang built-in not available"
#endif

#if !__has_builtin(__sync_fetch_and_xor_2)
	#error "__sync_fetch_and_xor_2 clang built-in not available"
#endif

#if !__has_builtin(__sync_fetch_and_xor_4)
	#error "__sync_fetch_and_xor_4 clang built-in not available"
#endif

#if !__has_builtin(__sync_fetch_and_xor_8)
	#error "__sync_fetch_and_xor_8 clang built-in not available"
#endif

#if !__has_builtin(__sync_fetch_and_xor_16)
	#error "__sync_fetch_and_xor_16 clang built-in not available"
#endif

#if !__has_builtin(__sync_fetch_and_nand)
	#error "__sync_fetch_and_nand clang built-in not available"
#endif

#if !__has_builtin(__sync_fetch_and_nand_1)
	#error "__sync_fetch_and_nand_1 clang built-in not available"
#endif

#if !__has_builtin(__sync_fetch_and_nand_2)
	#error "__sync_fetch_and_nand_2 clang built-in not available"
#endif

#if !__has_builtin(__sync_fetch_and_nand_4)
	#error "__sync_fetch_and_nand_4 clang built-in not available"
#endif

#if !__has_builtin(__sync_fetch_and_nand_8)
	#error "__sync_fetch_and_nand_8 clang built-in not available"
#endif

#if !__has_builtin(__sync_fetch_and_nand_16)
	#error "__sync_fetch_and_nand_16 clang built-in not available"
#endif

#if !__has_builtin(__sync_add_and_fetch)
	#error "__sync_add_and_fetch clang built-in not available"
#endif

#if !__has_builtin(__sync_add_and_fetch_1)
	#error "__sync_add_and_fetch_1 clang built-in not available"
#endif

#if !__has_builtin(__sync_add_and_fetch_2)
	#error "__sync_add_and_fetch_2 clang built-in not available"
#endif

#if !__has_builtin(__sync_add_and_fetch_4)
	#error "__sync_add_and_fetch_4 clang built-in not available"
#endif

#if !__has_builtin(__sync_add_and_fetch_8)
	#error "__sync_add_and_fetch_8 clang built-in not available"
#endif

#if !__has_builtin(__sync_add_and_fetch_16)
	#error "__sync_add_and_fetch_16 clang built-in not available"
#endif

#if !__has_builtin(__sync_sub_and_fetch)
	#error "__sync_sub_and_fetch clang built-in not available"
#endif

#if !__has_builtin(__sync_sub_and_fetch_1)
	#error "__sync_sub_and_fetch_1 clang built-in not available"
#endif

#if !__has_builtin(__sync_sub_and_fetch_2)
	#error "__sync_sub_and_fetch_2 clang built-in not available"
#endif

#if !__has_builtin(__sync_sub_and_fetch_4)
	#error "__sync_sub_and_fetch_4 clang built-in not available"
#endif

#if !__has_builtin(__sync_sub_and_fetch_8)
	#error "__sync_sub_and_fetch_8 clang built-in not available"
#endif

#if !__has_builtin(__sync_sub_and_fetch_16)
	#error "__sync_sub_and_fetch_16 clang built-in not available"
#endif

#if !__has_builtin(__sync_or_and_fetch)
	#error "__sync_or_and_fetch clang built-in not available"
#endif

#if !__has_builtin(__sync_or_and_fetch_1)
	#error "__sync_or_and_fetch_1 clang built-in not available"
#endif

#if !__has_builtin(__sync_or_and_fetch_2)
	#error "__sync_or_and_fetch_2 clang built-in not available"
#endif

#if !__has_builtin(__sync_or_and_fetch_4)
	#error "__sync_or_and_fetch_4 clang built-in not available"
#endif

#if !__has_builtin(__sync_or_and_fetch_8)
	#error "__sync_or_and_fetch_8 clang built-in not available"
#endif

#if !__has_builtin(__sync_or_and_fetch_16)
	#error "__sync_or_and_fetch_16 clang built-in not available"
#endif

#if !__has_builtin(__sync_and_and_fetch)
	#error "__sync_and_and_fetch clang built-in not available"
#endif

#if !__has_builtin(__sync_and_and_fetch_1)
	#error "__sync_and_and_fetch_1 clang built-in not available"
#endif

#if !__has_builtin(__sync_and_and_fetch_2)
	#error "__sync_and_and_fetch_2 clang built-in not available"
#endif

#if !__has_builtin(__sync_and_and_fetch_4)
	#error "__sync_and_and_fetch_4 clang built-in not available"
#endif

#if !__has_builtin(__sync_and_and_fetch_8)
	#error "__sync_and_and_fetch_8 clang built-in not available"
#endif

#if !__has_builtin(__sync_and_and_fetch_16)
	#error "__sync_and_and_fetch_16 clang built-in not available"
#endif

#if !__has_builtin(__sync_xor_and_fetch)
	#error "__sync_xor_and_fetch clang built-in not available"
#endif

#if !__has_builtin(__sync_xor_and_fetch_1)
	#error "__sync_xor_and_fetch_1 clang built-in not available"
#endif

#if !__has_builtin(__sync_xor_and_fetch_2)
	#error "__sync_xor_and_fetch_2 clang built-in not available"
#endif

#if !__has_builtin(__sync_xor_and_fetch_4)
	#error "__sync_xor_and_fetch_4 clang built-in not available"
#endif

#if !__has_builtin(__sync_xor_and_fetch_8)
	#error "__sync_xor_and_fetch_8 clang built-in not available"
#endif

#if !__has_builtin(__sync_xor_and_fetch_16)
	#error "__sync_xor_and_fetch_16 clang built-in not available"
#endif

#if !__has_builtin(__sync_nand_and_fetch)
	#error "__sync_nand_and_fetch clang built-in not available"
#endif

#if !__has_builtin(__sync_nand_and_fetch_1)
	#error "__sync_nand_and_fetch_1 clang built-in not available"
#endif

#if !__has_builtin(__sync_nand_and_fetch_2)
	#error "__sync_nand_and_fetch_2 clang built-in not available"
#endif

#if !__has_builtin(__sync_nand_and_fetch_4)
	#error "__sync_nand_and_fetch_4 clang built-in not available"
#endif

#if !__has_builtin(__sync_nand_and_fetch_8)
	#error "__sync_nand_and_fetch_8 clang built-in not available"
#endif

#if !__has_builtin(__sync_nand_and_fetch_16)
	#error "__sync_nand_and_fetch_16 clang built-in not available"
#endif

#if !__has_builtin(__sync_bool_compare_and_swap)
	#error "__sync_bool_compare_and_swap clang built-in not available"
#endif

#if !__has_builtin(__sync_bool_compare_and_swap_1)
	#error "__sync_bool_compare_and_swap_1 clang built-in not available"
#endif

#if !__has_builtin(__sync_bool_compare_and_swap_2)
	#error "__sync_bool_compare_and_swap_2 clang built-in not available"
#endif

#if !__has_builtin(__sync_bool_compare_and_swap_4)
	#error "__sync_bool_compare_and_swap_4 clang built-in not available"
#endif

#if !__has_builtin(__sync_bool_compare_and_swap_8)
	#error "__sync_bool_compare_and_swap_8 clang built-in not available"
#endif

#if !__has_builtin(__sync_bool_compare_and_swap_16)
	#error "__sync_bool_compare_and_swap_16 clang built-in not available"
#endif

#if !__has_builtin(__sync_val_compare_and_swap)
	#error "__sync_val_compare_and_swap clang built-in not available"
#endif

#if !__has_builtin(__sync_val_compare_and_swap_1)
	#error "__sync_val_compare_and_swap_1 clang built-in not available"
#endif

#if !__has_builtin(__sync_val_compare_and_swap_2)
	#error "__sync_val_compare_and_swap_2 clang built-in not available"
#endif

#if !__has_builtin(__sync_val_compare_and_swap_4)
	#error "__sync_val_compare_and_swap_4 clang built-in not available"
#endif

#if !__has_builtin(__sync_val_compare_and_swap_8)
	#error "__sync_val_compare_and_swap_8 clang built-in not available"
#endif

#if !__has_builtin(__sync_val_compare_and_swap_16)
	#error "__sync_val_compare_and_swap_16 clang built-in not available"
#endif

#if !__has_builtin(__sync_lock_test_and_set)
	#error "__sync_lock_test_and_set clang built-in not available"
#endif

#if !__has_builtin(__sync_lock_test_and_set_1)
	#error "__sync_lock_test_and_set_1 clang built-in not available"
#endif

#if !__has_builtin(__sync_lock_test_and_set_2)
	#error "__sync_lock_test_and_set_2 clang built-in not available"
#endif

#if !__has_builtin(__sync_lock_test_and_set_4)
	#error "__sync_lock_test_and_set_4 clang built-in not available"
#endif

#if !__has_builtin(__sync_lock_test_and_set_8)
	#error "__sync_lock_test_and_set_8 clang built-in not available"
#endif

#if !__has_builtin(__sync_lock_test_and_set_16)
	#error "__sync_lock_test_and_set_16 clang built-in not available"
#endif

#if !__has_builtin(__sync_lock_release)
	#error "__sync_lock_release clang built-in not available"
#endif

#if !__has_builtin(__sync_lock_release_1)
	#error "__sync_lock_release_1 clang built-in not available"
#endif

#if !__has_builtin(__sync_lock_release_2)
	#error "__sync_lock_release_2 clang built-in not available"
#endif

#if !__has_builtin(__sync_lock_release_4)
	#error "__sync_lock_release_4 clang built-in not available"
#endif

#if !__has_builtin(__sync_lock_release_8)
	#error "__sync_lock_release_8 clang built-in not available"
#endif

#if !__has_builtin(__sync_lock_release_16)
	#error "__sync_lock_release_16 clang built-in not available"
#endif

#if !__has_builtin(__sync_swap)
	#error "__sync_swap clang built-in not available"
#endif

#if !__has_builtin(__sync_swap_1)
	#error "__sync_swap_1 clang built-in not available"
#endif

#if !__has_builtin(__sync_swap_2)
	#error "__sync_swap_2 clang built-in not available"
#endif

#if !__has_builtin(__sync_swap_4)
	#error "__sync_swap_4 clang built-in not available"
#endif

#if !__has_builtin(__sync_swap_8)
	#error "__sync_swap_8 clang built-in not available"
#endif

#if !__has_builtin(__sync_swap_16)
	#error "__sync_swap_16 clang built-in not available"
#endif

#if !__has_builtin(__sync_synchronize)
	#error "__sync_synchronize clang built-in not available"
#endif

#if !__has_builtin(__sync_fetch_and_min)
	#error "__sync_fetch_and_min clang built-in not available"
#endif

#if !__has_builtin(__sync_fetch_and_max)
	#error "__sync_fetch_and_max clang built-in not available"
#endif

#if !__has_builtin(__sync_fetch_and_umin)
	#error "__sync_fetch_and_umin clang built-in not available"
#endif

#if !__has_builtin(__sync_fetch_and_umax)
	#error "__sync_fetch_and_umax clang built-in not available"
#endif

#if !__has_builtin(__builtin_abort)
	#error "abort clang built-in not available"
#else
	#define abort __builtin_abort
#endif

#if !__has_builtin(__builtin_index)
	#error "index clang built-in not available"
#else
	#define index __builtin_index
#endif

#if !__has_builtin(__builtin_rindex)
	#error "rindex clang built-in not available"
#else
	#define rindex __builtin_rindex
#endif

#if !__has_builtin(__builtin_objc_memmove_collectable)
	#error "objc_memmove_collectable clang built-in not available"
#else
	#define objc_memmove_collectable __builtin_objc_memmove_collectable
#endif

#if !__has_builtin(__builtin_annotation)
	#error "annotation clang built-in not available"
#else
	#define annotation __builtin_annotation
#endif

#if !__has_builtin(__builtin_assume)
	#error "assume clang built-in not available"
#else
	#define assume __builtin_assume
#endif

#if !__has_builtin(__builtin_addcb)
	#error "addcb clang built-in not available"
#else
	#define addcb __builtin_addcb
#endif

#if !__has_builtin(__builtin_addcs)
	#error "addcs clang built-in not available"
#else
	#define addcs __builtin_addcs
#endif

#if !__has_builtin(__builtin_addc)
	#error "addc clang built-in not available"
#else
	#define addc __builtin_addc
#endif

#if !__has_builtin(__builtin_addcl)
	#error "addcl clang built-in not available"
#else
	#define addcl __builtin_addcl
#endif

#if !__has_builtin(__builtin_addcll)
	#error "addcll clang built-in not available"
#else
	#define addcll __builtin_addcll
#endif

#if !__has_builtin(__builtin_subcb)
	#error "subcb clang built-in not available"
#else
	#define subcb __builtin_subcb
#endif

#if !__has_builtin(__builtin_subcs)
	#error "subcs clang built-in not available"
#else
	#define subcs __builtin_subcs
#endif

#if !__has_builtin(__builtin_subc)
	#error "subc clang built-in not available"
#else
	#define subc __builtin_subc
#endif

#if !__has_builtin(__builtin_subcl)
	#error "subcl clang built-in not available"
#else
	#define subcl __builtin_subcl
#endif

#if !__has_builtin(__builtin_subcll)
	#error "subcll clang built-in not available"
#else
	#define subcll __builtin_subcll
#endif

#if !__has_builtin(__builtin_uadd_overflow)
	#error "uadd_overflow clang built-in not available"
#else
	#define uadd_overflow __builtin_uadd_overflow
#endif

#if !__has_builtin(__builtin_uaddl_overflow)
	#error "uaddl_overflow clang built-in not available"
#else
	#define uaddl_overflow __builtin_uaddl_overflow
#endif

#if !__has_builtin(__builtin_uaddll_overflow)
	#error "uaddll_overflow clang built-in not available"
#else
	#define uaddll_overflow __builtin_uaddll_overflow
#endif

#if !__has_builtin(__builtin_usub_overflow)
	#error "usub_overflow clang built-in not available"
#else
	#define usub_overflow __builtin_usub_overflow
#endif

#if !__has_builtin(__builtin_usubl_overflow)
	#error "usubl_overflow clang built-in not available"
#else
	#define usubl_overflow __builtin_usubl_overflow
#endif

#if !__has_builtin(__builtin_usubll_overflow)
	#error "usubll_overflow clang built-in not available"
#else
	#define usubll_overflow __builtin_usubll_overflow
#endif

#if !__has_builtin(__builtin_umul_overflow)
	#error "umul_overflow clang built-in not available"
#else
	#define umul_overflow __builtin_umul_overflow
#endif

#if !__has_builtin(__builtin_umull_overflow)
	#error "umull_overflow clang built-in not available"
#else
	#define umull_overflow __builtin_umull_overflow
#endif

#if !__has_builtin(__builtin_umulll_overflow)
	#error "umulll_overflow clang built-in not available"
#else
	#define umulll_overflow __builtin_umulll_overflow
#endif

#if !__has_builtin(__builtin_sadd_overflow)
	#error "sadd_overflow clang built-in not available"
#else
	#define sadd_overflow __builtin_sadd_overflow
#endif

#if !__has_builtin(__builtin_saddl_overflow)
	#error "saddl_overflow clang built-in not available"
#else
	#define saddl_overflow __builtin_saddl_overflow
#endif

#if !__has_builtin(__builtin_saddll_overflow)
	#error "saddll_overflow clang built-in not available"
#else
	#define saddll_overflow __builtin_saddll_overflow
#endif

#if !__has_builtin(__builtin_ssub_overflow)
	#error "ssub_overflow clang built-in not available"
#else
	#define ssub_overflow __builtin_ssub_overflow
#endif

#if !__has_builtin(__builtin_ssubl_overflow)
	#error "ssubl_overflow clang built-in not available"
#else
	#define ssubl_overflow __builtin_ssubl_overflow
#endif

#if !__has_builtin(__builtin_ssubll_overflow)
	#error "ssubll_overflow clang built-in not available"
#else
	#define ssubll_overflow __builtin_ssubll_overflow
#endif

#if !__has_builtin(__builtin_smul_overflow)
	#error "smul_overflow clang built-in not available"
#else
	#define smul_overflow __builtin_smul_overflow
#endif

#if !__has_builtin(__builtin_smull_overflow)
	#error "smull_overflow clang built-in not available"
#else
	#define smull_overflow __builtin_smull_overflow
#endif

#if !__has_builtin(__builtin_smulll_overflow)
	#error "smulll_overflow clang built-in not available"
#else
	#define smulll_overflow __builtin_smulll_overflow
#endif

#if !__has_builtin(__builtin_addressof)
	#error "addressof clang built-in not available"
#else
	#define addressof __builtin_addressof
#endif

#if !__has_builtin(__builtin_operator_new)
	#error "operator_new clang built-in not available"
#else
	#define operator_new __builtin_operator_new
#endif

#if !__has_builtin(__builtin_operator_delete)
	#error "operator_delete clang built-in not available"
#else
	#define operator_delete __builtin_operator_delete
#endif

#if !__has_builtin(__builtin___get_unsafe_stack_start)
	#error "__get_unsafe_stack_start clang built-in not available"
#else
	#define __get_unsafe_stack_start __builtin___get_unsafe_stack_start
#endif

#if !__has_builtin(__builtin___get_unsafe_stack_ptr)
	#error "__get_unsafe_stack_ptr clang built-in not available"
#else
	#define __get_unsafe_stack_ptr __builtin___get_unsafe_stack_ptr
#endif

#include <NativeMath.h>
#include <NativeMemory.h>
#include <NativeTime.h>
#include <NativeDynamicLinking.h>

#endif /* nativepath_h */
