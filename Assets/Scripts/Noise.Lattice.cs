using Unity.Mathematics;
using Unity.VisualScripting;

using static Unity.Mathematics.math;

public static partial class Noise {

    struct LatticeSpan4 {
		public int4 p0, p1;
		public float4 t;
        public float4 g0, g1;

        
	}

    static LatticeSpan4 GetLatticeSpan4(float4 coordinates) {
        LatticeSpan4 span;
        span.p0 = (int4) floor(coordinates);
        span.p1 = span.p0 + 1;
        span.t = coordinates - span.p0;
        span.t = span.t * span.t * span.t * (6 * span.t * span.t - 15 * span.t + 10);
        span.g0 = coordinates - span.p0;
        span.g1 = span.g0 - 1f;
        return span;
    }

    public struct Lattice1D<G> : INoise where G : struct, IGradient {
        public float4 GetNoise4(float4x3 position, xxHash4 hash) {
            LatticeSpan4 x = GetLatticeSpan4(position.c0);
            var g = default(G);
            return lerp(g.Evaluate(hash.eat(x.p0), x.g0), g.Evaluate(hash.eat(x.p1), x.g1), x.t);
        }
    }

    public struct Lattice2D<G> : INoise where G : struct, IGradient {
        public float4 GetNoise4(float4x3 position, xxHash4 hash) {
            LatticeSpan4 x = GetLatticeSpan4(position.c0);
            LatticeSpan4 z = GetLatticeSpan4(position.c2);
            xxHash4 h0 = hash.eat(x.p0), h1 = hash.eat(x.p1);
            var g = default(G);
            return lerp(
                lerp(g.Evaluate(h0.eat(z.p0), x.g0, z.g0), g.Evaluate(h0.eat(z.p1), x.g0, z.g1), z.t), 
                lerp(g.Evaluate(h1.eat(z.p0), x.g1, z.g0), g.Evaluate(h1.eat(z.p1), x.g1, z.g1), z.t), 
                x.t);
        }
    }

    public struct Lattice3D<G> : INoise where G : struct, IGradient {
        public float4 GetNoise4(float4x3 position, xxHash4 hash) {
            LatticeSpan4 x = GetLatticeSpan4(position.c0);
            LatticeSpan4 z = GetLatticeSpan4(position.c2);
            LatticeSpan4 y = GetLatticeSpan4(position.c1);

            xxHash4 h0 = hash.eat(x.p0), h1 = hash.eat(x.p1);

            xxHash4 h00 = h0.eat(z.p0), h01 = h0.eat(z.p1);
            xxHash4 h10 = h1.eat(z.p0), h11 = h1.eat(z.p1);

            var g = default(G);

            return  lerp(
                        lerp(
                            lerp(g.Evaluate(h00.eat(y.p0), x.g0, y.g0, z.g0), g.Evaluate(h00.eat(y.p1), x.g0, y.g1, z.g0), y.t), 
                            lerp(g.Evaluate(h01.eat(y.p0), x.g0, y.g0, z.g1), g.Evaluate(h01.eat(y.p1), x.g0, y.g1, z.g1), y.t),
                            z.t), 
                        lerp(
                            lerp(g.Evaluate(h10.eat(y.p0), x.g1, y.g0, z.g0), g.Evaluate(h10.eat(y.p1), x.g1, y.g1, z.g0), y.t),
                            lerp(g.Evaluate(h11.eat(y.p0), x.g1, y.g0, z.g1), g.Evaluate(h11.eat(y.p1), x.g1, y.g1, z.g1), y.t),
                            z.t),
                    x.t);
        }
    }

}