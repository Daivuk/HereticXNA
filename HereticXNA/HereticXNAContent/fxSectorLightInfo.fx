float4x4 MatrixTransform;
float4x4 InvertViewProjection;
float2 Viewport;

uniform extern texture ScreenTexture;    

sampler ScreenS = sampler_state
{
    Texture = <ScreenTexture>;    
};

float4x4 World;
float4x4 View;
float4x4 Projection;
float2 texSize;
float spread;

uniform extern texture WorkTexture;
sampler workSampler = sampler_state
{
	Texture = <WorkTexture>;
	mipfilter = POINT; 
	magfilter = POINT; 
	AddressU = Clamp;
    AddressV = Clamp;
};

struct VertexShaderInput
{
    float4 Position : POSITION0;
};

struct VertexShaderOutput
{
    float4 Position : POSITION0;
};

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
    VertexShaderOutput output;

    float4 worldPosition = mul(input.Position, World);
    float4 viewPosition = mul(worldPosition, View);
    output.Position = mul(viewPosition, Projection);

    return output;
}

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
    return float4(1, 1, 0, 1);
}

float4 PixelShaderBlurU(float2 texCoord: TEXCOORD0) : COLOR
{
 	float accumR = 0;
 	float accumG = 0;
	float x;

	for (x = -1; x <= 1; ++x)
	{
 		accumR = max(tex2D(ScreenS, float2(texCoord.x + x / texSize.x, texCoord.y)).r, accumR);
	}
	for (x = -8; x <= 8; ++x)
	{
 		accumG += tex2D(ScreenS, float2(texCoord.x + x * spread / texSize.x, texCoord.y)).g;
	}

	accumG /= 12;

    return float4(accumR, accumG, 0, 1);
}

float4 PixelShaderBlurV(float2 texCoord: TEXCOORD0) : COLOR
{
 	float accumR = 0;
 	float accumG = 0;
	float y;

	for (y = -1; y <= 1; ++y)
	{
 		accumR = max(tex2D(ScreenS, float2(texCoord.x, texCoord.y + y / texSize.y)).r, accumR);
	}
	for (y = -8; y <= 8; ++y)
	{
 		accumG += tex2D(ScreenS, float2(texCoord.x, texCoord.y + y * spread / texSize.y)).g;
	}

	accumG /= 12;

    return float4(accumR, accumG, 0, 1);
}

technique TechniquePlain
{
    pass Pass1
    {
        VertexShader = compile vs_2_0 VertexShaderFunction();
        PixelShader = compile ps_2_0 PixelShaderFunction();
    }
}

technique TechniqueBlurU
{
    pass Pass1
    {
        PixelShader = compile ps_2_0 PixelShaderBlurU();
    }
}

technique TechniqueBlurV
{
    pass Pass1
    {
        PixelShader = compile ps_2_0 PixelShaderBlurV();
    }
}
