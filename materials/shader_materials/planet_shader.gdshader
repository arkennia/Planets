shader_type spatial;

varying vec3 tex_pos;

void vertex() {
	// Called for every vertex the material is visible on.
	tex_pos = VERTEX.xyz;
}

void fragment() {
	// Called for every pixel the material is visible on.
	// NORMAL_MAP = texture(normalmap, tex_position).xyz;
	ALBEDO = COLOR.xyz;
}

//void light() {
//	// Called for every pixel for every light affecting the material.
//	// Uncomment to replace the default light processing function with this one.
//}
