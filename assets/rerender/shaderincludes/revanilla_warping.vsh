uniform float u_warping_windWaveIntensity = 1.0;
uniform float u_warping_windSpeed;
uniform float u_warping_windWaveCounter;
uniform float u_warping_windWaveCounterHighFreq;
uniform float u_warping_waterWaveCounter;

vec4 warping_applyVertex(int renderFlags, vec4 msPosition) {
#if WAVINGSTUFF == 1
	if ((renderFlags & WindModeBitMask) > 0) {
		
		int windMode = (renderFlags >> WindModePostion) & 0xF;
		int windData =  (renderFlags >> WindDataPosition) & 0x7;
		
		float x = msPosition.x + msPosition.x;
		float z = msPosition.z + msPosition.z;
		
		if (windMode != 6) {
			float y = msPosition.y + msPosition.y;
			
			// Fixes jitter due to float rounding errors
			y = ceil(y * 10000) / 10000.0;
			
			float heightBend = 0;
			
			float strength = u_warping_windWaveIntensity * (1 + u_warping_windSpeed) / 30.0;
			float bendCounter = u_warping_windWaveCounter;
			float vbendMul = 1/5.0;
			float wwaveHighFreq = u_warping_windWaveCounterHighFreq;
			float strengthFactorY = 1;
			
			int windwaveConfig = 0;
			
			switch (windMode) {
				case 1: // Weak Wind
					strength = 0.005 + 0.015 * u_warping_windSpeed;
					heightBend = (fract(y) + windData) / 7.0 * 1.3;
					break;
				case 2: // Normal wind
					strength = 0.005 + 0.015 * u_warping_windSpeed;
					heightBend = (fract(y) + windData) / 4 * 1.3;
					break;
				case 3: // Leaves
					strength *= 0.5;
					heightBend = (fract(y) + windData) / 12.0 * 1.3;
					heightBend = heightBend / 2 + pow(heightBend, 1.5) / 2; // the pow makes the bend neatly rounded
					break;
				case 4: // Bend (for small stems)
					strength = 0;
					heightBend = (fract(y) + windData) / 7.0 * 1.3;
					break;
				case 5: // Tall Bend (for thick and/or tall stems)
					strength = 0;
					heightBend = (fract(y) + windData) / 14.0 * 1.3;
					heightBend = heightBend / 2 + pow(heightBend, 1.5) / 2; // the pow makes the bend neatly rounded
					vbendMul = 0.0;
					break;
				// case 6: Water
				case 7: // Extra Weak Wind
					strength = 0.01;
					heightBend = (fract(y) + windData) / 7.0 * 0.6;
					break;
				case 8: // Fruit
					strength *= 0.15;
					if (windData == 0) windData = -1;   // Slight fudge for very tall fruit such as pears
					y += (windData + 4) / 32.0;    // All vertices on the whole fruit should have the same y - or close to it - if windData was set correctly
					strengthFactorY = 3;
					break;
				case 9: // Weak Wind No Bend (for foliage with non bending stems)
					strength *= 0.2;
					heightBend = 0;
					break;
				case 10: // Weak Wind, Inverse Bend (for vines)
					strength *= 0.5;
					//strength = 0.02; // Not sure actually why this looks better and seems to scale just fine with the windspeed
					heightBend = ((1 - fract(y)) + windData) / 14.0 * 1.5;
					break;
			}
			
			
			// 1. Determine bend
			float bend = u_warping_windSpeed * heightBend * u_warping_windWaveIntensity;
			if (bend != 0)
			{
				float bendNoise = u_warping_windSpeed * 0.2 + 1.4 * noise_gnoise(vec3(mod(x, 4096.0) / 10, mod(z, 4096.0) / 10, mod(bendCounter, 1024.0) / 4));
				bend *= (0.8 + bendNoise);
				bend = min(4, bend);
			}
			
			// 2. Add more noise
			
			x += wwaveHighFreq;
			y += wwaveHighFreq;
			z += wwaveHighFreq;
			
			// 3. Generate wiggle from a set of curves
			// Visualized: http://fooplot.com/#W3sidHlwZSI6MCwiZXEiOiIyKnNpbih4LzgpK3Npbih4LzIpK3NpbigwLjUrMip4KStzaW4oMSszKngpIiwiY29sb3IiOiIjMDAwMDAwIn0seyJ0eXBlIjoxMDAwLCJ3aW5kb3ciOlsiLTI0Ljc5NTUzMjIyNjU2MjQ4NiIsIjI0Ljc5NTUzMjIyNjU2MjQ4NiIsIi0xNS4yNTg3ODkwNjI0OTk5OTEiLCIxNS4yNTg3ODkwNjI0OTk5OTEiXX1d
			msPosition.x += bend + strength * (2 * sin(x/2) + sin(x + y) + sin(0.5 + 4*x + 2*y) + sin(1 + 6*x + 3*y)/3);
			
			// This might need to be a new mode. It makes sunflower leaves nicely wiggly
			if (windMode == 1) msPosition.x += sin(x*20)*strength / 5.0 * u_warping_windSpeed;
			
			msPosition.y += -bend * vbendMul + strength * strengthFactorY * (sin(5*y)/15 + cos(10*x/strengthFactorY) / 10 + sin(3*z/strengthFactorY)/2 + cos(x/strengthFactorY*2)/2.2);
			msPosition.z += strength * (2 * sin(z/4) + sin(z + 3 * y) + sin(0.5 + 4*z + 2*y) + sin(1 + 6*z + y)/3);
			
		}
		else {
			// Water wave
			vec3 noisePos = vec3(x / 3, z / 3, u_warping_waterWaveCounter / 8 + u_warping_windWaveCounter / 6);
			msPosition.y += noise_gnoise(noisePos) / 10;
		}
	}
#endif
	
	return msPosition;
}