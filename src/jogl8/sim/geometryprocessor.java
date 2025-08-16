package jogl8.sim;

import java.nio.FloatBuffer;
import java.nio.IntBuffer;
import java.util.ArrayList;
import java.util.HashMap;
import java.util.Iterator;
import java.util.List;

import assimp.AiMesh;
import glm_.vec3.Vec3;
import jogl8.utils;

public class geometryprocessor {
	
		
	public static class VecHash extends HashMap<Vec3, Vec3>{
		private static final long serialVersionUID = 452425699485455628L;
		//get with default value
		public Vec3 getdv(Vec3 Key) {
			if(this.containsKey(Key)) {
				return this.get(Key);
			}else {
				return new Vec3(0);
			}
		}
	}
	
	IntBuffer adj;
	IntBuffer adj_locator;

	
	int Diffusion_Max = 0;
	
	AiMesh workmesh;
	public HashMap<Vec3, Integer> IndexRemap = new HashMap<Vec3, Integer>();
	public HashMap<Vec3, ArrayList<Integer>> TriCon = new HashMap<Vec3, ArrayList<Integer>>();

	VecHash NormalRemap = new VecHash();
	buildgrid BG;
	public ArrayList<ArrayList<Integer>> voxels = new ArrayList<ArrayList<Integer>>();	
	public float MAX = -1000.0f;
	public float MIN = 10000.0f;

	public geometryprocessor(AiMesh loadmesh, buildgrid grid) {
		BG = grid;
		utils.LOG("BG.points.length :  " + BG.points.length);
		for(int m=0;m<BG.points.length;m++) {
			voxels.add(new ArrayList<Integer>());
		}
		
		this.workmesh = loadmesh;
		int invocounts = workmesh.getNumFaces();
		//utils.LOG(invocounts+"wtf");
		for(int i=0;i<invocounts;i++) {
			List<Integer> triangle = workmesh.getFaces().get(i);
			Vec3 A = workmesh.getVertices().get(triangle.get(0));
			Vec3 B = workmesh.getVertices().get(triangle.get(1));
			Vec3 C = workmesh.getVertices().get(triangle.get(2));
			Vec3[] list = new Vec3[]{A,B,C};
			LoopPerTriangle(i,list);
		}
		utils.log(workmesh.getNumVertices()+  " 合并前");
		utils.log(IndexRemap.size()+  " 合并后");
		
		/*
		int _MAX = -1;
		Iterator it = TriCon.keySet().iterator();
		while(it.hasNext()) {
			ArrayList<Integer> tr = TriCon.get(it.next());
			for(int i=0;i<tr.size();i++) {
				_MAX = Math.max(_MAX, tr.get(i));
			}
		}
		utils.LOG(_MAX+"_MAX tr.get(i)");*/
	}
	
	public static double calradian(Vec3 basePos, Vec3 Pos1, Vec3 Pos2) {
		Vec3 v1 = Pos1.minus(basePos).normalize();
		Vec3 v2 = Pos2.minus(basePos).normalize();
		float cos = v1.dot(v2);
		return Math.acos(cos);
	}
	
	public double calArea(Vec3 base, Vec3 p1, Vec3 p2) {
		double cosC = (p1.minus(base).normalize()).dot(p2.minus(base).normalize());
		double sinC = Math.sqrt(1.0 - cosC * cosC);
		return p1.length() * p2.length() * sinC;
	}
	
	public Vec3 ignore(Vec3 v1, Vec3 v2, Vec3 org) {
		v1 = v1.normalize();
		v2 = v2.normalize();
		if(Math.abs(v1.dot(v2))>0.95) {
			return v1.plus(v2).div(2);
		}
		return org;
	}
	
	public float Byangle(Vec3 v1, Vec3 v2) {
		if(v1.length()==0) {return 1;}
		v1 = v1.normalize();
		v2 = v2.normalize();
		float angle = Math.abs(v1.dot(v2));
		angle = 1-angle;
		return angle;
		
	}
	
	public void LoopPerTriangle(int primitiveIndex, Vec3[] in) {
		float approximation = 1;
		Vec3 temp = in[0].times(approximation).minus(in[2].times(approximation));
		Vec3 flatnormal = temp.cross(in[1].times(approximation).minus(in[2].times(approximation))).normalize();
		double s = 1;

		//不能在这里normalize要存入的向量，因为要保留弧度制倍数以做到配平
		{
			Vec3 position = in[0];
			Vec3 basevec = NormalRemap.getdv(position);
			//double s = calArea(in[0], in[1], in[2]);

			Vec3 weightflat = flatnormal.times(s * calradian(in[0], in[1], in[2]));

			NormalRemap.put(position, basevec.plus(weightflat));
			
			if(!TriCon.containsKey(position)) {
				ArrayList<Integer> list = new ArrayList<Integer>();
				list.add(primitiveIndex);
				TriCon.put(position, list);
			}else {
				TriCon.get(position).add(primitiveIndex);//点引用面
			}
			if(!IndexRemap.containsKey(position)) {
				IndexRemap.put(position, IndexRemap.size());
			}
		}
		
		{
			Vec3 position = in[1];
			Vec3 basevec = NormalRemap.getdv(position);
			//double s = calArea(in[1], in[2], in[0]);

			Vec3 weightflat = flatnormal.times(s  * calradian(in[1], in[2], in[0]));

			NormalRemap.put(position, basevec.plus(weightflat));
			
			if(!TriCon.containsKey(position)) {
				ArrayList<Integer> list = new ArrayList<Integer>();
				list.add(primitiveIndex);
				TriCon.put(position, list);
			}else {
				TriCon.get(position).add(primitiveIndex);
			}
			if(!IndexRemap.containsKey(position)) {
				IndexRemap.put(position, IndexRemap.size());
			}
		}
		
		{
			Vec3 position = in[2];
			Vec3 basevec = NormalRemap.getdv(position);
			//double s = calArea(in[2], in[1], in[0]);

			Vec3 weightflat = flatnormal.times(s  * calradian(in[2], in[1], in[0]));

			NormalRemap.put(position, basevec.plus(weightflat));

			if(!TriCon.containsKey(position)) {
				ArrayList<Integer> list = new ArrayList<Integer>();
				list.add(primitiveIndex);
				TriCon.put(position, list);
			}else {
				TriCon.get(position).add(primitiveIndex);
			}
			if(!IndexRemap.containsKey(position)) {
				IndexRemap.put(position, IndexRemap.size());
			}
		}
	}
	
	private Vec3 loop(Vec3 root , Vec3 normal, int lv) {
		lv += 1;
		if(lv == 1) return normal;
		for(int i = 0;i<TriCon.get(root).size();i++) {
			List<Integer> triangle = workmesh.getFaces().get(TriCon.get(root).get(i));
			for(int j = 0;j<=2;j++) {
				Vec3 A = workmesh.getVertices().get(triangle.get(j));

				if(A.hashCode()!=root.hashCode()) {
					Vec3 adj_a_n = NormalRemap.get(A);
					normal=normal.plus(adj_a_n);
					loop(A, normal, lv);
				}
			}
		}
		return normal;
	}
	
	public FloatBuffer CreateBufferObject() {
		//3 floats for position ; 3 floats for normal
		FloatBuffer remapped = FloatBuffer.allocate(IndexRemap.size() * 6);
		Iterator<Vec3> keysetiterator = IndexRemap.keySet().iterator();
		while(keysetiterator.hasNext()) {
			//Our position
			Vec3 root = keysetiterator.next();
			//Find corresponding normal
			Vec3 normal = NormalRemap.get(root);
			//Find corresponding index
			int index = IndexRemap.get(root);
			
			
			//统计连接性
			normal = loop(root, normal, 0);
			
			

			
			normal = normal.normalize();
			
			//since 13
			remapped.put(index*6+0, root.getArray(), 0, 3);
			remapped.put(index*6+3, normal.getArray(), 0, 3);
		}
		remapped.clear();
		return remapped;
	}
	
	
	public void FindAdjFace(List<List<Integer>> faces, int t, HashMap<Integer, Integer>uniqueVertices, int lv, int old) {
		
		if(lv > Diffusion_Max) {return;}
		
		List<Integer> face = faces.get(t);
		Vec3 pos = workmesh.getVertices().get(face.get(0));
		ArrayList<Integer> con_primitives_p0 = TriCon.get(pos);
		
		Vec3 pos1 = workmesh.getVertices().get(face.get(1));
		ArrayList<Integer> con_primitives_p1 = TriCon.get(pos1);
		
		Vec3 pos2 = workmesh.getVertices().get(face.get(2));
		ArrayList<Integer> con_primitives_p2 = TriCon.get(pos2);
		
		int ref_edge01 = -1;
		for(int i = 0; i< con_primitives_p0.size(); i++) {
			int ref_by_0_primitive = con_primitives_p0.get(i);
			for(int j = 0; j< con_primitives_p1.size(); j++) {
				int ref_by_1_primitive = con_primitives_p1.get(j);
				//一条边引用两个面
				if(ref_by_0_primitive == ref_by_1_primitive && ref_by_0_primitive!= t) {
					ref_edge01 = ref_by_0_primitive;
				}
			}
		}
		
		
		int ref_edge02 = -1;
		for(int i = 0; i< con_primitives_p0.size(); i++) {
			int ref_by_0_primitive = con_primitives_p0.get(i);
			for(int j = 0; j< con_primitives_p2.size(); j++) {
				int ref_by_2_primitive = con_primitives_p2.get(j);
				if(ref_by_0_primitive == ref_by_2_primitive && ref_by_0_primitive!= t) {
					ref_edge02 = ref_by_0_primitive;
				}
			}
		}
		
		//图元编号不变
		//但是顶点编号不能由face.geet.get获得
		
		int ref_edge12 = -1;
		for(int i = 0; i< con_primitives_p1.size(); i++) {
			int ref_by_1_primitive = con_primitives_p1.get(i);
			for(int j = 0; j< con_primitives_p2.size(); j++) {
				int ref_by_2_primitive = con_primitives_p2.get(j);
				if(ref_by_1_primitive == ref_by_2_primitive && ref_by_1_primitive != t) {
					ref_edge12 = ref_by_1_primitive;
				}
			}
		}
		
		if(ref_edge01==t || ref_edge02==t ||ref_edge12 == t) {utils.LOG("!!!!!get fucked ======t !");}
		//因为con_primitives_px是点参与构成的面
		//点是当前轮的三角形的顶点
		//con_primitives_px肯定也包括自己的引用

		if(ref_edge01==-1 || ref_edge02==-1 ||ref_edge12 == -1) {utils.LOG("!!!!!get fucked");}
		if(ref_edge01 >=faces.size()|| ref_edge02>=faces.size() ||ref_edge12 >=faces.size()) {utils.LOG("!!!!!get fucked >faces.size()");}

		int pointTo2 = -1;
		int pointTo1 = -1;
		int pointTo0 = -1;
		
		for(int i=0;i<3;i++) {
			Vec3 k_pos = workmesh.getVertices().get(faces.get(ref_edge01).get(i));
			if(k_pos.hashCode()==pos.hashCode()||k_pos.hashCode()==pos1.hashCode()||k_pos.hashCode()==pos2.hashCode()) {
				//如果pointTo2保持是-1
				//那说明三个点全部相等
				//而不是vec3.hashcode不准确
				//这意味着ref_edge01==t
			}else {
				//这有问题
				//face.get.get返回的索引索引的是workmesh.vertices
				//不是合并后的顶点
				Vec3 actualpos = workmesh.getVertices().get(faces.get(ref_edge01).get(i));			
				pointTo2 = IndexRemap.get(actualpos);
				break;
			}
		}
		
		
		for(int i=0;i<3;i++) {
			Vec3 k_pos = workmesh.getVertices().get(faces.get(ref_edge02).get(i));
			if(k_pos.hashCode()==pos.hashCode()||k_pos.hashCode()==pos1.hashCode()||k_pos.hashCode()==pos2.hashCode()) {
			}else {
				Vec3 actualpos = workmesh.getVertices().get(faces.get(ref_edge02).get(i));			
				pointTo1 = IndexRemap.get(actualpos);			
				break;
			}
		}
		
		for(int i=0;i<3;i++) {
			Vec3 k_pos = workmesh.getVertices().get(faces.get(ref_edge12).get(i));
			if(k_pos.hashCode()==pos.hashCode()||k_pos.hashCode()==pos1.hashCode()||k_pos.hashCode()==pos2.hashCode()) {
			}else {
				Vec3 actualpos = workmesh.getVertices().get(faces.get(ref_edge12).get(i));			
				pointTo0 = IndexRemap.get(actualpos);		
				break;
			}
		}
		
		if(pointTo0==-1 || pointTo1==-1 ||pointTo2 == -1) {utils.LOG("!!!!!get fucked pointTo2");}

		uniqueVertices.put(pointTo0, 0);
		uniqueVertices.put(pointTo1, 0);
		uniqueVertices.put(pointTo2, 0);
		
		lv+=1;
		
		if(!(ref_edge01==t) && !(ref_edge01==old)) {
			FindAdjFace(faces, ref_edge01, uniqueVertices, lv, t);
		}
		if(!(ref_edge02==t) && !(ref_edge02==old)) {
			FindAdjFace(faces, ref_edge02, uniqueVertices, lv, t);
		}
		if(!(ref_edge12==t) && !(ref_edge12==old)) {
			FindAdjFace(faces, ref_edge12, uniqueVertices, lv, t);
		}	
		return;
	}
	
	
	//带有邻接 
	public IntBuffer RemappingIndices() {
		//1 triangle = 3 vertices
		IntBuffer remapped = IntBuffer.allocate(workmesh.getNumFaces() * 3); 
		int totalcount = 0;
		utils.LOG(workmesh.getNumFaces() + "faces");
		
		
		adj_locator = IntBuffer.allocate(workmesh.getNumFaces() * 2); 

		
		//图元数*3的是索引
		for(int i=0;i<workmesh.getNumFaces();i++) {
			List<List<Integer>> faces = workmesh.getFaces();
			for(int j=0;j<3;j++) {
				Vec3 pos = workmesh.getVertices().get(faces.get(i).get(j));
				Integer compress = IndexRemap.get(pos);
				remapped.put(compress);
				//EBO储存最多的顶点数量，但都是id形式。
			}
			//Perface operations start here
			HashMap<Integer, Integer> uniqueVertices = new HashMap<Integer, Integer>();
			FindAdjFace(faces, i, uniqueVertices, 1, i);
			adj_locator.put(totalcount);
			totalcount += uniqueVertices.size();
			adj_locator.put(totalcount);

			
			
			Primitive(i, workmesh);
		}
		
		remapped.clear();
		utils.LOG("Debug value max: "+ this.MAX);
		utils.LOG("Debug value min: "+ this.MIN);
		
		adj = IntBuffer.allocate(totalcount); 

		for(int i=0;i<workmesh.getNumFaces();i++) {
			List<List<Integer>> faces = workmesh.getFaces();
			HashMap<Integer, Integer> uniqueVertices = new HashMap<Integer, Integer>();
			FindAdjFace(faces, i, uniqueVertices, 1, i);
			Iterator<Integer> hashite = uniqueVertices.keySet().iterator();
			while(hashite.hasNext()) {
				adj.put(hashite.next());
			}
		}
		adj_locator.clear();
		adj.clear();

		return remapped;
	}
	
	
	
	
	public void Primitive(int i, AiMesh workmesh) {
		

		List<List<Integer>> faces = workmesh.getFaces();
		Vec3 pos0 = workmesh.getVertices().get(faces.get(i).get(0));
		Vec3 pos1 = workmesh.getVertices().get(faces.get(i).get(1));
		Vec3 pos2 = workmesh.getVertices().get(faces.get(i).get(2));
		Vec3 boundmin = utils.f3min(utils.f3min(pos1, pos2), pos0);
		Vec3 boundmax = utils.f3max(utils.f3max(pos1, pos2), pos0);

		int[] Alpha = utils.Sort(boundmin, BG.GlobalMin, BG.unitsize, new int[]{BG.xcount-1, BG.ycount-1, BG.zcount-1});
		int[] Beta = utils.Sort(boundmax, BG.GlobalMin, BG.unitsize, new int[]{BG.xcount-1, BG.ycount-1, BG.zcount-1});

		if(Beta[0]>=BG.xcount ||Beta[1]>=BG.ycount  ||Beta[2]>=BG.zcount || Alpha[0]<0 || Alpha[1]<0 || Alpha[2]<0) {
			utils.LOG("Bugged\n\n\n");
		}

		for(int X=Alpha[0]; X<=Beta[0]; X++) {
			for(int Y=Alpha[1]; Y<=Beta[1]; Y++) {
				for(int Z=Alpha[2]; Z<=Beta[2]; Z++) {

					voxels.get(X+Y*BG.xcount+Z*BG.xcount*BG.ycount).add(i);

				}
			}
		}

		
	}
	
	
	public IntBuffer IG;
	public FloatBuffer FG;

	
	public void GridToBuffer() {

		int size = 0;
		for(int i=0;i<voxels.size();i++) {
			ArrayList<Integer> OneGrid = voxels.get(i);
			size+=OneGrid.size();
		}
		IntBuffer GridBO = IntBuffer.allocate(size);
		utils.LOG("体素size: " + size);
		
		//capacity不是位深度....是float数量
		int helpersize = voxels.size() * (3+3+2);
		FloatBuffer GridHelper = FloatBuffer.allocate(helpersize+10);
		GridHelper.put(this.BG.GlobalMin.toFloatArray());
		GridHelper.put(this.BG.GlobalMax.toFloatArray());//6
		GridHelper.put(BG.unitsize);//1
		GridHelper.put(BG.xcount);//3
		GridHelper.put(BG.ycount);
		GridHelper.put(BG.zcount);
		for(int i=0;i<voxels.size();i++) {
			Vec3 location = BG.points[i];
			Vec3 BBMin = location.minus(BG.unitsize/2);
			Vec3 BBMax = location.plus(BG.unitsize/2);
			int start =  GridBO.position();
			int end = start + voxels.get(i).size();
			//因为voxels.get(i).size()索引向最后一位
			GridHelper.put(BBMin.toFloatArray());
			GridHelper.put(BBMax.toFloatArray());
			GridHelper.put(start);
			GridHelper.put(end);
			for(int j=0;j<voxels.get(i).size();j++) {
				int primitiveID = voxels.get(i).get(j);
				GridBO.put(primitiveID);
			}
		}
		
		GridBO.clear();
		GridHelper.clear();
		
		this.IG = GridBO;
		this.FG = GridHelper;
	}

}
