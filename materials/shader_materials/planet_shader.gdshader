shader_type spatial;

varying vec3 tex_pos;
uniform uint heightMapDim;
uniform sampler3D heightMap;
uniform sampler3D moisture;
uniform sampler3D normalMap;
uniform float waterLevel = 0.32;
uniform float mountainLevel = 0.7;

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
    if (height < waterLevel)
        return Desert;

    if (height > mountainLevel)
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
	// Called for every vertex the material is visible on.
	tex_pos = VERTEX.xyz/3.0 + 0.5;	
	//float x0 = texture(heightMap, tex_pos + vec3(-1, 0, 0)).r; // Left
    //float x1 = texture(heightMap, tex_pos + vec3(1, 0, 0)).r; // Right
    //float hx = abs(x1 - x0); // Height difference
    //
    //vec3 x0w = (tex_pos + vec3(-1, 0, 0)) * 3.0 - 0.5; // x0 in world coordinates
    //vec3 x1w = (tex_pos + vec3(1, 0, 0)) * 3.0 - 0.5; // x1 in world coordinates
	//float dx = distance(x0w, x1w);
	//
	//float slope = hx/dx;
	//COLOR = vec4(vec3(hx), 1.0);
	NORMAL = texture(normalMap, tex_pos).xyz * VERTEX.xyz;
}

void fragment() {
	// Called for every pixel the material is visible on.
	// NORMAL_MAP = texture(heightMap, tex_pos).xyz;
	// NORMAL_MAP = texture(normalMap, tex_pos).xyz;
	float n = texture(heightMap, tex_pos).x;
	float m = texture(moisture, tex_pos).x;	
	// vec4 vc = normalize(vec4(size.zxy, z0 - z1));	
	ALBEDO = GetColor(n, m).xyz;
	// ALBEDO = COLOR.xyz;
}



//void light() {
//	// Called for every pixel for every light affecting the material.
//	// Uncomment to replace the default light processing function with this one.
//}
