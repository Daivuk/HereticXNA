float4x4 matWorldViewProj;
float2 cameraAngles;

uniform extern texture SkyTexture;
sampler skySampler = sampler_state
{
	Texture = <SkyTexture>;
	mipfilter = LINEAR; 
	magfilter = LINEAR; 
	AddressU = Wrap;
    AddressV = Mirror;
};

struct sVSIn
{
    float3 Position : POSITION0;
};

struct sVSOut
{
    float4 Position : POSITION0;
	float3 PositionTransfer : TEXCOORD0;
};

sVSOut VSMain(sVSIn input)
{
    sVSOut output;

    output.Position = mul(float4(input.Position.xyz, 1), matWorldViewProj);
	output.PositionTransfer = output.Position.xyz;

    return output;
}

float4 PSMain(sVSOut input) : COLOR0
{
	float2 texCoord = float2(input.PositionTransfer.xy / input.PositionTransfer.z);
	texCoord.y = 1 - texCoord.y / 1.5625;
	texCoord.x /= 2;
	texCoord.x -= cameraAngles.x / 3.141592 * 2;
	float angleX = cameraAngles.y / 3.141592 * 2;
	texCoord.y -= angleX * 1.5625;

	float4 texColor = tex2D(skySampler, texCoord);

    return texColor;
}

technique Technique1
{
    pass Pass1
    {
        VertexShader = compile vs_2_0 VSMain();
        PixelShader = compile ps_2_0 PSMain();
    }
}
