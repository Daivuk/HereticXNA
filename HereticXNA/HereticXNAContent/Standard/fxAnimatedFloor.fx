float4x4 matWorldViewProj;
float lightLevel;
float animDelta;

uniform extern texture DiffuseTexture1;
sampler diffuseSampler1 = sampler_state
{
	Texture = <DiffuseTexture1>;
	MinFilter = Anisotropic; // Minification Filter
	MagFilter = Linear; // Magnification Filter
	MipFilter = Linear; // Mip-mapping	
	AddressU = Wrap;
    AddressV = Wrap;
};

uniform extern texture DiffuseTexture2;
sampler diffuseSampler2 = sampler_state
{
	Texture = <DiffuseTexture2>;
	MinFilter = Anisotropic; // Minification Filter
	MagFilter = Linear; // Magnification Filter
	MipFilter = Linear; // Mip-mapping	
	AddressU = Wrap;
    AddressV = Wrap;
};

struct sVSIn
{
    float3 Position : POSITION0;
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

    output.Position = mul(float4(input.Position.xyz, 1), matWorldViewProj);
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
	float4 texColor1 = tex2D(diffuseSampler1, input.TexCoord.xy);
	float4 texColor2 = tex2D(diffuseSampler2, input.TexCoord.xy);
	float4 texColor = lerp(texColor1, texColor2, animDelta);
	if (texColor.a < .5) discard;

	// Light level with distance fog
	float intensity = getIntensityFromDis(lightLevel, input.TexCoord.z);

	// Calculate ambient

    return texColor * intensity;
}

float4 PSMainNoAmb(sVSOut input) : COLOR0
{
	// Texture color, and discard for alpha test
	float4 texColor1 = tex2D(diffuseSampler1, input.TexCoord.xy);
	float4 texColor2 = tex2D(diffuseSampler2, input.TexCoord.xy);
	float4 texColor = lerp(texColor1, texColor2, animDelta);
	if (texColor.a < .5) discard;

	// Light level with distance fog
	float intensity = getIntensityFromDis(lightLevel, input.TexCoord.z);

    return texColor * intensity;
}

float4 PSMainNoTex(sVSOut input) : COLOR0
{
	// Texture color, and discard for alpha test
	float4 texColor1 = tex2D(diffuseSampler1, input.TexCoord.xy);
	float4 texColor2 = tex2D(diffuseSampler2, input.TexCoord.xy);
	float4 texColor = lerp(texColor1, texColor2, animDelta);
	if (texColor.a < .5) discard;

	// Light level with distance fog
	float intensity = getIntensityFromDis(lightLevel, input.TexCoord.z);

	// Calculate ambient

    return intensity;
}

float4 PSMainNoTexNoAmb(sVSOut input) : COLOR0
{
	// Texture color, and discard for alpha test
	float4 texColor1 = tex2D(diffuseSampler1, input.TexCoord.xy);
	float4 texColor2 = tex2D(diffuseSampler2, input.TexCoord.xy);
	float4 texColor = lerp(texColor1, texColor2, animDelta);
	if (texColor.a < .5) discard;

	// Light level with distance fog
	float intensity = getIntensityFromDis(lightLevel, input.TexCoord.z);

    return intensity;
}



technique Technique1
{
    pass Pass1
    {
        VertexShader = compile vs_2_0 VSMain();
        PixelShader = compile ps_2_0 PSMain();
    }
}

technique TechniqueNoAmb
{
    pass Pass1
    {
        VertexShader = compile vs_2_0 VSMain();
        PixelShader = compile ps_2_0 PSMainNoAmb();
    }
}

technique TechniqueNoTex
{
    pass Pass1
    {
        VertexShader = compile vs_2_0 VSMain();
        PixelShader = compile ps_2_0 PSMainNoTex();
    }
}

technique TechniqueNoTexNoAmb
{
    pass Pass1
    {
        VertexShader = compile vs_2_0 VSMain();
        PixelShader = compile ps_2_0 PSMainNoTexNoAmb();
    }
}
