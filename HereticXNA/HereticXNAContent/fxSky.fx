float4x4 World;
float4x4 View;
float4x4 Projection;
float2 CameraAngles;

uniform extern texture DiffuseTexture;
sampler diffuseSampler = sampler_state
{
	Texture = <DiffuseTexture>;
	mipfilter = LINEAR; 
	magfilter = LINEAR; 
	AddressU = Wrap;
    AddressV = Mirror;
};


struct VertexShaderInput
{
    float4 Position : POSITION0;
	float2 TexCoord : TEXCOORD0;
	float4 Color : COLOR0;
};

struct VertexShaderOutput
{
    float4 Position : POSITION0;
	float4 PositionTransfer : TEXCOORD0;
	float4 Color : COLOR0;
};

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
    VertexShaderOutput output;

    float4 worldPosition = mul(input.Position, World);
    float4 viewPosition = mul(worldPosition, View);
    output.Position = mul(viewPosition, Projection);

	output.PositionTransfer = output.Position;
	output.Color = input.Color;

    return output;
}

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
	float2 texCoord = float2(input.PositionTransfer.xy / input.PositionTransfer.z);
	texCoord.y = 1 - texCoord.y / 1.5625;

	texCoord.x /= 2;
	texCoord.x -= CameraAngles.x / 3.141592 * 2;
	float angleX = CameraAngles.y / 3.141592 * 2;
	texCoord.y -= angleX * 1.5625;

	float4 texColor = tex2D(diffuseSampler, texCoord);
    return texColor;
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
        VertexShader = compile vs_2_0 VertexShaderFunction();
        PixelShader = compile ps_2_0 PixelShaderFunction();
    }
}
