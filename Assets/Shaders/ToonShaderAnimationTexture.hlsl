#define TOON_SHADER_HOOK_APP_DATA uint id : SV_VertexID;

#include "Packages/com.deltation.toon-shader/Assets/DELTation/ToonShader/Shaders/ToonShaderForwardPass_AppData.hlsl"

#define TOON_SHADER_HOOK_INPUT_BUFFER TEXTURE2D(_AnimationTexture); SAMPLER(sampler_AnimationTexture); float4 _AnimationTexture_TexelSize; float _FrameRate;
#include "Packages/com.deltation.toon-shader/Assets/DELTation/ToonShader/Shaders/ToonShaderInput.hlsl"

void ApplyAnimationTexture(inout appdata input)
{
    float4 texel_size = _AnimationTexture_TexelSize;
    float frame_count = texel_size.z;
    float duration = frame_count / _FrameRate;
    float normalized_time = (_Time.y / duration) % 1;

    uint vertex_id = input.id;
    float position_v = (vertex_id * 3 + 0.5) * texel_size.y;
    float2 position_uv = float2(normalized_time, position_v);
    input.positionOS = SAMPLE_TEXTURE2D_LOD(_AnimationTexture, sampler_AnimationTexture, position_uv, 0);

    float normal_v = (vertex_id * 3 + 1.5) * texel_size.y;
    float2 normal_uv = float2(normalized_time, normal_v);
    input.normalOS = SAMPLE_TEXTURE2D_LOD(_AnimationTexture, sampler_AnimationTexture, normal_uv, 0).xyz;

    float tangent_v = (vertex_id * 3 + 2.5) * texel_size.y;
    float2 tangent_uv = float2(normalized_time, tangent_v);
    input.tangentOS = SAMPLE_TEXTURE2D_LOD(_AnimationTexture, sampler_AnimationTexture, tangent_uv, 0);
}

#define TOON_SHADER_HOOK_VERTEX_INPUT(input) ApplyAnimationTexture(input) 