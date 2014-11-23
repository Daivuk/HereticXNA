float4x4 matWorldViewProj;
float4 spriteSize2spritePosXY;
float4 camRight2SpritePosZLightLevel1;

uniform extern texture SpriteTexture;
sampler spriteSampler = sampler_state
{
	Texture = <SpriteTexture>;
	mipfilter = LINEAR; 
	magfilter = LINEAR; 
	AddressU = Clamp;
    AddressV = Clamp;
};

struct sVSIn
{
    float2 Position : POSITION0;
	float2 TexCoord : TEXCOORD0;
};

struct sVSOut
{
    float4 Position : POSITION0;
	float3 TexCoord : TEXCOORD0;
};

sVSOut VSMain(sVSIn input)
{
    sVSOut output;

	float3 position = float3(
		spriteSize2spritePosXY.zw + camRight2SpritePosZLightLevel1.xy * spriteSize2spritePosXY.x * input.Position.x,
		camRight2SpritePosZLightLevel1.z + input.Position.y * spriteSize2spritePosXY.y);

    output.Position = mul(float4(position, 1), matWorldViewProj);
	output.TexCoord = float3(input.TexCoord.xy, output.Position.z);

    return output;
}

float getIntensityFromDis(float sectorLight, float dis)
{
	dis = clamp(dis, 256, 8192 + 256) - 256;
	float intensity = sectorLight * sectorLight * 2.0;
	intensity = intensity - dis * 2.0 / intensity / 8192;

	return intensity;
}

float4 PSMain(sVSOut input) : COLOR0
{
	// Texture color, and discard for alpha test
	float4 texColor = tex2D(spriteSampler, input.TexCoord.xy);
	if (texColor.a < .5) discard;

	// Light level with distance fog
	float intensity = getIntensityFromDis(camRight2SpritePosZLightLevel1.w, input.TexCoord.z);

    return texColor * intensity;
}

technique Technique1
{
    pass Pass1
    {
        VertexShader = compile vs_2_0 VSMain();
        PixelShader = compile ps_2_0 PSMain();
    }
}
