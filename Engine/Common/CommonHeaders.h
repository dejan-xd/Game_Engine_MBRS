#pragma once
#pragma warning(disable:4530) // disable exception warning

// C/C++
// NOTE: don't put here any headers that include std::vector or std::deque
#include <stdint.h>
#include <assert.h>
#include <typeinfo>
#include <memory>
#include <unordered_map>
#include <mutex>

#if defined(_WIN64)
#include <DirectXMath.h>
#endif

// macro
#ifdef _DEBUG
#define DEBUG_OP(x) x
#else
#define DEBUG_OP(x)
#endif

// common headers
#include "PrimitiveTypes.h"
#include "..\Utilities\Math.h"
#include "..\Utilities\Utilities.h"
#include "..\Utilities\MathTypes.h"
#include "Id.h"
