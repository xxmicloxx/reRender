#version 330 core
#extension GL_ARB_explicit_attrib_location: enable

in vec3 v_vsNormal;
in vec3 v_msNormal;
in vec2 v_uv;
in vec2 v_uv2;
in vec4 v_rgbaLight;

layout (location = 0) out vec4 o_color;
layout (location = 1) out vec4 o_normal;
layout (location = 2) out vec4 o_lighting;

uniform sampler2D t_terrain;
uniform sampler2D t_terrainLinear;
uniform vec2 u_blockTextureSize;

#include revanilla_colormap.fsh

void main(void) {
    vec4 texColor;

    vec4 brownSoilColor = texture(t_terrain, v_uv);
    vec4 grassColor;

    if (v_msNormal.y < 0) {
        // bottom
        texColor = brownSoilColor;
    } else {
        vec2 grassUvOffset = v_msNormal.y > 0 ? vec2(u_blockTextureSize.x, 0.0) : vec2(0.0);
        
        grassColor = colormap_getMapped(t_terrainLinear, texture(t_terrain, v_uv2 + grassUvOffset));
        
        texColor = brownSoilColor * (1 - grassColor.a) + grassColor * grassColor.a;
    }
    
    texColor = pow(texColor, vec4(2.2));

    if (texColor.a < 0.01) discard;

    o_color = vec4(texColor.rgb, 1.0);
    o_normal = vec4(v_vsNormal, 1.0);
    o_lighting = v_rgbaLight;
}