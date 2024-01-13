#ifdef _WIN64

#include "CommonHeaders.h"
#include <filesystem>

#ifndef WIN32_LEAN_AND_MEAN
#define WIN32_LEAN_AND_MEAN
#endif // !WIN32_LEAN_AND_MEAN

#include <Windows.h>
#include <crtdbg.h>

namespace {
	// TODO: move this function to IO utility header/library once (if) it is created
	std::filesystem::path set_current_directory_to_executable_path() {
		// set the working directory to the executable path
		wchar_t path[MAX_PATH]{};
		const u32 length{ GetModuleFileName(0, &path[0], MAX_PATH) };
		if (!length || GetLastError() == ERROR_INSUFFICIENT_BUFFER) return {};
		std::filesystem::path p{ path };
		std::filesystem::current_path(p.parent_path());
		return std::filesystem::current_path();
	}
}

#ifndef USE_WITH_EDITOR

extern bool engine_initialize();
extern void engine_update();
extern void engine_shutdown();

int WINAPI WinMain(_In_ HINSTANCE, _In_opt_ HINSTANCE, _In_ LPSTR, _In_ int) {

#if _DEBUG
	_CrtSetDbgFlag(_CRTDBG_ALLOC_MEM_DF | _CRTDBG_LEAK_CHECK_DF);
#endif

	set_current_directory_to_executable_path();

	if (engine_initialize()) {
		MSG msg{};
		bool is_running{ true };
		while (is_running) {
			while (PeekMessage(&msg, NULL, 0, 0, PM_REMOVE)) {
				// NOTE: Inner while loop reads, removes and dispatches messages from the message queue,
				//		 until there are no messages left to precoess.
				TranslateMessage(&msg);
				DispatchMessage(&msg);
				is_running &= (msg.message != WM_QUIT);
			}
			engine_update();
		}
	}
	engine_shutdown();
	return 0;
}
#endif // !USE_WITH_EDITOR

#endif // !_WIN64