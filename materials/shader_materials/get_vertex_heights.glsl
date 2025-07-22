#[compute]
#version 450

// Invocations in the (x, y, z) dimension
layout(local_size_x = 32, local_size_y = 1, local_size_z = 1) in;

layout(binding = 0) uniform sampler3D heightMap;

layout(set = 0, binding = 1, std430) restrict buffer HeightsBuffer{
    float data[];
} heights;

layout(set = 0, binding = 2, std430) restrict buffer VertexBuffer {
    vec4 data[];
} vertices;

void main()
{
    vec3 v0 = vertices.data[gl_GlobalInvocationID.x].xyz;
    float idx = vertices.data[gl_GlobalInvocationID.x].w;
    float h = texture(heightMap, v0/3.0 + 0.5).x;
    heights.data[int(idx)] = h;
}