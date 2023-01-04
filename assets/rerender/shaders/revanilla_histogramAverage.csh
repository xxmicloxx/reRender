#version 430 core

#define GROUP_SIZE 256

layout(local_size_x = 256) in;

layout(std430, binding = 0) buffer ssbo_histogram {
    uint b_histogram[256];
};

layout(std430, binding = 1) buffer ssbo_average {
    float b_average;
};

shared uint s_histogram[256];

uniform float u_minLogLum;
uniform float u_logLumRange;
uniform int u_numPixels;
uniform float u_timeCoeff;

void main() {
    uint countForThisBin = b_histogram[gl_LocalInvocationIndex];
    s_histogram[gl_LocalInvocationIndex] = countForThisBin * gl_LocalInvocationIndex;
    
    barrier();
    
    b_histogram[gl_LocalInvocationIndex] = 0;
    
    // manually unrolled following code:
    // for (uint cutoff = (GROUP_SIZE >> 1); cutoff > 0; cutoff >>= 1) {
    //     if (uint(gl_LocalInvocationIndex) < cutoff) {
    //         s_histogram[gl_LocalInvocationIndex] += s_histogram[gl_LocalInvocationIndex + cutoff];
    //     }
    //     
    //     barrier();
    // }
    if (uint(gl_LocalInvocationIndex) < 128) {
        s_histogram[gl_LocalInvocationIndex] += s_histogram[gl_LocalInvocationIndex + 128];
    }
    barrier();
    
    if (uint(gl_LocalInvocationIndex) < 64) {
        s_histogram[gl_LocalInvocationIndex] += s_histogram[gl_LocalInvocationIndex + 64];
    }
    barrier();

    if (uint(gl_LocalInvocationIndex) < 32) {
        s_histogram[gl_LocalInvocationIndex] += s_histogram[gl_LocalInvocationIndex + 32];
    }
    barrier();

    if (uint(gl_LocalInvocationIndex) < 16) {
        s_histogram[gl_LocalInvocationIndex] += s_histogram[gl_LocalInvocationIndex + 16];
    }
    barrier();

    if (uint(gl_LocalInvocationIndex) < 8) {
        s_histogram[gl_LocalInvocationIndex] += s_histogram[gl_LocalInvocationIndex + 8];
    }
    barrier();

    if (uint(gl_LocalInvocationIndex) < 4) {
        s_histogram[gl_LocalInvocationIndex] += s_histogram[gl_LocalInvocationIndex + 4];
    }
    barrier();

    if (uint(gl_LocalInvocationIndex) < 2) {
        s_histogram[gl_LocalInvocationIndex] += s_histogram[gl_LocalInvocationIndex + 2];
    }
    barrier();

    if (uint(gl_LocalInvocationIndex) == 0) {
        s_histogram[0] += s_histogram[1];
    }
    barrier();
    
    if (gl_LocalInvocationIndex == 0) {
        // divide by all pixels that are not in bucket 0
        // we do this by dividing through 1 - the amount in bucket 0
        float weightedLogAverage = (s_histogram[0] / max(u_numPixels - float(countForThisBin), 1.0)) - 1.0;
        
        float weightedAvgLum = exp2(((weightedLogAverage / 254.0) * u_logLumRange) + u_minLogLum);
        float invWeightedAvgLum = max((1 / weightedAvgLum) - 15, 0.0);
        weightedAvgLum = clamp(1 / invWeightedAvgLum, 0.02, 0.2);
        
        // interpolate with last value
        float lumLastFrame = b_average;
        float adaptedLum = lumLastFrame + (weightedAvgLum - lumLastFrame) * u_timeCoeff;
        b_average = adaptedLum;
    }
}