#pragma once
#include <cstdint>

// unsigned integers
using u8 = uint8_t; //0-255
using u16 = uint16_t;
using u32 = uint32_t;
using u64 = uint64_t;

// signed integers
using s8 = int8_t;
using s16 = int16_t;
using s32 = int32_t;
using s64 = int64_t;

constexpr u8 u8_invalid_id{ 0xff };
constexpr u16 u16_invalid_id{ 0xffff };
constexpr u32 u32_invalid_id{ 0xffff'ffff };
constexpr u64 u64_invalid_id{ 0xffff'ffff'ffff'ffff };

using f32 = float;