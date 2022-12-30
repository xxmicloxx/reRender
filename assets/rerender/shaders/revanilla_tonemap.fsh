#version 330 core
#extension GL_ARB_explicit_attrib_location: enable

in vec2 v_texCoord;

out vec4 o_color;

uniform sampler2D t_scene;

vec3 tonemap_filmic(vec3 x)
{
    float a = 2.51f;
    float b = 0.03f;
    float c = 2.43f;
    float d = 0.59f;
    float e = 0.14f;
    return (x*(a*x+b))/(x*(c*x+d)+e);
}

void main(void)
{
    vec3 color = texture(t_scene, v_texCoord).rgb;
    
    // exposure
    color *= pow(2, -1.4);
    
    o_color = vec4(pow(tonemap_filmic(color), vec3(1/2.2)), 1.0);
    //o_color = vec4(pow(vec3(1.0, 0.95, 0.4), vec3(1/2.2)), 1.0);
}