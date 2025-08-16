package jogl8.ui;

import java.awt.event.KeyListener;
import java.awt.event.MouseAdapter;

import javax.swing.JFrame;

import com.jogamp.opengl.GLEventListener;
import com.jogamp.opengl.awt.GLCanvas;
import com.jogamp.opengl.util.FPSAnimator;

import jogl8.control;
import jogl8.sim.gridtrace;

public class testui_internalpause {
	public testui_internalpause() {
		FPSAnimator anm = new FPSAnimator(15);
		JFrame testframe = new JFrame("测试");
		testframe.setSize(control.WindowWidth, control.WindowHeight);
		GLCanvas testcanvas = new GLCanvas();
		interactives ic = new interactives();
		GLEventListener MAIN = new gridtrace(anm);
		interactives.AutoAnimationMouse drunkmouse = ic.new AutoAnimationMouse();
		interactives.StandardMouse standardmouse = ic.new StandardMouse();
		interactives.KeyboardListener standardkeyboard = ic.new KeyboardListener();
		interactives.debugKeyboard debugkeyboard = ic.new debugKeyboard();
		
		//....
		MouseAdapter  mouse = standardmouse;
		KeyListener keylistener = standardkeyboard;
		//....
		testcanvas.addKeyListener(keylistener);
		testcanvas.addMouseMotionListener(mouse);
		testcanvas.addMouseListener(mouse);
		
		testcanvas.addGLEventListener(MAIN);
		testframe.getContentPane().add(testcanvas);
		testframe.setVisible(true);
		anm.add(testcanvas);
		anm.start();
	}
}
