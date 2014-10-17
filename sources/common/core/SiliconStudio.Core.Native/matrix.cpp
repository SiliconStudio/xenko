/*
 NEON math library for the iPhone / iPod touch
 
 Copyright (c) 2009 Justin Saunders
 
 This software is provided 'as-is', without any express or implied warranty.
 In no event will the authors be held liable for any damages arising
 from the use of this software.
 Permission is granted to anyone to use this software for any purpose,
 including commercial applications, and to alter it and redistribute it freely,
 subject to the following restrictions:
 
 1. The origin of this software must not be misrepresented; you must
 not claim that you wrote the original software. If you use this
 software in a product, an acknowledgment in the product documentation
 would be appreciated but is not required.
 
 2. Altered source versions must be plainly marked as such, and must
 not be misrepresented as being the original software.
 
 3. This notice may not be removed or altered from any source distribution.
*/

#include "coreconfig.h"

// possibly needed header for iOS
//#ifdef __arm__
//#include "arm/arch.h"
//#endif

#ifdef PLATFORM_ANDROID

#ifdef __thumb__
#error "This file should be compiled in ARM mode only."
// Note in Xcode, right click file, Get Info->Build, Other compiler flags = "-marm"
#endif

extern "C" {

// Note asm is taken from reader "Jeff" post Wolfgang's blog:
// https://www.blogger.com/comment.g?blogID=398682525365778708&postID=7527893703750196380&page=1
CORE_EXPORT( void ) NEON_Matrix4Mul(const float* a, const float* b, float* output )
{
#ifdef _ARM_ARCH_7
        __asm__ __volatile
        (
         // Store A & B leaving room at top of registers for result (q0-q3)
         "vldmia %2, { q4-q7 }  \n\t"
         "vldmia %1, { q8-q11 } \n\t"
         
         // result = first column of B x first row of A
         "vmul.f32 q0, q8, d8[0]\n\t"
         "vmul.f32 q1, q8, d10[0]\n\t"
         "vmul.f32 q2, q8, d12[0]\n\t"
         "vmul.f32 q3, q8, d14[0]\n\t"
         
         // result += second column of B x second row of A
         "vmla.f32 q0, q9, d8[1]\n\t"
         "vmla.f32 q1, q9, d10[1]\n\t"
         "vmla.f32 q2, q9, d12[1]\n\t"
         "vmla.f32 q3, q9, d14[1]\n\t"
         
         // result += third column of B x third row of A
         "vmla.f32 q0, q10, d9[0]\n\t"
         "vmla.f32 q1, q10, d11[0]\n\t"
         "vmla.f32 q2, q10, d13[0]\n\t"
         "vmla.f32 q3, q10, d15[0]\n\t"
         
         // result += last column of B x last row of A
         "vmla.f32 q0, q11, d9[1]\n\t"
         "vmla.f32 q1, q11, d11[1]\n\t"
         "vmla.f32 q2, q11, d13[1]\n\t"
         "vmla.f32 q3, q11, d15[1]\n\t"
         
         // output = result registers
         "vstmia %0, { q0-q3 }"
         : // no output
         : "r" (output), "r" (a), "r" (b)       // input - note *value* of pointer doesn't change
         : "memory", "q0", "q1", "q2", "q3", "q4", "q5", "q6", "q7", "q8", "q9", "q11" //clobber
         );
#else
         output[ 0] = a[0] * b[ 0] + a[4] * b[ 1] + a[ 8] * b[ 2] + a[12] * b[ 3];
         output[ 1] = a[1] * b[ 0] + a[5] * b[ 1] + a[ 9] * b[ 2] + a[13] * b[ 3];
         output[ 2] = a[2] * b[ 0] + a[6] * b[ 1] + a[10] * b[ 2] + a[14] * b[ 3];
         output[ 3] = a[3] * b[ 0] + a[7] * b[ 1] + a[11] * b[ 2] + a[15] * b[ 3];
         output[ 4] = a[0] * b[ 4] + a[4] * b[ 5] + a[ 8] * b[ 6] + a[12] * b[ 7];
         output[ 5] = a[1] * b[ 4] + a[5] * b[ 5] + a[ 9] * b[ 6] + a[13] * b[ 7];
         output[ 6] = a[2] * b[ 4] + a[6] * b[ 5] + a[10] * b[ 6] + a[14] * b[ 7];
         output[ 7] = a[3] * b[ 4] + a[7] * b[ 5] + a[11] * b[ 6] + a[15] * b[ 7];
         output[ 8] = a[0] * b[ 8] + a[4] * b[ 9] + a[ 8] * b[10] + a[12] * b[11];
         output[ 9] = a[1] * b[ 8] + a[5] * b[ 9] + a[ 9] * b[10] + a[13] * b[11];
         output[10] = a[2] * b[ 8] + a[6] * b[ 9] + a[10] * b[10] + a[14] * b[11];
         output[11] = a[3] * b[ 8] + a[7] * b[ 9] + a[11] * b[10] + a[15] * b[11];
         output[12] = a[0] * b[12] + a[4] * b[13] + a[ 8] * b[14] + a[12] * b[15];
         output[13] = a[1] * b[12] + a[5] * b[13] + a[ 9] * b[14] + a[13] * b[15];
         output[14] = a[2] * b[12] + a[6] * b[13] + a[10] * b[14] + a[14] * b[15];
         output[15] = a[3] * b[12] + a[7] * b[13] + a[11] * b[14] + a[15] * b[15];
#endif
}
         
CORE_EXPORT( void ) NEON_Matrix4Vector4Mul(const float* m, const float* v, float* output)
{
#ifdef _ARM_ARCH_7
         __asm__ __volatile
         (
         // Store m & v leaving room at top of registers for result (q0)
         "vldmia %1, {q1-q4 }   \n\t"   // q2-q5 = m
         "vldmia %2, {q5}               \n\t"   // q1    = v
         
         // result = first column of A x V.x
         "vmul.f32 q0, q1, d10[0]\n\t"
         
         // result += second column of A x V.y
         "vmla.f32 q0, q2, d10[1]\n\t"
         
         // result += third column of A x V.z
         "vmla.f32 q0, q3, d11[0]\n\t"
         
         // result += last column of A x V.w
         "vmla.f32 q0, q4, d11[1]\n\t"
         
         // output = result registers
         "vstmia %0, {q0}"
         
         : // no output
         : "r" (output), "r" (m), "r" (v)       // input - note *value* of pointer doesn't change
         : "memory", "q0", "q1", "q2", "q3", "q4", "q5" //clobber
         );
#else
         output[0] = m[0] * v[0] + m[4] * v[1] + m[ 8] * v[2] + m[12] * v[3];
         output[1] = m[1] * v[0] + m[5] * v[1] + m[ 9] * v[2] + m[13] * v[3];
         output[2] = m[2] * v[0] + m[6] * v[1] + m[10] * v[2] + m[14] * v[3];
         output[3] = m[3] * v[0] + m[7] * v[1] + m[11] * v[2] + m[15] * v[3];
#endif
}
}

#endif
