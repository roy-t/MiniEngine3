struct PS_INPUT
{
    float4 pos : SV_POSITION;
    float2 tex : TEXCOORD;
};

// Draws a CCW full screen triangle using only vertex ids
// as seen in https://github.com/Microsoft/DirectX-Graphics-Samples/issues/211
// usage:
// - set rasterizer to cull none
// - set input layout to null 
// - call context.Draw(3)
#pragma VertexShader
PS_INPUT TextureVS(uint vertexId : SV_VERTEXID)
{
    PS_INPUT output;
    
    float2 tex = float2(uint2(vertexId, vertexId << 1) & 2);
    float4 pos = float4(lerp(float2(-1, 1), float2(1, -1), tex), 0, 1);
    
    output.tex = tex;
    output.pos = pos;

    return output;
}

#pragma VertexShader
float4 PositionVS(uint vertexId : SV_VERTEXID) : SV_POSITION
{
    return float4(vertexId == 1 ? 3 : -1, vertexId == 2 ? 3 : -1, 0.5, 1);
}