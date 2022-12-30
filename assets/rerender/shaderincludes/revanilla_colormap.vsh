uniform vec4 u_colormap_rects[40];
uniform float u_colormap_seasonTemp;
uniform float u_colormap_atlasHeight;
uniform float u_colormap_seaLevel;
uniform float u_colormap_seasonRel;

out vec2 v_colormap_climateUv;
out vec2 v_colormap_seasonUv;

out float v_colormap_seasonWeight;
out float v_colormap_frostAlpha;
out float v_colormap_hereTemp;

void colormap_calcUvs(int colormapData, vec4 wsPosition, float sunlightLevel, bool isLeaves) {
    int seasonMapIndex = colormapData & 0x3f;
    int climateMapIndex = (colormapData >> 8) & 0xf;
    int frostableBit = (colormapData >> 12) & 1;
    float tempRel = clamp(((colormapData >> 16) & 0xff) / 255.0, 0.001, 0.999);
    float rainfallRel = clamp(((colormapData >> 24) & 0xff) / 255.0, 0.001, 0.999);

    v_colormap_frostAlpha = 0;
    v_colormap_hereTemp = tempRel + u_colormap_seasonTemp;
    if (frostableBit > 0 && v_colormap_hereTemp < 0.333) {
        v_colormap_frostAlpha = (noise_value(wsPosition.xyz / 2) + noise_value(wsPosition.xyz * 2)) * 1.25 - 0.5;
        v_colormap_frostAlpha -= max(0, 1 - pow(2 * sunlightLevel, 10));
    }

    if (climateMapIndex > 0) {
        #if RADEONHDFIX == 1
        vec4 rect = u_colormap_rects[climateMapIndex];
        #else
        vec4 rect = u_colormap_rects[climateMapIndex - 1];
        #endif

        v_colormap_climateUv = vec2(rect.x + rect.z * tempRel, rect.y + rect.w * rainfallRel);
    } else {
        v_colormap_climateUv = vec2(-1.0);
    }

    if (seasonMapIndex > 0) {
        vec4 rect = u_colormap_rects[seasonMapIndex - 1];

        float div1 = isLeaves ? 6 : 24;
        float div2 = isLeaves ? 2 : 12;

        float noise = (noise_value(wsPosition.xyz / div1) + noise_value(wsPosition.xyz / div2) - 0.55) * 1.25;

        float b = noise_value(wsPosition.xyz) + noise_value(wsPosition.xyz / 2);

        float mul = 1.0 / (rect.w * u_colormap_atlasHeight);
        v_colormap_seasonUv = vec2(
            rect.x + rect.z * clamp(u_colormap_seasonRel + b / 40, 0.01, 0.99),
            rect.y + rect.w * clamp(noise, 0.5 * mul, 15.5 * mul)
        );

        // different seasonWeight for tropical seasonTints - rich greens based on varying rainfall, but turn this off (anaemic / dying appearance) in colder areas
        if ((colormapData & 0xc0) == 0x40)
        {
            // we dial this down to nothing (leaving dead-looking climate tinted foliage only) if the temperature is below around 0 degrees, browning starts below around 20 degrees
            v_colormap_seasonWeight = clamp((tempRel + u_colormap_seasonTemp / 2) * 0.9 - 0.1, 0, 1) * clamp(2 * (0.5 - cos(rainfallRel * 255.0 / 42.0)) / 2.1, 0.1, 0.75);
        } else {
            // We need ground level temperature (i.e. reversing the seaLevel adjustment in ClientWorldMap.GetAdjustedTemperature()). This formula is shamelessly copied from TerraGenConfig.cs
            float x = tempRel * 255;
            float seaLevelDist = wsPosition.y - u_colormap_seaLevel;
            x += max(0, seaLevelDist * 1.5);
            
            v_colormap_seasonWeight = clamp(0.5 - cos(x / 42.0) / 2.3 + max(0, 128 - x) / 256 / 2 - max(0, x - 130) / 200, 0, 1);
        }
    } else {
        v_colormap_seasonUv = vec2(-1.0);
    }
}