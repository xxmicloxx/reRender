#version 330 core
#extension GL_ARB_explicit_attrib_location: enable

layout (location = 0) out vec4 o_color;
layout (location = 1) out vec4 o_normal;
layout (location = 2) out vec4 o_lighting;

in vec2 v_uv;
in float v_damageEffect;
in vec3 v_wsNormal;
in vec3 v_vsNormal;
in vec3 v_msPosition;
in vec4 v_color;
flat in int v_renderFlags;

uniform sampler2D t_entity;
uniform float u_alphaTest = 0.001;
uniform vec4 u_rgbaLight;
uniform int u_entityId;
uniform float u_frostAlpha;

#include revanilla_noise.ash
#include revanilla_effect.fsh
#include revanilla_colorspace.ash
#include revanilla_gbflags.ash

void main(void) {
    vec4 texColor = texture(t_entity, v_uv);
    
    int eidFloor = (u_entityId / 100) * 100;
    float seed = (u_entityId - eidFloor) / 5.0;
    
    texColor = effect_applyFrost(u_frostAlpha, texColor, v_wsNormal, v_msPosition + vec3(seed));
    texColor = colorspace_toLinear(texColor);
    texColor *= v_color;
    
    if (texColor.a < u_alphaTest) discard;
    
    int gbflags = 0;
    
    o_color = vec4(texColor.rgb, 1.0);
    o_normal = vec4(v_vsNormal, gbflags_pack(gbflags));
    o_lighting = u_rgbaLight;
}