#if SHADOWQUALITY > 1
uniform float u_shadow_rangeNear;
uniform mat4 u_shadow_toMapNear;
uniform sampler2DShadow t_shadow_mapNear;
#endif

#if SHADOWQUALITY > 0
uniform float u_shadow_rangeFar;
uniform mat4 u_shadow_toMapFar;
uniform sampler2DShadow t_shadow_mapFar;

uniform float u_shadow_mapWidthInv;
uniform float u_shadow_mapHeightInv;
#endif

void shadow_calcCoords(vec3 wsPosition, out vec4 shadowCoordsNear, out vec4 shadowCoordsFar) {
    float nearSub = 0;
#if SHADOWQUALITY > 0
    float len = length(wsPosition);
#endif
    
#if SHADOWQUALITY > 1
    // Near map
    shadowCoordsNear = u_shadow_toMapNear * vec4(wsPosition, 1.0);
    
    float distanceNear = clamp(
        max(max(0, 0.03 - shadowCoordsNear.x) * 100, max(0, shadowCoordsNear.x - 0.97) * 100) +
        max(max(0, 0.03 - shadowCoordsNear.y) * 100, max(0, shadowCoordsNear.y - 0.97) * 100) +
        max(0, shadowCoordsNear.z - 0.98) * 100 +
        max(0, len / u_shadow_rangeNear - 0.15),
        0, 1
    );
    
    nearSub = shadowCoordsNear.w = clamp(1.0 - distanceNear, 0.0, 1.0);
#endif
    
#if SHADOWQUALITY > 0
    // Far map
    shadowCoordsFar = u_shadow_toMapFar * vec4(wsPosition, 1.0);
    
    float distanceFar = clamp(
        max(max(0, 0.03 - shadowCoordsFar.x) * 10, max(0, shadowCoordsFar.x - 0.97) * 10) +
        max(max(0, 0.03 - shadowCoordsFar.y) * 10, max(0, shadowCoordsFar.y - 0.97) * 10) +
        max(0, shadowCoordsFar.z - 0.98) * 10 +
        max(0, len / u_shadow_rangeFar - 0.15),
        0, 1
    );
    
    distanceFar = distanceFar * 2 - 0.5;
    
    shadowCoordsFar.w = max(0, clamp(1.0 - distanceFar, 0.0, 1.0) - nearSub);
#endif
}

float shadow_getBrightnessWithCoords(vec4 coordsNear, vec4 coordsFar) {
#if SHADOWQUALITY > 0
    float totalFar = 0.0;
    
    if (coordsFar.z < 0.999 && coordsFar.w > 0) {
        for (int x = -1; x <= 1; ++x) {
            for (int y = -1; y <= 1; ++y) {
                vec3 coordsOffset = vec3(x * u_shadow_mapWidthInv, y * u_shadow_mapHeightInv, -0.0009);
                float inLight = texture(t_shadow_mapFar, coordsFar.xyz + coordsOffset);
                totalFar += 1 - inLight;
            }
        }
    }

    totalFar /= 9.0;

    float b = 1.0 - totalFar * coordsFar.w;
#endif

#if SHADOWQUALITY > 1
    float totalNear = 0.0;

    if (coordsNear.z < 0.999 && coordsNear.w > 0) {
        for (int x = -1; x <= 1; ++x) {
            for (int y = -1; y <= 1; ++y) {
                vec3 coordsOffset = vec3(x * u_shadow_mapWidthInv, y * u_shadow_mapHeightInv, -0.0005);
                float inLight = texture(t_shadow_mapNear, coordsNear.xyz + coordsOffset);
                totalNear += 1 - inLight;
            }
        }
    }

    totalNear /= 9.0;

    b -= totalNear * coordsNear.w;
#endif

#if SHADOWQUALITY > 0
    b = clamp(b, 0.0, 1.0);
    return b;
#endif

    // shadow mapping is off -> never in shadow
    return 1.0;
}

float shadow_getBrightness(vec3 wsPosition) {
    vec4 coordsNear, coordsFar;
    shadow_calcCoords(wsPosition, coordsNear, coordsFar);
    return shadow_getBrightnessWithCoords(coordsNear, coordsFar);
}