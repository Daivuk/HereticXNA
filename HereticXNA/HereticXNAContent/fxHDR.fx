float4x4 MatrixTransform;
float2 Viewport;
float g_fDT;
float lumMultiplier;
float2 texelSize;

uniform extern texture Texture0;
sampler sampler0 = sampler_state
{
	Texture = <Texture0>;
	mipfilter = POINT; 
	magfilter = POINT; 
	AddressU = Clamp;
    AddressV = Clamp;
};

uniform extern texture BloomTexture;
sampler bloomSampler = sampler_state
{
	Texture = <BloomTexture>;
	mipfilter = LINEAR; 
	magfilter = LINEAR; 
	AddressU = Clamp;
    AddressV = Clamp;
};

uniform extern texture LevelsTexture;
sampler levelsSampler = sampler_state
{
	Texture = <LevelsTexture>;
	mipfilter = POINT; 
	magfilter = POINT; 
	AddressU = Wrap;
    AddressV = Wrap;
};

uniform extern texture LastLevelsTexture;
sampler lastLevelsSampler = sampler_state
{
	Texture = <LastLevelsTexture>;
	mipfilter = POINT; 
	magfilter = POINT; 
	AddressU = Wrap;
    AddressV = Wrap;
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

static const float3 LUM_CONVERT = float3(0.299f, 0.587f, 0.114f); 

float lumFromRGB(float3 rgb)
{
	return dot(rgb, LUM_CONVERT);   
}

float g_fMiddleGrey = 0.5f; 
float g_fMaxLuminance = 16.0f;
float blurDis = 1;

float4 PixelShaderHDR(VertexShaderOutput input) : COLOR0
{	
	float4 gColor = tex2D(sampler0, input.TexCoord.xy);

	gColor.rgb = pow(gColor.rgb, 1.1);
	gColor.rgb = gColor.rgb * lumMultiplier;

	float lum = lumFromRGB(gColor.rgb);

	// Put the excess in the alpha, we will use that for blending
	gColor.a = clamp((lum - 1) * 4, 0, 1);

	return gColor;
}

float4 PixelShaderBloom(VertexShaderOutput input) : COLOR0
{	
	float4 gColor = tex2D(sampler0, input.TexCoord.xy);

	gColor.rgb *= gColor.a;
	gColor.a = 1;

	return gColor;
}

uniform float weight[9] = {
0.000229,	0.005977,	0.060598,	0.241732,	0.382928,	0.241732,	0.060598,	0.005977,	0.000229
 };


float4 blurBloomV(float2 texCoord)
{
	float4 gAccum = 0;
	
	for (float x = -4; x <= 4; ++x)
	{
		gAccum += tex2D(bloomSampler, float2(texCoord.x, texCoord.y + x * blurDis * texelSize.y)) * weight[x + 4];
	}

	return gAccum;
}

float4 PixelShaderFinalAndBlurBloomV(VertexShaderOutput input) : COLOR0
{	
	float4 gColor = tex2D(sampler0, input.TexCoord.xy);
	float4 gBloom = blurBloomV(input.TexCoord);

	return gColor + gBloom;
}

float4 PixelShaderBloomBlurU(VertexShaderOutput input) : COLOR0
{	
	float4 gAccum = 0;
	
	for (float x = -4; x <= 4; ++x)
	{
		gAccum += tex2D(sampler0, float2(input.TexCoord.x + x * blurDis * texelSize.x, input.TexCoord.y)) * weight[x + 4];
	}

	return gAccum;
}

technique TechniqueHDR
{
    pass Pass1
    {
        VertexShader = compile vs_3_0 SpriteVertexShader();
        PixelShader = compile ps_3_0 PixelShaderHDR();
    }
}

technique TechniqueBloom
{
    pass Pass1
    {
        VertexShader = compile vs_3_0 SpriteVertexShader();
        PixelShader = compile ps_3_0 PixelShaderBloom();
    }
}

technique TechniqueFinalAndBlurBloomV
{
    pass Pass1
    {
        VertexShader = compile vs_3_0 SpriteVertexShader();
        PixelShader = compile ps_3_0 PixelShaderFinalAndBlurBloomV();
    }
}

technique TechniqueBloomBlurU
{
    pass Pass1
    {
        VertexShader = compile vs_3_0 SpriteVertexShader();
        PixelShader = compile ps_3_0 PixelShaderBloomBlurU();
    }
}
