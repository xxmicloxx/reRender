#version 330 core
#extension GL_ARB_explicit_attrib_location: enable

out vec2 v_texCoord;
flat out vec3 v_wsPositionCamera;

uniform mat4 u_invModelView;

void main(void)
{
    // https://rauwendaal.net/2014/06/14/rendering-a-screen-covering-triangle-in-opengl/
    float x = -1.0 + float((gl_VertexID & 1) << 2);
    float y = -1.0 + float((gl_VertexID & 2) << 1);
    gl_Position = vec4(x, y, 0, 1);
    v_texCoord = vec2((x+1.0) * 0.5, (y + 1.0) * 0.5);

    v_wsPositionCamera = (u_invModelView * vec4(0, 0, 0, 1)).xyz;
}