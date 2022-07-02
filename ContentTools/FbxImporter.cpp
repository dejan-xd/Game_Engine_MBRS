#include "FbxImporter.h"
#include "Geometry.h"
#include <mutex>

// If you get any compilation or linker errors than make sure that:
// 1) FBS SDK 2020.2 or later version is installed on your system
// 2) the include path to fbxsdk.h is added to "Additional Include Directories" (compiler settings)
// 3) the library paths in the following secion point to the correct location

#if _DEBUG
#pragma comment(lib, "C:\\Program Files\\Autodesk\\FBX\\FBX SDK\\2020.2\\lib\\vs2019\\x64\\debug\\libfbxsdk-md.lib")
#pragma comment(lib, "C:\\Program Files\\Autodesk\\FBX\\FBX SDK\\2020.2\\lib\\vs2019\\x64\\debug\\libxml2-md.lib")
#pragma comment(lib, "C:\\Program Files\\Autodesk\\FBX\\FBX SDK\\2020.2\\lib\\vs2019\\x64\\debug\\zlib-md.lib")
#else
#pragma comment(lib, "C:\\Program Files\\Autodesk\\FBX\\FBX SDK\\2020.2\\lib\\vs2019\\x64\\release\\libfbxsdk-md.lib")
#pragma comment(lib, "C:\\Program Files\\Autodesk\\FBX\\FBX SDK\\2020.2\\lib\\vs2019\\x64\\release\\libxml2-md.lib")
#pragma comment(lib, "C:\\Program Files\\Autodesk\\FBX\\FBX SDK\\2020.2\\lib\\vs2019\\x64\\release\\zlib-md.lib")
#endif

namespace primal::tools {
	namespace {
		std::mutex fbx_mutex{};

	} // anonymous namespace

	bool fbx_context::initialize_fbx() {
		assert(!is_valid());

		_fbx_manager = FbxManager::Create();
		if (!_fbx_manager) {
			return false;
		}

		FbxIOSettings* ios{ FbxIOSettings::Create(_fbx_manager, IOSROOT) };
		assert(ios);
		_fbx_manager->SetIOSettings(ios);

		return true;
	}

	void fbx_context::load_fbx_file(const char* file) {
		assert(_fbx_manager && !_fbx_scene);
		_fbx_scene = FbxScene::Create(_fbx_manager, "Importer Scene");
		if (!_fbx_scene) return;

		FbxImporter* importer{ FbxImporter::Create(_fbx_manager, "Importer") };
		if (!(importer && importer->Initialize(file, -1, _fbx_manager->GetIOSettings()) && importer->Import(_fbx_scene))) return;

		importer->Destroy();

		// get scene scale in meters
		_scene_scale = (f32)_fbx_scene->GetGlobalSettings().GetSystemUnit().GetConversionFactorTo(FbxSystemUnit::m);
	}

	void fbx_context::get_scene(FbxNode* root /*= nullptr*/) {
		assert(is_valid());

		if (!root) {
			root = _fbx_scene->GetRootNode();
			if (!root) return;
		}

		const s32 num_nodes{ root->GetChildCount() };
		for (s32 i{ 0 }; i < num_nodes; ++i) {
			FbxNode* node{ root->GetChild(i) };
			if (!node) continue;

			if (node->GetMesh()) {
				lod_group lod{};
				get_mesh(node, lod.meshes);
				if (lod.meshes.size()) {
					lod.name = lod.meshes[0].name;
					_scene->lod_groups.emplace_back(lod);
				}
			}
			else if (node->GetLodGroup()) {
				get_lod_group(node);
			}
			else {
				get_scene(node); // see if there is a mesh somewhere further down the hierarchy
			}

		}
	}

	void fbx_context::get_mesh(FbxNode* node, utl::vector<mesh>& meshes) {
		assert(node);

		if (FbxMesh * fbx_mesh{ node->GetMesh() }) {
			if (fbx_mesh->RemoveBadPolygons() < 0) return;

			// triangulate the mesh if needed
			FbxGeometryConverter gc{ _fbx_manager };
			fbx_mesh = static_cast<FbxMesh*>(gc.Triangulate(fbx_mesh, true));
			if (!fbx_mesh || fbx_mesh->RemoveBadPolygons() < 0) return;

			mesh m;
			m.lod_id = (u32)meshes.size();
			m.lod_threshold = -1.f;
			m.name = (node->GetName()[0] != '\0') ? node->GetName() : fbx_mesh->GetName();

			//if (get_mesh_data(fbx_mesh, m)) {
			//	meshes.emplace_back(m);
			//}
		}
	}

	void fbx_context::get_lod_group(FbxNode* node) {

	}

	EDITOR_INTERFACE void ImportFbx(const char* file, scene_data* data) {
		assert(file && data);
		scene scene{};

		// NOTE: anything that involves using the FBX SDK should be a single-threaded
		{
			std::lock_guard lock{ fbx_mutex };
			fbx_context fbx_context{ file, &scene, data };
			if (fbx_context.is_valid()) {
				// get scene
			}
			else {
				// TODO: send failure log message to editor
				return;
			}
		}

		process_scene(scene, data->settings);
		pack_data(scene, *data);
	}
}