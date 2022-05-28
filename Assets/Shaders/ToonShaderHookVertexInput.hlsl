#include "Packages/com.deltation.toon-shader/Assets/DELTation/ToonShader/Shaders/ToonShaderInstancing.hlsl"

void ApplyAnimationTexture(inout appdata input)
{
    float4 texel_size = _AnimationTexture_TexelSize;
    float frame_count = texel_size.z;
    float duration = frame_count / _FrameRate;
    float animation_time = TOON_SHADER_ACCESS_INSTANCED_PROP(_AnimationTime);
    float normalized_time = animation_time / duration % 1;

    uint vertex_id = input.id;
    float position_v = (vertex_id * 3 + 0.5) * texel_size.y;
    float2 position_uv = float2(normalized_time, position_v);
    input.positionOS = SAMPLE_TEXTURE2D_LOD(_AnimationTexture, sampler_AnimationTexture, position_uv, 0);

    #ifdef ANIMATION_TEXTURE_READ_NORMALS
    float normal_v = (vertex_id * 3 + 1.5) * texel_size.y;
    float2 normal_uv = float2(normalized_time, normal_v);
    input.normalOS = SAMPLE_TEXTURE2D_LOD(_AnimationTexture, sampler_AnimationTexture, normal_uv, 0).xyz;
    #endif

    #ifdef ANIMATION_TEXTURE_READ_TANGENTS
    float tangent_v = (vertex_id * 3 + 2.5) * texel_size.y;
    float2 tangent_uv = float2(normalized_time, tangent_v);
    input.tangentOS = SAMPLE_TEXTURE2D_LOD(_AnimationTexture, sampler_AnimationTexture, tangent_uv, 0);
    #endif
}

#define TOON_SHADER_HOOK_VERTEX_INPUT(input) ApplyAnimationTexture(input)