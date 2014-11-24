float4x4 MatrixTransform;
float4x4 InvertViewProjection;
float2 Viewport;

float3 lPos;
float lRadius;
float4 lColor;

float4 floorLimits;
float2 floorCeil;

uniform extern texture GBufferTexture0;
sampler colorSampler = sampler_state
{
	Texture = <GBufferTexture0>;
	mipfilter = POINT; 
	magfilter = POINT; 
	AddressU = Clamp;
    AddressV = Clamp;
};

uniform extern texture GBufferTexture1;
sampler depthSampler = sampler_state
{
	Texture = <GBufferTexture1>;
	mipfilter = POINT; 
	magfilter = POINT; 
	AddressU = Clamp;
    AddressV = Clamp;
};

uniform extern texture GBufferTexture2;
sampler normalSampler = sampler_state
{
	Texture = <GBufferTexture2>;
	mipfilter = POINT; 
	magfilter = POINT; 
	AddressU = Clamp;
    AddressV = Clamp;
};

texture ShadowCubeMap;
samplerCUBE shadowCubeMapSampler = sampler_state 
{ 
    texture = <ShadowCubeMap>;
	mipfilter = LINEAR; 
	magfilter = LINEAR; 
	AddressU = Clamp;
    AddressV = Clamp;
};

uniform extern texture SpreadTexture;
sampler spreadSampler = sampler_state
{
	Texture = <SpreadTexture>;
	mipfilter = LINEAR; 
	magfilter = LINEAR; 
	AddressU = Clamp;
    AddressV = Clamp;
};

struct VertexShaderInput
{
    float4 Position : POSITION0;
	float2 TexCoord : TEXCOORD0;
};

struct VertexShaderOutput
{
    float4 Position : POSITION0;
	float2 TexCoord : TEXCOORD0;
};

struct sInSpriteVS
{
	float2 Position : POSITION0;
	float2 TexCoord : TEXCOORD0;
};

struct sOutSpriteVS
{
	float4 Position : POSITION0;
	float2 TexCoord : TEXCOORD0;
};

sOutSpriteVS SpriteVS(sInSpriteVS input)
{
	sOutSpriteVS output;

	output.Position = float4(input.Position.xy, 0, 1);
	output.TexCoord = input.TexCoord;

	return output;
}

void SpriteVertexShader(inout float4 color    : COLOR0,
                       inout float2 texCoord : TEXCOORD0,
                       inout float4 position : POSITION0)
{
   // Half pixel offset for correct texel centering.
   position.xy -= 0.5;

   // Viewport adjustment.
   position.xy = position.xy / Viewport;
   position.xy *= float2(2, -2);
   position.xy -= float2(1, -1);
}

float4 PixelShaderAmbient(VertexShaderOutput input) : COLOR0
{	
	float4 gColor = tex2D(colorSampler, input.TexCoord.xy);
	float gDepth = tex2D(depthSampler, input.TexCoord.xy).r;
	float4 gNormal = tex2D(normalSampler, input.TexCoord.xy);

	float4 ambient = lerp(1, float4(.95, .8, 1, 1) * .15, gNormal.a);
	
    return gColor * ambient;
}

float4 PixelShaderOmni(VertexShaderOutput input) : COLOR0
{	
	float4 gColor = tex2D(colorSampler, input.TexCoord.xy);
	float gDepth = tex2D(depthSampler, input.TexCoord.xy).r;
	float4 gNormal = tex2D(normalSampler, input.TexCoord.xy);

	// Position
	float4 position;
    position.xy = float2(input.TexCoord.x * 2 - 1, -(input.TexCoord.y * 2 - 1));
    position.z = gDepth;
    position.w = 1.0f;
    position = mul(position, InvertViewProjection);
	position /= position.w;

	// Normal
	float3 normal = gNormal * 2 - 1;
	float3 dir = lPos - position.xyz;

	// Attenuation stuff
	float dis = length(dir);
	float disSqr = dis * dis;
	disSqr /= lRadius * lRadius;
	float dotNormal = dot(normal, dir) / dis;
	dotNormal = 1 - (1 - dotNormal) * (1 - dotNormal);
	float intensity = clamp(1 - disSqr, 0, 1);
	dotNormal = clamp(dotNormal, 0, 1);
	intensity *= dotNormal;

    return gColor * lColor * intensity;
}

float ComputeLightCoverageVarianceBest(float3 texCoords, float LightDis)
{
	float2 gShadow = 0;

	float xm = texCoords.x - 0.015625;
	float xp = texCoords.x + 0.015625;

	float ym = texCoords.y - 0.015625;
	float yp = texCoords.y + 0.015625;

	float zm = texCoords.z - 0.015625;
	float zp = texCoords.z + 0.015625;

	gShadow.xy += texCUBE(shadowCubeMapSampler, float3(xm, ym, zm)).xy;
	gShadow.xy += texCUBE(shadowCubeMapSampler, float3(xm, ym, texCoords.z)).xy;
	gShadow.xy += texCUBE(shadowCubeMapSampler, float3(xm, ym, zp)).xy;

	gShadow.xy += texCUBE(shadowCubeMapSampler, float3(xm, texCoords.y, zm)).xy;
	gShadow.xy += texCUBE(shadowCubeMapSampler, float3(xm, texCoords.y, texCoords.z)).xy;
	gShadow.xy += texCUBE(shadowCubeMapSampler, float3(xm, texCoords.y, zp)).xy;

	gShadow.xy += texCUBE(shadowCubeMapSampler, float3(xm, yp, zm)).xy;
	gShadow.xy += texCUBE(shadowCubeMapSampler, float3(xm, yp, texCoords.z)).xy;
	gShadow.xy += texCUBE(shadowCubeMapSampler, float3(xm, yp, zp)).xy;

	gShadow.xy += texCUBE(shadowCubeMapSampler, float3(texCoords.x, ym, zm)).xy;
	gShadow.xy += texCUBE(shadowCubeMapSampler, float3(texCoords.x, ym, texCoords.z)).xy;
	gShadow.xy += texCUBE(shadowCubeMapSampler, float3(texCoords.x, ym, zp)).xy;

	gShadow.xy += texCUBE(shadowCubeMapSampler, float3(texCoords.x, texCoords.y, zm)).xy;
	gShadow.xy += texCUBE(shadowCubeMapSampler, texCoords).xy;
	gShadow.xy += texCUBE(shadowCubeMapSampler, float3(texCoords.x, texCoords.y, zp)).xy;

	gShadow.xy += texCUBE(shadowCubeMapSampler, float3(texCoords.x, yp, zm)).xy;
	gShadow.xy += texCUBE(shadowCubeMapSampler, float3(texCoords.x, yp, texCoords.z)).xy;
	gShadow.xy += texCUBE(shadowCubeMapSampler, float3(texCoords.x, yp, zp)).xy;

	gShadow.xy += texCUBE(shadowCubeMapSampler, float3(xp, ym, zm)).xy;
	gShadow.xy += texCUBE(shadowCubeMapSampler, float3(xp, ym, texCoords.z)).xy;
	gShadow.xy += texCUBE(shadowCubeMapSampler, float3(xp, ym, zp)).xy;

	gShadow.xy += texCUBE(shadowCubeMapSampler, float3(xp, texCoords.y, zm)).xy;
	gShadow.xy += texCUBE(shadowCubeMapSampler, float3(xp, texCoords.y, texCoords.z)).xy;
	gShadow.xy += texCUBE(shadowCubeMapSampler, float3(xp, texCoords.y, zp)).xy;

	gShadow.xy += texCUBE(shadowCubeMapSampler, float3(xp, yp, zm)).xy;
	gShadow.xy += texCUBE(shadowCubeMapSampler, float3(xp, yp, texCoords.z)).xy;
	gShadow.xy += texCUBE(shadowCubeMapSampler, float3(xp, yp, zp)).xy;

	gShadow.xy *= 0.03703703703703703703703703703704;

	float Diff = LightDis - gShadow.x;
	float bias = 8 / lRadius;
	if(Diff > bias)
	{
		gShadow.y -= bias * bias;
		gShadow.x -= bias;
		float Variance = gShadow.y - gShadow.x * gShadow.x;
		return Variance / (Variance + Diff * Diff);
	}
	else
	{
		return 1.0;
	}
}

float ComputeLightCoverageVarianceMedium(float3 texCoords, float LightDis)
{
	float2 gShadow = 0;

	float xm = texCoords.x - 0.0078125;
	float xp = texCoords.x + 0.0078125;

	float ym = texCoords.y - 0.0078125;
	float yp = texCoords.y + 0.0078125;

	float zm = texCoords.z - 0.0078125;
	float zp = texCoords.z + 0.0078125;

	gShadow.xy += texCUBE(shadowCubeMapSampler, float3(xm, ym, zm)).xy;
	gShadow.xy += texCUBE(shadowCubeMapSampler, float3(xm, ym, zp)).xy;

	gShadow.xy += texCUBE(shadowCubeMapSampler, float3(xm, yp, zm)).xy;
	gShadow.xy += texCUBE(shadowCubeMapSampler, float3(xm, yp, zp)).xy;

	gShadow.xy += texCUBE(shadowCubeMapSampler, float3(xp, ym, zm)).xy;
	gShadow.xy += texCUBE(shadowCubeMapSampler, float3(xp, ym, zp)).xy;

	gShadow.xy += texCUBE(shadowCubeMapSampler, float3(xp, yp, zm)).xy;
	gShadow.xy += texCUBE(shadowCubeMapSampler, float3(xp, yp, zp)).xy;

	gShadow.xy *= 0.125;

	float Diff = LightDis - gShadow.x;
	float bias = 8 / lRadius;
	if(Diff > bias)
	{
		gShadow.y -= bias * bias;
		gShadow.x -= bias;
		float Variance = gShadow.y - gShadow.x * gShadow.x;
		return Variance / (Variance + Diff * Diff);
	}
	else
	{
		return 1.0;
	}
}

float4 PixelShaderOmniShadow(VertexShaderOutput input) : COLOR0
{	
	float4 gColor = tex2D(colorSampler, input.TexCoord.xy);
	float gDepth = tex2D(depthSampler, input.TexCoord.xy).r;
	float4 gNormal = tex2D(normalSampler, input.TexCoord.xy);

	// Position
	float4 position;
    position.xy = float2(input.TexCoord.x * 2 - 1, -(input.TexCoord.y * 2 - 1));
    position.z = gDepth;
    position.w = 1.0f;
    position = mul(position, InvertViewProjection);
	position /= position.w;

	// Normal
	float3 normal = gNormal * 2 - 1;
	float3 dir = lPos - position.xyz;

	// Attenuation stuff
	float dis = distance(lPos, position.xyz);
	float disSqr = dis * dis;
	disSqr /= lRadius * lRadius;
	float dotNormal = dot(normal, dir) / dis;
	dotNormal = 1 - dotNormal;
	dotNormal *= dotNormal;
	dotNormal = 1 - dotNormal;
	float intensity = clamp(1 - disSqr, 0, 1);
	dotNormal = clamp(dotNormal, 0, 1);
	intensity *= dotNormal;

	// Shadow map stuff
	float shadowIntensity = ComputeLightCoverageVarianceMedium(float3(-dir.xy, dir.z) / dis, dis / lRadius);

    return gColor * lColor * intensity * shadowIntensity;
}

float4 PixelShaderFloorLight(VertexShaderOutput input) : COLOR0
{	
	float4 gColor = tex2D(colorSampler, input.TexCoord.xy);
	float gDepth = tex2D(depthSampler, input.TexCoord.xy).r;
	float4 gNormal = tex2D(normalSampler, input.TexCoord.xy);

	// Position
	float4 position;
    position.xy = float2(input.TexCoord.x * 2 - 1, -(input.TexCoord.y * 2 - 1));
    position.z = gDepth;
    position.w = 1.0f;
    position = mul(position, InvertViewProjection);
	position /= position.w;

	// Normal
	gNormal.b *= gNormal.b;
	float normalDot = 1 - gNormal.b;

	// Getting the values from our spread texture
	float2 texCoord = position.xy;
	texCoord = clamp(texCoord, floorLimits.xy, floorLimits.zw);
	texCoord -= floorLimits.xy;
	texCoord /= floorLimits.zw - floorLimits.xy;
	texCoord.y = 1 - texCoord.y;
	float4 spread = tex2D(spreadSampler, texCoord.xy);

	// Attenuation stuff
	float dis = position.z - floorCeil.x;
	dis = clamp(dis, 0, lRadius) / lRadius;
	float intensity = lerp(spread.r, spread.g, 1 - (1 - dis) * (1 - dis)) * normalDot;
	dis = pow(dis, 2);
	dis = 1 - dis;
	intensity *= dis;
	
    return gColor * lColor * intensity;
}

float4 PixelShaderCeilingLight(VertexShaderOutput input) : COLOR0
{	
	float4 gColor = tex2D(colorSampler, input.TexCoord.xy);
	float gDepth = tex2D(depthSampler, input.TexCoord.xy).r;
	float4 gNormal = tex2D(normalSampler, input.TexCoord.xy);

	// Position
	float4 position;
    position.xy = float2(input.TexCoord.x * 2 - 1, -(input.TexCoord.y * 2 - 1));
    position.z = gDepth;
    position.w = 1.0f;
    position = mul(position, InvertViewProjection);
	position /= position.w;

	// Normal
	gNormal.b = 1 - gNormal.b;
	gNormal.b *= gNormal.b;
	gNormal.b = 1 - gNormal.b;
	float normalDot = gNormal.b;

	// Getting the values from our spread texture
	float2 texCoord = position.xy;
	texCoord = clamp(texCoord, floorLimits.xy, floorLimits.zw);
	texCoord -= floorLimits.xy;
	texCoord /= floorLimits.zw - floorLimits.xy;
	texCoord.y = 1 - texCoord.y;
	float4 spread = tex2D(spreadSampler, texCoord.xy);

	// Attenuation stuff
	float dis = floorCeil.y - position.z;
	dis = clamp(dis, 0, lRadius) / lRadius;
	float intensity = lerp(spread.r, spread.g, 1 - (1 - dis) * (1 - dis)) * normalDot;
	dis = pow(dis, 2);
	dis = 1 - dis;
	intensity *= dis;
	
    return gColor * lColor * intensity;
}

technique TechniqueAmbient
{
    pass Pass1
    {
        VertexShader = compile vs_3_0 SpriteVS();
        PixelShader = compile ps_3_0 PixelShaderAmbient();
    }
}

technique TechniqueOmni
{
    pass Pass1
    {
        VertexShader = compile vs_3_0 SpriteVS();
        PixelShader = compile ps_3_0 PixelShaderOmni();
    }
}

technique TechniqueOmniShadow
{
    pass Pass1
    {
        VertexShader = compile vs_3_0 SpriteVS();
        PixelShader = compile ps_3_0 PixelShaderOmniShadow();
    }
}

technique TechniqueFloorLight
{
    pass Pass1
    {
        VertexShader = compile vs_3_0 SpriteVS();
        PixelShader = compile ps_3_0 PixelShaderFloorLight();
    }
}

technique TechniqueCeilingLight
{
    pass Pass1
    {
        VertexShader = compile vs_3_0 SpriteVS();
        PixelShader = compile ps_3_0 PixelShaderCeilingLight();
    }
}
