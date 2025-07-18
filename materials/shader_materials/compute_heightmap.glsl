#[compute]
#version 450

// Invocations in the (x, y, z) dimension
layout(local_size_x = 8, local_size_y = 8, local_size_z = 8) in;

// Prepare memory for the image, which will be both read and written to
// `restrict` is used to tell the compiler that the memory will only be accessed
// by the `heightmap` variable.
layout(r8, binding = 0) restrict uniform image3D heightmap;

// `readonly` is used to tell the compiler that we will not write to this memory.
// This allows the compiler to make some optimizations it couldn't otherwise.
layout(rgba8, binding = 1) restrict readonly uniform image2D gradient;

// The code we want to execute in each invocation
void main() {
    ivec3 coords = ivec3(gl_GlobalInvocationID.xyz);
    ivec3 dim = imageSize(heightmap);
    // gl_GlobalInvocationID.x uniquely identifies this invocation across all work groups
    // data_buffer.data[gl_GlobalInvocationID.x] *= 2.0;
    ivec3 center = dim / 2;
    int min_xy = min (center.x, center.y);
    int smallest_radius = min(min_xy, center.z);

    float d = distance(coords, center);
    int gradient_max_x = imageSize(gradient).x - 1;

    int gradient_x = int(mix(0.0, float(gradient_max_x), d / float(smallest_radius)));
    ivec2 gradient_pos = ivec2(gradient_x, 0);
    vec4 gradient_color = imageLoad(gradient, gradient_pos);

    // Even though the image format only has the red channel,
	// this will still return a vec4: `vec4(red, 0.0, 0.0, 1.0)`
    vec4 pixel = imageLoad(heightmap, coords);
    // pixel.r *= gradient_color.r;

    // pixel.r = step(0.2, pixel.r) * pixel.r;
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
	imageStore(heightmap, coords, pixel);
}