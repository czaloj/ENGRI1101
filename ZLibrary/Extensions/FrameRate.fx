float4x4 WVP;
float4 ViewRect;

float StartIndex;
float MaxIndex;

struct VSI
{
    float2 IndexPercent : POSITION0
	float4 Color : COLOR0
};
struct VSO
{
    float4 Position : POSITION0;
	float4 COLOR : COLOR0;
};

VSO VS(VSI input)
{
    VSO output;

	float p = fmod(input.Index + MaxIndex, MaxIndex);
	float4 pos = float4(ViewRect.x + p * ViewRect.z, ViewRect.y + p * ViewRect.w, 0, 1);

    output.Position = mul(pos, WVP);
	output.Color = input.Color;

    return output;
}

float4 PS(VSO input) : COLOR0
{
	return input.Color;
}

technique Default
{
    pass Line
    {
        VertexShader = compile vs_3_0 VS();
        PixelShader = compile ps_3_0 PS();
    }
}
