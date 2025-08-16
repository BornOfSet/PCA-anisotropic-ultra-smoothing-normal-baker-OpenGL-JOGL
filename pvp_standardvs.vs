//pvp_standardvs
//这个文件之所以是标准文件，是因为它只是简单地获取法线和顶点信息，僵硬地传给下一个阶段，通用性很高

#version 450 core


struct VertexData {
    float position[3];
    float normal[3];
};

layout(binding = 0, std430) readonly buffer ssbo {
    VertexData data[];
};

vec3 getPosition(int index) {
    return vec3(
        data[index].position[0], 
        data[index].position[1], 
        data[index].position[2]
    );
}

vec3 getNormal(int index) {
    return vec3(
        data[index].normal[0], 
        data[index].normal[1], 
        data[index].normal[2]
    );
}

out vec4 onormal;

void main()
{
    gl_Position = vec4(getPosition(gl_VertexID), 1);
    onormal = vec4(getNormal(gl_VertexID) , gl_VertexID);
}