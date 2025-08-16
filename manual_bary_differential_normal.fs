//微分法线
//我难以在平面上绘制曲面
//但我可以让曲面有曲面的造型，平面有平面的造型，彻底不用三角形插值，而解决对角线问题
//但是我们通常会遇到问题，也就是在平面上表达近似的曲面



#version 450 core
in flat vec3 MBv1;
in flat vec3 MBv2;
in flat vec3 MBv3;
in vec3 interpolatepos; 
in flat vec3 Nv1;
in flat vec3 Nv2;
in flat vec3 Nv3;
in vec3 interpolatenml; 

uniform mat4 view;

layout (location=0) out vec4 color;

void main(){

    float detaillevel = 0.01;
    float p12 = length(cross(MBv1 - interpolatepos, MBv2 - interpolatepos));
    float p23 = length(cross(MBv2 - interpolatepos, MBv3 - interpolatepos));
    float p13 = length(cross(MBv1 - interpolatepos, MBv3 - interpolatepos));
    float coe3 = p12 / (p12 + p23 + p13);
    float coe1 = p23 / (p12 + p23 + p13);
    float coe2 = p13 / (p12 + p23 + p13);
    vec3 currentPos = coe3 * MBv3 + coe2 * MBv2 + coe1 * MBv1;
    //我们需要保证这个点坐落在模型上
    vec3 deltapos_A = (coe3 + detaillevel) * MBv3 + (coe2 - detaillevel) * MBv2 + coe1 * MBv1;
    vec3 deltapos_B = coe3 * MBv3 + (coe2 + detaillevel) * MBv2 + (coe1 - detaillevel) * MBv1;
    
    vec3 differential = normalize(cross(deltapos_A - currentPos, currentPos - deltapos_B));
    vec3 worldnormal = (transpose(view) * vec4(differential,1)).xyz;
    vec3 light = vec3(0,0,-1);
    float fixedintensity = dot(light, worldnormal);
    float viewintensity = dot(light, differential);
    color = vec4(viewintensity);
}