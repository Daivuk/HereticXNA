float4x4 World;
float4x4 View;
float4x4 Projection;
float3 CamPos;
float lightLevel;
float4 tint;
float ambientEpsilon;
float ambientSize;
float ambientAmount;
float2 floorCeil;
float AnimT;
float2 uvOffset;

uniform extern texture DiffuseTexture;
sampler diffuseSampler = sampler_state
{
	Texture = <DiffuseTexture>;
	mipfilter = LINEAR; 
	magfilter = LINEAR; 
	AddressU = Wrap;
    AddressV = Wrap;
};

uniform extern texture AmbientTexture;
sampler ambientSampler = sampler_state
{
	Texture = <AmbientTexture>;
	mipfilter = LINEAR; 
	magfilter = LINEAR; 
	AddressU = Wrap;
    AddressV = Wrap;
};

uniform extern texture AnimatedTexture1;
sampler animatedSampler1 = sampler_state
{
	Texture = <AnimatedTexture1>;
	mipfilter = LINEAR; 
	magfilter = LINEAR; 
	AddressU = Wrap;
    AddressV = Wrap;
};

uniform extern texture AnimatedTexture2;
sampler animatedSampler2 = sampler_state
{
	Texture = <AnimatedTexture2>;
	mipfilter = LINEAR; 
	magfilter = LINEAR; 
	AddressU = Wrap;
    AddressV = Wrap;
};

struct VertexShaderInput
{
    float4 Position : POSITION0;
	float3 Normal : NORMAL;
	float2 TexCoord : TEXCOORD0;
	float4 Color : COLOR0;
};

struct VertexShaderOutput
{
    float4 Position : POSITION0;
	float3 TexCoord : TEXCOORD0;
	float2 AmbientTexCoord : TEXCOORD1;
	float4 Color : COLOR0;
};

struct VertexShaderOutput_deferred
{
    float4 Position : POSITION0;
	float3 Normal : NORMAL;
	float2 TexCoord : TEXCOORD0;
	float2 Depth : TEXCOORD1;
	float2 AmbientTexCoord : TEXCOORD2;
	float4 Color : COLOR0;
};

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
    VertexShaderOutput output;

    float4 worldPosition = mul(input.Position, World);
    float4 viewPosition = mul(worldPosition, View);
    output.Position = mul(viewPosition, Projection);

	output.TexCoord.xy = input.TexCoord.xy;
	output.TexCoord.z = distance(worldPosition.xyz, CamPos); // This is the distance with the camera
	output.AmbientTexCoord.x = ((worldPosition.x + 32768) / (65536.0 / 4) + .5);
	output.AmbientTexCoord.y = -((worldPosition.y + 32768) / (65536.0 / 4) + .5);
	output.Color = input.Color;

    return output;
}

VertexShaderOutput_deferred VertexShaderFunction_deferred(VertexShaderInput input)
{
    VertexShaderOutput_deferred output;

    float4 worldPosition = mul(input.Position, World);
    float4 viewPosition = mul(worldPosition, View);
    output.Position = mul(viewPosition, Projection);

	output.TexCoord.xy = input.TexCoord.xy;
	output.AmbientTexCoord.x = ((worldPosition.x + 32768) / (65536.0 / 4) + .5);
	output.AmbientTexCoord.y = -((worldPosition.y + 32768) / (65536.0 / 4) + .5);
	output.Depth = output.Position.zw;
	output.Color = input.Color;
	output.Normal = input.Normal;

    return output;
}

float getIntensityFromDis(float sectorLight, float dis)
{
	dis = clamp(dis, 256, 8192 + 256) - 256;
	float intensity = sectorLight * sectorLight * 2.0;
	intensity = intensity - dis * 2.0 / intensity / 8192;

	return intensity;
}

float2 applyAmbientOnIntensity(float2 texCoord, float intensity)
{
	float2 ambient = 0;
	float pixelSize = 1.0 / 2048.0;
	pixelSize /= 2.5;
	float2 tmp;
	float floor = floorCeil.x * 2048.0;
	float ceil = floorCeil.y * 2048.0;
	float diff;

	for (float y = -5; y <= 5; y += 1.0)
	{
		for (float x = -5; x <= 5; x += 1.0)
		{
			tmp = tex2D(ambientSampler, 
				float2(	texCoord.x + x * pixelSize,
						texCoord.y + y * pixelSize)).rg * 2048.0;

			diff = ambientSize - clamp(tmp.g - tmp.r, 0, ambientSize);

			tmp.r = (tmp.r - floor);
			tmp.r = clamp(tmp.r + diff * 4, 0, ambientSize) / ambientSize;

			tmp.g = (ceil - tmp.g);
			tmp.g = clamp(tmp.g + diff * 4, 0, ambientSize) / ambientSize;

			ambient += tmp;
		}
	}

	ambient /= 121;

	tmp.r = lerp(intensity, intensity * ambientAmount * .5, ambient.r);
	tmp.g = lerp(intensity, intensity * ambientAmount * .5, ambient.g);

	return tmp;
}

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
	// Texture color, and discard for alpha test
	float4 texColor = tex2D(diffuseSampler, input.TexCoord.xy + uvOffset);
	if (texColor.a < .5) discard;

	// Light level with distance fog
	float intensity = getIntensityFromDis(lightLevel, input.TexCoord.z);

	// Calculate ambient
	float2 ambientIntensity = applyAmbientOnIntensity(input.AmbientTexCoord, intensity);
	intensity = lerp(ambientIntensity.r, ambientIntensity.g, input.Color.r);

    return texColor * intensity * tint;
}

struct PixelShaderOutput_deferred
{
	float4 Color : SV_Target0;
	float4 Depth : SV_Target1;
	float4 Normal: SV_Target2;
};

PixelShaderOutput_deferred PixelShaderFunction_deferred(VertexShaderOutput_deferred input) : COLOR0
{
	// Texture color, and discard for alpha test
	float4 texColor = tex2D(diffuseSampler, input.TexCoord.xy + uvOffset);
	if (texColor.a < .5) discard;

	// Calculate ambient
	float2 ambientIntensity = applyAmbientOnIntensity(input.AmbientTexCoord, 1);
	float intensity = lerp(ambientIntensity.r, ambientIntensity.g, input.Color.r);

	PixelShaderOutput_deferred output;

	output.Color = texColor * intensity * tint;
	output.Depth = input.Depth.x / input.Depth.y;
	output.Normal.xyz = input.Normal * .5 + .5;
	output.Normal.a = lightLevel;

	return output;
}

float4 PixelShaderFunctionAnimatedFloor(VertexShaderOutput input) : COLOR0
{
	// Texture color, and discard for alpha test
	float4 texColor = tex2D(animatedSampler1, input.TexCoord + uvOffset);
	float4 texColor2 = tex2D(animatedSampler2, input.TexCoord + uvOffset);
	texColor = lerp(texColor2, texColor, AnimT);
	if (texColor.a < .5) discard;

	// Light level with distance fog
	float intensity = getIntensityFromDis(lightLevel, input.TexCoord.z);

	// Calculate ambient
	float2 ambientIntensity = applyAmbientOnIntensity(input.AmbientTexCoord, intensity);
	intensity = lerp(ambientIntensity.r, ambientIntensity.g, input.Color.r);

    return texColor * intensity * tint;
}

PixelShaderOutput_deferred PixelShaderFunctionAnimatedFloor_deferred(VertexShaderOutput_deferred input) : COLOR0
{
	// Texture color, and discard for alpha test
	float4 texColor = tex2D(animatedSampler1, input.TexCoord + uvOffset);
	float4 texColor2 = tex2D(animatedSampler2, input.TexCoord + uvOffset);
	texColor = lerp(texColor2, texColor, AnimT);
	if (texColor.a < .5) discard;

	// Calculate ambient
	float2 ambientIntensity = applyAmbientOnIntensity(input.AmbientTexCoord, 1);
	float intensity = lerp(ambientIntensity.r, ambientIntensity.g, input.Color.r);

	PixelShaderOutput_deferred output;

	output.Color = texColor * intensity * tint;
	output.Depth = input.Depth.x / input.Depth.y;
	output.Normal.xyz = input.Normal * .5 + .5;
	output.Normal.a = lightLevel;

	return output;
}

float4 PixelShaderFunctionNoTexture(VertexShaderOutput input) : COLOR0
{
	// Texture color, and discard for alpha test
	float4 texColor = tex2D(diffuseSampler, input.TexCoord.xy + uvOffset);
	if (texColor.a < .5) discard;

	// Light level with distance fog
	float intensity = getIntensityFromDis(lightLevel, input.TexCoord.z);

	// Calculate ambient
	float2 ambientIntensity = applyAmbientOnIntensity(input.AmbientTexCoord, intensity);
	intensity = lerp(ambientIntensity.r, ambientIntensity.g, input.Color.r);

	return float4(intensity.rrr, 1) * tint;
}

float4 PixelShaderFunctionNoAmbient(VertexShaderOutput input) : COLOR0
{
	// Texture color, and discard for alpha test
	float4 texColor = tex2D(diffuseSampler, input.TexCoord.xy + uvOffset);
	if (texColor.a < .5) discard;

	// Light level with distance fog
	float intensity = getIntensityFromDis(lightLevel, input.TexCoord.z);

    return texColor * intensity * tint;
}

float4 PixelShaderFunctionNoTextureNoAmbient(VertexShaderOutput input) : COLOR0
{
	// Texture color, and discard for alpha test
	float4 texColor = tex2D(diffuseSampler, input.TexCoord.xy + uvOffset);
	if (texColor.a < .5) discard;

	// Light level with distance fog
	float intensity = getIntensityFromDis(lightLevel, input.TexCoord.z);

	return float4(intensity.rrr, 1) * tint;
}

technique TechniqueMain
{
    pass Pass1
    {
        VertexShader = compile vs_3_0 VertexShaderFunction();
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}

technique TechniqueNoTexture
{
    pass Pass1
    {
        VertexShader = compile vs_3_0 VertexShaderFunction();
        PixelShader = compile ps_3_0 PixelShaderFunctionNoTexture();
    }
}

technique TechniqueNoAmbient
{
    pass Pass1
    {
        VertexShader = compile vs_3_0 VertexShaderFunction();
        PixelShader = compile ps_3_0 PixelShaderFunctionNoAmbient();
    }
}

technique TechniqueNoTextureNoAmbient
{
    pass Pass1
    {
        VertexShader = compile vs_3_0 VertexShaderFunction();
        PixelShader = compile ps_3_0 PixelShaderFunctionNoTextureNoAmbient();
    }
}

technique TechniqueAnimatedFloor
{
    pass Pass1
    {
        VertexShader = compile vs_3_0 VertexShaderFunction();
        PixelShader = compile ps_3_0 PixelShaderFunctionAnimatedFloor();
    }
}

technique TechniqueMain_deferred
{
    pass Pass1
    {
        VertexShader = compile vs_3_0 VertexShaderFunction_deferred();
        PixelShader = compile ps_3_0 PixelShaderFunction_deferred();
    }
}

technique TechniqueNoTexture_deferred
{
    pass Pass1
    {
        VertexShader = compile vs_3_0 VertexShaderFunction_deferred();
        PixelShader = compile ps_3_0 PixelShaderFunctionNoTexture();
    }
}

technique TechniqueNoAmbient_deferred
{
    pass Pass1
    {
        VertexShader = compile vs_3_0 VertexShaderFunction_deferred();
        PixelShader = compile ps_3_0 PixelShaderFunctionNoAmbient();
    }
}

technique TechniqueNoTextureNoAmbient_deferred
{
    pass Pass1
    {
        VertexShader = compile vs_3_0 VertexShaderFunction_deferred();
        PixelShader = compile ps_3_0 PixelShaderFunctionNoTextureNoAmbient();
    }
}

technique TechniqueAnimatedFloor_deferred
{
    pass Pass1
    {
        VertexShader = compile vs_3_0 VertexShaderFunction_deferred();
        PixelShader = compile ps_3_0 PixelShaderFunctionAnimatedFloor_deferred();
    }
}
