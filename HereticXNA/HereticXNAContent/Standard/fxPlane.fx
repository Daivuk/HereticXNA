float4x4 matWorldViewProj;
float4 ambientLimits;
float4 ambientLimitsZPixelSize;
float3 ambientEpsilonSizeAmount;
float lightLevel;

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

uniform extern texture AmbientTexture;
sampler ambientSampler = sampler_state
{
	Texture = <AmbientTexture>;
	mipfilter = LINEAR; 
	magfilter = LINEAR; 
	AddressU = Clamp;
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
	float4 TexCoord : TEXCOORD0;
	float2 AmbientTexCoord : TEXCOORD1;
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
	output.TexCoord = float4(input.TexCoord.xy, output.Position.z, input.Position.z);
	output.AmbientTexCoord = (input.Position.xy - ambientLimits.xy) / ambientLimits.zw;

    return output;
}

sVSOutNoAmb VSMainNoAmb(sVSIn input)
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



float applyAmbientOnIntensityFloor(float2 texCoord, float intensity, float floor)
{
	float ambient = 0;
	float2 tmp;
	float diff;

	for (float y = -2.5; y <= 2.5; ++y)
	{
		for (float x = -2.5; x <= 2.5; ++x)
		{
			tmp = tex2D(ambientSampler, 
				float2(	texCoord.x + x * ambientLimitsZPixelSize.z,
						texCoord.y + y * ambientLimitsZPixelSize.w)).rg * ambientLimitsZPixelSize.y + ambientLimitsZPixelSize.x;

			diff = ambientEpsilonSizeAmount.y - clamp(tmp.g - tmp.r, 0, ambientEpsilonSizeAmount.y);

			tmp.r = (tmp.r - floor);
			tmp.r = clamp(tmp.r + diff * 4, 0, ambientEpsilonSizeAmount.y) / ambientEpsilonSizeAmount.y;

			ambient += tmp.r;
		}
	}

	ambient /= 36;

	return lerp(intensity, intensity * ambientEpsilonSizeAmount.z * .5, ambient);
}



float4 PSMain(sVSOut input) : COLOR0
{
	// Texture color, and discard for alpha test
	float4 texColor = tex2D(diffuseSampler, input.TexCoord.xy);
	if (texColor.a < .5) discard;

	// Light level with distance fog
	float intensity = getIntensityFromDis(lightLevel, input.TexCoord.z);

	// Calculate ambient
	intensity = applyAmbientOnIntensityFloor(input.AmbientTexCoord, intensity, input.TexCoord.w).r;

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
	intensity = applyAmbientOnIntensityFloor(input.AmbientTexCoord, intensity, input.TexCoord.w);

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
        VertexShader = compile vs_3_0 VSMain();
        PixelShader = compile ps_3_0 PSMain();
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
        VertexShader = compile vs_3_0 VSMain();
        PixelShader = compile ps_3_0 PSMainNoTex();
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
