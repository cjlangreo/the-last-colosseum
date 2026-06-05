shader_type canvas_item;

uniform float percent : hint_range(0, 1.0) = 0.5;
uniform float darkness : hint_range(0, 1.0) = 0.3;
uniform bool ability_active = false;

vec3 rgb2hsv(vec3 rgb) {
	float r = rgb.r;
	float g = rgb.g;
	float b = rgb.b;
	
	float max_val = max(r, max(g, b));
	float min_val = min(r, min(g, b));
	float delta = max_val - min_val;
	
	float h = 0.0;
	float s = 0.0;
	float v = max_val;
	
	if (delta > 0.0001) {
		s = delta / max_val;
		
		if (abs(r - max_val) < 0.0001)      h = (g - b) / delta;
		else if (abs(g - max_val) < 0.0001) h = 2.0 + (b - r) / delta;
		else                                h = 4.0 + (r - g) / delta;
		
		h *= 60.0;
		if (h < 0.0) h += 360.0;
	}
	
	return vec3(h, s, v);
}

vec3 hsv2rgb(vec3 hsv) {
	float h = hsv.x;
	float s = hsv.y;
	float v = hsv.z;
	
	float c = v * s;
	float x = c * (1.0 - abs(mod(h / 60.0, 2.0) - 1.0));
	float m = v - c;
	
	vec3 rgb;
	
	if (h < 60.0)       rgb = vec3(c, x, 0.0);
	else if (h < 120.0) rgb = vec3(x, c, 0.0);
	else if (h < 180.0) rgb = vec3(0.0, c, x);
	else if (h < 240.0) rgb = vec3(0.0, x, c);
	else if (h < 300.0) rgb = vec3(x, 0.0, c);
	else                rgb = vec3(c, 0.0, x);
	
	rgb += m;
	return rgb;
}

void fragment() {
	vec3 hsv_color = rgb2hsv(COLOR.rgb);
	hsv_color.x = mod(hsv_color.x + fract(TIME) * 360.0, 360.0);
  COLOR.rgb = ability_active ? hsv2rgb(hsv_color) : COLOR.rgb;
  
	COLOR.rgb = mix(COLOR.rgb, COLOR.rgb * darkness,step(percent, 1.0-UV.y));
  
}
