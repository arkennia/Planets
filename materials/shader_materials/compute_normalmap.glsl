#[compute]
#version 450

// Invocations in the (x, y, z) dimension
layout(local_size_x = 16, local_size_y = 16, local_size_z = 1) in;

layout(binding = 0) uniform sampler3D heightMap;

layout(r8, binding = 1) restrict writeonly uniform image3D normalMap;



// The code we want to execute in each invocation
void main() {
    ivec3 coords = ivec3(gl_GlobalInvocationID.xyz);
    // ivec3 dim = imageSize(heightMap);


    vec4 hPixel = texture(heightMap, coords);

    //hPixel.r = n;
	// imageStore(heightMap, coords, hPixel);
}