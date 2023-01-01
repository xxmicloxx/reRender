#version 330 core
#extension GL_ARB_explicit_attrib_location: enable

in vec2 v_texCoord;

out vec4 o_color;

uniform sampler2D t_scene;

#include revanilla_colorspace.ash

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
    
    const float exposure = 0;
    color *= pow(2, exposure);
    
    vec3 tonemapped = tonemap_filmic(color);
    o_color = vec4(colorspace_toGamma(tonemapped), 1.0);
    //o_color = colorspace_toGamma(vec4(vec3(1.0, 1.0, 1.0), 1.0));
}