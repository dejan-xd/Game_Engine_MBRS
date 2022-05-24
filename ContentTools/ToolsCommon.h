#pragma once
#include "CommonHeaders.h"
#include <combaseapi.h>

#ifndef EDITOR_INTERFACE
// C++ name mangling
#define EDITOR_INTERFACE extern "C" __declspec(dllexport)
#endif // !EDITOR_INTERFACE