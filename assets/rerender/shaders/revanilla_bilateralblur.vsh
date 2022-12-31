#version 330 core

uniform vec2 u_frameSize;
uniform int u_isVertical;

out vec2 v_texCoords[11];

void main(void)
{
    float x = -1.0 + float((gl_VertexID & 1) << 2);
    float y = -1.0 + float((gl_VertexID & 2) << 1);
    gl_Position = vec4(x, y, 0, 1);
    vec2 texCoord = vec2((x+1.0) * 0.5, (y + 1.0) * 0.5);

    if (u_isVertical == 1) {
        float pixelSize = 1.0 / u_frameSize.y;

        for (int i = -5; i < 5; i++) {
            v_texCoords[i + 5] = texCoord + vec2(0, pixelSize * i);
        }

    } else {
        float pixelSize = 1.0 / u_frameSize.x;

        for (int i = -5; i < 5; i++) {
            v_texCoords[i + 5] = texCoord + vec2(pixelSize * i, 0);
        }
    }
}