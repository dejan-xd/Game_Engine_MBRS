// Common Types

#if !defined(PRIMAL_COMMONHLSLI) && !defined(__cplusplus)
#error Do not include this header directly in shader files. Only include this file via Common.hlsli.
#endif

struct GlobalShaderData {
	float4x4 View;
	float4x4 Projection;
	float4x4 InvProjection;
	float4x4 ViewProjection;
	float4x4 InvViewProjection;

	float3 CameraPosition;
	float ViewWidth;

	float3 CameraDirection;
	float ViewHeight;

	float DeltaTime;
};

struct PerObjectData {
	float4x4 World;
	float4x4 InvWorld;
	float4x4 WorldViewprojection;
};

#ifdef __cplusplus
static_assert((sizeof(PreObjectData) % 16) == 0, "Make sure PerObjectData is formatted in 16-byte chunks without any implicit padding.");
#endif