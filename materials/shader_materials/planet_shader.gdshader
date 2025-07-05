shader_type spatial;

uniform sampler3D noise;
uniform float height_scale = 0.5;
uniform sampler3D normalmap;
varying vec3 tex_position;
uniform sampler2D heightMapTest;

void vertex() {
	// Called for every vertex the material is visible on.

	tex_position = VERTEX.xyz/3.5;
	float h = texture(heightMapTest, UV).x * height_scale;
	float height = texture(heightMapTest, VERTEX.xz/2.5).x * height_scale;
	//VERTEX += height.x * height_scale * NORMAL * 50.0;
	VERTEX += height * 100.0 * NORMAL;
	COLOR.xyz = vec3(0,1,0);
	//NORMAL = normalize((MODELVIEW_MATRIX * vec4(NORMAL, 0.0)).xyz);
}

void fragment() {
	// Called for every pixel the material is visible on.
	// NORMAL_MAP = texture(normalmap, tex_position).xyz;
}

//void light() {
//	// Called for every pixel for every light affecting the material.
//	// Uncomment to replace the default light processing function with this one.
//}
