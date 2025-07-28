#[compute]
#version 450

// Invocations in the (x, y, z) dimension
layout(local_size_x = 8, local_size_y = 8, local_size_z = 8) in;

// `readonly` is used to tell the compiler that we will not write to this memory.
// This allows the compiler to make some optimizations it couldn't otherwise.
layout(rgba8, binding = 0) restrict readonly uniform image2D gradient;

// Prepare memory for the image, which will be both read and written to
// `restrict` is used to tell the compiler that the memory will only be accessed
// by the `noise1` variable.
layout(r8, binding = 1) restrict uniform image3D noise1;
layout(r8, binding = 2) restrict uniform image3D noise2;
layout(r8, binding = 3) restrict uniform image3D noise3;
// layout(r8, binding = 4) restrict uniform image3D moisture;
layout(r8, binding = 4) restrict uniform image3D heightMap;



// The code we want to execute in each invocation
void main() {
    ivec3 coords = ivec3(gl_GlobalInvocationID.xyz);
    ivec3 dim = imageSize(heightMap);
    // gl_GlobalInvocationID.x uniquely identifies this invocation across all work groups
    // data_buffer.data[gl_GlobalInvocationID.x] *= 2.0;

    // ivec3 center = dim / 2;
    // int min_xy = min (center.x, center.y);
    // int smallest_radius = min(min_xy, center.z);

    // float d = distance(coords, center);
    // int gradient_max_x = imageSize(gradient).x - 1;

    // int gradient_x = int(mix(0.0, float(gradient_max_x), d / float(smallest_radius)));
    // ivec2 gradient_pos = ivec2(gradient_x, 0);
    // vec4 gradient_color = imageLoad(gradient, gradient_pos);

    // Even though the image format only has the red channel,
	// this will still return a vec4: `vec4(red, 0.0, 0.0, 1.0)`
    // vec4 pixel1 = imageLoad(noise1, coords);
    float pixel1 = imageLoad(noise1, coords).x;
    float pixel2 = imageLoad(noise2, coords).x;
    float pixel3 = imageLoad(noise3, coords).x;
    vec4 hPixel = imageLoad(heightMap, coords);
    float n = pixel1 * 1.0 + pixel2 * 0.3 + pixel3 * 0.1;
    n /= 1.0 + 0.3 + 0.1;
    n = pow(n * 1.2, 2.0);
    hPixel.r = n;

	imageStore(heightMap, coords, hPixel);
}