struct VS_INPUT
{
    float3 position : POSITION;
};
    
struct PS_INPUT
{
    float4 position : SV_POSITION;
    float3 world : TEXCOORD1;
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
    return output;
}
      
// From https://bgolus.medium.com/the-best-darn-grid-shader-yet-727f9278b9d8      
float PristineGrid(float2 uv, float2 lineWidth)
{
    lineWidth = saturate(lineWidth);
    float4 uvDDXY = float4(ddx(uv), ddy(uv));
    float2 uvDeriv = float2(length(uvDDXY.xz), length(uvDDXY.yw));
    bool2 invertLine = lineWidth > 0.5;
    float2 targetWidth = invertLine ? 1.0 - lineWidth : lineWidth;
    float2 drawWidth = clamp(targetWidth, uvDeriv, 0.5);
    float2 lineAA = max(uvDeriv, 0.000001) * 1.5;
    float2 gridUV = abs(frac(uv) * 2.0 - 1.0);
    gridUV = invertLine ? gridUV : 1.0 - gridUV;
    float2 grid2 = smoothstep(drawWidth + lineAA, drawWidth - lineAA, gridUV);
    grid2 *= saturate(targetWidth / drawWidth);
    grid2 = lerp(grid2, targetWidth, saturate(uvDeriv * 2.0 - 1.0));
    grid2 = invertLine ? 1.0 - grid2 : grid2;
    return lerp(grid2.x, 1.0, grid2.y);
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
        float3 lineColor = albedo * 0.2f;
        float grid = PristineGrid(input.world.xz, 0.05f);
        albedo = lerp(albedo, lineColor, grid);           
    }
          
    const float3 lightDirection = normalize(float3(-3.0f, -1.0f, 0.0f));
    const float3 lightColor = float3(1.0f, 1.0f, 1.0f);
    const float3 ambient = float3(0.1f, 0.1f, 0.1f);
    
    float3 diffuse = max(dot(normal, -lightDirection), 0.0f) * lightColor;
    float3 color = saturate(ambient + diffuse) * albedo; // TODO: instead of saturate, just let it grow out of bounds and fix in tonemap
    output.albedo = float4(color, 1.0f);    

    return output;
}