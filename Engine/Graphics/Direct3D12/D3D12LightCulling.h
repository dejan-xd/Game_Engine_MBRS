#pragma once
#include "D3D12CommonHeaders.h"

namespace primal::graphics::d3d12 {

	struct d3d12_frame_info;

}

namespace primal::graphics::d3d12::delight {

	constexpr u32 light_culling_tile_size{ 16 };

	bool initialize();
	void shutdown();

	[[nodiscard]] id::id_type add_culler();
	void remove_culler(id::id_type id);

	void cull_lights(id3d12_graphics_command_list* const cmd_list, const d3d12_frame_info& d3d12_info, d3dx::d3d12_resource_barrier& barriers);

}