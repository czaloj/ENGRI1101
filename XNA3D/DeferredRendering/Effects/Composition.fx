float2 GBufferTextureSize;
sampler Albedo : register(s0);
sampler LightMap : register(s1);
sampler Depth : register(s2);

//Vertex Input Structure
struct VSI
{
	float3 Position : POSITION0;
	float2 UV : TEXCOORD0;
};

//Vertex Output Structure
struct VSO
{
	float4 Position : POSITION0;
	float2 UV : TEXCOORD0;
};

//Vertex Shader
VSO VS(VSI input)
{
	//Initialize Output
	VSO output;
	//Pass Position
	output.Position = float4(input.Position, 1);
	//Pass Texcoord's
	output.UV = input.UV - float2(1.0f / GBufferTextureSize.xy);
	//Return
	return output;
}

struct PSO
{
	float4 Color : Color0;
	float Depth : Depth0;
};

//Pixel Shader
PSO PS(VSO input)
{
	PSO output;
	output.Color = float4(0,0,0,0);
	output.Depth = tex2D(Depth, input.UV).x;
	if(output.Depth < 1)
	{
		//Sample Albedo
		float4 Color = tex2D(Albedo, input.UV).xyzw;
		//if(Color.z == 0) { discard; }
		//Sample Light Map
		float4 Lighting = tex2D(LightMap, input.LUV);
		//Accumulate to Final Color
		output.Color = float4(Color.xyz * Lighting.xyz + Lighting.w, 1);
	}
	else
	{
		discard;
	}
	return output;
}

//Technique
technique Default
{
	pass p0
	{
		VertexShader = compile vs_3_0 VS();
		PixelShader = compile ps_3_0 PS();
	}
}