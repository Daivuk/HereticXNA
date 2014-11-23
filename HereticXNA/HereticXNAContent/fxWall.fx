float4x4 World;
float4x4 View;
float4x4 Projection;
float3 CamPos;
float lightLevel;
float4 tint;
float ambientEpsilon;
float ambientSize;
float ambientAmount;
float AnimT;

uniform extern texture DiffuseTexture;
sampler diffuseSampler = sampler_state
{
	Texture = <DiffuseTexture>;
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
	float4 Color : COLOR0;
};

struct VertexShaderOutput_deferred
{
    float4 Position : POSITION0;
	float3 Normal : NORMAL;
	float2 TexCoord : TEXCOORD0;
	float2 Depth : TEXCOORD1;
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
	output.Depth.xy = output.Position.zw;
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

float applyVAmbientOnIntensity(float floorDis, float ceilDis, float intensity)
{
	float ambient = floorDis * ambientEpsilon;
	ambient = clamp(ambient, 0, ambientSize) / ambientSize;
	ambient = 1 - (1 - ambient) * (1 - ambient);
	intensity = lerp(intensity * ambientAmount, intensity, ambient);

	ambient = ceilDis * ambientEpsilon;
	ambient = clamp(ambient, 0, ambientSize) / ambientSize;
	ambient = 1 - (1 - ambient) * (1 - ambient);
	intensity = lerp(intensity * ambientAmount, intensity, ambient);

	return intensity;
}

float applyHAmbientOnIntensity(float leftDis, float rightDis, float intensity)
{
	float ambient = leftDis * ambientEpsilon;
	ambient = clamp(ambient, 0, ambientSize) / ambientSize;
	ambient = 1 - (1 - ambient) * (1 - ambient);
	intensity = lerp(intensity * ambientAmount, intensity, ambient);

	ambient = rightDis * ambientEpsilon;
	ambient = clamp(ambient, 0, ambientSize) / ambientSize;
	ambient = 1 - (1 - ambient) * (1 - ambient);
	intensity = lerp(intensity * ambientAmount, intensity, ambient);

	return intensity;
}

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
	// Texture color, and discard for alpha test
	float4 texColor = tex2D(diffuseSampler, input.TexCoord.xy);
	if (texColor.a < .5) discard;

	// Light level with distance fog
	float intensity = getIntensityFromDis(lightLevel, input.TexCoord.z);

	// Calculate ambient
	intensity = applyVAmbientOnIntensity(input.Color.r, input.Color.g, intensity);
	intensity = applyHAmbientOnIntensity(input.Color.b, input.Color.a, intensity);

    return texColor * intensity * tint;
}

struct PixelShaderOutput_deferred
{
	float4 Color : SV_Target0;
	float4 Depth : SV_Target1;
	float4 Normal: SV_Target2;
};

PixelShaderOutput_deferred PixelShaderFunction_deferred(VertexShaderOutput_deferred input)
{
	// Texture color, and discard for alpha test
	float4 texColor = tex2D(diffuseSampler, input.TexCoord.xy);
	if (texColor.a < .5) discard;

	// Calculate ambient
	float intensity = applyVAmbientOnIntensity(input.Color.r, input.Color.g, 1);
	intensity = applyHAmbientOnIntensity(input.Color.b, input.Color.a, intensity);

	PixelShaderOutput_deferred output;

	output.Color = texColor * intensity * tint;
	output.Depth = input.Depth.x / input.Depth.y;
	output.Normal.xyz = input.Normal.xyz * .5 + .5;
	output.Normal.a = lightLevel;

    return output;
}

float4 PixelShaderFunctionAnimatedWall(VertexShaderOutput input) : COLOR0
{
	// Texture color, and discard for alpha test
	float4 texColor = tex2D(animatedSampler1, input.TexCoord);
	float4 texColor2 = tex2D(animatedSampler2, input.TexCoord);
	texColor = lerp(texColor2, texColor, AnimT);
	if (texColor.a < .5) discard;

	// Light level with distance fog
	float intensity = getIntensityFromDis(lightLevel, input.TexCoord.z);

	// Calculate ambient
	intensity = applyVAmbientOnIntensity(input.Color.r, input.Color.g, intensity);
	intensity = applyHAmbientOnIntensity(input.Color.b, input.Color.a, intensity);

    return texColor * intensity * tint;
}

PixelShaderOutput_deferred PixelShaderFunctionAnimatedWall_deferred(VertexShaderOutput_deferred input)
{
	// Texture color, and discard for alpha test
	float4 texColor = tex2D(animatedSampler1, input.TexCoord);
	float4 texColor2 = tex2D(animatedSampler2, input.TexCoord);
	texColor = lerp(texColor2, texColor, AnimT);
	if (texColor.a < .5) discard;

	// Calculate ambient
	float intensity = applyVAmbientOnIntensity(input.Color.r, input.Color.g, 1);
	intensity = applyHAmbientOnIntensity(input.Color.b, input.Color.a, intensity);

	PixelShaderOutput_deferred output;

	output.Color = texColor * intensity * tint;
	output.Depth = input.Depth.x / input.Depth.y;
	output.Normal.xyz = input.Normal.xyz * .5 + .5;
	output.Normal.a = lightLevel;

    return output;
}

float4 PixelShaderFunctionNoTexture(VertexShaderOutput input) : COLOR0
{
	// Texture color, and discard for alpha test
	float4 texColor = tex2D(diffuseSampler, input.TexCoord.xy);
	if (texColor.a < .5) discard;

	// Light level with distance fog
	float intensity = getIntensityFromDis(lightLevel, input.TexCoord.z);

	// Calculate ambient
	intensity = applyVAmbientOnIntensity(input.Color.r, input.Color.g, intensity);
	intensity = applyHAmbientOnIntensity(input.Color.b, input.Color.a, intensity);

	return float4(intensity.rrr, 1) * tint;
}

PixelShaderOutput_deferred PixelShaderFunctionNoTexture_deferred(VertexShaderOutput_deferred input)
{
	// Texture color, and discard for alpha test
	float4 texColor = tex2D(diffuseSampler, input.TexCoord.xy);
	if (texColor.a < .5) discard;

	// Calculate ambient
	float intensity = applyVAmbientOnIntensity(input.Color.r, input.Color.g, 1);
	intensity = applyHAmbientOnIntensity(input.Color.b, input.Color.a, intensity);

	PixelShaderOutput_deferred output;

	output.Color = intensity * tint;
	output.Depth = input.Depth.x / input.Depth.y;
	output.Normal.xyz = input.Normal.xyz * .5 + .5;
	output.Normal.a = lightLevel;

    return output;
}

float4 PixelShaderFunctionNoAmbient(VertexShaderOutput input) : COLOR0
{
	// Texture color, and discard for alpha test
	float4 texColor = tex2D(diffuseSampler, input.TexCoord.xy);
	if (texColor.a < .5) discard;

	// Light level with distance fog
	float intensity = getIntensityFromDis(lightLevel, input.TexCoord.z);

    return texColor * intensity * tint;
}

PixelShaderOutput_deferred PixelShaderFunctionNoAmbient_deferred(VertexShaderOutput_deferred input)
{
	// Texture color, and discard for alpha test
	float4 texColor = tex2D(diffuseSampler, input.TexCoord.xy);
	if (texColor.a < .5) discard;

	// Calculate ambient
	float intensity = 1;

	PixelShaderOutput_deferred output;

	output.Color = texColor * intensity * tint;
	output.Depth = input.Depth.x / input.Depth.y;
	output.Normal.xyz = input.Normal.xyz * .5 + .5;
	output.Normal.a = lightLevel;

    return output;
}

float4 PixelShaderFunctionNoTextureNoAmbient(VertexShaderOutput input) : COLOR0
{
	// Texture color, and discard for alpha test
	float4 texColor = tex2D(diffuseSampler, input.TexCoord.xy);
	if (texColor.a < .5) discard;

	// Light level with distance fog
	float intensity = getIntensityFromDis(lightLevel, input.TexCoord.z);

	return float4(intensity.rrr, 1) * tint;
}

PixelShaderOutput_deferred PixelShaderFunctionNoTextureNoAmbient_deferred(VertexShaderOutput_deferred input)
{
	// Texture color, and discard for alpha test
	float4 texColor = tex2D(diffuseSampler, input.TexCoord.xy);
	if (texColor.a < .5) discard;

	// Calculate ambient
	float intensity = 1;

	PixelShaderOutput_deferred output;

	output.Color = intensity * tint;
	output.Depth = input.Depth.x / input.Depth.y;
	output.Normal.xyz = input.Normal.xyz * .5 + .5;
	output.Normal.a = lightLevel;

    return output;
}

technique TechniqueMain
{
    pass Pass1
    {
        VertexShader = compile vs_2_0 VertexShaderFunction();
        PixelShader = compile ps_2_0 PixelShaderFunction();
    }
}

technique TechniqueNoTexture
{
    pass Pass1
    {
        VertexShader = compile vs_2_0 VertexShaderFunction();
        PixelShader = compile ps_2_0 PixelShaderFunctionNoTexture();
    }
}

technique TechniqueNoAmbient
{
    pass Pass1
    {
        VertexShader = compile vs_2_0 VertexShaderFunction();
        PixelShader = compile ps_2_0 PixelShaderFunctionNoAmbient();
    }
}

technique TechniqueNoTextureNoAmbient
{
    pass Pass1
    {
        VertexShader = compile vs_2_0 VertexShaderFunction();
        PixelShader = compile ps_2_0 PixelShaderFunctionNoTextureNoAmbient();
    }
}

technique TechniqueAnimatedWall
{
    pass Pass1
    {
        VertexShader = compile vs_2_0 VertexShaderFunction();
        PixelShader = compile ps_2_0 PixelShaderFunctionAnimatedWall();
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
        PixelShader = compile ps_3_0 PixelShaderFunctionNoTexture_deferred();
    }
}

technique TechniqueNoAmbient_deferred
{
    pass Pass1
    {
        VertexShader = compile vs_3_0 VertexShaderFunction_deferred();
        PixelShader = compile ps_3_0 PixelShaderFunctionNoAmbient_deferred();
    }
}

technique TechniqueNoTextureNoAmbient_deferred
{
    pass Pass1
    {
        VertexShader = compile vs_3_0 VertexShaderFunction_deferred();
        PixelShader = compile ps_3_0 PixelShaderFunctionNoTextureNoAmbient_deferred();
    }
}

technique TechniqueAnimatedWall_deferred
{
    pass Pass1
    {
        VertexShader = compile vs_3_0 VertexShaderFunction_deferred();
        PixelShader = compile ps_3_0 PixelShaderFunctionAnimatedWall_deferred();
    }
}
