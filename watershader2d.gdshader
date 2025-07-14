shader_type canvas_item;

uniform float intensity = 0.01; // Subtle ripple strength
uniform float wave_scale = 10.0;

uniform sampler2D screen_texture : hint_screen_texture;

void fragment() {
    vec2 uv = SCREEN_UV;

    // Gentle wave distortion
    vec2 wave = vec2(
        sin(uv.y * wave_scale + TIME * 0.5),
        cos(uv.x * wave_scale + TIME * 0.5)
    ) * intensity;

    vec2 base_uv = uv + wave;

    COLOR = texture(screen_texture, base_uv);
}