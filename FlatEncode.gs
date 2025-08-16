#version 430

layout(triangles) in;
layout(triangle_strip, max_vertices = 3) out;


in vec3 Direc[];
in vec3 Orig[];

out vec3 DirecPS;
out vec3 OrigPS;
out vec3 Tangent;
out vec3 flatNormal;//注意，平坦切线和平滑法线不垂直





void main(){

    //planar coordinate vector 1
    vec3 pcv1 = Orig[1] - Orig[0];
    vec3 pcv2 = Orig[2] - Orig[0];

    //orthogonal delta  向量本身就是正交的，因为只有正交的向量，y里面的组件不会贡献到x中去。一切非正交的向量都必须归到正交向量上才能运算
    vec2 delta1 = gl_in[1].gl_Position.xy - gl_in[0].gl_Position.xy;
    vec2 delta2 = gl_in[2].gl_Position.xy - gl_in[0].gl_Position.xy;

    //加减法自动消掉位移
    delta1 = 0.5 * delta1;
    delta2 = 0.5 * delta2;



    float det = (delta1.x * delta2.y - delta1.y * delta2.x);
    float invDet = 1.0f / det;

    vec3 tang = invDet * (delta2.y * pcv1 - delta1.y * pcv2);

    vec3 flatn = cross(pcv1, pcv2);

	gl_Position = gl_in[0].gl_Position; DirecPS = Direc[0]; OrigPS = Orig[0];  Tangent = tang; flatNormal = flatn;
	EmitVertex();
	gl_Position = gl_in[1].gl_Position; DirecPS = Direc[1]; OrigPS = Orig[1];  Tangent = tang; flatNormal = flatn;
	EmitVertex();
	gl_Position = gl_in[2].gl_Position; DirecPS = Direc[2]; OrigPS = Orig[2];  Tangent = tang; flatNormal = flatn;
	EmitVertex();
	EndPrimitive();
	
}