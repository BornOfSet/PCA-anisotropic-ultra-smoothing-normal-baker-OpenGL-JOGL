//根据面积的线框
//来源于https://www.shadertoy.com/view/l3jcR1
//我们本质上要构造的是triangle_fan
//0索引指的是空间中任意一点，在我们的情况下，它是absoluteworldpos，该值等同于包括它的三角形的三个顶点的重心插值
//在场景中有3个独一无二的点，这是我们最后要插值的点
//在这3个点之外，应该添加两个点，使
//pa可以访问△pac △pab
//pb可以访问△pab △pbc
//pc可以访问△pac △pbc
//而在默认情况下，无法在0之前访问△pac，无法在2之后访问△pac

#version 450 core
in flat vec3 priver1;
in flat vec3 priver2;
in flat vec3 priver3;
in vec3 absoluteworldpos; 

uniform mat4 view;
uniform mat4 scale;
uniform mat4 move;

layout (location=0) out vec4 color;

in flat vec3 prino1;
in flat vec3 prino2;
in flat vec3 prino3;

vec3 mainfunction(){

    vec3 scattered[5] = vec3[5](
        priver3,
        priver1,
        priver2,
        priver3,
        priver1
    );    
    
    vec3 attribu[5] = vec3[5](
        prino3,
        prino1,
        prino2,
        prino3,
        prino1
    );    

    vec3 ourpoint = absoluteworldpos;
    vec3 si[5];
    float ri[5];
    for(int i=0;i<5;i++){
        si[i] = scattered[i] - ourpoint;
        ri[i] = length(si[i]);
    }

    float area[4];
    float di[4];

    for(int i=0;i<4;i++){
        area[i] = length(cross(si[i],si[i+1]))/2.0f;
        di[i] = dot(si[i],si[i+1]);
        if(ri[i]<=0.0015f){
            return attribu[i];
        }
        if(area[i]<=0.0003f){
            return (ri[i+1]*attribu[i]+ri[i]*attribu[i+1]) / (ri[i+1] + ri[i]);
        }
    }
                
    vec3 F = vec3(0);
    float W = 0.0f;
    
    for(int i=1;i<=3;i++){
        float w = 0.0f;
        if(area[i-1]!=0.0f){
            w = w + (ri[i-1]-di[i-1]/ri[i])/area[i-1];
        }
        if(area[i]!=0.0f){
            w = w + (ri[i+1]-di[i]/ri[i])/area[i];
        }
        F = F + w * attribu[i];
        W = W + w;
    }
    return F/W;

}



void main(){

    vec3 receive = mainfunction();
    // Output to screen
    color = vec4(receive,1.0f);

}