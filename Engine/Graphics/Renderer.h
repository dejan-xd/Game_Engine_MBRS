#pragma once
#include "CommonHeaders.h"
#include "..\Platform\Window.h"

namespace primal::graphics {

	class surface {};

	struct renderer_surface {
		platform::window window{};
		surface surface{};
	};

	enum class graphics_platform : u32 {
		direct3d12 = 0,
		// vulkan = 1,
		// open_gl = 2,
		// etc.
	};

	bool initialize(graphics_platform platfomr);
	void shutdown();
	void render();
}