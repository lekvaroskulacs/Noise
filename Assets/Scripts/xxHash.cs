using Unity.Mathematics;

readonly public struct xxHash {

    const uint primeA = 0b10011110001101110111100110110001;
	const uint primeB = 0b10000101111010111100101001110111;
	const uint primeC = 0b11000010101100101010111000111101;
	const uint primeD = 0b00100111110101001110101100101111;
	const uint primeE = 0b00010110010101100110011110110001;

    public readonly uint accumulator;

    public static xxHash seed (int seed) => (uint)seed + primeE;

    public xxHash(uint accumulator) {
        this.accumulator = accumulator;
    }

    public xxHash eat(int data) =>
         rotateLeft(accumulator + (uint)data * primeC, 17) * primeD;
    

    public xxHash eat(byte data) =>
	    rotateLeft(accumulator + data * primeE, 11) * primeA;

    public static implicit operator uint(xxHash hash) {
        uint avalanche = hash.accumulator;
        avalanche ^= avalanche >> 15;
        avalanche *= primeB;
        avalanche ^= avalanche << 13;
        avalanche *= primeC;
        avalanche ^= avalanche >> 16;
        return avalanche;
    }

    public static implicit operator xxHash(uint accumulator) => new xxHash(accumulator);

    public static uint rotateLeft(uint data, int steps) => (data << steps) | (data >> 32 - steps);
}
    
readonly public struct xxHash4 {

	const uint primeB = 0b10000101111010111100101001110111;
	const uint primeC = 0b11000010101100101010111000111101;
	const uint primeD = 0b00100111110101001110101100101111;
	const uint primeE = 0b00010110010101100110011110110001;

    public readonly uint4 accumulator;

    public static xxHash4 seed (int4 seed) => (uint4)seed + primeE;

    public xxHash4(uint4 accumulator) {
        this.accumulator = accumulator;
    }

    public xxHash4 eat(int4 data) =>
         rotateLeft(accumulator + (uint4)data * primeC, 17) * primeD;
    

    public static implicit operator uint4(xxHash4 hash) {
        uint4 avalanche = hash.accumulator;
        avalanche ^= avalanche >> 15;
        avalanche *= primeB;
        avalanche ^= avalanche << 13;
        avalanche *= primeC;
        avalanche ^= avalanche >> 16;
        return avalanche;
    }

    public static implicit operator xxHash4(uint4 accumulator) => new xxHash4(accumulator);

    public static implicit operator xxHash4(xxHash hash) => new xxHash4(hash.accumulator);

    public static uint4 rotateLeft(uint4 data, int steps) => (data << steps) | (data >> 32 - steps);

    public uint4 BytesA => (uint4)this & 255;

	public uint4 BytesB => ((uint4)this >> 8) & 255;

	public uint4 BytesC => ((uint4)this >> 16) & 255;

	public uint4 BytesD => (uint4)this >> 24;

	public float4 Floats01A => (float4)BytesA * (1f / 255f);

	public float4 Floats01B => (float4)BytesB * (1f / 255f);

	public float4 Floats01C => (float4)BytesC * (1f / 255f);

	public float4 Floats01D => (float4)BytesD * (1f / 255f);
}
    
