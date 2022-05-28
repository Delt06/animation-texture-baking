#include "Packages/com.deltation.toon-shader/Assets/DELTation/ToonShader/Shaders/ToonShaderInstancing.hlsl"

#define TOON_SHADER_HOOK_INPUT_BUFFER TEXTURE2D(_AnimationTexture); SAMPLER(sampler_AnimationTexture); float4 _AnimationTexture_TexelSize; float _FrameRate;

#define TOON_SHADER_CUSTOM_INSTANCING_BUFFER TOON_SHADER_DEFINE_INSTANCED_PROP(float, _AnimationTime)
