#include "Platform/PlatformTypes.h"
#include "Platform/Platform.h"
#include "Graphics/Renderer.h"
#include "Graphics/Direct3D12/D3D12Core.h"
#include "Content/ContentToEngine.h"
#include "Components/Entity.h"
#include "Components/Transform.h"
#include "TestRenderer.h"
#include "ShaderCompilation.h"
#include <filesystem>
#include <fstream>

#if TEST_RENDERER

using namespace primal;

// Multithreading test worker spawn code ////////////////////////////////////////////
#define ENABLE_TEST_WORKERS 0

constexpr u32 num_threads{ 8 };
bool          shutdown{ false };
std::thread   workers[num_threads];

utl::vector<u8> buffer(1024 * 1024, 0);
// Test worker for upload context
void buffer_test_worker() {
	while (!shutdown) {
		auto* resource = graphics::d3d12::d3dx::create_buffer(buffer.data(), (u32)buffer.size());
		// NOTE: We can also use core::release(resource) since we're not using the buffer for rendering.
		//		 However, this is a nice test for deferred_release functionality.
		graphics::d3d12::core::deferred_release(resource);
	}
}

template<class FnPtr, class... Args> void init_test_workers(FnPtr&& fnPtr, Args&&... args) {
#if ENABLE_TEST_WORKERS
	shutdown = false;
	for (auto& w : workers)
		w = std::thread(std::forward<FnPtr>(fnPtr), std::forward<Args>(args)...);
#endif
}

void joint_test_workers() {
#if ENABLE_TEST_WORKERS
	shutdown = true;
	for (auto& w : workers) w.join();
#endif
}
////////////////////////////////////////

game_entity::entity entity{};
id::id_type model_id{ id::invalid_id };
graphics::camera camera{};

graphics::renderer_surface _surfaces[4];
time_it timer{};

bool resized{ false };
bool is_restarting{ false };
void destroy_render_surface(graphics::renderer_surface& surface);
bool test_initialize();
void test_shutdown();

LRESULT win_proc(HWND hwnd, UINT msg, WPARAM wparam, LPARAM lparam) {

	bool toggle_fullscreen{ false };

	switch (msg) {
	case WM_DESTROY:
	{
		bool all_closed{ true };
		for (u32 i{ 0 }; i < _countof(_surfaces); ++i) {
			if (_surfaces[i].window.is_valid()) {
				if (_surfaces[i].window.is_closed()) {
					destroy_render_surface(_surfaces[i]);
				}
				else {
					all_closed = false;
				}
			}
		}
		if (all_closed && !is_restarting) {
			PostQuitMessage(0);
			return 0;
		}
	}
	break;
	case WM_SIZE:
		resized = (wparam != SIZE_MINIMIZED);
		break;
	case WM_SYSCHAR:
		toggle_fullscreen = (wparam == VK_RETURN && (HIWORD(lparam) & KF_ALTDOWN));
		break;
	case WM_KEYDOWN:
		if (wparam == VK_ESCAPE) {
			PostMessage(hwnd, WM_CLOSE, 0, 0);
			return 0;
		}
		else if (wparam == VK_F11) {
			is_restarting = true;
			test_shutdown();
			test_initialize();
		}
	}

	if ((resized && GetAsyncKeyState(VK_LBUTTON) >= 0) || toggle_fullscreen) {
		platform::window win{ platform::window_id{(id::id_type)GetWindowLongPtr(hwnd, GWLP_USERDATA)} };
		for (u32 i{ 0 }; i < _countof(_surfaces); ++i) {
			if (win.get_id() == _surfaces[i].window.get_id()) {
				if (toggle_fullscreen) {
					win.set_fullscreen(!win.is_fullscreen());
					// The default window procedure will play a system notification sound when pressing the Alt+Enter keyboard combination if WM_SYSCHAR is
					// not handled. By returning 0 we can tell the system that we handled this message.
					return 0;
				}
				else {
					_surfaces[i].surface.resize(win.width(), win.height());
					resized = false;
				}
				break;
			}
		}
	}

	return DefWindowProc(hwnd, msg, wparam, lparam);
}

game_entity::entity create_one_game_entity() {
	transform::init_info transform_info{};
	math::v3a rot{ 0, 3.14f, 0 };
	DirectX::XMVECTOR quat{ DirectX::XMQuaternionRotationRollPitchYawFromVector(DirectX::XMLoadFloat3A(&rot)) };
	math::v4a rot_quat;
	DirectX::XMStoreFloat4A(&rot_quat, quat);
	memcpy(&transform_info.rotation[0], &rot_quat.x, sizeof(transform_info.rotation));

	game_entity::entity_info entity_info{};
	entity_info.transform = &transform_info;
	game_entity::entity ntt{ game_entity::create(entity_info) };
	assert(ntt.is_valid());
	return ntt;
}

bool read_file(std::filesystem::path path, std::unique_ptr<u8[]>& data, u64& size) {
	if (!std::filesystem::exists(path)) return false;

	size = std::filesystem::file_size(path);
	assert(size);
	if (!size) return false;
	data = std::make_unique<u8[]>(size);
	std::ifstream file{ path, std::ios::in | std::ios::binary };
	if (!file || !file.read((char*)data.get(), size)) {
		file.close();
		return false;
	}

	file.close();
	return true;
}

void create_render_surface(graphics::renderer_surface& surface, platform::window_init_info info) {
	surface.window = platform::create_window(&info);
	surface.surface = graphics::create_surface(surface.window);
}

void destroy_render_surface(graphics::renderer_surface& surface) {
	graphics::renderer_surface temp{ surface };
	surface = {};

	if (temp.surface.is_valid()) graphics::remove_surface(temp.surface.get_id());
	if (temp.window.is_valid()) platform::remove_window(temp.window.get_id());
}

bool test_initialize() {
	while (!compile_shaders()) {
		// Pop up a message box allowing the user to retry the compilation
		if (MessageBox(nullptr, L"Failed to comiple engine shaders.", L"Shader Compilation Error", MB_RETRYCANCEL) != IDRETRY) return false;
	}

	if (!graphics::initialize(graphics::graphics_platform::direct3d12)) return false;

	platform::window_init_info info[]{
		{&win_proc, nullptr, L"Render window 1", 100, 100, 400, 800},
		{&win_proc, nullptr, L"Render window 2", 150, 150, 800, 400},
		{&win_proc, nullptr, L"Render window 3", 200, 200, 400, 400},
		{&win_proc, nullptr, L"Render window 4", 250, 250, 800, 800},
	};
	static_assert(_countof(info) == _countof(_surfaces));

	for (u32 i{ 0 }; i < _countof(_surfaces); ++i) {
		create_render_surface(_surfaces[i], info[i]);
	}

	// load test model
	std::unique_ptr<u8[]> model;
	u64 size{ 0 };
	if (!read_file("..\\..\\enginetest\\model.model", model, size)) return false;

	model_id = content::create_resource(model.get(), content::asset_type::mesh);
	if (!id::is_valid(model_id)) return false;

	init_test_workers(buffer_test_worker);

	entity = create_one_game_entity();
	camera = graphics::create_camera(graphics::perspective_camera_init_info(entity.get_id()));
	assert(camera.is_valid());

	is_restarting = false;
	return true;
}

void test_shutdown() {
	if (camera.is_valid()) graphics::remove_camera(camera.get_id());
	if (entity.is_valid()) game_entity::remove(entity.get_id());

	joint_test_workers();

	if (id::is_valid(model_id)) {
		content::destroy_resource(model_id, content::asset_type::mesh);
	}

	for (u32 i{ 0 }; i < _countof(_surfaces); ++i) {
		destroy_render_surface(_surfaces[i]);
	}
	graphics::shutdown();
}

bool engine_test::initialize() {
	return test_initialize();
}

void engine_test::run() {
	timer.begin();
	std::this_thread::sleep_for(std::chrono::milliseconds(10));
	for (u32 i{ 0 }; i < _countof(_surfaces); ++i) {
		if (_surfaces[i].surface.is_valid()) {
			_surfaces[i].surface.render();
		}
	}
	timer.end();
}

void engine_test::shutdown() {
	test_shutdown();
}

#endif // TEST_RENDERER