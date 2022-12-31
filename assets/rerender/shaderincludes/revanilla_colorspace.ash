vec4 colorspace_toLinear(vec4 srgb) {
    // approximation, but good enough for us
    return vec4(pow(srgb.rgb, vec3(2.2)), srgb.a);
}

vec3 colorspace_toLinear(vec3 srgb) {
    return colorspace_toLinear(vec4(srgb, 1.0)).rgb;
}

vec4 colorspace_toGamma(vec4 linear, float gamma) {
    return vec4(pow(linear.rgb, vec3(1/gamma)), linear.a);
}

vec3 colorspace_toGamma(vec3 linear, float gamma) {
    return colorspace_toGamma(vec4(linear, 1.0), gamma).rgb;
}

vec4 colorspace_toGamma(vec4 linear) {
    // default gamma is 2.2
    return colorspace_toGamma(linear, 2.2);
}

vec3 colorspace_toGamma(vec3 linear) {
    return colorspace_toGamma(vec4(linear, 1.0)).rgb;
}