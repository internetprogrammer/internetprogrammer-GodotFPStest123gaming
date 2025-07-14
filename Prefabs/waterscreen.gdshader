shader_type canvas_item;

uniform float intensity = 0.02;
uniform sampler2D screen_texture : hint_screen_texture;

void fragment() {
    vec2 uv = SCREEN_UV;
    uv.y += sin(uv.x * 0.05f + TIME * 5.0) * intensity;
    uv.x += cos(uv.y * 0.05f + TIME * 5.0) * intensity;
    COLOR = texture(screen_texture, uv);
}