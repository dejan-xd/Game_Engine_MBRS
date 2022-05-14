#pragma once
#include "CommonHeaders.h"
#include "..\Platform\Window.h"

namespace primal::graphics {

    class surface {};

    struct renderer_surface {
        platform::window window{};
        surface surface{};
    };
}