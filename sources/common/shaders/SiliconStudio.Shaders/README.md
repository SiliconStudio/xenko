SiliconStudio.Shader
====================

Shader source code manipulation library.
With it, you can:
* Parse HLSL and GLSL
* Implement custom AST visitors
* Semantic information: inferred type, etc...
* Transform HLSL into GLSL

## HLSL to GLSL

It can convert HLSL code to various GLSL dialect, including Core, ES 2.0, ES 3.0+ and Vulkan.

### Intrisics

|                         | Status |
| ----------------------- | ------ |
| Stages                  | VS & PS only |
| Standard intrinsics     | :heavy_check_mark: except: noise |
| SM5 intrinsics          | :heavy_check_mark: except: dst, ddx_coarse, ddx_fine, ddy_coarse, ddy_fine, firstbithigh, firstbitlow, countbits, f16tof32, f32tof16, fma, mad, msad4, rcp, reversebits |
| Barriers intrinsics     | :heavy_multiplication_x: (no CS) |
| Interlocked intrinsics  | :heavy_multiplication_x: (no CS) |
| Integer reinterpret intrinsics | :heavy_multiplication_x: asuint, asint |
| Flow statements         | :heavy_check_mark: except: errorf, printf, abort |
| Texture objects         | :heavy_check_mark: |
| Buffer objects          | :heavy_check_mark: |
| Class/struct methods    | :heavy_multiplication_x: |
| Preprocessor            | :heavy_check_mark: |
| Remap VS projected coordinates | :heavy_check_mark: |
| Multiple Render Targets | :heavy_check_mark: |
| Constant Buffers        | :heavy_check_mark: |
| RW and Structured Buffers | :heavy_multiplication_x: |

### Texture / samplers

By default, it generates "combined" samplers:

```
Texture2D texture;
SamplerState sampler;

texture.Sample(texture, sampler);
```

will generate a single `sampler2D texture_sampler`

There is also a mode to generate separate texture/samplers for platforms that support it (i.e. Vulkan).

### Known issues

* Small type inference issue with Texture object, happens when doing texture.Sample().r, where it doesn't resolve generics type properly
* Preprocessor seems to first concat then replace defines, which makes such patterns fail:
```
#define REGISTER_INDEX 1
#define REGISTER_EXPR b ## REGISTER_INDEX
cbuffer A : register(REGISTER_EXPR)
```
