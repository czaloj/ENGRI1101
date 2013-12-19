//Vertex Shader Constants
float4x4 World;
float4x4 View;
float4x4 Projection;
float4x4 WorldViewIT;

//Color Texture
texture Texture;

//Normal Texture
texture NormalMap;

//Specular Texture
texture SpecularMap;

//Bone Tweaks
static const int BONE_COUNT = 2;
float4x4 BoneMatrices[BONE_COUNT];

//Albedo Sampler
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
	float4 BlendWeights : BLENDWEIGHT;
	float4 BlendIndices : BLENDINDICES;
};



//Vertex Output Structure
struct VSO
{
float4 Position : POSITION0;
float2 UV : TEXCOORD0;
float3 Depth : TEXCOORD1;
float3x3 TBN : TEXCOORD2;
};

struct SkinRes
{
float4 WorldPosition;
float3x3 TBN;
}


SkinRes skin(float4 wPos, float3 N, float3 T, float3 B, float4 indices, float4 weights)
{
	SkinRes out;
	float4x4 mX, mY, mZ, mW;
	mX = BoneMatrices[indices.x] * weights.x;
	mY = BoneMatrices[indices.y] * weights.y;
	mZ = BoneMatrices[indices.z] * weights.z;
	mW = BoneMatrices[indices.w] * weights.w;

	out.WorldPosition = mul(wPos, mX) + mul(wPos, mY) + mul(wPos, mZ) + mul(wPos, mW);
	out.TBN[0] = normalize(mul(T, (float3x3)mX) + mul(T, (float3x3)mY) + mul(T, (float3x3)mZ) + mul(T, (float3x3)mW));
	out.TBN[1] = normalize(mul(B, (float3x3)mX) + mul(B, (float3x3)mY) + mul(B, (float3x3)mZ) + mul(B, (float3x3)mW));
	out.TBN[2] = normalize(mul(N, (float3x3)mX) + mul(N, (float3x3)mY) + mul(N, (float3x3)mZ) + mul(N, (float3x3)mW));

	return out;
}

//Vertex Shader
VSO VS(VSI input)
{
	//Initialize Output
	VSO output;
	//Transform Position
	SkinRes bTrans = skin(input.Position, input.Normal, input.Tangent, input.BiTangent, input.BlendIndices, input.BlendWeights);
	float4 viewPosition = mul(bTrans.WorldPosition, View);
	output.Position = mul(viewPosition, Projection);
	//Pass Depth
	output.Depth.x = output.Position.z;
	output.Depth.y = output.Position.w;
	output.Depth.z = viewPosition.z;
	//Build TBN Matrix
	output.TBN[0] = normalize(mul(bTrans.TBN[0], (float3x3)WorldViewIT));
	output.TBN[1] = normalize(mul(bTrans.TBN[1], (float3x3)WorldViewIT));
	output.TBN[2] = normalize(mul(bTrans.TBN[2], (float3x3)WorldViewIT));
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
	//Pass Albedo from Texture
	output.Albedo = tex2D(AlbedoSampler, input.UV);
	//Pass Extra - Can be whatever you want, in this case will be a Specular Value
	output.Albedo.w = tex2D(SpecularSampler, input.UV).x;
	
	//Read Normal From Texture
	half3 normal = tex2D(NormalSampler, input.UV).xyz * 2.0f - 1.0f;
	//Transform Normal to WorldViewSpace from TangentSpace
	normal = normalize(mul(normal, input.TBN));
	//Pass Encoded Normal
	output.Normals.xyz = encode(normal);
	//Pass this instead to disable normal mapping
	//output.Normals.xyz = encode(normalize(input.TBN[2]));
	//Pass Extra - Can be whatever you want, in this case will be a Specular Value
	output.Normals.w = tex2D(SpecularSampler, input.UV).y;
	
	//Pass Depth(Screen Space, for lighting)
	output.Depth = input.Depth.x / input.Depth.y;
	//Pass Depth(View Space, for SSAO)
	output.Depth.g = input.Depth.z;
	
	//Return Output
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