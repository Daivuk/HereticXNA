float4x4 matWorldViewProj;
float lightLevel;
float3 ambientEpsilonSizeAmount;

uniform extern texture DiffuseTexture;
sampler diffuseSampler = sampler_state
{
	Texture = <DiffuseTexture>;
	MinFilter = Anisotropic; // Minification Filter
	MagFilter = Linear; // Magnification Filter
	MipFilter = Linear; // Mip-mapping	
	AddressU = Wrap;
    AddressV = Wrap;
};

struct sVSIn
{
    float3 Position : POSITION0;
	float4 Color : COLOR0;
	float2 TexCoord : TEXCOORD0;
};

struct sVSOut
{
    float4 Position : POSITION0;
	float4 Color : COLOR0;
	float3 TexCoord : TEXCOORD0;
};

struct sVSInNoAmb
{
    float3 Position : POSITION0;
	float2 TexCoord : TEXCOORD0;
};

struct sVSOutNoAmb
{
    float4 Position : POSITION0;
	float3 TexCoord : TEXCOORD0;
};



sVSOut VSMain(sVSIn input)
{
    sVSOut output;

    output.Position = mul(float4(input.Position.xyz, 1), matWorldViewProj);
	output.Color = input.Color;
	output.TexCoord = float3(input.TexCoord.xy, output.Position.z);

    return output;
}

sVSOutNoAmb VSMainNoAmb(sVSInNoAmb input)
{
    sVSOutNoAmb output;

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

float applyVAmbientOnIntensity(float floorDis, float ceilDis, float intensity)
{
	float ambient = floorDis * ambientEpsilonSizeAmount.x;
	ambient = clamp(ambient, 0, ambientEpsilonSizeAmount.y) / ambientEpsilonSizeAmount.y;
	ambient = 1 - (1 - ambient) * (1 - ambient);
	intensity = lerp(intensity * ambientEpsilonSizeAmount.z, intensity, ambient);

	ambient = ceilDis * ambientEpsilonSizeAmount.x;
	ambient = clamp(ambient, 0, ambientEpsilonSizeAmount.y) / ambientEpsilonSizeAmount.y;
	ambient = 1 - (1 - ambient) * (1 - ambient);
	intensity = lerp(intensity * ambientEpsilonSizeAmount.z, intensity, ambient);

	return intensity;
}

float applyHAmbientOnIntensity(float leftDis, float rightDis, float intensity)
{
	float ambient = leftDis * ambientEpsilonSizeAmount.x;
	ambient = clamp(ambient, 0, ambientEpsilonSizeAmount.y) / ambientEpsilonSizeAmount.y;
	ambient = 1 - (1 - ambient) * (1 - ambient);
	intensity = lerp(intensity * ambientEpsilonSizeAmount.z, intensity, ambient);

	ambient = rightDis * ambientEpsilonSizeAmount.x;
	ambient = clamp(ambient, 0, ambientEpsilonSizeAmount.y) / ambientEpsilonSizeAmount.y;
	ambient = 1 - (1 - ambient) * (1 - ambient);
	intensity = lerp(intensity * ambientEpsilonSizeAmount.z, intensity, ambient);

	return intensity;
}



float4 PSMain(sVSOut input) : COLOR0
{
	// Texture color, and discard for alpha test
	float4 texColor = tex2D(diffuseSampler, input.TexCoord.xy);
	if (texColor.a < .5) discard;

	// Light level with distance fog
	float intensity = getIntensityFromDis(lightLevel, input.TexCoord.z);

	// Calculate ambient
	intensity = applyVAmbientOnIntensity(input.Color.r, input.Color.g, intensity);
	intensity = applyHAmbientOnIntensity(input.Color.b, input.Color.a, intensity);

    return texColor * intensity;
}

float4 PSMainNoAmb(sVSOutNoAmb input) : COLOR0
{
	// Texture color, and discard for alpha test
	float4 texColor = tex2D(diffuseSampler, input.TexCoord.xy);
	if (texColor.a < .5) discard;

	// Light level with distance fog
	float intensity = getIntensityFromDis(lightLevel, input.TexCoord.z);

    return texColor * intensity;
}

float4 PSMainNoTex(sVSOut input) : COLOR0
{
	// Texture color, and discard for alpha test
	float4 texColor = tex2D(diffuseSampler, input.TexCoord.xy);
	if (texColor.a < .5) discard;

	// Light level with distance fog
	float intensity = getIntensityFromDis(lightLevel, input.TexCoord.z);

	// Calculate ambient
	intensity = applyVAmbientOnIntensity(input.Color.r, input.Color.g, intensity);
	intensity = applyHAmbientOnIntensity(input.Color.b, input.Color.a, intensity);

    return intensity;
}

float4 PSMainNoTexNoAmb(sVSOutNoAmb input) : COLOR0
{
	// Texture color, and discard for alpha test
	float4 texColor = tex2D(diffuseSampler, input.TexCoord.xy);
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
        VertexShader = compile vs_2_0 VSMainNoAmb();
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
        VertexShader = compile vs_2_0 VSMainNoAmb();
        PixelShader = compile ps_2_0 PSMainNoTexNoAmb();
    }
}
