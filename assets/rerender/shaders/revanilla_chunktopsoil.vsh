#version 330 core
#extension GL_ARB_explicit_attrib_location: enable

layout (location = 0) in vec3 i_vertexPosition;
layout (location = 1) in vec2 i_uv;
layout (location = 2) in vec4 i_rgbaLight;
layout (location = 3) in int i_renderFlags;
layout (location = 4) in vec2 i_uv2;
layout (location = 5) in int i_colormapData;

out vec3 v_vsNormal;
out vec3 v_msNormal;
out vec2 v_uv;
out vec2 v_uv2;
out vec4 v_rgbaLight;

uniform vec3 u_origin;
uniform mat4 u_modelView;
uniform mat4 u_projection;
uniform vec3 u_playerPos;

#include vertexflagbits.ash
#include revanilla_noise.ash
#include revanilla_colormap.vsh
#include revanilla_colorspace.ash

void main(void) {
    vec4 msPosition = vec4(i_vertexPosition + u_origin, 1.0);
    v_msNormal = unpackNormal(i_renderFlags);

    vec4 vsPosition = u_modelView * msPosition;
    vec4 vsNormal = u_modelView * vec4(v_msNormal, 0.0);

    colormap_calcUvs(i_colormapData, vec4(i_vertexPosition + u_origin + u_playerPos, 1.0), i_rgbaLight.a, false);

    gl_Position = u_projection * vsPosition;

    v_vsNormal = vsNormal.xyz;
    v_uv = i_uv;
    v_uv2 = i_uv2;
    v_rgbaLight = colorspace_toLinear(i_rgbaLight);
}