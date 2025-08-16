//plays the same role as grid_simple_lambert does
#version 330 core
in vec3 AtVertex;
layout (location=0) out vec4 color;
void main(){

    float dark = 1-(AtVertex.z+1)/2;
    dark = pow(dark, 4) * 10;
    color = vec4(1*dark,0,0,1);

}