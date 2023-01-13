#include "D3D12Camera.h"
#include "EngineAPI/GameEntity.h"

namespace primal::graphics::d3d12::camera {

	namespace {

	} // anonymous namespace

    d3d12_camera::d3d12_camera(camera_init_info info) : _up{ DirectX::XMLoadFloat3(&info.up) }, _near_z{ info.near_z }, _far_z{ info.far_z },
        _field_of_view{ info.field_of_view }, _aspect_ratio{ info.aspect_ratio }, _projection_type{ info.type }, _entity_id{ info.entity_id }, _is_dirty{ true } {

        assert(id::is_valid(_entity_id));
        update();
    }

    void d3d12_camera::update() {

    }

	void d3d12_camera::up(math::v3 up) {
		_up = DirectX::XMLoadFloat3(&up);
	}

	void d3d12_camera::field_of_view(f32 fov){
		assert(_projection_type == graphics::camera::perspective);
		_field_of_view = fov;
		_is_dirty = true;
	}

	void d3d12_camera::aspect_ratio(f32 aspect_ratio){
		assert(_projection_type == graphics::camera::perspective);
		_aspect_ratio = aspect_ratio;
		_is_dirty = true;
	}

	void d3d12_camera::view_width(f32 width){
		assert(width);
		assert(_projection_type == graphics::camera::orthographic);
		_view_width = width;
		_is_dirty = true;
	}

	void d3d12_camera::view_height(f32 height){
		assert(height);
		assert(_projection_type == graphics::camera::orthographic);
		_view_height = height;
		_is_dirty = true;
	}

	void d3d12_camera::near_z(f32 near_z){
		_near_z = near_z;
		_is_dirty = true;
	}

	void d3d12_camera::far_z(f32 far_z){
		_far_z = far_z;
		_is_dirty = true;
	}
}