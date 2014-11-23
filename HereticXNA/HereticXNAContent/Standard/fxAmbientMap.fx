float4x4 matProjection;
float2 floorCeil;

struct sVSIn
{
    float3 Position : POSITION0;
};

struct sVSOut
{
    float4 Position : POSITION0;
};

sVSOut VSMain(sVSIn input)
{
    sVSOut output;

    output.Position = mul(float4(input.Position, 1), matProjection);

    return output;
}

float4 PSMain(sVSOut input) : COLOR0
{
    return float4(floorCeil.r, floorCeil.g, 0, 0);
}

technique Technique1
{
    pass Pass1
    {
        VertexShader = compile vs_2_0 VSMain();
        PixelShader = compile ps_2_0 PSMain();
    }
}
