#version 450 core





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
    ivec3 gridid;
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


struct VertexSection{
    int start;
    int end;
};


layout(std430, binding=8 ) readonly buffer VertexRefBlock{
    int vertexref[];
};

layout(std430, binding=9 ) readonly buffer VertexSectionBlock{
    VertexSection sections[];
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




//解决了一些噪点问题
bool anyOfVec(vec3 f, vec3 s){
    float E = 0.0001;
    if(f.x < s.x+E){return false;}
    if(f.y < s.y+E){return false;}
    if(f.z < s.z+E){return false;}
    return true;
}



float GaussianKernel(vec3 dist){

    float fdist = length(dist);//这个大概是一维高斯
    float x2 = fdist*fdist;
    float a = 0.1;
    return exp(-a * x2);
}




Vertex Volumetric(HitInfo Hit){


    if(!Hit.boolean){
        Vertex r = Vertex(vec3(0), vec3(0), vec3(0));
        r.pos = vec3(0);
        r.normal = vec3(5);
        r.overlay = vec3(0,1,0);
        return r;
    }  


    GridHelper current = grids[Hit.gridid.x + Hit.gridid.y * int(xcount) + Hit.gridid.z * int(xcount) * int(ycount)];
    vec3 bbmax = vec3(current.bbmax[0], current.bbmax[1], current.bbmax[2]);
    vec3 bbmin = vec3(current.bbmin[0], current.bbmin[1], current.bbmin[2]);
    vec3 current_center = (bbmax + bbmin)/2;



    vec3 globalmin = vec3(fglobalmin[0], fglobalmin[1], fglobalmin[2]);
    float SearchRadius = 2;//grid里面可能找不到点，需要保证这个数字够大，克服代码的缺陷性
    float Power = 3;//没什么影响
    float M = 0;//不要改这个数字


    Vertex r = Vertex(vec3(0), vec3(0), vec3(0));

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

    vec3 where = Hit.where;


    vec3 L1 = A - where;
    vec3 L2 = B - where;
    vec3 L3 = C - where;

    vec3 L4 = B-A;
    vec3 L5 = C-A;
    float S1 = length(cross(L1,L2));
    float S2 = length(cross(L1,L3));
    float S3 = length(cross(L2,L3));
    float S4 = length(cross(L4,L5));

    float c1 = S3/S4;
    float c2 = S2/S4;
    float c3 = S1/S4;

    vec3 p_normal = vec3(c1*Normal1 + c2*Normal2 + c3*Normal3);
    p_normal = normalize(p_normal);

    if(SearchRadius>unitsize*15){return r;}


    //简单地找8个立方体
    //或考虑点在当前体素里靠近哪个边缘
    //或者作球找
    vec3 Sphere_bbox_max = where + SearchRadius;
    vec3 Sphere_bbox_min = where - SearchRadius;

    float E = 0.001;
    float x_begin = floor((Sphere_bbox_min.x-globalmin.x)/unitsize+E); 
        x_begin = x_begin<0?0:x_begin; 
        x_begin = x_begin>=xcount?xcount-1:x_begin;

    float x_end = floor((Sphere_bbox_max.x-globalmin.x)/unitsize-E); 
        x_end = x_end<0?0:x_end; 
        x_end = x_end>=xcount?xcount-1:x_end;

    float y_begin = floor((Sphere_bbox_min.y-globalmin.y)/unitsize+E); 
        y_begin = y_begin<0?0:y_begin; 
        y_begin = y_begin>=ycount?ycount-1:y_begin;

    float y_end = floor((Sphere_bbox_max.y-globalmin.y)/unitsize-E); 
        y_end = y_end<0?0:y_end; 
        y_end = y_end>=ycount?ycount-1:y_end;

    float z_begin = floor((Sphere_bbox_min.z-globalmin.z)/unitsize+E); 
        z_begin = z_begin<0?0:z_begin; 
        z_begin = z_begin>=zcount?zcount-1:z_begin;

    float z_end = floor((Sphere_bbox_max.z-globalmin.z)/unitsize-E); 
        z_end = z_end<0?0:z_end; 
        z_end = z_end>=zcount?zcount-1:z_end;
    

    float weight_function = 0;
    vec3 weight_pos = where;


    for(int a = int(x_begin); a <= int(x_end); a++){
        for(int b = int(y_begin); b <= int(y_end); b++){
            for(int c = int(z_begin); c <= int(z_end); c++){
                VertexSection which = sections[a + b * int(xcount) + c * int(xcount) * int(ycount)];
                for(int i= int(which.start);i< int(which.end);i++){
                    VertexStruct pX = vertices[vertexref[i]];
                    vec3 X = vec3(pX.pos[0],pX.pos[1],pX.pos[2]);
                    float mag = length(X - where);
                    if(mag<=SearchRadius){
                        vec3 angle = vec3(pX.normal[0], pX.normal[1], pX.normal[2]);
                        float angle_weight = dot(angle,p_normal);
                        float weight = 1;
                        weight_function += weight;
                        weight_pos += weight * X;
                    }
                }
            }
        }    
    }

    vec3 mean_Pos = weight_pos / weight_function;
    mat3 covariance = mat3(0);


//0degree -> 1
//90degree -> 0
//120degree-> 0

//0->1
//90->0.5
//120->0


//x  0  y 1
//*0.5->x0 y0.5
//x 0.5 y 1


//1*(m)+(1-m)
    vec3 sum = vec3(0);

    for(int a = int(x_begin); a <= int(x_end); a++){
        for(int b = int(y_begin); b <= int(y_end); b++){
            for(int c = int(z_begin); c <= int(z_end); c++){
                VertexSection which = sections[a + b * int(xcount) + c * int(xcount) * int(ycount)];
                for(int i= int(which.start);i< int(which.end);i++){
                    VertexStruct pX = vertices[vertexref[i]];
                    vec3 X = vec3(pX.pos[0],pX.pos[1],pX.pos[2]);
                    float mag = length(X - where);
                    if(mag<=SearchRadius){
                        vec3 angle = vec3(pX.normal[0], pX.normal[1], pX.normal[2]);
                        float angle_weight = dot(angle,p_normal);
                        float weight = 1 - pow(mag / (SearchRadius), Power);
                        weight *= clamp(angle_weight,0,1);
                        //weight *= dot(X-where, angle)>-0.1?1:0;
                        //
                        vec3 dist = X - mean_Pos;
                        covariance += mat3(dist*dist.x*weight, 
                             dist*dist.y*weight,
                             dist*dist.z*weight);
                        //sum += GaussianKernel(X - where) * angle * clamp(angle_weight * (1-M) + M,0,1);

                    }
                }
            }
        }
    }

    covariance /= weight_function;

    r.pos = mean_Pos;
    r.normal = inverse(covariance) * p_normal;

    return r;

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

    return HitInfo(prim, c, reached, a>=double(0.) && a<=double(1.) && b>=double(0.) && b<=double(1.) && a+b<=double(1.) && c>=0, primrefs[id], ivec3(0));


}


HitInfo InsideOneGrid(vec3 Dir, GridHelper which, ivec3 where_id){
    double FLT_MAX = 4096;
    HitInfo hitprim;
    for(int i= int(which.start);i< int(which.end);i++){
        TriangleStruct prim = triangles[primrefs[i]];
        HitInfo info = Hit(Dir, prim, i);
        if(info.boolean){
            if(info.dist<FLT_MAX){
                FLT_MAX = info.dist;
                hitprim = info;
                hitprim.gridid = where_id;
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
				

		analyseOutput = InsideOneGrid(Dir, which, ivec3(startX,startY,startZ));
        

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

    vec3 tspaceN = transpose(oTBN) * normalize(interpolation.normal);
    scene = vec4((tspaceN+1)*0.5, 1);
    //scene.xyz = (interpolation.pos);

}