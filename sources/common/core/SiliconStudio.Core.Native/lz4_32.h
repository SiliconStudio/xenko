// This file imports lz4.h prefixing all functions with I32_

#pragma once

#define LZ4_ARCH64 0
#define LZ4_FUNC(name) I32_ ## name
#define LZ4_MK_OPT

#include "lz4.h"
#include "lz4hc.h"