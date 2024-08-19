using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Jobs;
using Unity.Burst;
using Unity.Collections;
using Unity.VisualScripting;
using static Unity.Mathematics.math;
using Unity.Mathematics;


public class HashVisualization : Visualization
{

    static readonly int hashesId = Shader.PropertyToID("_hashes");

    [SerializeField] int seed;

    NativeArray<uint4> hashes;

    ComputeBuffer hashesBuffer;

    [SerializeField] SpaceTRS domain = new SpaceTRS {
		scale = 8f
	};
    
    protected override void enableVisualization(int dataLength, MaterialPropertyBlock propertyBlock) {
        isDirty = true;
        hashes = new NativeArray<uint4>(dataLength, Allocator.Persistent);
		hashesBuffer = new ComputeBuffer(dataLength * 4, 4);

        propertyBlock.SetBuffer(hashesId, hashesBuffer);
    }

    protected override void disableVisualization() {
        hashesBuffer.Release();
        hashes.Dispose();
        hashesBuffer = null;
    }


    protected override void UpdateVisualization(
        NativeArray<float3x4> positions, 
        int resolution, 
        JobHandle handle
        ) {

            new HashJob() {
                positions = positions,
                hashes = hashes,
                hash = xxHash4.seed(seed),
                domainTRS = domain.Matrix
            }.ScheduleParallel(hashes.Length, resolution, handle).Complete();
    
            hashesBuffer.SetData(hashes.Reinterpret<uint>(sizeof(uint) * 4));

        }


}
