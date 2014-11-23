float4x4 World;
float4x4 View;
float4x4 Projection;
float3 CamPos;

uniform extern texture DiffuseTexture;
sampler diffuseSampler = sampler_state
{
	Texture = <DiffuseTexture>;
	mipfilter = LINEAR; 
	magfilter = LINEAR; 
	AddressU = Clamp;
    AddressV = Clamp;
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

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
	float4 texColor = tex2D(diffuseSampler, input.TexCoord.xy);
	if (texColor.a < .5) discard;
	float intensity = input.Color.r * input.Color.r * 1.5;
	float dis = input.TexCoord.z;
	intensity = intensity - dis * .75 / intensity / 8192;

	float3 rgb = texColor.rgb * intensity;
	texColor.rgb = rgb;

    return texColor * float4(1, 1, 1, input.Color.a);
}

struct PixelShaderOutput_deferred
{
	float4 Color : SV_Target0;
	float4 Depth : SV_Target1;
	float4 Normal: SV_Target2;
};

PixelShaderOutput_deferred PixelShaderFunction_deferred(VertexShaderOutput_deferred input)
{
	float4 texColor = tex2D(diffuseSampler, input.TexCoord.xy);
	if (texColor.a < .5) discard;
	float intensity = input.Color.r * input.Color.r * 1.5;

	PixelShaderOutput_deferred output;

	output.Color = texColor;
	output.Depth = input.Depth.x / input.Depth.y;
	output.Normal.xyz = normalize(input.Normal.xyz) * .5 + .5;
	output.Normal.a = 1 - input.Color.r;

	return output;
}

technique Technique1
{
    pass Pass1
    {
        VertexShader = compile vs_2_0 VertexShaderFunction();
        PixelShader = compile ps_2_0 PixelShaderFunction();
    }
}

technique Technique1_deferred
{
    pass Pass1
    {
        VertexShader = compile vs_3_0 VertexShaderFunction_deferred();
        PixelShader = compile ps_3_0 PixelShaderFunction_deferred();
    }
}
