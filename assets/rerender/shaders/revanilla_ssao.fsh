#version 330 core
#extension GL_ARB_explicit_attrib_location: enable

out float o_occlusion;

in vec2 v_texCoord;
flat in vec3 v_wsPositionCamera;

uniform sampler2D t_depth;
uniform sampler2D t_normal;
uniform sampler2D t_noise;
uniform vec2 u_screenSize;
uniform vec3[64] u_samples;
uniform mat4 u_invModelView;
uniform mat4 u_invProjection;
uniform mat4 u_projection;

#include revanilla_deferred.fsh

const int c_kernelSize = 20;
const float c_radius = 0.9;
const float c_bias = 0.004;

float ssao(vec3 fragPos, vec3 normal) {
    // tile noise texture over screen based on screen dimensions divided by noise size
    vec2 noiseScale = vec2(u_screenSize.x/8.0, u_screenSize.y/8.0);
    vec3 randomVec = texture(t_noise, v_texCoord * noiseScale).xyz;

    vec3 tangent = normalize(randomVec - normal * dot(randomVec, normal));
    vec3 bitangent = cross(normal, tangent);
    mat3 TBN = mat3(tangent, bitangent, normal);

    float occlusion = 0.0;

    for (int i = 0; i < c_kernelSize; ++i)
    {
        vec3 smp = TBN * u_samples[i];
        smp = fragPos + smp * c_radius;
    
        vec4 offset = vec4(smp, 1.0);
        offset = u_projection * offset;
        offset.xyz /= offset.w;
        offset.xyz = offset.xyz * 0.5 + 0.5;
        
        offset.x = clamp(offset.x, v_texCoord.x - 0.04, v_texCoord.x + 0.04);
        offset.y = clamp(offset.y, v_texCoord.y - 0.04, v_texCoord.y + 0.04);
        
        float sampleDepth = deferred_getVsPosition(offset.xy).z;
        
        float depthDiff = sampleDepth - (smp.z + c_bias * -smp.z);
        float rangeCheck = 0;
        
        if (depthDiff >= 0 && depthDiff < 0.2) {
            rangeCheck = smoothstep(0.0, 1.0, c_radius / abs(fragPos.z - sampleDepth));
        }
        
        occlusion += rangeCheck;
    }

    return occlusion / c_kernelSize;
}

void main(void)
{
    vec3 vsPosition = deferred_getVsPosition().xyz;
    vec3 vsNormal = texture(t_normal, v_texCoord).xyz;

    float distanceFade = clamp(1.2 - (-vsPosition.z) / 250, 0, 1);
    
    if (distanceFade == 0) {
        o_occlusion = 1.0;
        return;
    }
    
    float occlusion = ssao(vsPosition, normalize(vsNormal));
    
    o_occlusion = clamp(1.0 - min(1, occlusion * distanceFade), 0, 1);
}