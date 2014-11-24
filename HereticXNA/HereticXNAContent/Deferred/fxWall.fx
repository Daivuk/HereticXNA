float4x4 matWorldViewProj;
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
	float3 Normal : NORMAL0;
	float4 Color : COLOR0;
	float2 TexCoord : TEXCOORD0;
};

struct sVSOut
{
    float4 Position : POSITION0;
	float3 Normal : TEXCOORD0;
	float4 Color : COLOR0;
	float2 TexCoord : TEXCOORD1;
	float2 Depth : TEXCOORD2;
};

struct sVSInNoAmb
{
    float3 Position : POSITION0;
	float3 Normal : NORMAL0;
	float2 TexCoord : TEXCOORD0;
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
	output.Color = input.Color;
	output.TexCoord = input.TexCoord.xy;
	output.Depth.xy = output.Position.zw;

    return output;
}

sVSOutNoAmb VSMainNoAmb(sVSInNoAmb input)
{
    sVSOutNoAmb output;

    output.Position = mul(float4(input.Position.xyz, 1), matWorldViewProj);
	output.Normal = input.Normal;
	output.TexCoord = input.TexCoord.xy;
	output.Depth.xy = output.Position.zw;

    return output;
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


struct PSOut
{
	float4 Color : SV_Target0;
	float4 Depth : SV_Target1;
	float4 Normal: SV_Target2;
};


PSOut PSMain(sVSOut input)
{
	// Texture color, and discard for alpha test
	float4 texColor = tex2D(diffuseSampler, input.TexCoord.xy);
	if (texColor.a < .5) discard;

	// Light level with distance fog
	float intensity = 1;

	// Calculate ambient
	intensity = applyVAmbientOnIntensity(input.Color.r, input.Color.g, intensity);
	intensity = applyHAmbientOnIntensity(input.Color.b, input.Color.a, intensity);

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
	float4 texColor = tex2D(diffuseSampler, input.TexCoord.xy);
	if (texColor.a < .5) discard;

	// Light level with distance fog
	float intensity = 1;

	PSOut output;
	output.Color = texColor * intensity;
	output.Depth = input.Depth.x / input.Depth.y;
	output.Normal.xyz = input.Normal.xyz * .5 + .5;
	output.Normal.a = 0;

    return output;
}

PSOut PSMainNoTex(sVSOut input)
{
	// Texture color, and discard for alpha test
	float4 texColor = tex2D(diffuseSampler, input.TexCoord.xy);
	if (texColor.a < .5) discard;

	// Light level with distance fog
	float intensity = 1;

	// Calculate ambient
	intensity = applyVAmbientOnIntensity(input.Color.r, input.Color.g, intensity);
	intensity = applyHAmbientOnIntensity(input.Color.b, input.Color.a, intensity);

	PSOut output;
	output.Color = intensity;
	output.Depth = input.Depth.x / input.Depth.y;
	output.Normal.xyz = input.Normal.xyz * .5 + .5;
	output.Normal.a = 0;

    return output;
}

PSOut PSMainNoTexNoAmb(sVSOutNoAmb input)
{
	// Texture color, and discard for alpha test
	float4 texColor = tex2D(diffuseSampler, input.TexCoord.xy);
	if (texColor.a < .5) discard;

	// Light level with distance fog
	float intensity = 1;

	PSOut output;
	output.Color = intensity;
	output.Depth = input.Depth.x / input.Depth.y;
	output.Normal.xyz = input.Normal.xyz * .5 + .5;
	output.Normal.a = 0;

    return output;
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
