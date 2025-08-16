#version 450 core
layout(points) in;
layout(points, max_vertices = 25) out;
uniform mat4 view;
uniform mat4 scale;
uniform mat4 move;


in flat int theTri_toRen[];

struct VertexStruct{
    float pos[3];
    float normal[3];
};


layout(std430, binding=0 ) readonly buffer VertexBlock{
    VertexStruct vertices[];
};


struct TriangleStruct{
    int V1;
    int V2;
    int V3;
};

//gl_VertexID 指的是 面索引
//所以需要构造 到点
layout(std430, binding=1 ) readonly buffer TriangleBlock{
    TriangleStruct triangles[];
};




struct adjLocator{
    int start;
    int end;
};

layout(std430, binding=8 ) readonly buffer SparsePointCloud{
    int pointcloud[];
};


layout(std430, binding=9 ) readonly buffer FindPointsByTriangle{
    adjLocator adj[];
};


uniform int rand;

void main() {

    int tri = theTri_toRen[0];
    TriangleStruct tri_a = triangles[tri];
    VertexStruct pA = vertices[tri_a.V1];
    VertexStruct pB = vertices[tri_a.V2];
    VertexStruct pC = vertices[tri_a.V3];

    vec3 A = vec3(pA.pos[0],pA.pos[1],pA.pos[2]);
    vec3 B = vec3(pB.pos[0],pB.pos[1],pB.pos[2]);
    vec3 C = vec3(pC.pos[0],pC.pos[1],pC.pos[2]);

    if(rand==tri){

        //works fine ，面图元=3
        gl_Position = move * scale * view * vec4(A,1);    EmitVertex();

        gl_Position = move * scale * view * vec4(B,1);    EmitVertex();

        gl_Position = move * scale * view * vec4(C,1);    EmitVertex();



        adjLocator adjtri = adj[tri];
        for(int i = adjtri.start; i< adjtri.end; i++){
            VertexStruct pA = vertices[pointcloud[i]];
            vec3 A = vec3(pA.pos[0],pA.pos[1],pA.pos[2]);
            gl_Position = move * scale * view * vec4(A,1);EmitVertex();
        }

        EndPrimitive();
    }
}