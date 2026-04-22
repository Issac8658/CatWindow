shader_type canvas_item;
render_mode blend_add;

uniform vec2 chromatic_distance;

void fragment() {
	vec4 col1 = texture(TEXTURE, UV + chromatic_distance * TEXTURE_PIXEL_SIZE);
	COLOR.r = col1.r;
	COLOR.a = clamp(COLOR.a + col1.a, 0.0, 1.0);
}

//void light() {
//	// Called for every pixel for every light affecting the CanvasItem.
//	// Uncomment to replace the default light processing function with this one.
//}
