shader_type canvas_item;
render_mode unshaded;

uniform sampler2D SCREEN_TEXTURE : hint_screen_texture;

uniform float[1024] current_values;

void fragment() {
		COLOR = texture(SCREEN_TEXTURE, SCREEN_UV + vec2(SCREEN_PIXEL_SIZE.x, 0));
	if (UV.x > 1.0 - TEXTURE_PIXEL_SIZE.x * 4.0) {
		COLOR.rgb = vec3(current_values[int((1.0 - UV.y) * 1024.0)]);
		COLOR.w = 1.0;
	}
}