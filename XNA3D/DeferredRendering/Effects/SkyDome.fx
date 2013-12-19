float4x4 View;
float4x4 Projection;
float TimeX;

sampler Gradient : register(s0);

//Vertex Input Structure
struct VSI
{
	float4 Position : POSITION0;
	float3 Normal : NORMAL0;
};

//Vertex Input Structure
struct VSO
{
	float4 Position : POSITION0;
	float Y : TEXCOORD0;
};

//Vertex Shader
VSO VS(VSI input)
{
	VSO output;
	float4 viewPosition = float4(mul(input.Position.xyz, (float3x3)View), 1);
	output.Position = mul(viewPosition, Projection);
	output.Y = (1 - input.Normal.y) / 2.0;
	return output;
}

float4 PS(VSO input) : COLOR0
{
	float4 output = tex2D(Gradient, float2(TimeX, input.Y));
	return output;
}

technique Default
{
	pass p0
	{
		VertexShader = compile vs_3_0 VS();
		PixelShader = compile ps_3_0 PS();
	}
}