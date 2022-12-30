in vec2 v_colormap_climateUv;
in vec2 v_colormap_seasonUv;

in float v_colormap_seasonWeight;
in float v_colormap_frostAlpha;
in float v_colormap_hereTemp;

vec4 colormap_getFrosted(vec4 color) {
    if (v_colormap_hereTemp < 0.333 && v_colormap_frostAlpha > 0) {
        float w = clamp((0.333 - v_colormap_hereTemp) * 15, 0, 1);
        float b = (color.r + color.g + color.b) / 3.0;
        vec3 frostColor = vec3(b + v_colormap_frostAlpha * 0.2);
        float faw = v_colormap_frostAlpha * w;
        color.rgb = color.rgb * (1 - faw) + frostColor * faw;
    }

    return color;
}

vec4 colormap_getMapped(sampler2D sourceTex, vec4 color) {
    vec4 tint = vec4(1);
    bool mapped = false;
    
    if (v_colormap_climateUv.x >= 0) {
        tint = texture(sourceTex, v_colormap_climateUv);
        mapped = true;
    }
    
    if (v_colormap_seasonUv.x >= 0 && v_colormap_seasonWeight > 0) {
        vec4 seasonColor = texture(sourceTex, v_colormap_seasonUv);
        tint = mix(tint, seasonColor, v_colormap_seasonWeight);
        mapped = true;
    }
    
    if (v_colormap_hereTemp < 0.333 && v_colormap_frostAlpha > 0) {
        float w = clamp((0.333 - v_colormap_hereTemp) * 15, 0, 1);
        
        if (mapped) {
            tint.rgb = mix(tint.rgb, tint.rgb * (1 - v_colormap_frostAlpha) + vec3(1.0) * v_colormap_frostAlpha, w);
        } else {
            return colormap_getFrosted(color);
        }
    }
    
    return color * tint;
}