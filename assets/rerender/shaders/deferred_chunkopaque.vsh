#version 330 core
#extension GL_ARB_explicit_attrib_location: enable

layout (location = 0) in vec3 i_vertexPosition;
layout (location = 1) in vec2 i_uv;
layout (location = 2) in vec4 i_rgbaLight;
layout (location = 3) in int i_renderFlags;
layout (location = 4) in int i_colormapData;

out vec3 v_vsPosition;
//out vec3 v_vsNormal;
out vec2 v_uv;

uniform vec3 u_origin;
uniform mat4 u_modelView;
uniform mat4 u_projection;

void main(void) {
    vec4 wsPosition = vec4(i_vertexPosition + u_origin, 1.0);

    vec4 vsPosition = u_modelView * wsPosition;
    v_vsPosition = vsPosition.xyz;

    v_uv = i_uv;

    gl_Position = u_projection * vsPosition;
}