float4x4 World;
float4x4 View;
float4x4 Projection;

struct VSI
{
    float4 Position : POSITION0;
	float4 Color : COLOR0;
};
struct VSO
{
    float4 Position : POSITION0;
	float4 Color : COLOR0;
};

/// ---
/// A Very Simple Shader
/// ---
VSO VS_Line(VSI input)
{
    VSO output;

    float4 worldPosition = mul(input.Position, World);
    float4 viewPosition = mul(worldPosition, View);
    output.Position = mul(viewPosition, Projection);
	output.Color = input.Color;

    return output;
}
float4 PS_Line(VSO input) : COLOR0
{
    return input.Color;
}



struct VSI_N
{
	float4 Position : POSITION0;
	float4 Color : COLOR0;
};
struct VSO_N
{
	float4 Position : POSITION0;
	float4 Color : COLOR0;
};
VSO_N VS_Node_Fancy(VSI_N input)
{
	VSO_N output;
	
	float4 worldPosition = mul(input.Position, World);
    float4 viewPosition = mul(worldPosition, View);
    output.Position = mul(viewPosition, Projection);
	output.Color = input.Color;
	return output;
}
float4 PS_Node(VSO_N input) : COLOR0
{
	return input.Color;
}


technique Fancy
{
    pass Line
    {
        VertexShader = compile vs_2_0 VS_Line();
        PixelShader = compile ps_2_0 PS_Line();
    }
	pass Node
	{
		VertexShader = compile vs_2_0 VS_Node_Fancy();
        PixelShader = compile ps_2_0 PS_Node();
	}
}

technique Fast
{
    pass Line
    {
        VertexShader = compile vs_2_0 VS_Line();
        PixelShader = compile ps_2_0 PS_Line();
    }
	pass Node
	{
		VertexShader = compile vs_2_0 VS_Node_Fancy();
        PixelShader = compile ps_2_0 PS_Node();
	}
}