using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public enum ControlAction {
	Left		= 0,
	Right		= 1,
	Up			= 2,
	Down		= 3,
	Fire		= 4,
	Reload		= 5,
	SwitchMode	= 6,
}

public static class GameControls {
	
	private abstract class ActionBind {
		public abstract float GetState();
	}
	
	private class ActionBindKeyboard : ActionBind {
		private KeyCode key;
		public ActionBindKeyboard(KeyCode key) { this.key = key; }
		public override float GetState() { return Input.GetKey(key) ? 1.0f : 0.0f; }
	}
	
	private class ActionState {
		
		private ActionBind[] binds;
		private float state = 0.0f;
		private float delta = 0.0f;
		private bool clear = false;
		
		public ActionState(ActionBind[] binds) { this.binds = binds; }
		
		public void Update() {
			float s = float.NegativeInfinity;
			foreach(ActionBind bind in binds) {
				s = Mathf.Max(s,bind.GetState());
			}
			if(s > 0.0f && clear) return;
			delta = s - state;
			state = s;
			clear = false;
		}
		
		public float GetState() { return state; }
		public float GetDelta() { return delta; }
		
		public float ClearState() {
			if(state < Mathf.Epsilon) return state;
			float s = state;
			state = 0.0f;
			clear = true;
			return s;
		}
	}
	
	private static ActionState[] states = new ActionState[] {
		new ActionState(new ActionBind[] { new ActionBindKeyboard(KeyCode.A) }),		// ControlAction.Left
		new ActionState(new ActionBind[] { new ActionBindKeyboard(KeyCode.D) }),		// ControlAction.Right
		new ActionState(new ActionBind[] { new ActionBindKeyboard(KeyCode.W) }),		// ControlAction.Up
		new ActionState(new ActionBind[] { new ActionBindKeyboard(KeyCode.S) }),		// ControlAction.Down
		new ActionState(new ActionBind[] { new ActionBindKeyboard(KeyCode.Mouse0) }),	// ControlAction.Fire
		new ActionState(new ActionBind[] { new ActionBindKeyboard(KeyCode.R) }),		// ControlAction.Reload
		new ActionState(new ActionBind[] { new ActionBindKeyboard(KeyCode.Q) }),		// ControlAction.SwitchMode
	};
		
	private static Vector3 cursor_hit_position = Vector3.zero;
	private static bool is_pointer_over_gui;
	private static bool is_enabled = true;
	
	private static bool intersect_ground_plane(Ray ray,out Vector3 point) {
		point = Vector3.zero;
		Vector3 ray_direction = ray.direction;
		Vector3 ray_point = ray.GetPoint(0.0f);
		if(Mathf.Abs(ray_direction.y) < Mathf.Epsilon) return false;
		
		float distance = -ray_point.y / ray_direction.y;
		if(distance < 0.0f) return false;
		
		point = ray_point + ray_direction * distance;
		return true;
	}
	
	public static void Update() {
		foreach(ActionState state in states) {
			state.Update();
		}
		
		RaycastHit hit;
		Ray ray = GameCamera.GetCurrentCamera().ScreenPointToRay(CusorPosition);
		if(Physics.Raycast(ray,out hit,100.0f,GameLayer.HitLayer)) {
			cursor_hit_position = hit.point;
			GameObject game_object = hit.collider.gameObject;
			if(game_object.layer == GameLayer.Character) {
				Character character = game_object.GetComponent<Character>();
				if(character != null) cursor_hit_position = character.CharacterCenter;
			}
		} else {
			Vector3 point = Vector3.zero;
			if(intersect_ground_plane(ray,out point)) cursor_hit_position = point;
		}
	}
	
	public static float GetActionState(ControlAction action) { return states[(int)action].GetState(); }
	public static float GetActionStateDelta(ControlAction action) { return states[(int)action].GetDelta(); }
	public static float ClearActionState(ControlAction action) { return states[(int)action].ClearState(); }
	
	public static void Enable() { is_enabled = true; }
	
	public static void Disable() {
		is_enabled = false;
		foreach(ActionState state in states) {
			state.ClearState();
		}
	}
	
	public static Vector3 CusorPosition { get { return Input.mousePosition; } }
	public static Vector3 CursorHitPosition { get { return cursor_hit_position; } }
	public static bool IsEnabled { get { return is_enabled; } }
}
