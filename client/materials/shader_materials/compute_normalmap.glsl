#[compute]
#version 450

// Invocations in the (x, y, z) dimension
layout(local_size_x = 4, local_size_y = 4, local_size_z = 1) in;

layout(r8, binding = 0) uniform image3D heightMap;

layout(rgba8, binding = 1) restrict uniform image3D normalMap;



// The code we want to execute in each invocation
void main() {
    ivec3 coords = ivec3(gl_GlobalInvocationID.xyz);
    ivec2 center = ivec2(coords.x, coords.y);
    vec4 pixel = imageLoad(heightMap, coords);

    float fx0 = imageLoad(heightMap, ivec3(center.x - 1, center.y, coords.z)).r;
    float fx1 = imageLoad(heightMap, ivec3(center.x + 1, center.y, coords.z)).r;
    float fy0 = imageLoad(heightMap, ivec3(center.x, center.y - 1, coords.z)).r;
    float fy1 = imageLoad(heightMap, ivec3(center.x, center.y + 1, coords.z)).r;

    float hx = fx0 - fx1;
    float hy = fy0 - fy1;

    float dx = distance(fx0, fx1);
    float dy = distance(fy0, fy1);

    vec3 n = normalize(vec3(hx/dx, hy/(dy), pixel.r * 2.0));

    imageStore(normalMap, coords, vec4(n.xyz, 1.0));
}