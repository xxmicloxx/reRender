vec4 effect_applyFrost(float frostAlpha, vec4 texColor, vec3 wsNormal, vec3 noisePos) {
    if (frostAlpha > 0) {
        noisePos = round(noisePos * 32.0) / 32;
        noisePos *= 1.5;
        
        frostAlpha *= (noise_value(noisePos * 2) + noise_value(noisePos * 16)) * 1.25 - 0.5;
        frostAlpha += max(0, wsNormal.y/3);
        
        float heretemp = -10;
        float w = clamp((0.333 - heretemp) * 15, 0, 1);
        
        vec3 frostColor = vec3(1);
        float faw = frostAlpha * w;
        texColor.rgb = texColor.rgb * (1 - faw) + frostColor * faw;
    }
    
    return texColor;
}