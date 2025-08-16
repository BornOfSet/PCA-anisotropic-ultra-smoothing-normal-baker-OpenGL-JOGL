package jogl8.sim;

import java.nio.FloatBuffer;
import java.nio.IntBuffer;
import java.util.ArrayList;
import java.util.HashMap;
import java.util.Iterator;

import assimp.AiMesh;
import glm_.vec3.Vec3;
import jogl8.utils;

public class GridforPoints {
	public ArrayList<ArrayList<Integer>> voxels = new ArrayList<ArrayList<Integer>>();	
	
	public IntBuffer GridHelper;
	public IntBuffer GridBO;
	
	public GridforPoints(HashMap<Vec3, Integer> IndexRemap, buildgrid grid) {
		
		for(int m=0;m<grid.points.length;m++) {
			voxels.add(new ArrayList<Integer>());
		}
		
		int NumVertices = IndexRemap.size();
		utils.LOG(NumVertices + "顶点数量" + grid.points.length + "grid");
		Iterator<Vec3> keysetiterator = IndexRemap.keySet().iterator();
		
		//每次运行到while的时候都会重新执行表达式，你要必要它反复执行创造一堆keysetiterator，然后不断刷新
		while(keysetiterator.hasNext()) {
			Vec3 pos = keysetiterator.next();
			int[] loc = utils.Sort(pos, grid.GlobalMin, grid.unitsize, new int[]{grid.xcount-1, grid.ycount-1, grid.zcount-1});
			//Vec3 range = pos.minus(grid.GlobalMin);
			//Vec3 loc = range.div(grid.unitsize);
			/*int X = utils.Clean(loc.getX());
			X = Math.min(X, grid.xcount-1);
			X = Math.max(0, X);
			
			int Y = utils.Clean(loc.getY());
			Y = Math.min(Y, grid.ycount-1);
			Y = Math.max(0, Y);
			
			int Z = utils.Clean(loc.getZ());
			Z = Math.min(grid.zcount-1, Z);
			Z = Math.max(0, Z);*/
			//voxels.get(X+Y*grid.xcount+Z*grid.xcount*grid.ycount).add(IndexRemap.get(pos));
			voxels.get(loc[0]+loc[1]*grid.xcount+loc[2]*grid.xcount*grid.ycount).add(IndexRemap.get(pos));

		}
		
		
		GridBO = IntBuffer.allocate(NumVertices);
		GridHelper = IntBuffer.allocate(voxels.size() * 2);
		//capacity不是位深度....是float数量
		
				
		for(int i=0;i<voxels.size();i++) {

			int start =  GridBO.position();
			int end = start + voxels.get(i).size();

			GridHelper.put(start);
			GridHelper.put(end);
			for(int j=0;j<voxels.get(i).size();j++) {
				int whichVertex = voxels.get(i).get(j);
				GridBO.put(whichVertex);
			}
		}
		
		GridBO.clear();
		GridHelper.clear();
		
		
	}
}
