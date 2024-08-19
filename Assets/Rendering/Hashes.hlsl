#if defined(UNITY_PROCEDURAL_INSTANCING_ENABLED)
    StructuredBuffer<uint> _hashes;
    StructuredBuffer<float3> _positions;
    StructuredBuffer<float3> _normals;
 #endif

float4 _config;

void ConfigureProcedural() {
    #if defined(UNITY_PROCEDURAL_INSTANCING_ENABLED)
    
        unity_ObjectToWorld = 0.0;
        unity_ObjectToWorld._m03_m13_m23_m33 = float4(
            _positions[unity_InstanceID],
            1.0
        );

        unity_ObjectToWorld._m03_m13_m23 += (_config.z * (1.0 / 255.0) * (_hashes[unity_InstanceID] >> 24) * 0.5) * _normals[unity_InstanceID];

        unity_ObjectToWorld._m00_m11_m22 = _config.y;
    
    #endif
}

float3 GetHashColor() {
    #if defined(UNITY_PROCEDURAL_INSTANCING_ENABLED)
        uint hash = _hashes[unity_InstanceID];
        return (1.0f / 255.0f) * float3(hash & 255, (hash >> 8) & 255, (hash >> 16) & 255);
    #else
        return 1.0;
    #endif
}

void ShaderGraphFunction_float (float3 In, out float3 Out, out float3 Color) {
	Out = In;
	Color = GetHashColor();
}

void ShaderGraphFunction_half (half3 In, out half3 Out, out half3 Color) {
	Out = In;
	Color = GetHashColor();
}