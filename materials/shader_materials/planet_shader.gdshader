shader_type spatial;

varying vec3 tex_pos;
uniform sampler3D noise1;
uniform sampler3D noise2;
uniform sampler3D noise3;
uniform sampler3D moisture;

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
	if (height < 0.1)
        return DeepWater;
    if (height < 0.3)
        return Water;
    if (height < 0.33)
        return Savannah;

    if (height > 0.8)
    {
        if (m < 0.1) return Savannah;
        if (m < 0.2) return MountainSide;
        return m < 0.5 ? Tundra : Snow;
    }

    if (height > 0.6)
    {
        if (m < 0.33) return Desert;
        return m < 0.66 ? Savannah : Forest;
    }

    if (height > 0.3)
    {
        if (m < 0.2) return Desert;
        if (m < 0.5) return Grassland;
        if (m < 0.83) return Jungle;
    }

    if (m < 0.16) return Grassland;
    return m < 0.33 ? Grassland : Jungle;

}

void vertex() {
	// Called for every vertex the material is visible on.	
	// tex_pos = VERTEX.xyz/64.0f;
}

void fragment() {
	//// Called for every pixel the material is visible on.
	//// NORMAL_MAP = texture(normalmap, tex_position).xyz;
	//float n1 = texture(noise1, tex_pos).x; 
	//float n2 = texture(noise2, tex_pos * 2.0f).x;
	//float n3 = texture(noise3, tex_pos * 4.0f).x;
	//
	//float n = n1 * 1.0f + n2 * 0.33f + n3 * 0.1f;
	//n /= 1.0f + 0.33f + 0.1f;	
	//float m = texture(moisture, tex_pos).x;
	//ALBEDO = GetColor(n, m).xyz;
	ALBEDO = COLOR.xyz;
}



//void light() {
//	// Called for every pixel for every light affecting the material.
//	// Uncomment to replace the default light processing function with this one.
//}
