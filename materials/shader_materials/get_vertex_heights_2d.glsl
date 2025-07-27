#[compute]
#version 450
#define PI 3.14159;

// Invocations in the (x, y, z) dimension
layout(local_size_x = 32, local_size_y = 1, local_size_z = 1) in;

layout(binding = 0) uniform sampler2D heightMap;

layout(set = 0, binding = 1, std430) restrict buffer HeightsBuffer{
    float data[];
} heights;

layout(set = 0, binding = 2, std430) restrict buffer VertexBuffer {
    vec4 data[];
} vertices;


void main()
{
    vec2 uv = vec2(gl_GlobalInvocationID.xy);
    vec3 v = vertices.data[gl_GlobalInvocationID.x].xyz / 3.0 + 0.5;
    v = normalize(v);
    uv.x = 0.5 + atan(v.z, v.x) / 3.14159 * 2.0;
    uv.y = 0.5 + v.y * 0.5;
    float idx = vertices.data[gl_GlobalInvocationID.x].w;
    float h = texture(heightMap, uv).x;

    heights.data[int(idx)] = h;
}