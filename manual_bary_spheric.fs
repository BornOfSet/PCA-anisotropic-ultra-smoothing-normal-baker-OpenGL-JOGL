//已知三角形的一点A，做BC的垂线a，a经过点A，且a交BC于点E，AE的模长为一个已知数（面积/底），求E的坐标


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

vec3 CalHeightVec(vec3 P1, vec3 P2, vec3 S){
    vec3 V =  normalize(P2-P1);
    vec3 H =  P1-S;
    float t = -(2*H.x*V.x + 2*H.y*V.y + 2*H.z*V.z)/(2*V.x*V.x + 2*V.y*V.y + 2*V.z*V.z);
    return P1+V*t;
}

vec3 CalRest(vec3 v1, vec3 v2, vec3 v3){
    float ip32 = dot(normalize(interpolatepos - v3), normalize(v2 - v3));
    float ip31 = dot(normalize(interpolatepos - v3), normalize(v1 - v3));
    vec3 avg32 = (Nv2 + Nv3) / 2;
    vec3 avg31 = (Nv1 + Nv3) / 2;
    ip32 = acos(ip32);
    ip31 = acos(ip31);
    vec3 rest3 = ip31/(ip31 + ip32) * avg32 + ip32/(ip31 + ip32) * avg31;
    return rest3;
}

void main(){
    
    vec3 foot3 = CalHeightVec(MBv2,MBv1,MBv3);
    vec3 foot2 = CalHeightVec(MBv1,MBv3,MBv2);
    vec3 foot1 = CalHeightVec(MBv3,MBv2,MBv1);
    
    //height vector
    //hv.length = S/opposite.length
    vec3 hv3 = foot3-MBv3;
    vec3 hv2 = foot2-MBv2;
    vec3 hv1 = foot1-MBv1;

    //test
    //如果想要cross算面积，你应该输入p1-p2,p1-p3，而不能直接输入p1,p2
    //float area3_12 = length(hv3) * length(MBv1-MBv2);
    //float area2_31 = length(hv2) * length(MBv1-MBv3);
    //float area1_23 = length(hv1) * length(MBv2-MBv3);

    vec3 project3 = interpolatepos-MBv3;
    vec3 project2 = interpolatepos-MBv2;
    vec3 project1 = interpolatepos-MBv1;

    //test
    //float sum = length(project1)+length(project2)+length(project3);
    //vec3 length_test = (length(project1)/sum)*MBv1 + (length(project3)/sum)*MBv3 + (length(project2)/sum)*MBv2;


    float cos3 = dot(normalize(hv3),normalize(project3));
    float cos2 = dot(normalize(hv2),normalize(project2));
    float cos1 = dot(normalize(hv1),normalize(project1));

    float cosproject3 = length(project3)*cos3;
    float cosproject2 = length(project2)*cos2;
    float cosproject1 = length(project1)*cos1;

    /*//Square
    float area = length(cross(MBv1-MBv2,MBv1-MBv3));
    float edge12 = length(MBv1-MBv2);
    float edge13 = length(MBv1-MBv3);
    float edge23 = length(MBv2-MBv3);
    float height3 = area / edge12;
    float height1 = area / edge23;
    float height2 = area / edge13;
    */

    float ratio3 = 1 - cosproject3 / length(hv3);
    float ratio2 = 1 - cosproject2 / length(hv2);
    float ratio1 = 1 - cosproject1 / length(hv1);

    vec3 test = ratio1*MBv1 + ratio2*MBv2 + ratio3*MBv3;
    vec3 normal_out = ratio1*Nv1 + ratio2*Nv2 + ratio3*Nv3;

    //rings_output
    //test *= 10;
    //color = vec4(pow(test.x,10),pow(test.y,10),pow(test.z,10),1);

    //color = vec4(normal_out,1);
    
    //以上属于重写一遍重心插值....
    //你想要的圆形效果如下
    float arg1 = 1 - min(length(project1),length(hv1)) / length(hv1);
    float arg2 = 1 - min(length(project2),length(hv2)) / length(hv2);
    float arg3 = 1 - min(length(project3),length(hv3)) / length(hv3);


    vec3 lerp3 = arg3 * Nv3 + (1-arg3) * CalRest(MBv1, MBv2, MBv3);
    vec3 lerp2 = arg2 * Nv2 + (1-arg2) * CalRest(MBv1, MBv3, MBv2);
    vec3 lerp1 = arg1 * Nv1 + (1-arg1) * CalRest(MBv2, MBv3, MBv1);
    
    vec3 sphere = lerp1 + lerp2 + lerp3;
    //sphere *= 5;
    //color = vec4(pow(sphere.x,10),pow(sphere.y,10),pow(sphere.z,10),1);
    color = vec4(normalize(sphere),1);

}