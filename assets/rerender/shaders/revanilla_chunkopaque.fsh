#version 330 core
#extension GL_ARB_explicit_attrib_location: enable

in vec3 v_vsNormal;
in vec2 v_uv;
in vec4 v_rgbaLight;
in vec3 v_msPosition;
flat in int v_renderFlags;

layout (location = 0) out vec4 o_color;
layout (location = 1) out vec4 o_normal;
layout (location = 2) out vec4 o_lighting;

uniform sampler2D t_terrain;
uniform sampler2D t_terrainLinear;
uniform float u_alphaTest;
uniform float u_viewDistanceLod0;

#include vertexflagbits.ash
#include revanilla_colormap.fsh

float calculateAlphaTest(vec4 texColor) {
    float aTest = texColor.a;

    // Lod 0 fade
    // This makes the lod fade more noticable, actually O_O
    if ((v_renderFlags & Lod0BitMask) != 0) {

        // We made this transition smoother, because it looks better,
        // if you notice chunk popping, revert to the old, harsher transition
        // Radfast and Tyron, May 28 2021 ^_^
        // Thanks Radfast and Tyron <3 --xxmicloxx
        float b = clamp(10 * (1.05 - length(v_msPosition.xz) / u_viewDistanceLod0) - 2.5, 0, 1);
        //float b = clamp(20 * (1.05 - length(worldPos.xz) / viewDistanceLod0) - 5, 0, 1);

        aTest -= 1-b;
    }

    return aTest;
}

void main(void) {
    vec4 texColor = pow(colormap_getMapped(t_terrainLinear, texture(t_terrain, v_uv)), vec4(2.2));

    float aTest = calculateAlphaTest(texColor);
    if (aTest < u_alphaTest) discard;

    o_color = vec4(texColor.xyz, 1.0);
    o_normal = vec4(v_vsNormal, 1.0);
    o_lighting = v_rgbaLight;
}