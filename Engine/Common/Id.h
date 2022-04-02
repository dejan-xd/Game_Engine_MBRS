#pragma once
#include "CommonHeaders.h"

namespace primal::id {
	using id_type = u32;

	constexpr u32 generation_bits{ 8 };
	constexpr u32 index_bits{ sizeof(id_type) * 8 - generation_bits };
	constexpr id_type index_mask{ id_type(1) << index_bits - 1 };
	constexpr id_type generation_mask{ id_type(1) << generation_bits - 1 };
	constexpr id_type id_mask{ id_type{-1} };

	using generation_type = std::conditional_t<generation_bits <= 16, std::conditional_t<generation_bits <= 8, u8, u16>, u32>;

	static_assert(sizeof(generation_type) * 8 >= generation_bits);
	static_assert((sizeof(id_type) - sizeof(generation_type)) > 0);

	inline bool is_valid(id_type id) {
		return id != id_mask;
	}

	inline id_type index(id_type id) {
		return id & index_mask;
	}

	inline id_type generation(id_type id) {
		return (id >> index_bits) & generation_mask;
	}

	inline id_type new_generation(id_type id) {
		const id_type generation{ id::generation(id) + 1 };
		assert(generation < 255);
		return index(id) | (generation << index_bits);
	}
}