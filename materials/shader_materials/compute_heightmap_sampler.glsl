#[compute]
#version 450

// Invocations in the (x, y, z) dimension
layout(local_size_x = 32, local_size_y = 1, local_size_z = 1) in;

// `readonly` is used to tell the compiler that we will not write to this memory.
// This allows the compiler to make some optimizations it couldn't otherwise.
layout(rgba8, binding = 0) restrict readonly uniform image2D gradient;

// Prepare memory for the image, which will be both read and written to
// `restrict` is used to tell the compiler that the memory will only be accessed
// by the `noise1` variable.
layout( binding = 1) uniform sampler3D noise1;
layout( binding = 2) uniform sampler3D noise2;
layout( binding = 3) uniform sampler3D noise3;
// layout(r8, binding = 4) restrict uniform image3D moisture;
// layout(r8, binding = 4) restrict uniform image3D heightMap;
layout(set = 0, binding = 5, std430) restrict buffer VertexBuffer {
    vec4 data[];
} vertices;
layout(set = 0, binding = 6, std430) restrict buffer HeightsBuffer{
    float data[];
} heights;



// The code we want to execute in each invocation
void main() {
    // ivec3 coords = ivec3(gl_GlobalInvocationID.xyz);
    // ivec3 dim = imageSize(heightMap);

    vec3 v = vertices.data[gl_GlobalInvocationID.x].xyz;
    int idx = int(vertices.data[gl_GlobalInvocationID.x].w);


    float pixel1 = texture(noise1, v).x;
    float pixel2 = texture(noise2, v * 2.0).x;
    float pixel3 = texture(noise3, v * 2.0).x;
    // vec4 hPixel = imageLoad(heightMap, v/dim);
    float n = pixel1 * 1.0 + pixel2 * 0.33 + pixel3 * 0.1;
    n /= 1.0 + 0.33 + 0.1;
    n = pow(n * 1.2, 4.0);
    heights.data[int(idx)] = n;
    //hPixel.r = n;

    // hPixel.r = step(0.1, hPixel.r) * hPixel.r;
    // If the pixel is below a certain threshold, this sets it to 0.0.
	// The `step()` function is like `clamp()`, but it returns 0.0 if the value is
	// below the threshold, or 1.0 if it is above.
	//
	// This is why we multiply it by the pixel's value again: to get the original
	// value back if it is above the threshold. This shorthand replaces an `if`
	// statement, which would cause branching and thus potentially slow down the
	// shader.
    // Store the pixel back into the image.
	// WARNING: make sure you are writing to the same coordinate that you read from.
	// If you don't, you may end up writing to a pixel, before that pixel is read
	// by a different invocation and cause errors.
	// imageStore(heightMap, coords, hPixel);
}