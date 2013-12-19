//Inverse View Projection
float4x4 InverseViewProjection;
//Inverse View
float4x4 inverseView;
//Camera Position
float3 CameraPosition;
//Light Vector
float3 L;
//Light Color
float4 LightColor;//Light Intensity
float LightIntensity;
//GBuffer Texture Size
float2 GBufferTextureSize;

//GBuffer Texture0
sampler GBuffer0 : register(s0);
//GBuffer Texture1
sampler GBuffer1 : register(s1);
//GBuffer Texture2
sampler GBuffer2 : register(s2);

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
	//Just Straight Pass Position
	output.Position = float4(input.Position, 1);
	//Pass UV too
	output.UV = input.UV - float2(1.0f / GBufferTextureSize.xy);
	//Return
	return output;
}

//Manually Linear Sample
float4 manualSample(sampler Sampler, float2 UV, float2 textureSize)
{
	float2 texelpos = textureSize * UV; 
	float2 lerps = frac(texelpos); 
	float texelSize = 1.0 / textureSize; 
	float4 sourcevals[4]; 
	sourcevals[0] = tex2D(Sampler, UV); 
	sourcevals[1] = tex2D(Sampler, UV + float2(texelSize, 0)); 
	sourcevals[2] = tex2D(Sampler, UV + float2(0, texelSize)); 
	sourcevals[3] = tex2D(Sampler, UV + float2(texelSize, texelSize)); 
	float4 interpolated = lerp(lerp(sourcevals[0], sourcevals[1], lerps.x), 
	lerp(sourcevals[2], sourcevals[3], lerps.x ), lerps.y);
	return interpolated;
}

//Phong Shader
float4 Phong(float3 Position, float3 N, float SpecularIntensity, float SpecularPower)
{
	//Calculate Reflection vector
	float3 R = normalize(reflect(L, N));
	//Calculate Eye vector
	float3 E = normalize(CameraPosition - Position.xyz);
	//Calculate N.L
	float NL = dot(N, -L);
	//Calculate Diffuse
	float3 Diffuse = NL * LightColor.xyz;
	//Calculate Specular
	float Specular = SpecularIntensity * pow(saturate(dot(R, E)), SpecularPower);
	//Calculate Final Product
	return LightIntensity * float4(Diffuse.rgb, Specular);
}

//Decoding of GBuffer Normals
float3 decode(float3 enc)
{
	return (2.0f * enc.xyz- 1.0f);
}

//Pixel Shader
float4 PS(VSO input) : COLOR0
{
	//Get All Data from Normal part of the GBuffer
	half4 encodedNormal = tex2D(GBuffer1, input.UV);
	//Decode Normal
	half3 Normal = mul(decode(encodedNormal.xyz), inverseView);
	//Get Specular Intensity from GBuffer
	float SpecularIntensity = tex2D(GBuffer0, input.UV).w;
	//Get Specular Power from GBuffer
	float SpecularPower = encodedNormal.w * 255;
	//Get Depth from GBuffer
	float Depth = manualSample(GBuffer2, input.UV, GBufferTextureSize).x;
	//Calculate Position in Homogenous Space
	float4 Position = 1.0f;
	Position.x = input.UV.x * 2.0f - 1.0f;
	Position.y = -(input.UV.x * 2.0f - 1.0f);
	Position.z = Depth;
	//Transform Position from Homogenous Space to World Space
	Position = mul(Position, InverseViewProjection);
	Position /= Position.w;//Return Phong Shaded Value
	return Phong(Position.xyz, Normal, SpecularIntensity, SpecularPower);
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