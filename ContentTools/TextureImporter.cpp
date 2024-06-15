#include "ToolsCommon.h"
#include "Content/ContentToEngine.h"
#include <DirectXTex.h>

using namespace DirectX;

namespace primal::tools {

	namespace {

		struct import_error {
			enum error_code : u32 {
				succeeded = 0,
				unknown,
				compress,
				decompress,
				load,
				mipmap_generation,
				max_size_exceeded,
				size_mismatch,
				format_mismatch,
				file_not_found,
			};
		};

		struct texture_dimension {
			enum dimension : u32 {
				texture_1d,
				texture_2d,
				texture_3d,
				texture_cube
			};
		};

		struct texture_import_settings {
			char* sources;		// string of one or more file paths separated by semi-colons ';'
			u32 source_count;	// number of file paths
			u32 dimension;
			u32 mip_levels;
			u32 array_size;
			f32 alpha_threshold;
			u32 prefer_bc7;
			u32 output_format;
			u32 compress;
		};

		struct texture_info {
			u32 width;
			u32 height;
			u32 array_size;
			u32 mip_levels;
			u32 format;
			u32 import_error;
			u32 flags;
		};

		struct texture_data {
			constexpr static u32 max_mips{ 14 }; // GameEngine supports up to 8k textures
			u8* subresource_data;
			u32 subresource_size;
			u8* icon;
			u32 icon_size;
			texture_info info;
			texture_import_settings import_settings;
		};

		[[nodiscard]] ScratchImage load_from_file(texture_data* const data, const char* file_name) {
			using namespace primal::content;
			assert(file_exists(file_name));
			ScratchImage scratch;
			if (!file_exists(file_name)) {
				data->info.import_error = import_error::file_not_found;
				return scratch;
			}

			data->info.import_error = import_error::load;

			WIC_FLAGS wic_flags{ WIC_FLAGS_NONE };
			TGA_FLAGS tga_flags{ TGA_FLAGS_NONE };

			if (data->import_settings.output_format == DXGI_FORMAT_BC4_UNORM || data->import_settings.output_format == DXGI_FORMAT_BC5_UNORM) {
				wic_flags |= WIC_FLAGS_IGNORE_SRGB;
				tga_flags |= TGA_FLAGS_IGNORE_SRGB;
			}

			const std::wstring wfile{ to_wstring(file_name) };
			const wchar_t* const file{ wfile.c_str() };

			// Try one of WIC formats first (e.g. BMP, JPEG, PNG, etc.).
			wic_flags |= WIC_FLAGS_FORCE_RGB;
			HRESULT hr{ LoadFromWICFile(file, wic_flags, nullptr, scratch) };

			// It wasn't a WIC format. Try TGA.
			if (FAILED(hr)) {
				hr = LoadFromTGAFile(file, tga_flags, nullptr, scratch);
			}

			// It wasn't a TGA either. Try HDR.
			if (FAILED(hr)) {
				hr = LoadFromHDRFile(file, nullptr, scratch);
				if (SUCCEEDED(hr)) data->info.flags |= texture_flags::is_hdr;
			}

			// It wasn't HDR. Try DDS.
			if (FAILED(hr)) {
				hr = LoadFromDDSFile(file, DDS_FLAGS_FORCE_RGB, nullptr, scratch);
				if (SUCCEEDED(hr)) {
					data->info.import_error = import_error::decompress;
					ScratchImage mip_scratch;
					hr = Decompress(scratch.GetImages(), scratch.GetImageCount(), scratch.GetMetadata(), DXGI_FORMAT_UNKNOWN, mip_scratch);

					if (SUCCEEDED(hr)) {
						scratch = std::move(mip_scratch);
					}
				}
			}

			if (SUCCEEDED(hr)) {
				data->info.import_error = import_error::succeeded;
			}

			return scratch;
		}

	} // anonymous namespace

	EDITOR_INTERFACE void DecompressMipmaps(texture_data* const data) {

	}

	EDITOR_INTERFACE void Import(texture_data* const data) {
		const texture_import_settings& settings{ data->import_settings };
		assert(settings.sources && settings.source_count);

		utl::vector<ScratchImage> scratch_images;
		utl::vector<Image> images;

		u32 width{ 0 };
		u32 height{ 0 };
		DXGI_FORMAT format{};
		utl::vector<std::string> files = split(settings.sources, ';');
		assert(files.size() == settings.source_count);

		for (u32 i{ 0 }; i < settings.source_count; ++i) {
			scratch_images.emplace_back(load_from_file(data, files[i].c_str()));
			if (data->info.import_error) return;
		}


	}

}