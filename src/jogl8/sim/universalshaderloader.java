package jogl8.sim;

import java.io.FileReader;
import java.io.IOException;
import java.nio.ByteBuffer;
import java.util.Scanner;

import com.jogamp.opengl.GL3;

import jogl8.utils;

public class universalshaderloader {

	public static String LoadShader(String  path) throws IOException {
		Scanner scan= new Scanner(new FileReader(path));
		String str = "";
		try(scan) {
			while(scan.hasNextLine()) {
				str = str  + scan.nextLine()+"\n";
			}
		}
		return str;
	}
	
	public static void Compile(GL3 gl , int what, String additionalinfo) {
		gl.glCompileShader(what);
		ByteBuffer b = ByteBuffer.allocate(512);
		gl.glGetShaderInfoLog(what, 512, null, b);
		byte[] ba = b.array();
		utils.log(additionalinfo + " Compile Error:  " + new String(ba));
	}
	
	//将特定的着色器绑定到给定的程序上
	public static void SetPipeline(GL3 gl, int shadingprogram , String VSfile , String GSfile , String FSfile) {
		String[] vsrc = new String[1];
		String[] gsrc = new String[1];
		String[] fsrc = new String[1];
		try {
			vsrc[0] = LoadShader(VSfile);
			gsrc[0] = LoadShader(GSfile);
			fsrc[0] = LoadShader(FSfile);
		} catch (IOException e) {
			e.printStackTrace();
		}
		int vs = gl.glCreateShader(GL3.GL_VERTEX_SHADER);
		int gs = gl.glCreateShader(GL3.GL_GEOMETRY_SHADER);
		int fs = gl.glCreateShader(GL3.GL_FRAGMENT_SHADER);
		gl.glShaderSource(vs, 1, vsrc, null);
		gl.glShaderSource(gs, 1, gsrc, null);
		gl.glShaderSource(fs, 1, fsrc, null);
		Compile(gl, vs, "Vertex   	  ");
		Compile(gl, gs, "Geometry	  ");
		Compile(gl, fs, "Fragment	  ");
		gl.glAttachShader(shadingprogram, vs);
		gl.glAttachShader(shadingprogram, gs);
		gl.glAttachShader(shadingprogram, fs);
	    gl.glLinkProgram(shadingprogram);
	}
	
	
	public static void SetPipeline(GL3 gl, int shadingprogram , String VSfile , String FSfile) {
		String[] vsrc = new String[1];
		String[] fsrc = new String[1];
		try {
			vsrc[0] = LoadShader(VSfile);
			fsrc[0] = LoadShader(FSfile);
		} catch (IOException e) {
			e.printStackTrace();
		}
		int vs = gl.glCreateShader(GL3.GL_VERTEX_SHADER);
		int fs = gl.glCreateShader(GL3.GL_FRAGMENT_SHADER);
		gl.glShaderSource(vs, 1, vsrc, null);
		gl.glShaderSource(fs, 1, fsrc, null);
		Compile(gl, vs, "Vertex   	  ");
		Compile(gl, fs, "Fragment	  ");
		gl.glAttachShader(shadingprogram, vs);
		gl.glAttachShader(shadingprogram, fs);
	    gl.glLinkProgram(shadingprogram);
	}
	
}
