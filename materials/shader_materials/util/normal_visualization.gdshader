shader_type spatial;
render_mode unshaded;

uniform bool WorldSpace = true; // toggle between world and local space
uniform bool RemapRange = false; // remap from -1..1 to 0..1

varying vec3 normal;

void vertex() {
	normal = NORMAL;
}

void fragment()
{
    // World space normal
    vec4 Wn = vec4( normal, 0.0 );
    //if ( WorldSpace == false ) // Local space normal
    //{
        //Wn = inverse( MODEL_MATRIX ) * Wn;
    //}
    if ( RemapRange == true )
    {
        ALBEDO = ( Wn.rgb + 1.0 )/2.0;
    }
    else
    {
        ALBEDO = Wn.rgb;
    }
}