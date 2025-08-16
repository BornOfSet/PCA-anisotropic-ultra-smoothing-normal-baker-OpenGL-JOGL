package jogl8.sim;

import java.util.ArrayList;
import java.util.HashMap;
import java.util.Iterator;
import java.util.List;

import assimp.AiMesh;
import glm_.vec3.Vec3;

public class Connectivity {
	
	int PointCloudVolume = 5;
	
	public void Scan(List<List<Integer>> faces ,Vec3 pos, ArrayList<Integer> exclude, int lv, HashMap<Vec3, ArrayList<Integer>> TriCon) {
		if(lv>PointCloudVolume) {return;}
		//List<Integer> face = faces.get(t);

		
	}

	public Connectivity(AiMesh workmesh, HashMap<Vec3, Integer> IndexRemap, HashMap<Vec3, ArrayList<Integer>> TriCon) {
		Iterator<Vec3> keysetiterator = IndexRemap.keySet().iterator();
		
		
		while(keysetiterator.hasNext()) {
			//Our position
			ArrayList<Integer> exclude = new ArrayList<Integer>();
			Vec3 vertexPos = keysetiterator.next();
			
			exclude.add(vertexPos.hashCode());
			Scan(workmesh.getFaces(), vertexPos, exclude, 0, TriCon);
			
		}
	}
}
