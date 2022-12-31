int gbflags_unpack(float normalAlpha) {
    return floatBitsToInt(normalAlpha);
}

int gbflags_unpack(vec4 normal) {
    return gbflags_unpack(normal.a);
}

float gbflags_pack(int flags) {
    return intBitsToFloat(flags);
}