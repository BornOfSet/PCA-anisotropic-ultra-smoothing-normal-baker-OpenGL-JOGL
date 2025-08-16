#version 450 core



out flat int theTri_toRen;


void main()
{
    theTri_toRen = gl_VertexID;
    gl_Position = vec4(0,0,0,0);


}