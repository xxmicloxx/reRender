#version 330 core
#extension GL_ARB_explicit_attrib_location: enable

layout(location = 0) in vec3 i_vertexPosition;
layout(location = 1) in vec2 i_uv;
layout(location = 2) in vec4 i_color;
layout(location = 3) in int i_flags;
layout(location = 4) in float i_damageEffect;
layout(location = 5) in int i_jointId;

out vec2 v_uv;
out float v_damageEffect;
out vec3 v_wsNormal;
out vec3 v_vsNormal;
out vec3 v_msPosition;
out vec4 v_color;
flat out int v_renderFlags;

uniform vec4 u_renderColor;
uniform int u_additionalRenderFlags;

uniform mat4 u_projection;
uniform mat4 u_view;
uniform mat4 u_model;
uniform vec3 u_playerPos;

uniform int u_skipRenderJointId;
uniform int u_skipRenderJointId2;
uniform mat4 u_elementTransforms[35];

#include vertexflagbits.ash
#include revanilla_noise.ash
#include revanilla_warping.vsh
#include revanilla_colorspace.ash

void main(void)
{
    v_damageEffect = i_damageEffect;
    
    mat4 animModelMat = u_model * u_elementTransforms[i_jointId];
    vec4 wsPosition = animModelMat * vec4(i_vertexPosition, 1.0);
    
    v_renderFlags = i_flags | u_additionalRenderFlags;

    wsPosition = warping_applyVertex(v_renderFlags, wsPosition);
    // wsPosition = applyGlobalWarping(wsPosition);

    // Cheap fix to "not render" head in first person mode
    if (i_jointId == u_skipRenderJointId || i_jointId == u_skipRenderJointId) {
        wsPosition.y -= 10000;
    }
    
    vec4 vsPosition = u_view * wsPosition;
    vec3 normal = unpackNormal(v_renderFlags);
    normal = (animModelMat * vec4(normal, 0.0)).xyz;
    
    gl_Position = u_projection * vsPosition;
    
    v_color = colorspace_toLinear(u_renderColor) * colorspace_toLinear(i_color);
    v_vsNormal = (u_view * vec4(normal, 0.0)).xyz;
    v_msPosition = i_vertexPosition.xyz * 1.5;
    
    v_uv = i_uv;
    v_wsNormal = normal;
}