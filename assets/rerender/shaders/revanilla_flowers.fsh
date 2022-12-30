#version 330 core
#extension GL_ARB_explicit_attrib_location: enable

layout (location = 0) out vec4 o_color;
layout (location = 1) out vec4 o_normal;
layout (location = 2) out vec4 o_lighting;

in float v_alpha;
in vec2 v_uv;
in vec2 v_uvstem;
in vec3 v_vsNormal;
in vec4 v_rgbaLight;

uniform sampler2D t_terrain;
uniform sampler2D t_terrainLinear;
uniform float u_alphaTest;

#include revanilla_colormap.fsh

void main(void) {
    if (v_alpha < 0.005) discard;

    vec4 petalColor = texture(t_terrain, v_uv);
    vec4 texColor = (petalColor.a > 0.005 ? colormap_getFrosted(petalColor) : colormap_getMapped(t_terrainLinear, texture(t_terrain, v_uvstem))) * vec4(vec3(1.0), v_alpha);
    texColor = pow(texColor, vec4(2.2));

    if (texColor.a < u_alphaTest) discard;

    o_color = vec4(texColor.rgb, 1.0);
    o_normal = vec4(v_vsNormal, 1.0);
    o_lighting = v_rgbaLight;
}