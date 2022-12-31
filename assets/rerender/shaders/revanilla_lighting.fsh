#version 330 core
#extension GL_ARB_explicit_attrib_location: enable

in vec2 v_texCoord;
flat in vec3 v_wsPositionCamera;

out vec4 o_scene;

uniform sampler2D t_depth;
uniform sampler2D t_color;
uniform sampler2D t_normal;
uniform sampler2D t_lighting;

uniform mat4 u_invModelView;
uniform mat4 u_invProjection;
uniform vec3 u_lightDirection;

#include revanilla_deferred.fsh
#include revanilla_shadow.fsh
#include revanilla_gbflags.ash

float lighting_calculateSunlightAmount(vec3 wsPosition, vec3 wsNormal, float sunlightLevel) {
    float nDotL = max(0.0, dot(wsNormal, u_lightDirection));
    float sunlight = 6.0 * nDotL * sunlightLevel;
    
    if (sunlight == 0.0) {
        return 0.0;
    }
    
    float brightness = shadow_getBrightness(wsPosition);
    return brightness * sunlight;
}

vec3 lighting_calculateInfluence(vec3 wsPosition, vec3 wsNormal, vec4 lighting) {
    float sunlightLevel = pow(lighting.a, 2);

    vec3 skylightColor = vec3(0.6, 0.8, 1.0);
    vec3 sunlightColor = vec3(1.0, 0.95, 0.7);

    vec3 ambient = vec3(0.1);
    vec3 skylightAmbient = vec3(sunlightLevel * 1.5) * skylightColor;
    vec3 skylightTop = vec3(max(0.0, dot(wsNormal, vec3(0, 1, 0))) * sunlightLevel) * skylightColor;
    vec3 sunlight = vec3(lighting_calculateSunlightAmount(wsPosition, wsNormal, sunlightLevel)) * sunlightColor;

    return ambient + skylightAmbient + skylightTop + sunlight;
}

void main(void)
{
    vec4 color = texture(t_color, v_texCoord);
    if (color.a < 0.25) discard;

    vec4 normalSample = texture(t_normal, v_texCoord);
    int gbflags = gbflags_unpack(normalSample);
    
    vec4 wsPosition = deferred_getWsPosition();
    vec4 vsNormal = vec4(normalSample.xyz, 0.0);
    vec4 wsNormal = u_invModelView * vsNormal;
    
    vec4 lighting = texture(t_lighting, v_texCoord);
    
    vec3 totalLight = lighting_calculateInfluence(wsPosition.xyz, wsNormal.xyz, lighting);
    
    o_scene = vec4(color.rgb * totalLight, 1.0);
}