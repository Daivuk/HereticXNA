float3 ambientColor;

uniform extern texture ColorTexture;
sampler colorSampler = sampler_state
{
	Texture = <ColorTexture>;
	mipfilter = POINT; 
	magfilter = POINT; 
	AddressU = Clamp;
    AddressV = Clamp;
};

uniform extern texture DepthTexture;
sampler depthSampler = sampler_state
{
	Texture = <DepthTexture>;
	mipfilter = POINT; 
	magfilter = POINT; 
	AddressU = Clamp;
    AddressV = Clamp;
};

uniform extern texture NormalTexture;
sampler normalSampler = sampler_state
{
	Texture = <NormalTexture>;
	mipfilter = POINT; 
	magfilter = POINT; 
	AddressU = Clamp;
    AddressV = Clamp;
};



struct sVSIn
{
    float4 Position : POSITION0;
	float2 TexCoord : TEXCOORD0;
};

struct sVSOut
{
    float4 Position : POSITION0;
	float2 TexCoord : TEXCOORD0;
};



sVSOut VSMain(sVSIn input)
{
	sVSOut output;

	output.Position = float4(input.Position.xy, 0, 1);
	output.TexCoord = input.TexCoord;

	return output;
}



float4 PSMain(sVSOut input) : COLOR0
{	
	float4 gColor = tex2D(colorSampler, input.TexCoord.xy);
	float gDepth = tex2D(depthSampler, input.TexCoord.xy).r;
	float4 gNormal = tex2D(normalSampler, input.TexCoord.xy);

	float4 ambient = lerp(float4(ambientColor, 1), 1, gNormal.a);
	
    return gColor * ambient;
}



technique Technique1
{
    pass Pass1
    {
        VertexShader = compile vs_2_0 VSMain();
        PixelShader = compile ps_2_0 PSMain();
    }
}
