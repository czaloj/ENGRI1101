//Vertex Shader Constants
float4x4 World;
float4x4 View;
float4x4 Projection;
float4x4 WorldViewIT;

const int MaxBones = 64;
float4x4 BoneMatrices[MaxBones];
float4x4 BoneMatrices_WVIT[MaxBones];

//Albedo Sampler
texture Texture;
sampler AlbedoSampler = sampler_state
{
	texture = <Texture>;
	MINFILTER = LINEAR;
	MAGFILTER = LINEAR;
	MIPFILTER = LINEAR;
	ADDRESSU = WRAP;
	ADDRESSV = WRAP;
};

//NormalMap Sampler
texture NormalMap;
sampler NormalSampler = sampler_state
{
	texture = <NormalMap>;
	MINFILTER = LINEAR;
	MAGFILTER = LINEAR;
	MIPFILTER = LINEAR;
	ADDRESSU = WRAP;
	ADDRESSV = WRAP;
};

//SpecularMap Sampler
texture SpecularMap;
sampler SpecularSampler = sampler_state
{
	texture = <SpecularMap>;
	MINFILTER = LINEAR;
	MAGFILTER = LINEAR;
	MIPFILTER = LINEAR;
	ADDRESSU = WRAP;
	ADDRESSV = WRAP;
};

//Vertex Input Structure
struct VSI
{
	float4 Position : POSITION0;
	float3 Normal : NORMAL0;
	float2 UV : TEXCOORD0;
	float3 Tangent : TANGENT0;
	float3 BiTangent : BINORMAL0;
};

//Skinned Vertex Input Structure
struct VSI_Skinned
{
	float4 Position : POSITION0;
	float3 Normal : NORMAL0;
	float2 UV : TEXCOORD0;
	float3 Tangent : TANGENT0;
	float3 BiTangent : BINORMAL0;
	float BoneIndex : BLENDINDICES0;
};

//Vertex Output Structure
struct VSO
{
	float4 Position : POSITION0;
	float2 UV : TEXCOORD0;
	float3 Depth : TEXCOORD1;
	float3x3 TBN : TEXCOORD2;
	float4 WorldPosition : TEXCOORD5;
};

//Vertex Shader
VSO VS(VSI input)
{
	//Initialize Output
	VSO output;
	//Transform Position
	float4 worldPosition = mul(input.Position, World);
	float4 viewPosition = mul(worldPosition, View);
	output.Position = mul(viewPosition, Projection);
	output.WorldPosition = worldPosition / worldPosition.w;
	
	//Pass Depth
	output.Depth.x = output.Position.z;
	output.Depth.y = output.Position.w;
	output.Depth.z = viewPosition.z;
	
	//Build TBN Matrix
	output.TBN[0] = normalize(mul(input.Tangent, (float3x3)WorldViewIT));
	output.TBN[1] = normalize(mul(input.BiTangent, (float3x3)WorldViewIT));
	output.TBN[2] = normalize(mul(input.Normal, (float3x3)WorldViewIT));
	
	//Pass UV
	output.UV = input.UV;
	
	//Return Output
	return output;
}

//Skinned Vertex Shader
VSO VS_Skinned(VSI_Skinned input)
{
	//Initialize Output
	VSO output;
	//Transform Position
	float4 worldPosition = mul(input.Position, BoneMatrices[input.BoneIndex]);
	float4 viewPosition = mul(worldPosition, View);
	output.Position = mul(viewPosition, Projection);
	output.WorldPosition = worldPosition / worldPosition.w;
	
	//Pass Depth
	output.Depth.x = output.Position.z;
	output.Depth.y = output.Position.w;
	output.Depth.z = viewPosition.z;
	
	//Build TBN Matrix
	output.TBN[0] = normalize(mul(input.Tangent, (float3x3)BoneMatrices_WVIT[input.BoneIndex]));
	output.TBN[1] = normalize(mul(input.BiTangent, (float3x3)BoneMatrices_WVIT[input.BoneIndex]));
	output.TBN[2] = normalize(mul(input.Normal, (float3x3)BoneMatrices_WVIT[input.BoneIndex]));
	
	//Pass UV
	output.UV = input.UV;
	
	//Return Output
	return output;
}

//Pixel Output Structure
struct PSO
{
	float4 Albedo : COLOR0;
	float4 Normals : COLOR1;
	float4 Depth : COLOR2;
};

//Normal Encoding Function
half3 encode(half3 n)
{
	n = normalize(n);
	n.xyz = 0.5f * (n.xyz + 1.0f);
	return n;
}

//Normal Decoding Function
half3 decode(half4 enc)
{
	return (2.0f * enc.xyz- 1.0f);
}

//Pixel Shader
PSO PS(VSO input)
{
	//Initialize Output
	PSO output;

	output.Albedo = tex2D(AlbedoSampler, input.UV);
	output.Albedo.w = tex2D(SpecularSampler, input.UV).x;

	half3 normal = tex2D(NormalSampler, input.UV).xyz * 2.0f - 1.0f;
	normal = normalize(mul(normal, input.TBN));
	output.Normals.xyz = encode(normal);

	//Pass this instead to disable normal mapping
	//output.Normals.xyz = encode(normalize(input.TBN[2]));
	output.Normals.w = tex2D(SpecularSampler, input.UV).y;

	output.Depth.x = input.Depth.x / input.Depth.y;
	output.Depth.y = input.Depth.z;
	output.Depth.z = 0;
	output.Depth.w = 0;

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

//Technique
technique Skinned
{
	pass p0
	{
		VertexShader = compile vs_3_0 VS_Skinned();
		PixelShader = compile ps_3_0 PS();
	}
}