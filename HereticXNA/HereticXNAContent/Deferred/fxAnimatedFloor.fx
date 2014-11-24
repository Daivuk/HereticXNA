float4x4 matWorldViewProj;
float4 ambientLimits;
float4 ambientLimitsZPixelSize;
float3 ambientEpsilonSizeAmount;
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
	float3 Normal : NORMAL0;
	float2 TexCoord : TEXCOORD0;
};

struct sVSOut
{
    float4 Position : POSITION0;
	float3 Normal : TEXCOORD0;
	float2 TexCoord : TEXCOORD1;
	float3 AmbientTexCoord : TEXCOORD2;
	float2 Depth : TEXCOORD3;
};

struct sVSOutNoAmb
{
    float4 Position : POSITION0;
	float3 Normal : TEXCOORD0;
	float2 TexCoord : TEXCOORD1;
	float2 Depth : TEXCOORD2;
};



sVSOut VSMain(sVSIn input)
{
    sVSOut output;

    output.Position = mul(float4(input.Position.xyz, 1), matWorldViewProj);
	output.Normal = input.Normal;
	output.TexCoord = input.TexCoord;
	output.AmbientTexCoord = float3((input.Position.xy - ambientLimits.xy) / ambientLimits.zw, input.Position.z);
	output.Depth.xy = output.Position.zw;

    return output;
}

sVSOutNoAmb VSMainNoAmb(sVSIn input)
{
    sVSOutNoAmb output;

    output.Position = mul(float4(input.Position.xyz, 1), matWorldViewProj);
	output.Normal = input.Normal;
	output.TexCoord = input.TexCoord;
	output.Depth.xy = output.Position.zw;

    return output;
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



struct PSOut
{
	float4 Color : SV_Target0;
	float4 Depth : SV_Target1;
	float4 Normal: SV_Target2;
};



PSOut PSMain(sVSOut input)
{
	// Texture color, and discard for alpha test
	float4 texColor1 = tex2D(diffuseSampler1, input.TexCoord.xy);
	float4 texColor2 = tex2D(diffuseSampler2, input.TexCoord.xy);
	float4 texColor = lerp(texColor1, texColor2, animDelta);

	// Light level with distance fog
	float intensity = 1;

	// Calculate ambient
	intensity = applyAmbientOnIntensityFloor(input.AmbientTexCoord.xy, intensity, input.AmbientTexCoord.z);

	PSOut output;
	output.Color = texColor * intensity;
	output.Depth = input.Depth.x / input.Depth.y;
	output.Normal.xyz = input.Normal.xyz * .5 + .5;
	output.Normal.a = 0;

    return output;
}

PSOut PSMainNoAmb(sVSOutNoAmb input)
{
	// Texture color, and discard for alpha test
	float4 texColor1 = tex2D(diffuseSampler1, input.TexCoord.xy);
	float4 texColor2 = tex2D(diffuseSampler2, input.TexCoord.xy);
	float4 texColor = lerp(texColor1, texColor2, animDelta);

	PSOut output;
	output.Color = texColor;
	output.Depth = input.Depth.x / input.Depth.y;
	output.Normal.xyz = input.Normal.xyz * .5 + .5;
	output.Normal.a = 0;

    return output;
}

PSOut PSMainNoTex(sVSOut input)
{
	// Light level with distance fog
	float intensity = 1;

	// Calculate ambient
	intensity = applyAmbientOnIntensityFloor(input.AmbientTexCoord.xy, intensity, input.AmbientTexCoord.z);

	PSOut output;
	output.Color = intensity;
	output.Depth = input.Depth.x / input.Depth.y;
	output.Normal.xyz = input.Normal.xyz * .5 + .5;
	output.Normal.a = 0;

    return output;
}

PSOut PSMainNoTexNoAmb(sVSOutNoAmb input)
{
	PSOut output;
	output.Color = 1;
	output.Depth = input.Depth.x / input.Depth.y;
	output.Normal.xyz = input.Normal.xyz * .5 + .5;
	output.Normal.a = 0;

    return output;
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
