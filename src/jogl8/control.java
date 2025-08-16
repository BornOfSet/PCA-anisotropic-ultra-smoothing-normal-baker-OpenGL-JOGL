package jogl8;

import java.awt.Point;

import glm_.vec3.Vec3;

public class control {
	public static Point StartLocation = new Point(0,0);
	public static Point RealtimeLocation = new Point(0,0);
	public static Vec3 Translation = new Vec3(0,0,0);
	public static Vec3 Resize = new Vec3(0.8);
	public static int WindowWidth = 1000;
	public static int WindowHeight = 500;
	public static float HardScale = 1;
	public static boolean StopAnimator = false;
	public static boolean RenewGeometryShader = false;
}
