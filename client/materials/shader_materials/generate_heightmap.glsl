#[compute]
#version 450

// Invocations in the (x, y, z) dimension
layout(local_size_x = 32, local_size_y = 1, local_size_z = 1) in;
layout(r8, binding = 0) restrict uniform image2D heightMap;
layout(set = 0, binding = 1, std430) restrict buffer HeightsBuffer{
    float data[];
} heights;
layout(set = 0, binding = 2, std430) restrict buffer VIdxBuffer{
    int data[];
} vIdx;

// The code we want to execute in each invocation
void main() {
    ivec2 coords = ivec2(gl_GlobalInvocationID.xy);
    vec4 pixel = vec4(0.0);
    float h = heights.data[vIdx.data[gl_GlobalInvocationID.x]];
    pixel.r = h;
    imageStore(heightMap, coords, pixel);
}