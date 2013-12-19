float4x4 WVP;
sampler Texture : register(s0);
float ZoomLevel = 1;

struct VSO
{
    float4 Position : POSITION0;
	float4 Color : COLOR0;
	float2 UV1 : TEXCOORD0;
	float2 UV2 : TEXCOORD1;
	float lz : TEXCOORD2;
};

VSO VS(
	float4 Position : POSITION0,
	float2 Corner : TEXCOORD0,
	float4 Color : COLOR0
	)
{
    VSO output;

    output.Position = mul(Position, WVP);
	float lz = log10(ZoomLevel);
	lz = max(0, lz);
	output.lz = fmod(lz, 1);
	output.Color = Color;
	Corner.y = 1 - Corner.y;
	output.UV1 = Corner * pow(10, floor(lz));
	output.UV2 = Corner * pow(10, ceil(lz));
    return output;
}

float4 PS(VSO input) : COLOR0
{
    float4 color = lerp(
		tex2D(Texture, input.UV1),
		tex2D(Texture, input.UV2),
		saturate(input.lz * (2 * 0 + 1) - 0)
		);
	return color * input.Color;
}

technique Default
{
    pass Default
    {
        VertexShader = compile vs_2_0 VS();
        PixelShader = compile ps_2_0 PS();
    }
}
