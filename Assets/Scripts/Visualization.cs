using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Jobs;
using Unity.Burst;
using Unity.Collections;
using Unity.VisualScripting;
using static Unity.Mathematics.math;
using Unity.Mathematics;


public abstract class Visualization : MonoBehaviour
{

    public enum Shape { Plane, Sphere, Torus }

	static Shapes.ScheduleDelegate[] shapeJobs = {
		Shapes.Job<Shapes.Plane>.scheduleParallel,
		Shapes.Job<Shapes.Sphere>.scheduleParallel,
		Shapes.Job<Shapes.Torus>.scheduleParallel
	};
	
	[SerializeField] Shape shape;

    static readonly int
        configId = Shader.PropertyToID("_config"),
        positionsId = Shader.PropertyToID("_positions"),
        normalsId = Shader.PropertyToID("_normals");

    [SerializeField] Mesh mesh;
    
    [SerializeField] Material material;
    
    [SerializeField] [Range(10, 200)] int resolution = 16;

    [SerializeField, Range(-0.5f, 0.5f)] float displacement = 0.1f;

    [SerializeField, Range(0.1f, 10f)] float instanceScale = 2f;


    NativeArray<float3x4> positions;

    NativeArray<float3x4> normals;

    MaterialPropertyBlock propertyBlock;

    ComputeBuffer positionsBuffer, normalsBuffer;

    protected bool isDirty;

    Bounds bounds;
    
    private void OnEnable() {
        isDirty = true;

        int length = resolution * resolution;
        length = length / 4 + (length & 1);
		positions = new NativeArray<float3x4>(length, Allocator.Persistent);
		normals = new NativeArray<float3x4>(length, Allocator.Persistent);
		positionsBuffer = new ComputeBuffer(length * 4, 3 * 4);
		normalsBuffer = new ComputeBuffer(length * 4, 3 * 4);

        propertyBlock ??= new MaterialPropertyBlock();
        enableVisualization(length, propertyBlock);
        propertyBlock.SetBuffer(positionsId, positionsBuffer);
        propertyBlock.SetBuffer(normalsId, normalsBuffer);
        propertyBlock.SetVector(configId, new Vector4(resolution, instanceScale / resolution, displacement));
    }

    private void OnDisable() {
        positionsBuffer.Release();
        positions.Dispose();
        positionsBuffer = null;
        normalsBuffer.Release();
        normals.Dispose();
        normalsBuffer = null;
        disableVisualization();
    }

    private void OnValidate() {
        if (positionsBuffer != null && enabled) {
            OnDisable();
            OnEnable();
        }
    }

    protected abstract void enableVisualization(int dataLength, MaterialPropertyBlock propertyBlock);

	protected abstract void disableVisualization();

    private void Update() {
        if (isDirty || transform.hasChanged) {
            isDirty = false;
            transform.hasChanged = false;

            UpdateVisualization(positions, resolution,
                shapeJobs[(int)shape](positions, normals, resolution, transform.worldToLocalMatrix, default));

            positionsBuffer.SetData(positions.Reinterpret<float3>(sizeof(float) * 4 * 3));
            normalsBuffer.SetData(normals.Reinterpret<float3>(sizeof(float) * 4 * 3));
            
            bounds = new Bounds(
				transform.position,
				float3(2f * cmax(abs(transform.lossyScale)) + displacement)
			);

        }

        Graphics.DrawMeshInstancedProcedural(mesh, 0, material, bounds, resolution * resolution, propertyBlock);
    }

    protected abstract void UpdateVisualization(NativeArray<float3x4> positions, int resolution, JobHandle handle);

}
