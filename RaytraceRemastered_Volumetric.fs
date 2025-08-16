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
    int gid;
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




in vec3 N;
in vec3 T;
in vec3 Nf;
in vec3 Tf;


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




Vertex Volumetric(HitInfo hit){
    
    VertexSection which = sections[hit.gid];
    Vertex r = Vertex(vec3(0), vec3(0), vec3(0));

    vec3 reached = hit.where;

    vec3 nearest = vec3(12);
    vec3 mid = vec3(12);
    vec3 far = vec3(12);

    int nearest_id = 0;
    int mid_id = 0;
    
        //堆排序
        
        //只需要选择最小三个，所以选择排序

    for(int i= int(which.start);i< int(which.end);i++){
        float min_dist = 4096;
        VertexStruct pA = vertices[vertexref[i]];
        vec3 A = vec3(pA.pos[0],pA.pos[1],pA.pos[2]);
        float dist = length(A - reached);
        if(dist<min_dist){
            min_dist = dist;
            nearest = A;
            nearest_id = i;
        }
    }

    for(int i= int(which.start);i< int(which.end);i++){
        float min_dist = 4096;
        VertexStruct pA = vertices[vertexref[i]];
        vec3 A = vec3(pA.pos[0],pA.pos[1],pA.pos[2]);
        float dist = length(A - reached);
        if(i!=nearest_id && dist<min_dist){
            min_dist = dist;
            mid = A;
            mid_id = i;
        }
    }
/*
    for(int i= int(which.start);i< int(which.end);i++){
        float min_dist = 4096;
        VertexStruct pA = vertices[vertexref[i]];
        vec3 A = vec3(pA.pos[0],pA.pos[1],pA.pos[2]);
        float dist = length(A - reached);
        if(i!=nearest_id && i!= mid_id && dist<min_dist){
            min_dist = dist;
            far = A;
        }
    }
*/

    r.pos = mix(nearest, mid, length(reached-nearest)/length(nearest-mid));
    return r;
}








HitInfo Hit(vec3 Dirf, TriangleStruct prim){

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

    return HitInfo(prim, c, reached, a>=double(0.) && a<=double(1.) && b>=double(0.) && b<=double(1.) && a+b<=double(1.), 0);


}


HitInfo InsideOneGrid(vec3 Dir, GridHelper which, int id){
    double FLT_MAX = 4096;
    HitInfo hitprim;
    for(int i= int(which.start);i< int(which.end);i++){
        TriangleStruct prim = triangles[primrefs[i]];
        HitInfo info = Hit(Dir, prim);
        if(info.boolean){
            if(info.dist<FLT_MAX){
                FLT_MAX = info.dist;
                hitprim = info;
                hitprim.gid = id;
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

        int id = startX + startY*ixc + startZ*ixc*iyc;
        GridHelper which = grids[id];
        vec3 bbmax = vec3(which.bbmax[0], which.bbmax[1], which.bbmax[2]);
        vec3 bbmin = vec3(which.bbmin[0], which.bbmin[1], which.bbmin[2]);

		vec3 GVTmaxraw = (bbmax - Orig)/Dir;
        vec3 GVTminraw = (bbmin - Orig)/Dir;
        vec3 GVTmax = max(GVTmaxraw, GVTminraw);
				

		analyseOutput = InsideOneGrid(Dir, which, id);
        

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
    mat3 fTBN = mat3(normalize(Tf), normalize(cross(Tf, Nf)),normalize(Nf));

    vec3 tspaceN = transpose(oTBN) * normalize(interpolation.normal);

    scene = vec4((tspaceN+1)*0.5, 1);
    scene.xyz = interpolation.pos;

}
