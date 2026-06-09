shader_type canvas_item;

uniform float speed = 0.0;
uniform float amplitude = 1.0;


float get_offset(){
	return sin(TIME * speed) * amplitude;
}

void vertex() {
	//VERTEX += vec2(get_offset(), -get_offset());
	VERTEX.x += get_offset();
}
