using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

using static Unity.Mathematics.math;

[BurstCompile(FloatPrecision.Standard, FloatMode.Fast, CompileSynchronously = true)]
struct HashJob : IJobFor {
        
    [ReadOnly] public NativeArray<float3x4> positions;

    [WriteOnly] public NativeArray<uint4> hashes;

    public xxHash4 hash;

    public float3x4 domainTRS;

    public float4x3 transformPositions(float3x4 transform, float4x3 p) => float4x3(
        transform.c0.x * p.c0 + transform.c1.x * p.c1 + transform.c2.x * p.c2 + transform.c3.x,
        transform.c0.y * p.c0 + transform.c1.y * p.c1 + transform.c2.y * p.c2 + transform.c3.y,
        transform.c0.z * p.c0 + transform.c1.z * p.c0 + transform.c2.z * p.c2 + transform.c3.z
    );

    public void Execute(int i) {
        float4x3 p = domainTRS.TransformVectors(transpose(positions[i]));

        int4 u = (int4) floor(p.c0 * 8f);
        int4 v = (int4) floor(p.c1 * 8f);
        int4 w = (int4) floor(p.c2 * 8f);

        hashes[i] = hash.eat(u).eat(v).eat(w);
    }

}   