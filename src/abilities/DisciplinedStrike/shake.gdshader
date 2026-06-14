shader_type canvas_item;

uniform bool enabled = false;
uniform float speed = 30;
uniform float amplitude = 1.0;


float get_offset(){
	return sin(TIME * speed) * amplitude;
}

void vertex() {
	VERTEX.x += get_offset() * float(enabled);
}