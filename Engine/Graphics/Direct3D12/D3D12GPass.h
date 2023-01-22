#pragma once

#include "D3D12CommonHeaders.h"

namespace primal::graphics::d3d12 {
	struct d3d12_frame_info;
}

namespace primal::graphics::d3d12::gpass {

	struct opaque_root_parameter {
		enum parameter : u32 {
			per_frame_data,
			position_buffer,
			element_buffer,
			srv_indices,
			per_object_data,

			count
		};
	};

	bool initialize();
	void shutdown();

	[[nodiscard]] const d3d12_render_texture& main_buffer();
	[[nodiscard]] const d3d12_depth_bufffer& depth_buffer();

	// NOTE: call this every frame before rendering anything in gpass
	void set_size(math::u32v2 size);
	void depth_prepass(id3d12_graphics_command_list* cmd_list, const d3d12_frame_info& info);
	void render(id3d12_graphics_command_list* cmd_list, const d3d12_frame_info& info);

	void add_transitions_for_depth_prepass(d3dx::d3d12_resource_barrier& barriers);
	void add_transitions_for_gpass(d3dx::d3d12_resource_barrier& barriers);
	void add_transitions_for_post_process(d3dx::d3d12_resource_barrier& barriers);

	void set_render_targets_for_depth_prepass(id3d12_graphics_command_list* cmd_list);
	void set_render_targets_for_gpass(id3d12_graphics_command_list* cmd_list);
}