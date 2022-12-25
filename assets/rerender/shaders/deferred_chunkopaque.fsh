#version 330 core
#extension GL_ARB_explicit_attrib_location: enable

in vec3 v_vsPosition;
in vec2 v_uv;

layout (location = 0) out vec4 o_color;

uniform sampler2D u_terrainTex;

void main(void) {
    vec4 texColor = texture(u_terrainTex, v_uv);
    if (texColor.a < 0.5) discard;

    o_color = vec4(texColor.rgb, 1.0);
}