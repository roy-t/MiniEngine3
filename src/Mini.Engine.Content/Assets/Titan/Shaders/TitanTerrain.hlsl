struct VS_INPUT
{
    float3 position : POSITION;
};
    
struct PS_INPUT
{
    float4 position : SV_POSITION;
    float3 world : TEXCOORD1;
    float depth : TEXCOORD0; // TODO: depth only required for PSLine
};
    
struct OUTPUT
{
    float4 albedo : SV_Target0;
};
    
struct TRIANGLE
{
    float3 albedo;
    float3 normal;
};
  
cbuffer Constants : register(b0)
{
    float4x4 WorldViewProjection;
};

StructuredBuffer<TRIANGLE> Triangles: register(t0);
    
#pragma VertexShader
PS_INPUT VS(VS_INPUT input)
{
    PS_INPUT output;      
    output.position = mul(WorldViewProjection, float4(input.position, 1.0));
    output.world = input.position.xyz;
    output.depth = output.position.z / output.position.w;
    return output;
}
    
#pragma PixelShader
OUTPUT PS(PS_INPUT input, uint primitiveId : SV_PrimitiveID)
{
    OUTPUT output;
    
    TRIANGLE t = Triangles[primitiveId];    
    float3 albedo = t.albedo;
    float3 normal = t.normal;
    
    // TODO: something like this only works if the tiles are always layed out the same even if there are an odd or even number of them
    if (input.world.x > -0.5 && input.world.x < 0.5)
    {  
        albedo = float3(1, 0, 0);
    }
        
    const float3 lightDirection = normalize(float3(-3.0, -1.0, 0.0));
    const float3 lightColor = float3(1.0, 1.0, 1.0);
    const float3 ambient = float3(0.1, 0.1, 0.1);
    
    float3 diffuse = max(dot(normal, -lightDirection), 0.0) * lightColor;
    float3 color = saturate(ambient + diffuse) * albedo; // TODO: instead of saturate, just let it grow out of bounds and fix in tonemap
    output.albedo = float4(color, 1.0);    

    return output;
}
    
#pragma PixelShader
OUTPUT PSLine(PS_INPUT input)
{
    OUTPUT output;
    // TODO: arbitrary number, might need tweaking when the near/far plane of the camera are changed 
    // or a different ViewProjection matrix is used.
    float depth = input.depth * 100.0; 
    float a = clamp(depth, 0.0, 1.0);
    output.albedo = float4(0.0, 1.0, 0.0, a);
    
    return output;
}