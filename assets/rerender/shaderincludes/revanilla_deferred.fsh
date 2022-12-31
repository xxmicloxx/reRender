// in vec2 v_texCoord;
// uniform sampler2D t_depth;
// uniform mat4 u_invProjection;
// uniform mat4 u_invModelView;

vec4 deferred_getVsPosition(vec2 texCoord) {
    float projectedZ = texture(t_depth, texCoord).r;
    vec4 ssPosition = vec4(vec3(texCoord, projectedZ) * 2.0 - 1.0, 1.0);
    vec4 vsPosition = u_invProjection * ssPosition;
    vsPosition.xyz /= vsPosition.w;
    vsPosition.w = 1.0;
    return vsPosition;
}

vec4 deferred_getVsPosition() {
    return deferred_getVsPosition(v_texCoord);
}

vec4 deferred_getWsPosition(vec2 texCoord) {
    vec4 vsPosition = deferred_getVsPosition(texCoord);
    return u_invModelView * vsPosition;
}

vec4 deferred_getWsPosition() {
    return deferred_getWsPosition(v_texCoord);
}