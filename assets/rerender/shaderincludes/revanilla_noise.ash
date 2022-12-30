float _noise_cmod289(float x) {return x - floor(x * (1.0 / 289.0)) * 289.0;}
vec4 _noise_cmod289(vec4 x) {return x - floor(x * (1.0 / 289.0)) * 289.0;}
vec4 _noise_perm(vec4 x) {return _noise_cmod289(((x * 34.0) + 1.0) * x);}

float noise_value(vec3 p) {
    vec3 a = floor(p);
    vec3 d = p - a;
    d = d * d * (3.0 - 2.0 * d);

    vec4 b = a.xxyy + vec4(0.0, 1.0, 0.0, 1.0);
    vec4 k1 = _noise_perm(b.xyxy);
    vec4 k2 = _noise_perm(k1.xyxy + b.zzww);

    vec4 c = k2 + a.zzzz;
    vec4 k3 = _noise_perm(c);
    vec4 k4 = _noise_perm(c + 1.0);

    vec4 o1 = fract(k3 * (1.0 / 41.0));
    vec4 o2 = fract(k4 * (1.0 / 41.0));

    vec4 o3 = o2 * d.z + o1 * (1.0 - d.z);
    vec2 o4 = o3.yw * d.x + o3.xz * (1.0 - d.x);

    return o4.y * d.y + o4.x * (1.0 - d.y);
}


// Value noise by https://www.shadertoy.com/view/4sfGzS; 30.6.21 radfast updated with part of the mod289 approach from https://gist.github.com/patriciogonzalezvivo/670c22f3966e662d2f83
const vec3 vn1 = vec3(0.0,0.0,1.0);
const vec3 vn2 = vec3(0.0,1.0,0.0);
const vec3 vn3 = vec3(0.0,1.0,1.0);
const vec3 vn4 = vec3(1.0,0.0,0.0);
const vec3 vn5 = vec3(1.0,0.0,1.0);
const vec3 vn6 = vec3(1.0,1.0,0.0);
const vec3 vn7 = vec3(1.0,1.0,1.0);

vec3 _noise_ghash( vec3 p )
{
	vec3 o;
	// these constants are the matrix m
	// individual components multiplied, because the whole matrix multiplication produces float rounding differences from C# equivalent code (Bell pepper)
	o.x = 127.1 * p.x + 311.7 * p.y + 74.7 * p.z;
	o.y = 269.5 * p.x + 183.3 * p.y + 246.1 * p.z;
	o.z = 113.5 * p.x + 271.9 * p.y + 124.6 * p.z;
	vec3 q = ((o * 0.025) + 8.0) * o;   // the constants 4.25 and 8.0 found empirically to give similar noise distribution to the sin approach
	return -1.0 + 2.0*fract(mod(q, 289.0) * (1.0 / 41.0));
}

float noise_gnoise( in vec3 p )
{
    vec3 i = floor( p );
    vec3 f = p - i;
	
    vec3 u = f*f*(3.0-2.0*f);

    vec4 a = vec4 ( dot(_noise_ghash(i), f),
                    dot(_noise_ghash(i + vn1), f - vn1),
    		    dot(_noise_ghash(i + vn2), f - vn2),
                    dot(_noise_ghash(i + vn3), f - vn3));
    vec4 b = vec4 ( dot(_noise_ghash(i + vn4), f - vn4),
                    dot(_noise_ghash(i + vn5), f - vn5),
    		    dot(_noise_ghash(i + vn6), f - vn6),
                    dot(_noise_ghash(i + vn7), f - vn7));
    
    vec4 c = mix(a, b, u.x);
    vec2 rg = mix(c.xy, c.zw, u.y);

    // Added 1.2 here because our old noise was stronger
    return 1.2 * mix(rg.x, rg.y, u.z);
}