#version 430 core

#define GROUP_SIZE 256

uniform float u_minLogLum;
uniform float u_inverseLogLumRange;

layout(local_size_x = 16, local_size_y = 16) in;
layout(rgba32f, binding = 0) uniform image2D t_lighting;
layout(std430, binding = 0) buffer ssbo_histogram
{
    uint b_histogram[256];
};

shared uint s_histogram[256];

const vec3 c_colorToLuma = vec3(0.2126, 0.7152, 0.0722);

uint colorToBin(vec3 hdrColor) {
    float lum = dot(hdrColor, c_colorToLuma);
    
    if (lum < 0.0001) {
        return 0;
    }
    
    float logLum = clamp((log2(lum) - u_minLogLum) * u_inverseLogLumRange, 0.0, 1.0);
    
    return uint(logLum * 254.0 + 1.0);
}

void main() {
    s_histogram[gl_LocalInvocationIndex] = 0;
    barrier();
    
    uvec2 dim = imageSize(t_lighting).xy;
    if (gl_GlobalInvocationID.x < dim.x && gl_GlobalInvocationID.y < dim.y) {
        vec3 hdrColor = imageLoad(t_lighting, ivec2(gl_GlobalInvocationID.xy)).rgb;
        uint binIndex = colorToBin(hdrColor);
        atomicAdd(s_histogram[binIndex], 1);
    }
    
    barrier();
    
    atomicAdd(b_histogram[gl_LocalInvocationIndex], s_histogram[gl_LocalInvocationIndex]);
}