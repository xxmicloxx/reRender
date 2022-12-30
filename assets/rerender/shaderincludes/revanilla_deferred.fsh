// in vec3 v_texCoord;
// uniform sampler2D t_depth;
// uniform mat4 u_invProjection;
// uniform mat4 u_invModelView;

vec4 deferred_getWsPosition() {
    float projectedZ = texture(t_depth, v_texCoord).r;
    vec4 ssPosition = vec4(vec3(v_texCoord, projectedZ) * 2.0 - 1.0, 1.0);
    vec4 vsPosition = u_invProjection * ssPosition;
    vsPosition.xyz /= vsPosition.w;
    vsPosition.w = 1.0;
    return u_invModelView * vsPosition;
}