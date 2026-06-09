shader_type canvas_item;

uniform float percent : hint_range(0, 1.0) = 0.5;
uniform float darkness : hint_range(0, 1.0) = 0.3;


void fragment() {
	COLOR.rgb = mix(COLOR.rgb, COLOR.rgb * darkness,step(percent, 1.0-UV.y));
}
