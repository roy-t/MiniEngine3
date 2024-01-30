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
        
    // Don't show grid lines on cliffs
    float nDu = dot(normal, float3(0.0f, 1.0f, 0.0f));
    if (nDu > 0.1f)
    {        
        // inspired by https://bgolus.medium.com/the-best-darn-grid-shader-yet-727f9278b9d8      
        // TODO: try pristine grid instead of the basic one
        float depth = input.depth * 400.0f;
        float a = clamp(depth, 0.0f, 1.0f);
        a = a * a;        
        float3 darker = albedo * (1.0f - a);
        
        const float lineWidth = 0.05f;
        float2 worldPos = (input.world.xz / 1.0f) + float2(0.5f, 0.5f);
        float2 lineAa = fwidth(worldPos);
        float2 lineUV = 1.0f - abs(frac(worldPos) * 2.0f - 1.0f);
        float2 lines = smoothstep(lineWidth + lineAa, lineWidth - lineAa, lineUV);
        float grid = lerp(lines.x, 1.0, lines.y);
        albedo = lerp(albedo, darker, grid);
    }
          
    const float3 lightDirection = normalize(float3(-3.0f, -1.0f, 0.0f));
    const float3 lightColor = float3(1.0f, 1.0f, 1.0f);
    const float3 ambient = float3(0.1f, 0.1f, 0.1f);
    
    float3 diffuse = max(dot(normal, -lightDirection), 0.0f) * lightColor;
    float3 color = saturate(ambient + diffuse) * albedo; // TODO: instead of saturate, just let it grow out of bounds and fix in tonemap
    output.albedo = float4(color, 1.0f);    

    return output;
}