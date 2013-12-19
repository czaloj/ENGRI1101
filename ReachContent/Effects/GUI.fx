float4x4 WVP;
sampler GUITexture : register(s0);

struct VSO
{
    float4 Position : POSITION0;
	float2 UV : TEXCOORD0;
	float4 Color : COLOR0;
};
VSO VS_Rect(
	float4 Pos : POSITION0,
	float2 UV : TEXCOORD0,
	float4 Color : COLOR0
	)
{
	VSO output;
	output.Position = mul(Pos, WVP);
	output.UV = UV;
	output.Color = Color;
	return output;
}
float4 PS_Rect(VSO input) : COLOR0
{
	float4 color = tex2D(GUITexture, input.UV);
	clip(color.a == 0 ? -1 : 1);
	return color * input.Color;
}

VSO VS_Font(
	float4 Pos : POSITION0,
	float2 UV : TEXCOORD0,
	float4 Color : COLOR0
	)
{
	VSO output;
    output.Position = mul(Pos, WVP);
	output.UV = UV;
	output.Color = Color;
	return output;
}
float4 PS_Font(VSO input) : COLOR0
{
	float4 color = tex2D(GUITexture, input.UV);
	clip(color.a == 0 ? -1 : 1);
    return color * input.Color;
}

technique Default
{
	pass Rect
	{
		VertexShader = compile vs_2_0 VS_Rect();
        PixelShader = compile ps_2_0 PS_Rect();
	}
    pass Font
    {
        VertexShader = compile vs_2_0 VS_Font();
        PixelShader = compile ps_2_0 PS_Font();
    }
}