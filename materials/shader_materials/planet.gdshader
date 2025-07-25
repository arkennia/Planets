shader_type spatial;
render_mode blend_mix, depth_draw_opaque, depth_test_default, cull_disabled, diffuse_lambert, specular_schlick_ggx, unshaded, ensure_correct_normals;


// Varyings
varying vec3 var_tex_coord;
varying float var_height;

uniform sampler3D Moisture : filter_nearest, repeat_enable;



const int MAX_VERTEX_COUNT = 20000;
uniform float Heights[MAX_VERTEX_COUNT];
uniform vec4 DeepWater : source_color = vec4(0.026, 0.022, 0.512, 1.0f);
uniform vec4 Water : source_color = vec4(0.123, 0.547, 0.957, 1.0f);
uniform vec4 Forest : source_color = vec4(0.037, 0.313, 0.047, 1.0f);
uniform vec4 Grassland : source_color = vec4(0.39, 0.824, 0.408, 1.0f);
uniform vec4 Jungle : source_color = vec4(0.063, 0.637, 0.045, 1.0f);
uniform vec4 Savannah : source_color = vec4(0.738, 0.672, 0.412,1.0f);
uniform vec4 Desert : source_color = vec4(0.895, 0.802, 0.625, 1.0f);
uniform vec4 MountainSide : source_color = vec4(0.488, 0.488, 0.488, 1.0f);
uniform vec4 Snow : source_color = vec4(1.0f);
uniform vec4 Tundra : source_color = vec4(0.56, 0.56, 0.56, 1.0f);


vec4 GetColor(float height, float m) {
	if (height < 0.06)
	    return DeepWater;
	if (height < 0.2)
	    return Water;
	if (height < 0.2 + 0.07)
	    return Desert;

	if (height > 0.7)
	{
	    if (m < 0.1) return Savannah;
	    if (m < 0.2) return MountainSide;
	    return m < 0.6 ? Tundra : Snow;
	}

	if (height > 0.6)
	{
	    if (m < 0.33) return Desert;
	    return m < 0.66 ? Grassland : Forest;
	}

	if (height > 0.4)
	{
	    if (m < 0.2) return Desert;
	    if (m < 0.4) return Grassland;
	    if (m < 0.83) return Jungle;
	}

	if (m < 0.16) return Savannah;
	return m < 0.33 ? Grassland : Jungle;

}


void vertex() {
// Input:4
	vec3 n_out4p0 = VERTEX;
// VaryingSetter:3
	var_tex_coord = n_out4p0;
// Input:6
	int n_out6p0 = VERTEX_ID;
	float n_out7p0;
// Expression:7
	n_out7p0 = 0.0;
	n_out7p0 = Heights[n_out6p0];
// VaryingSetter:8
	var_height = n_out7p0;
}

void fragment() {
// VaryingGetter:14
	float n_out14p0 = var_height;


// VaryingGetter:4
	vec3 n_out4p0 = var_tex_coord/3.0 + 0.5;


// MultiplyAdd:12
	vec3 n_in12p1 = vec3(3.00000, 3.00000, 3.00000);
	vec3 n_in12p2 = vec3(0.50000, 0.50000, 0.50000);
	//vec3 n_out12p0 = fma(n_out4p0, n_in12p1, n_in12p2);


	float n_out10p0;
// Texture3D:10
	n_out10p0 = texture(Moisture, n_out4p0).x;



	vec3 n_out17p0;	
	n_out17p0 = GetColor(var_height,n_out10p0).xyz;

// Output:0
	ALBEDO.xyz = n_out17p0.xyz;
}
