#version 330 core

uniform sampler2D t_input;
uniform sampler2D t_depth;
uniform vec2 u_frameSize;

in vec2 v_texCoords[11];

out vec4 o_color;

// For info on the bilateral blur see https://www.gamasutra.com/blogs/PeterWester/20140116/208742/Generating_smooth_and_cheap_SSAO_using_Temporal_Blur.php
// http://dev.theomader.com/gaussian-kernel-calculator/
void main(void)
{
    vec4 out_colour = vec4(0.0);

    float refDepth = texture(t_depth, v_texCoords[5]).r;
    float fac = 300;


    float w0 = (1 - clamp(abs(texture(t_depth, v_texCoords[0]).r - refDepth) * fac, 0, 1)) * 0.003456;
    float w1 = (1 - clamp(abs(texture(t_depth, v_texCoords[1]).r - refDepth) * fac, 0, 1)) * 0.015715;
    float w2 = (1 - clamp(abs(texture(t_depth, v_texCoords[2]).r - refDepth) * fac, 0, 1)) * 0.051008;
    float w3 = (1 - clamp(abs(texture(t_depth, v_texCoords[3]).r - refDepth) * fac, 0, 1)) * 0.118235;
    float w4 = (1 - clamp(abs(texture(t_depth, v_texCoords[4]).r - refDepth) * fac, 0, 1)) * 0.195779;
    float w5 = (1 - clamp(abs(texture(t_depth, v_texCoords[5]).r - refDepth) * fac, 0, 1)) * 0.231613;
    float w6 = (1 - clamp(abs(texture(t_depth, v_texCoords[6]).r - refDepth) * fac, 0, 1)) * 0.195779;
    float w7 = (1 - clamp(abs(texture(t_depth, v_texCoords[7]).r - refDepth) * fac, 0, 1)) * 0.118235;
    float w8 = (1 - clamp(abs(texture(t_depth, v_texCoords[8]).r - refDepth) * fac, 0, 1)) * 0.051008;
    float w9 = (1 - clamp(abs(texture(t_depth, v_texCoords[9]).r - refDepth) * fac, 0, 1)) * 0.015715;
    float w10 = (1 - clamp(abs(texture(t_depth, v_texCoords[10]).r - refDepth) * fac, 0, 1)) * 0.003456;

    float wsum = w0 + w1 + w2 + w3 + w4 + w5 + w6 + w7 + w8 + w9 + w10;

    out_colour += texture(t_input, v_texCoords[0]) * w0;
    out_colour += texture(t_input, v_texCoords[1]) * w1;
    out_colour += texture(t_input, v_texCoords[2]) * w2;
    out_colour += texture(t_input, v_texCoords[3]) * w3;
    out_colour += texture(t_input, v_texCoords[4]) * w4;
    out_colour += texture(t_input, v_texCoords[5]) * w5;
    out_colour += texture(t_input, v_texCoords[6]) * w6;
    out_colour += texture(t_input, v_texCoords[7]) * w7;
    out_colour += texture(t_input, v_texCoords[8]) * w8;
    out_colour += texture(t_input, v_texCoords[9]) * w9;
    out_colour += texture(t_input, v_texCoords[10]) * w10;

    o_color = out_colour / wsum;
    //outColor = out_colour / 1; // Borderlands mode XD
}