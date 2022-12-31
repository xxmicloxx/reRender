#version 330 core
#extension GL_ARB_explicit_attrib_location: enable

// Per vertex
layout (location = 0) in vec3 i_vertexPosition;
layout (location = 1) in vec2 i_uv;
layout (location = 2) in vec4 i_unusedRgbaLight;
layout (location = 3) in int i_renderFlags;

// Per instance
layout (location = 4) in vec3 in_relPos;
layout (location = 5) in int in_colormapDataPart;

// lowest 20 bits are UV offset (measured in 1/1024 of texture map width), 4 bits texture transparency height,
// 3 bits rotation, 5 bits unused  (could be windwave, deform, etc)
layout (location = 6) in int in_uvBaseAndRot;
layout (location = 7) in vec4 in_rgbaLight;

uniform vec3 u_origin;
uniform mat4 u_projection;
uniform mat4 u_modelView;
uniform vec3 u_playerPos;

// Uniforms specific to this instanced shader:

// Not exactly billboarding, semi-billboarding - a texture plane at up to 45 degrees from orthogonal to the player's
// view is acceptable (and looks less "artificial" than true billboarding)
uniform float u_billboardDistSq;
uniform int u_colormapBase;
uniform vec4 u_fplaneNear;
uniform vec4 u_fplaneL;
uniform vec4 u_fplaneR;
uniform float u_texSizeU;
uniform float u_texSizeV;

out float v_alpha;
out vec2 v_uv;
out vec2 v_uvstem;
out vec3 v_vsNormal;
out vec4 v_rgbaLight;

#include vertexflagbits.ash
#include revanilla_noise.ash
#include revanilla_warping.vsh
#include revanilla_colormap.vsh

mat4 rotation( in float angle )
{
    float c = cos(angle);
    float s = sin(angle);
    return mat4(
        c, 0, -s, 0,
        0, 1, 0, 0,
        s, 0, c, 0,
        0, 0, 0, 1
    );
}


bool outsideFrustum(vec4 point)
{
    // Our vertex could be on the opposite side from a vertex 1.41 away - if either one is in the frustum, we want
    // to keep
    if (dot(point, u_fplaneNear) < -3) return true;

    // value of 3 found empirically, larger numbers needed for wider fields of view, theoretical is
    // 0.866 = sqrt((1.41 / 2) ^ 2 + 0.5 * 0.5) which is the max possible distance from poso to a texture corner,
    // plus a bit more than that to allow for windwave

    if (dot(point, u_fplaneL) < -3) return true;  // left of sceen
    if (dot(point, u_fplaneR) < -3) return true;  // right of screen

    return false;
}

const float c_halfPi = 3.1415927 / 2;
const float c_quarterPi = 3.1415927 / 4;

void main()
{
    vec4 poso = vec4(in_relPos + u_origin, 1.0);
    poso.y += 0.5;
    
    if (outsideFrustum(poso))
    {
        // will be discarded
        v_alpha = 0;
    }
    else
    {
        // these 0.1 and 0.2 micro-adjustments give a better visual appearance at (4chunk) edges, for average windwave
        // (medium-strong breeze)
        float cameraAngle = ((poso.x + 0.2 == 0.0) ? sign(poso.z - 0.1) * c_halfPi
            : atan(poso.z - 0.1, poso.x + 0.2)) + (4.5 * c_halfPi);

        // 8 possible 45 degree rotations, starting at -22.5 degrees
        float angle = (((in_uvBaseAndRot >> 24) & 7) - 0.5) * c_quarterPi;

        // This adjustment to angle - based on the cameraAngle calculated above - rotates every model 0, 90, 180 or 270
        // degrees so that its appearance is unchanged (see texture uv.x adjustment below) but now its "front" faces
        // are facing the player; this allows for backface culling to be enabled
        int multiple = int((angle + cameraAngle) / c_halfPi) + 1;
        vec4 truePos = rotation(angle - multiple * c_halfPi) * vec4(i_vertexPosition, 1.0);
        float distSq = poso.x * poso.x + poso.z * poso.z;
        
        // length of truePos is 0.707; if normalized dot product > 0.707, view vector and plane vector are quite well
        // aligned (less than 45 degrees); same test is unnormalized dot product > 0.5
        if (distSq > u_billboardDistSq && abs(dot(poso.xz / sqrt(distSq), truePos.xz)) >= 0.5)
        {
            // will be discarded
            v_alpha = 0;
        }
        else
        {
            v_uv = i_uv;

            // These two lines mirror image the texture on one arm of the cross, depending on rotation: this ensures
            // that even as we move around the model (causing it to rotate to maintain backface culling), front and
            // back faces of each part of the cross match
            if ((multiple + int(i_vertexPosition.x != i_vertexPosition.z)) % 4 < 2) v_uv.x = u_texSizeU - v_uv.x;

            int flags = i_renderFlags;

            // Adjust top of grass model for the grass heights which are less than a full block
            // (slightly helps performance)
            float trueY = (in_uvBaseAndRot >> 18 & 0xf) / 6.0;
            truePos.y = trueY * i_vertexPosition.y;
            // The decimal represents 48 pixels in a 4096 pixel texturemap, we "know" this is the uv.y coordinate of
            // the bottom face set up by InstancedCrossTesselator.cs
            if (v_uv.y == 0.0) v_uv.y = (1.0 - trueY) * u_texSizeV;
            vec3 normal = unpackNormal(i_renderFlags);

            // v_msPosition = (truePos.xyz += in_relPos + u_origin);
            truePos.xyz += in_relPos + u_origin;
            vec4 wsPosition = warping_applyVertex(flags, truePos);
            // For correct appearance, we have to reduce the delta x, y, z of windwave proportionate to the reduced
            // height of the vertex above the ground (but still need windwave's noise functions to be based on the
            // original y = +1.41 vertex positions)
            wsPosition -= (wsPosition - truePos) * (1.0 - trueY);

            // But we do not reduce the temporal storm vertex warping, as that moves our base as well
            // worldPos = applyGlobalWarping(worldPos);

            v_uv.x += (in_uvBaseAndRot & 0x001FF) / 512.0;
            v_uv.y += ((in_uvBaseAndRot & 0x3FE00) / 512) / 512.0;
            v_uvstem.x = v_uv.x + u_texSizeU;
            v_uvstem.y = v_uv.y;

            vec4 cameraPos = u_modelView * wsPosition;
            gl_Position = u_projection * cameraPos;

            colormap_calcUvs(u_colormapBase + (in_colormapDataPart << 16), truePos + vec4(u_playerPos, 0.0),
                in_rgbaLight.a, false);
            
            v_alpha = 1;

            v_rgbaLight = in_rgbaLight;

            vec4 vsNormal = u_modelView * vec4(normal.xyz, 0.0);
            v_vsNormal = vsNormal.xyz;
        }
    }
}



