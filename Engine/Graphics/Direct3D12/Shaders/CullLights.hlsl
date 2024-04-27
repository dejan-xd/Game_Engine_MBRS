// Cull Lights

#include "Common.hlsli"

static const uint MaxLightsPerGroup = 1024;

groupshared uint _minDepthVS; // tile's minimum depth in view-space
groupshared uint _maxDepthVS; // tile's maximum depth in view-space
groupshared uint _lightCount; // number of lights that affect pixels in this tile
groupshared uint _lightIndexStartOffset; // offset in the global light index list where we copy _lightIndexList
groupshared uint _lightIndexList[MaxLightsPerGroup]; // indices of lights that affect this tile

ConstantBuffer<GlobalShaderData> GlobalData : register(b0, space0);
ConstantBuffer<LightCullingDispatchParameters> ShaderParams : register(b1, space0);
StructuredBuffer<Frustum> Frustums : register(t0, space0);
StructuredBuffer<LightCullingLightInfo> Lights : register(t1, space0);

RWStructuredBuffer<uint> LightIndexCounter : register(u0, space0);
RWStructuredBuffer<uint2> LightGrid_Opaque : register(u1, space0);
// TODO: add buffers to handle lights that could affect transparent objects (skipped u2 register for this purpose)
RWStructuredBuffer<uint> LightIndexList_Opaque : register(u3, space0);

// Implementation of light culling shader is based on "Forward vs Deferred vs Forward+ Rendering with DirectX 11" (2015) by Jeremiah van Oosten.
// https://www.3dgep.com/forward-plus/#light-culling
// 
// NOTE: TILE_SIZE is defined by the engine at compile-time.
[numthreads(TILE_SIZE, TILE_SIZE, 1)]
void CullLightsCS(ComputeShaderInput csIn)
{
    // INITIALIZATION SECTION
    if (csIn.GroupIndex == 0) // only the first thread in the group need to initialize groupshared memory
    {
        _minDepthVS = 0x7f7fffff; // FLT_MAX as uint
        _maxDepthVS = 0;
        _lightCount = 0;
    }

    uint i = 0, index = 0; // reusable index variables.
    
    // DEPTH MIN/MAX SECTION
    GroupMemoryBarrierWithGroupSync();
    
    const float depth = Texture2D(ResourceDescriptorHeap[ShaderParams.DepthBufferSrvIndex])[csIn.DispatchThreadID.xy].r;
    const float depthVS = ClipToView(float4(0.f, 0.f, depth, 1.f), GlobalData.InvProjection).z;
    // Negate depth because of right-handed coorinates (negative z-axis).
    // This make the comparisons easier to understand.
    const uint z = asuint(-depthVS);

    if (depth != 0) // Don't include far plane (depth == 0 is mapped to far-plane)
    {
        InterlockedMin(_minDepthVS, z);
        InterlockedMax(_maxDepthVS, z);
    }
    
    // LIGHT CUILLING SECTION
    GroupMemoryBarrierWithGroupSync();
    
    // UPDATE LIGHT GRID SECTION
    GroupMemoryBarrierWithGroupSync();
    
    // UPDATE LIGHT INDEX LIST SECTION
    GroupMemoryBarrierWithGroupSync();
}