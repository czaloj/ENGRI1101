float4x4 World;
float4x4 View;
float4x4 Projection;

float Transparency = 1;
float ThetaTime = 0;
float Amplitude = 1;
float Frequency = 1;
bool AsEdge = true;

struct VSO
{
    float4 Position : POSITION0;
	float2 Percent : TEXCOORD0;
	float3 CR : POSITION1;
	float4 WP : TEXCOORD2;
	float4 Color : COLOR0;
	float4 ColorT : COLOR1;
};
VSO VS_Line(
	float4 PosRatio : POSITION0,
	float3 CR : POSITION1,
	float2 C : POSITION2,
	float4 Color : COLOR0,
	float4 ColorT : COLOR1
	)
{
	VSO output;
	
	float4 pos = float4(PosRatio.xyz, 1);
	float4 worldPosition = mul(pos, World);
	output.WP = worldPosition;
    float4 viewPosition = mul(worldPosition, View);
    output.Position = mul(viewPosition, Projection);
	output.Percent = float2(PosRatio.w, C.y);
	output.Color = Color;
	output.ColorT = ColorT;
	output.CR = CR;

	return output;
}
float4 PS_Line(VSO input) : COLOR0
{
	input.WP /= input.WP.w;
	float2 lv = input.WP.xy - input.CR.xy;
	float d = length(lv);
	input.Percent.y = abs(input.Percent.y);
	clip((1 - sin(3.14159 * input.Percent.x)) - input.Percent.y + 0.2);
	float4 color;
	[branch]
	if(AsEdge)
	{
		color = lerp(input.Color, input.ColorT, input.Percent.y * (sin(ThetaTime) + 1 / (1 + Transparency)) / (1 / Transparency));
	}
	else
	{
		color =  lerp(input.Color, input.ColorT, input.Percent.y * cos((input.Percent.x + input.Percent.y) * -Frequency + ThetaTime) * Amplitude);
	}
	color.a = saturate(color.a) * Transparency;
	return color;
}



struct VSO_N
{
	float4 Position : POSITION0;
	float4 Color1 : COLOR0;
	float4 Color2 : COLOR1;
	float2 RL : TEXCOORD0;
};
float2 NodeRL[4] = {
	float2(-1, 1),
	float2(1, 1),
	float2(-1, -1),
	float2(1, -1)
	};
VSO_N VS_Node(
	float4 PosC : POSITION0,
	float4 Color1 : COLOR0,
	float4 Color2 : COLOR1
	)
{
	VSO_N output;
	
	float4 worldPosition = mul(float4(PosC.xyz, 1), World);
    float4 viewPosition = mul(worldPosition, View);
    output.Position = mul(viewPosition, Projection);
	output.Color1 = Color1;
	output.Color2 = Color2;
	output.RL = NodeRL[PosC.w];
	return output;
}
float4 PS_Node(VSO_N input) : COLOR0
{
	float r = length(input.RL);
	clip(1 - r);
	float d = dot(float2(cos(ThetaTime),sin(ThetaTime)), normalize(input.RL));
	d = (d + 1) * 0.5;
	return lerp(input.Color1, input.Color2, (1 - r) * Transparency);
}

technique Default
{
    pass Line
    {
        VertexShader = compile vs_3_0 VS_Line();
        PixelShader = compile ps_3_0 PS_Line();
    }
	pass Node
	{
		VertexShader = compile vs_3_0 VS_Node();
        PixelShader = compile ps_3_0 PS_Node();
	}
}