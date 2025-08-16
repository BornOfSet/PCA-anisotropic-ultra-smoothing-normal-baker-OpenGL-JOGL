package jogl8.ui;

import java.awt.event.MouseAdapter;

import javax.swing.JFrame;
import com.jogamp.opengl.awt.GLCanvas;
import com.jogamp.opengl.util.FPSAnimator;

import jogl8.sim.gridtrace;

public class testui_interactive {
	public testui_interactive() {
		FPSAnimator anm = new FPSAnimator(30);
		JFrame testframe = new JFrame("测试");
		testframe.setSize(500, 500);
		GLCanvas testcanvas = new GLCanvas();
		interactives ic = new interactives();
		interactives.AutoAnimationMouse drunkmouse = ic.new AutoAnimationMouse();
		interactives.StandardMouse standardmouse = ic.new StandardMouse();
		interactives.KeyboardListener standardkeyboard = ic.new KeyboardListener();
		MouseAdapter  mouse = standardmouse;//here
		testcanvas.addKeyListener(standardkeyboard);
		testcanvas.addMouseMotionListener(mouse);
		testcanvas.addMouseListener(mouse);
		testcanvas.addGLEventListener(new gridtrace());
		testframe.getContentPane().add(testcanvas);
		testframe.setVisible(true);
		anm.add(testcanvas);
		anm.start();
	}
}
