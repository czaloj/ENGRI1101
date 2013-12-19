float4x4 World;
float4x4 View;
float4x4 Projection;
bool ApplyWorld;

sampler FontMap : register(s0);

struct VSO
{
    float4 Position : POSITION0;
	float2 UV : TEXCOORD0;
};

struct VSI_I
{
	float4 UVRect : POSITION0;
	float2 PosUVApply : TEXCOORD0;
	float4x4 World : TEXCOORD1;
};
VSO VS_Instanced(VSI_I input)
{
	VSO output;
	
	float4 pos = (input.PosUVApply, 0, 1);
	float4 worldPosition = mul(pos, input.World);
	if(ApplyWorld)
	{
		worldPosition = mul(worldPosition, World);
    }
	float4 viewPosition = mul(worldPosition, View);
    output.Position = mul(viewPosition, Projection);
	output.UV = input.PosUVApply * input.UVRect.zw + input.UVRect.xy;

	return output;
}

float4 PS(VSO input) : COLOR0
{
    return tex2D(FontMap, input.UV);
}


technique Instanced
{
    pass Default2D
    {
        VertexShader = compile vs_2_0 VS_Instanced();
        PixelShader = compile ps_2_0 PS();
    }
}

technique Simple
{
    pass Default2D
    {
        VertexShader = compile vs_2_0 VS_Instanced();
        PixelShader = compile ps_2_0 PS();
    }
}