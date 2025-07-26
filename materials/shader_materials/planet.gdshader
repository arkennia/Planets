shader_type spatial;
// Varyings
varying vec3 var_tex_coord;
varying float var_height;

void vertex() {
// Input:4
	var_tex_coord = VERTEX.xyz / 3.0 + 0.5;
}

void fragment() {
// VaryingGetter:14
// Output:0
	ALBEDO.xyz = COLOR.rgb;
}
