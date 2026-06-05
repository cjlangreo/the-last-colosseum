shader_type canvas_item;

uniform vec3 color : source_color = vec3(1.0); //Shine color
uniform bool on = false;

void fragment()
{
	float cycle_speed = 20.0;
	if (on){
		float cycle = sin(TIME * cycle_speed);
		COLOR.rgb = mix(COLOR.rgb, color.rgb, ((cycle >= 0.0) ? abs(cycle) : 0.0) * 0.5f);
	}
}