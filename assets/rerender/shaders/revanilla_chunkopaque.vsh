#version 330 core
#extension GL_ARB_explicit_attrib_location: enable

layout (location = 0) in vec3 i_vertexPosition;
layout (location = 1) in vec2 i_uv;
layout (location = 2) in vec4 i_rgbaLight;
layout (location = 3) in int i_renderFlags;
layout (location = 4) in int i_colormapData;

out vec3 v_vsNormal;
out vec2 v_uv;
out vec4 v_rgbaLight;
out vec3 v_msPosition;
flat out int v_renderFlags;

uniform vec3 u_origin;
uniform mat4 u_modelView;
uniform mat4 u_projection;
uniform vec3 u_playerPos;

#include vertexflagbits.ash
#include revanilla_noise.ash
#include revanilla_warping.vsh
#include revanilla_colormap.vsh
#include revanilla_colorspace.ash

void main(void) {
    bool isLeaves = ((i_renderFlags & WindModeBitMask) > 0); 

    vec4 msPosition = vec4(i_vertexPosition + u_origin, 1.0);
    msPosition = warping_applyVertex(i_renderFlags, msPosition);

    vec3 msNormal = unpackNormal(i_renderFlags);

    vec4 vsPosition = u_modelView * msPosition;
    vec4 vsNormal = u_modelView * vec4(msNormal, 0.0);

    colormap_calcUvs(i_colormapData, vec4(i_vertexPosition + u_origin + u_playerPos, 1.0), i_rgbaLight.a, isLeaves);
    
    gl_Position = u_projection * vsPosition;

    v_vsNormal = vsNormal.xyz;
    v_msPosition = msPosition.xyz;
    v_uv = i_uv;
    v_rgbaLight = colorspace_toLinear(i_rgbaLight);
    v_renderFlags = i_renderFlags;

    // To fix Z-Fighting on blocks over certain other blocks. 
    if (gl_Position.z > 0) {
        int zOffset = (i_renderFlags & ZOffsetBitMask) >> 8;
        gl_Position.w += zOffset * 0.00025 / max(0.1, gl_Position.z * 0.05);
    }
}