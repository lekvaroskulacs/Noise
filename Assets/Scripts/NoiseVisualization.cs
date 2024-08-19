using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Jobs;
using Unity.Burst;
using Unity.Collections;
using Unity.VisualScripting;
using static Unity.Mathematics.math;
using Unity.Mathematics;


public class NoiseVisualization : Visualization
{

    static readonly int noiseId = Shader.PropertyToID("_noise");

    [SerializeField] int seed;

    public enum NoiseType { value, perlin };

    [SerializeField] NoiseType noiseType;

    NativeArray<float4> noise;

    ComputeBuffer noiseBuffer;

    [SerializeField] SpaceTRS domain = new SpaceTRS {
		scale = 8f
	};

    [SerializeField] Noise.scheduleDelegate[,] noiseJobs = {
        {
            Noise.Job<Noise.Lattice1D<Noise.Value>>.scheduleParallel,
            Noise.Job<Noise.Lattice2D<Noise.Value>>.scheduleParallel,
            Noise.Job<Noise.Lattice3D<Noise.Value>>.scheduleParallel
        },
        {
            Noise.Job<Noise.Lattice1D<Noise.Perlin>>.scheduleParallel,
            Noise.Job<Noise.Lattice2D<Noise.Perlin>>.scheduleParallel,
            Noise.Job<Noise.Lattice3D<Noise.Perlin>>.scheduleParallel
        }
    };

    [SerializeField, Range(1, 3)] int dimensions = 3; 
    
    protected override void enableVisualization(int dataLength, MaterialPropertyBlock propertyBlock) {
        isDirty = true;
        noise = new NativeArray<float4>(dataLength, Allocator.Persistent);
		noiseBuffer = new ComputeBuffer(dataLength * 4, 4);

        propertyBlock.SetBuffer(noiseId, noiseBuffer);
    }

    protected override void disableVisualization() {
        noiseBuffer.Release();
        noise.Dispose();
        noiseBuffer = null;
    }


    protected override void UpdateVisualization(
        NativeArray<float3x4> positions, 
        int resolution, 
        JobHandle handle
        ) {

            noiseJobs[(int)noiseType, dimensions - 1](positions, noise, seed, domain, resolution, handle).Complete();
            
            noiseBuffer.SetData(noise.Reinterpret<float4>(sizeof(float) * 4));

        }


}
