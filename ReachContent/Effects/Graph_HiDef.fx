float4x4 World;
float4x4 View;
float4x4 Projection;

float ThetaTime;
float Amplitude;
float Frequency;
bool AsEdge;

struct VSO
{
    float4 Position : POSITION0;
	float Percent : TEXCOORD0;
	float4 Color : COLOR0;
};

struct VSI_LP
{
	float Position : POSITION0;
};
struct VSI_LI
{
	float4 Start : POSITION1;
	float4 End : POSITION2;
	float4 Color1 : COLOR0;
	float4 Color2 : COLOR1;
};
/// ---
/// The Fast Instancing Shader
/// ---
VSO VS_Line_Fast(VSI_LP inputPos, VSI_LI inputInfo)
{
	VSO output;
	
	float4 pos;
	[branch]
	if(inputPos.Position > 0.5)
	{
		pos = inputInfo.End;
		output.Color = AsEdge ? inputInfo.Color1 : inputInfo.Color2;
	}
	else
	{
		pos = inputInfo.Start;
		output.Color = inputInfo.Color1;
	}
	float4 worldPosition = mul(pos, World);
    float4 viewPosition = mul(worldPosition, View);
    output.Position = mul(viewPosition, Projection);
	output.Percent = inputPos.Position;

	return output;
}
/// ---
/// The Fancy Instancing Shader
/// ---
VSO VS_Line_Fancy(VSI_LP inputPos, VSI_LI inputInfo)
{
	VSO output;
	
	float4 pos = lerp(inputInfo.Start, inputInfo.End, inputPos.Position);
	float4 worldPosition = mul(pos, World);
    float4 viewPosition = mul(worldPosition, View);
    output.Position = mul(viewPosition, Projection);
	if(AsEdge)
	{
		output.Color = inputInfo.Color1;
	}
	else
	{
		output.Color = lerp(inputInfo.Color1, inputInfo.Color2, inputPos.Position);
	}
	output.Percent = inputPos.Position;

	return output;
}

float4 PS_Line_Fancy(VSO input) : COLOR0
{
	[branch]
	if(AsEdge)
	{
		return (cos(ThetaTime) + 5) / 6 * input.Color;
	}
	else
	{
		return cos(input.Percent * -Frequency + ThetaTime) * Amplitude * input.Color;
	}
}

float4 PS_Line(VSO input) : COLOR0
{
    return input.Color;
}






struct VSI_NP
{
	float4 Position : POSITION0;
};
struct VSI_NI
{
	float4x4 World : TEXCOORD0;
	float4 Color1 : COLOR0;
	float4 Color2 : COLOR1;
};
struct VSO_N
{
	float4 Position : POSITION0;
	float2 InnerLocation : TEXCOORD0;
	float4 Color1 : COLOR0;
	float4 Color2 : COLOR1;
};
VSO_N VS_Node_Fancy(VSI_NP inputPos, VSI_NI inputInfo)
{
	VSO_N output;
	
	float4 pos = mul(inputPos.Position, inputInfo.World);
	float4 worldPosition = mul(pos, World);
    float4 viewPosition = mul(worldPosition, View);
    output.Position = mul(viewPosition, Projection);
	output.Color1 = inputInfo.Color1;
	output.Color2 = inputInfo.Color2;
	output.InnerLocation = inputPos.Position;

	return output;
}
VSO_N VS_Node_Fast(VSI_NP inputPos, VSI_NI inputInfo)
{
	VSO_N output;
	
	float4 pos = mul(inputPos.Position, inputInfo.World);
	float4 worldPosition = mul(pos, World);
    float4 viewPosition = mul(worldPosition, View);
    output.Position = mul(viewPosition, Projection);
	output.Color1 = inputInfo.Color1;
	output.Color2 = inputInfo.Color2;
	output.InnerLocation = inputPos.Position;

	return output;
}
float4 PS_Node_Fancy(VSO_N input) : COLOR0
{
	float d = dot(float2(cos(ThetaTime),sin(ThetaTime)), normalize(input.InnerLocation));
	//d = saturate(d);
	d = (d + 1) * 0.5;
	float r = length(input.InnerLocation);
	return lerp(input.Color2, input.Color1, d * (1 - r));
}
float4 PS_Node(VSO_N input) : COLOR0
{
	float r = length(input.InnerLocation);
	return lerp(input.Color1,input.Color2,(r > 1) ? 1 : r);
}



technique Fancy
{
    pass Line
    {
        VertexShader = compile vs_3_0 VS_Line_Fancy();
        PixelShader = compile ps_3_0 PS_Line_Fancy();
    }
	pass Node
	{
		VertexShader = compile vs_3_0 VS_Node_Fancy();
        PixelShader = compile ps_3_0 PS_Node_Fancy();
	}
}

technique Fast
{
    pass Line
    {
        VertexShader = compile vs_3_0 VS_Line_Fast();
        PixelShader = compile ps_3_0 PS_Line();
    }
	pass Node
	{
		VertexShader = compile vs_3_0 VS_Node_Fancy();
        PixelShader = compile ps_3_0 PS_Node();
	}
}