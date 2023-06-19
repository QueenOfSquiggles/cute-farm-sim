shader_type spatial;
render_mode cull_disabled;

#include "res://Assets/Materials/ShaderIncludes/grass_shader_help.gdshaderinc"

group_uniforms colouration;
	uniform vec4 grass_colour_base : source_color = vec4(0.0, 0.0, 0.0, 1.0);
	uniform vec4 grass_colour_tip : source_color = vec4(0.0, 1.0, 0.0, 1.0);
group_uniforms wind;
	uniform float grass_sway_pitch = 1.0;
	uniform float grass_sway_yaw = 1.0;
	uniform sampler2D wind_noise : hint_default_white;
	uniform float wind_noise_scale = 0.5;
	uniform float wind_scroll = 0.1;
group_uniforms PBR;
	uniform float min_roughness = 0.3;
	uniform float max_roughness = 1.0;
group_uniforms PlayerAffect;
	uniform float player_radius = 0.5;
	uniform float player_affect = 0.2;
	
global uniform vec3 weather_wind_velocity;
global uniform vec3 player_position;

const vec3 UP = vec3(0.0, 1.0, 0.0);
const float SMALLINATOR = 0.001;

varying float wind_strength;
void vertex() {
	NORMAL = UP;
	COLOR = mix(grass_colour_base, grass_colour_tip, 1.0 - UV.y);
	vec3 worldPos_static = (MODEL_MATRIX * vec4(VERTEX, 1.0)).xyz;
	vec3 worldPos = worldPos_static * wind_noise_scale;
	float time = TIME * length(weather_wind_velocity) * wind_scroll;
	worldPos += normalize(weather_wind_velocity) * time;
	vec3 wind_dir = normalize(weather_wind_velocity);
	mat3 to_model = inverse(mat3(MODEL_MATRIX));
	vec3 wind_forward = to_model * normalize(wind_dir);
	vec3 wind_right = normalize(cross(wind_forward, UP));
	wind_strength = length(wind_dir) * pow(worley2(worldPos.xz +0.1), 1.5);
	
	float sway_pitch = grass_sway_pitch * wind_strength;
	float sway_yaw = grass_sway_yaw * wind_strength * sin(time);
	
	if (distance(worldPos_static.xz, player_position.xz) < player_radius) {
		float factor = length(worldPos_static.xz - player_position.xz) / player_radius;
		//COLOR = vec4(1.0, 0.0, 0.0, 1.0);
		wind_strength = 0.0;
		vec2 dir = ((worldPos_static.xz - player_position.xz) / player_radius) * player_affect;
		sway_pitch = mix(dir.y, sway_pitch, factor);
		sway_yaw = mix(dir.x, sway_yaw, factor);
	}
	mat3 rot_right = mat3_from_axis_angle(sway_pitch, wind_right);
	mat3 rot_forward = mat3_from_axis_angle(sway_yaw, wind_forward);
	VERTEX = rot_right * rot_forward * VERTEX;
}

void fragment() {
	NORMAL = FRONT_FACING? NORMAL : -NORMAL;
	ALBEDO = COLOR.rgb;
	SPECULAR = 0.5;
	ROUGHNESS = clamp(1.0 - (wind_strength * 2.0), min_roughness, max_roughness);
	//ROUGHNESS = mix(ROUGHNESS, 1.0, smoothstep(0.0, 1.0, UV.y));
}