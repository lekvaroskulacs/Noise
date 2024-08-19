using System.Drawing;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using static Unity.Mathematics.math;

public static class Shapes {

    public struct Point4 {
        public float4x3 positions, normals;
    }

    public static float4x2 indexToUv(int i, float resolution, float invResolution) {
        float4x2 uvs;
        float4 i4 = 4f * i + float4(0f, 1f, 2f, 3f);
        uvs.c1 = floor(i4 * invResolution + 0.0001f);
        uvs.c0 = (i4 - resolution * uvs.c1) * invResolution;
        uvs.c1 = uvs.c1 * invResolution;

        return uvs;
    }

    public delegate JobHandle ScheduleDelegate (
		NativeArray<float3x4> positions, NativeArray<float3x4> normals,
		int resolution, float4x4 trs, JobHandle dependency
	);

    public interface IShape {

        public Point4 GetPoint4(int i, float resolution, float invResolution);

    }

    public struct Plane : IShape {

        public Point4 GetPoint4(int i, float resolution, float invResolution) {
            float4x2 uv = indexToUv(i, resolution, invResolution);
            return new Point4 {
                positions = float4x3(uv.c0 - 0.5f, 0f, uv.c1 - 0.5f),
                normals = float4x3(0f, 1f, 0f)
            };
        }
    }

    public struct Sphere : IShape {
        public Point4 GetPoint4(int i, float resolution, float invResolution) {
            float4x2 uv = indexToUv(i, resolution, invResolution);

            float4x3 pos;
            pos.c0 = uv.c0 - 0.5f;
            pos.c1 = uv.c1 - 0.5f;
            pos.c2 = 0.5f - abs(pos.c0) - abs(pos.c1);
            float4 offset = max(-pos.c2, 0f);
            pos.c0 += select(-offset, offset, pos.c0 < 0f);
            pos.c1 += select(-offset, offset, pos.c1 < 0f);

            float4 scale = 0.5f * rsqrt(pos.c0 * pos.c0 + pos.c1 * pos.c1 + pos.c2 * pos.c2);
            pos.c0 *= scale;
            pos.c1 *= scale;
            pos.c2 *= scale;    
            return new Point4 {
                positions = pos,
                normals = pos
            };

        }
    }

    public struct Torus : IShape {

		public Point4 GetPoint4 (int i, float resolution, float invResolution) {
			float4x2 uv = indexToUv(i, resolution, invResolution);

			float r1 = 0.375f;
			float r2 = 0.125f;
			float4 s = r1 + r2 * cos(2f * PI * uv.c1);

			Point4 p;
			p.positions.c0 = s * sin(2f * PI * uv.c0);
			p.positions.c1 = r2 * sin(2f * PI * uv.c1);
			p.positions.c2 = s * cos(2f * PI * uv.c0);
			p.normals = p.positions;
            p.normals.c0 -= r1 * sin(2f * PI * uv.c0);
			p.normals.c2 -= r1 * cos(2f * PI * uv.c0);
			return p;
		}
	}

    [BurstCompile(FloatPrecision.Standard, FloatMode.Fast, CompileSynchronously = true)]
    public struct Job<S> : IJobFor where S : struct, IShape {

        [WriteOnly] public NativeArray<float3x4> positions, normals;

        public float resolution, invResolution;

        public float3x4 positionTRS, normalTRS;

        public void Execute(int i) {

            Point4 p = default(S).GetPoint4(i, resolution, invResolution);

            positions[i] = transpose(positionTRS.TransformVectors(p.positions));

            float3x4 n = transpose(normalTRS.TransformVectors(p.normals, 0));
            normals[i] = new float3x4(normalize(n.c0), normalize(n.c1), normalize(n.c2), normalize(n.c3));

        }

        public static JobHandle scheduleParallel(
            NativeArray<float3x4> positions,
            NativeArray<float3x4> normals,
            int resolution, 
            float4x4 TRS,
            JobHandle dependency) => 
            
            new Job<S> {
                positions = positions,
                normals = normals,
                resolution = resolution,
                invResolution = 1f / resolution,
                positionTRS = TRS.Get3x4(),
                normalTRS = transpose(inverse(TRS)).Get3x4()
            }.ScheduleParallel(positions.Length, resolution, dependency);

    }

}