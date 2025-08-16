package jogl8.ui;

import java.awt.Point;
import java.awt.event.MouseEvent;

import glm_.vec3.Vec3;
import jogl8.control;
import jogl8.utils;

import java.awt.event.KeyEvent;
import java.awt.event.MouseAdapter;

public class interactives {
	
	public float sensitivity = 0.03f;
	
	//物体不具有惯性
	public class StandardMouse extends MouseAdapter{
		@Override
		public void mouseDragged(MouseEvent e) {
			Point coord = e.getLocationOnScreen();
			control.RealtimeLocation = coord;
		}
		
		@Override
		public void mousePressed(MouseEvent e) {
			Point coord = e.getLocationOnScreen();
			control.StartLocation = coord;
			control.RealtimeLocation = coord;
		}
		
		@Override
		public void mouseReleased(MouseEvent e) {
			control.StartLocation = new Point(0,0);
			control.RealtimeLocation = new Point(0,0);
		}
	}
	
	
	//物体具有惯性，单击空白处以停止
	//如果拖动后立刻松手，可以播放动画
	public class AutoAnimationMouse extends MouseAdapter{
		@Override
		public void mouseDragged(MouseEvent e) {
			Point coord = e.getLocationOnScreen();
			control.RealtimeLocation = coord;
		}
		
		@Override
		public void mousePressed(MouseEvent e) {
			Point coord = e.getLocationOnScreen();
			control.StartLocation = coord;
			control.RealtimeLocation = coord;
		}
		
		@Override
		public void mouseReleased(MouseEvent e) {
		}
	}
	
	
	//键盘
	public class KeyboardListener implements  java.awt.event.KeyListener{
		@Override
		public void keyTyped(KeyEvent e) {}
		@Override
		public void keyReleased(KeyEvent e) {}
		
		@Override
		public void keyPressed(KeyEvent e) {
			switch(e.getKeyCode()) {
			case 87: //OnPressedW();
				control.Translation = control.Translation.plus(new Vec3(0,0,sensitivity * 1));
				break;
			case 83: //OnPressedS();
				control.Translation = control.Translation.plus(new Vec3(0,0,sensitivity * -1));
				break;
			case 65: //OnPressedA();
				control.Translation = control.Translation.plus(new Vec3(sensitivity * -1,0,0));
				break;
			case 68: //OnPressedD();
				control.Translation = control.Translation.plus(new Vec3(sensitivity * 1,0,0));
				break;
			case 81: //OnPressedQ();
				control.Translation = control.Translation.plus(new Vec3(0,sensitivity * 1,0));
				break;
			case 69: //OnPressedE();
				control.Translation = control.Translation.plus(new Vec3(0,sensitivity * -1,0));
				break;
			case 32: //OnPressedSpace();
				control.Translation = new Vec3(0,0,0);
				control.HardScale = 1;
				break;
			case 38: //UpKey();
				control.Resize = control.Resize.plus(0.1,0.1,0);
				break;
			case 40: //DownKey();
				control.Resize = new Vec3(0.8);
				break;
			case 37: //LeftKey();
				control.HardScale -= 0.02;
				break;
			case 39: //RightKey();
				control.HardScale += 0.02;
				control.HardScale = Math.max(0.1f, control.HardScale);
				break;
			default:
				;
			}
		}
	}
	
	
	
	//特殊键盘--该键盘的按键仅做测试用途，应该改为按钮驱动
	public class debugKeyboard implements  java.awt.event.KeyListener{
		@Override
		public void keyTyped(KeyEvent e) {
			// TODO Auto-generated method stub
			
		}

		@Override
		public void keyPressed(KeyEvent e) {
			// TODO Auto-generated method stub
			switch(e.getKeyCode()) {
			case 49://on pressed 1
				control.StopAnimator = true;
				break;
			default:
				;
			}
		}

		@Override
		public void keyReleased(KeyEvent e) {
			// TODO Auto-generated method stub
			
		}
	}
	
}