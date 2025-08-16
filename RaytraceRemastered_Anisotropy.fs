#version 450 core




#define GAMMA 5.8284271247
#define C_STAR 0.9238795325
#define S_STAR 0.3826834323
#define SVD_EPS 0.0000001

vec2 approx_givens_quat(float s_pp, float s_pq, float s_qq) {
    float c_h = 2 * (s_pp - s_qq);
    float s_h2 = s_pq * s_pq;
    float c_h2 = c_h * c_h;
    if (GAMMA * s_h2 < c_h2) {
        float omega = 1.0f / sqrt(s_h2 + c_h2);
        return vec2(omega * c_h, omega * s_pq);
    }
    return vec2(C_STAR, S_STAR);
}

// the quaternion is stored in vec4 like so:
// (c, s * vec3) meaning that .x = c
mat3 quat_to_mat3(vec4 quat) {
    float qx2 = quat.y * quat.y;
    float qy2 = quat.z * quat.z;
    float qz2 = quat.w * quat.w;
    float qwqx = quat.x * quat.y;
    float qwqy = quat.x * quat.z;
    float qwqz = quat.x * quat.w;
    float qxqy = quat.y * quat.z;
    float qxqz = quat.y * quat.w;
    float qyqz = quat.z * quat.w;

    return mat3(1.0f - 2.0f * (qy2 + qz2), 2.0f * (qxqy + qwqz), 2.0f * (qxqz - qwqy),
        2.0f * (qxqy - qwqz), 1.0f - 2.0f * (qx2 + qz2), 2.0f * (qyqz + qwqx),
        2.0f * (qxqz + qwqy), 2.0f * (qyqz - qwqx), 1.0f - 2.0f * (qx2 + qy2));
}

mat3 symmetric_eigenanalysis(mat3 A) {
    mat3 S = transpose(A) * A;
    // jacobi iteration
    mat3 q = mat3(1.0f);
    for (int i = 0; i < 5; i++) {
        vec2 ch_sh = approx_givens_quat(S[0].x, S[0].y, S[1].y);
        vec4 ch_sh_quat = vec4(ch_sh.x, 0, 0, ch_sh.y);
        mat3 q_mat = quat_to_mat3(ch_sh_quat);
        S = transpose(q_mat) * S * q_mat;
        q = q * q_mat;

        ch_sh = approx_givens_quat(S[0].x, S[0].z, S[2].z);
        ch_sh_quat = vec4(ch_sh.x, 0, -ch_sh.y, 0);
        q_mat = quat_to_mat3(ch_sh_quat);
        S = transpose(q_mat) * S * q_mat;
        q = q * q_mat;

        ch_sh = approx_givens_quat(S[1].y, S[1].z, S[2].z);
        ch_sh_quat = vec4(ch_sh.x, ch_sh.y, 0, 0);
        q_mat = quat_to_mat3(ch_sh_quat);
        S = transpose(q_mat) * S * q_mat;
        q = q * q_mat;

    }
    return q;
}

vec2 approx_qr_givens_quat(float a0, float a1) {
    float rho = sqrt(a0 * a0 + a1 * a1);
    float s_h = a1;
    float max_rho_eps = rho;
    if (rho <= SVD_EPS) {
        s_h = 0;
        max_rho_eps = SVD_EPS;
    }
    float c_h = max_rho_eps + a0;
    if (a0 < 0) {
        float temp = c_h - 2 * a0;
        c_h = s_h;
        s_h = temp;
    }
    float omega = 1.0f / sqrt(c_h * c_h + s_h * s_h);
    return vec2(omega * c_h, omega * s_h);
}

struct QR_mats {
    mat3 Q;
    mat3 R;
};

QR_mats qr_decomp(mat3 B) {
    QR_mats qr_decomp_result;
    mat3 R;
    // 1 0
    // (ch, 0, 0, sh)
    vec2 ch_sh10 = approx_qr_givens_quat(B[0].x, B[0].y);
    mat3 Q10 = quat_to_mat3(vec4(ch_sh10.x, 0, 0, ch_sh10.y));
    R = transpose(Q10) * B;

    // 2 0
    // (ch, 0, -sh, 0)
    vec2 ch_sh20 = approx_qr_givens_quat(R[0].x, R[0].z);
    mat3 Q20 = quat_to_mat3(vec4(ch_sh20.x, 0, -ch_sh20.y, 0));
    R = transpose(Q20) * R;

    // 2 1
    // (ch, sh, 0, 0)
    vec2 ch_sh21 = approx_qr_givens_quat(R[1].y, R[1].z);
    mat3 Q21 = quat_to_mat3(vec4(ch_sh21.x, ch_sh21.y, 0, 0));
    R = transpose(Q21) * R;

    qr_decomp_result.R = R;

    qr_decomp_result.Q = Q10 * Q20 * Q21;
    return qr_decomp_result;
}

struct SVD_mats {
    mat3 U;
    mat3 Sigma;
    mat3 V;
};

SVD_mats svd(mat3 A) {
    SVD_mats svd_result;
    svd_result.V = symmetric_eigenanalysis(A);

    mat3 B = A * svd_result.V;

    // sort singular values
    float rho0 = dot(B[0], B[0]);
    float rho1 = dot(B[1], B[1]);
    float rho2 = dot(B[2], B[2]);
    if (rho0 < rho1) {
        vec3 temp = B[1];
        B[1] = -B[0];
        B[0] = temp;
        temp = svd_result.V[1];
        svd_result.V[1] = -svd_result.V[0];
        svd_result.V[0] = temp;
        float temp_rho = rho0;
        rho0 = rho1;
        rho1 = temp_rho;
    }
    if (rho0 < rho2) {
        vec3 temp = B[2];
        B[2] = -B[0];
        B[0] = temp;
        temp = svd_result.V[2];
        svd_result.V[2] = -svd_result.V[0];
        svd_result.V[0] = temp;
        rho2 = rho0;
    }
    if (rho1 < rho2) {
        vec3 temp = B[2];
        B[2] = -B[1];
        B[1] = temp;
        temp = svd_result.V[2];
        svd_result.V[2] = -svd_result.V[1];
        svd_result.V[1] = temp;
    }

    QR_mats QR = qr_decomp(B);
    svd_result.U = QR.Q;
    svd_result.Sigma = QR.R;
    return svd_result;
}

struct UP_mats {
    mat3 U;
    mat3 P;
};

UP_mats SVD_to_polar(SVD_mats B) {
    UP_mats polar;
    polar.P = B.V * B.Sigma * transpose(B.V);
    polar.U = B.U * transpose(B.V);
    return polar;
}

UP_mats polar_decomp(mat3 A) {
    SVD_mats B = svd(A);
    UP_mats polar;
    polar.P = B.V * B.Sigma * transpose(B.V);
    polar.U = B.U * transpose(B.V);
    return polar;
}











struct VertexStruct{
    float pos[3];
    float normal[3];
};

struct Vertex{
    vec3 pos;
    vec3 normal;
    vec3 overlay;
};

struct TriangleStruct{
    int V1;
    int V2;
    int V3;
};

struct AdjacentStruct{
    int V1;
    int V2;
    int V3;
    int F1;
    int F2;
    int F3;
};



struct GridHelper{
    float bbmin[3];
    float bbmax[3];
    float start;
    float end;
};

struct HitInfo{
    TriangleStruct id;
    double dist;
    vec3 where;
    bool boolean;
    int tid;
};

layout(std430, binding=0 ) readonly buffer VertexBlock{
    VertexStruct vertices[];
};

layout(std430, binding=1 ) readonly buffer TriangleBlock{
    TriangleStruct triangles[];
};

layout(std430, binding=2 ) readonly buffer GridRefBlock{
    int primrefs[];
};


layout(std430, binding=3 ) readonly buffer GridHelperBlock{
    float fglobalmin[3];
    float fglobalmax[3];
    float unitsize;
    float xcount;
    float ycount;
    float zcount;
    GridHelper grids[];
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


in vec3 N;
in vec3 T;
//in vec3 Nf;
//in vec3 Tf;


in vec3 Direc;
in vec3 Orig;
in vec3 Tangent;

in vec3 flatNormal;
uniform vec3 LIGHTDIRECTION;

layout (location=0) out vec4 scene;



vec4 quatmul(vec4 P, vec4 Q){
    vec3 vQ = Q.xyz;
    float rQ = Q.w;
    vec3 vP = P.xyz;
    float rP = P.w;
    
    float W = rQ*rP-dot(vQ,vP);
    vec3 V = rQ * vP + rP * vQ + cross(vQ,vP);
    
    return vec4(V,W);

}

//解决了一些噪点问题
bool anyOfVec(vec3 f, vec3 s){
    float E = 0.0001;
    if(f.x < s.x+E){return false;}
    if(f.y < s.y+E){return false;}
    if(f.z < s.z+E){return false;}
    return true;
}




Vertex Volumetric(HitInfo Hit){

    float k = 10;
    float r = 1;

    TriangleStruct prim = Hit.id;
    VertexStruct pA = vertices[prim.V1];
    VertexStruct pB = vertices[prim.V2];
    VertexStruct pC = vertices[prim.V3];

    vec3 A = vec3(pA.pos[0],pA.pos[1],pA.pos[2]);
    vec3 B = vec3(pB.pos[0],pB.pos[1],pB.pos[2]);
    vec3 C = vec3(pC.pos[0],pC.pos[1],pC.pos[2]);
    vec3 Normal1 = vec3(pA.normal[0],pA.normal[1],pA.normal[2]);
    vec3 Normal2 = vec3(pB.normal[0],pB.normal[1],pB.normal[2]);
    vec3 Normal3 = vec3(pC.normal[0],pC.normal[1],pC.normal[2]);

    vec3 point = Hit.where; //要对空间中的任意一点插值

    vec3 new_pos = vec3(0);
    float weight_function = 0;

    Vertex result = Vertex(vec3(0), vec3(0), vec3(0));
    for(int i = adj[Hit.tid].start; i < adj[Hit.tid].end; i++){
        int p = pointcloud[i];
        VertexStruct pD = vertices[p];
        vec3 D = vec3(pD.pos[0],pD.pos[1],pD.pos[2]);

        float len = length(D - point);
        float weight = 1 - pow( len / r, 3);
        weight_function += weight;
        new_pos += weight * D;
    }

    {
        float len = length(A - point);
        float weight = 1 - pow( len / r, 3);
        weight_function += weight;
        new_pos += weight * A;
    }
    {
        float len = length(B - point);
        float weight = 1 - pow( len / r, 3);
        weight_function += weight;
        new_pos += weight * B;
    }
    {
        float len = length(C - point);
        float weight = 1 - pow( len / r, 3);
        weight_function += weight;
        new_pos += weight * C;
    }




    

    //新的位置肯定是有变化的，不然怎么平滑流体？
    //咦，有没有可能对着片元输出的结果重建网格呢？
    //就是说每个UV都映射一个位置，通过在FS中遍历低模的UV，并知道对应的位置，然后，输入UV，就可以查询映射关系把模型重新组装成3D的，并且伴随位置的变化，看起来更平滑？
    vec3 weighted_mean = new_pos / weight_function;
    mat3 covariance = mat3(0);

    for(int i = adj[Hit.tid].start; i < adj[Hit.tid].end; i++){
        int p = pointcloud[i];
        VertexStruct pD = vertices[p];
        vec3 D = vec3(pD.pos[0],pD.pos[1],pD.pos[2]);

        float len = length(D - point);
        float weight = 1 - pow( len / r, 3);
        vec3 weight_dist = D - weighted_mean;

        //可能吧
        covariance += mat3(weight_dist*weight_dist.x*weight, weight_dist*weight_dist.y*weight, weight_dist*weight_dist.z*weight);
    }


    {
        float len = length(A - point);
        float weight = 1 - pow( len / r, 3);
        vec3 weight_dist = A - weighted_mean;
        covariance += mat3(weight_dist*weight_dist.x*weight, weight_dist*weight_dist.y*weight, weight_dist*weight_dist.z*weight);
    }
    {
        float len = length(B - point);
        float weight = 1 - pow( len / r, 3);
        vec3 weight_dist = B - weighted_mean;
        covariance += mat3(weight_dist*weight_dist.x*weight, weight_dist*weight_dist.y*weight, weight_dist*weight_dist.z*weight);
    }
    {
        float len = length(C - point);
        float weight = 1 - pow( len / r, 3);
        vec3 weight_dist = C - weighted_mean;
        covariance += mat3(weight_dist*weight_dist.x*weight, weight_dist*weight_dist.y*weight, weight_dist*weight_dist.z*weight);
    }



    covariance /= weight_function;

    //俺寻思
    SVD_mats dsv = svd(covariance);
    
    /*
    mat3 houDiag = mat3(vec3(dsv.Sigma[0][0], 0, 0).
                        vec3(0, dsv.Sigma[1][1], 0)
                        vec3(0, 0, dsv.Sigma[2][2]));
    */

    mat3 anisotropy = mat3(dsv.U * dsv.Sigma * transpose(dsv.U));


/*
    vec3 L1 = A - point;
    vec3 L2 = B - point;
    vec3 L3 = C - point;

    vec3 L4 = B-A;
    vec3 L5 = C-A;
    float S1 = length(cross(L1,L2));
    float S2 = length(cross(L1,L3));
    float S3 = length(cross(L2,L3));
    float S4 = length(cross(L4,L5));

    float c1 = S3/S4;
    float c2 = S2/S4;
    float c3 = S1/S4;

*/

    result.normal = normalize(anisotropy * point - point);
    return result;

}



Vertex TEST(HitInfo Hit){



    TriangleStruct prim = Hit.id;
    VertexStruct pA = vertices[prim.V1];
    VertexStruct pB = vertices[prim.V2];
    VertexStruct pC = vertices[prim.V3];

    vec3 A = vec3(pA.pos[0],pA.pos[1],pA.pos[2]);
    vec3 B = vec3(pB.pos[0],pB.pos[1],pB.pos[2]);
    vec3 C = vec3(pC.pos[0],pC.pos[1],pC.pos[2]);


    vec3 point = Hit.where; //要对空间中的任意一点插值


    Vertex result = Vertex(vec3(0), vec3(0), vec3(0));
    for(int i = adj[Hit.tid].start; i < adj[Hit.tid].end; i++){
        int p = pointcloud[i];
        VertexStruct pD = vertices[p];
        vec3 D = vec3(pD.pos[0],pD.pos[1],pD.pos[2]);

        result.pos += D;
    }
    result.pos /= adj[Hit.tid].end - adj[Hit.tid].start;

    return result;

}







HitInfo Hit(vec3 Dirf, TriangleStruct prim, int id){

    dvec3 Dir = dvec3(Dirf);
    VertexStruct pA = vertices[prim.V1];
    VertexStruct pB = vertices[prim.V2];
    VertexStruct pC = vertices[prim.V3];

    dvec3 A = dvec3(pA.pos[0],pA.pos[1],pA.pos[2]);
    dvec3 B = dvec3(pB.pos[0],pB.pos[1],pB.pos[2]);
    dvec3 C = dvec3(pC.pos[0],pC.pos[1],pC.pos[2]);

    //Given raydirection & rayorigin , we have to calculate where this ray may intersect with plane
    //Plane is defined by triangle . For an arbitary point on that plane , P = a(P3-P1) + b(P2-P1) + P1
    //P is also on ray, so we have P = RO + i*RD
    //RO - P1 = a(P3-P1) + b(P2-P1) - i*RD
    dvec3 e1 = C-A;
    dvec3 e2 = B-A;
    dvec3 res = Orig-A;


    double det = dot(e1,cross(e2,Dir));
    double deta = dot(res,cross(e2,Dir));
    double detb = dot(e1,cross(res,Dir));
    
    double deti = dot(e1,cross(e2,res));
    double a = deta/det;
    double b = detb/det;
    double c = -deti/det;
    vec3 reached = Orig + float(c)*Dirf;

    //The interesting thing is , the algorithm we're going to use to test being in triangle , matches our definitions of 
    //P = a(P3-P1) + b(P2-P1) + P1

    return HitInfo(prim, c, reached, a>=double(0.) && a<=double(1.) && b>=double(0.) && b<=double(1.) && a+b<=double(1.), primrefs[id]);


}


HitInfo InsideOneGrid(vec3 Dir, GridHelper which){
    double FLT_MAX = 4096;
    HitInfo hitprim;
    for(int i= int(which.start);i< int(which.end);i++){
        TriangleStruct prim = triangles[primrefs[i]];
        HitInfo info = Hit(Dir, prim, i);
        if(info.boolean){
            if(info.dist<FLT_MAX){
                FLT_MAX = info.dist;
                hitprim = info;
            }
        }
    }
    return hitprim;
}








HitInfo RemasteredSearchForNext(ivec3 where_id){

    int startX = (where_id.x);
    int startY = (where_id.y);
    int startZ = (where_id.z);

    int ixc = int(xcount);
    int iyc = int(ycount);

    int depth = 50;
    vec3 Dir = -normalize(N);
    HitInfo analyseOutput;


    int stepX = Dir.x > 0 ? 1 : -1;
    int stepY = Dir.y > 0 ? 1 : -1;
    int stepZ = Dir.z > 0 ? 1 : -1;
	
	bool passTarget = false;

    for(int i = 0;i<depth;i++){

        bool X = startX < 0 || startX >= ixc;
        bool Y = startY < 0 || startY >= iyc;
        bool Z = startZ < 0 || startZ >= int(zcount);

        if(X||Y||Z){
            break;
        }

        GridHelper which = grids[startX + startY*ixc + startZ*ixc*iyc];
        vec3 bbmax = vec3(which.bbmax[0], which.bbmax[1], which.bbmax[2]);
        vec3 bbmin = vec3(which.bbmin[0], which.bbmin[1], which.bbmin[2]);

		vec3 GVTmaxraw = (bbmax - Orig)/Dir;
        vec3 GVTminraw = (bbmin - Orig)/Dir;
        vec3 GVTmax = max(GVTmaxraw, GVTminraw);
				

		analyseOutput = InsideOneGrid(Dir, which);
        

		if(!analyseOutput.boolean){
			if(GVTmax.x<GVTmax.z){
				if(GVTmax.x<GVTmax.y){
					//x<y x<z
					startX += stepX;
				}
				else{
					//y<x<z
					startY += stepY;
				}
			}
			else{
				if(GVTmax.z<GVTmax.y){
					//z<x z<y
					startZ += stepZ;
				}
				else{
					//y<z<x
					startY += stepY;
				}    
			}

		}else{
			break;
		}


        
    }

    return analyseOutput;
}


void main(){
    
    vec3 globalmin = vec3(fglobalmin[0], fglobalmin[1], fglobalmin[2]);
    vec3 globalmax = vec3(fglobalmax[0], fglobalmax[1], fglobalmax[2]);
    vec3 Dir = -normalize(N);
    vec3 VTmin = (globalmin - Orig)/Dir;
    vec3 VTmax = (globalmax - Orig)/Dir;
    VTmin = min(VTmax, VTmin);
    float Tmin = max(  VTmin.x ,max( VTmin.y,VTmin.z) );


    vec3 where = Orig + Dir * Tmin;
	ivec3 where_id;
	ivec3 shootAbroad = max(ivec3(0,0,0), ivec3(min(floor((where-globalmin)/unitsize),vec3(xcount-1,ycount-1,zcount-1))));
	ivec3 shootInside = max(ivec3(0,0,0), ivec3(min(floor((Orig -globalmin)/unitsize),vec3(xcount-1,ycount-1,zcount-1))));
	where_id = anyOfVec(Orig,globalmin)&&anyOfVec(globalmax,Orig)?shootInside:shootAbroad;


    HitInfo analyse = RemasteredSearchForNext(where_id);
    Vertex interpolation = Volumetric(analyse);


    vec3 sTa = normalize(T);
    vec3 sNo = normalize(N);
    float LengthTN = dot(sTa, sNo);
    vec3 alignedN = LengthTN * sNo;
    vec3 newT = sTa - alignedN;
    mat3 oTBN = mat3(normalize(newT), normalize(cross(newT, sNo)), sNo);

    vec3 tspaceN = transpose(oTBN) * (interpolation.normal);
    scene = vec4((tspaceN+1)*0.5, 1);
}
