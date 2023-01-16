using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class GameCamera : MonoBehaviour {
	
	private static readonly Vector3 camera_direction = Vector3.forward;
	private static Camera current_camera = null;
	
	[SerializeField] private Player target = null;
	[SerializeField] private float distance = 10.0f;
	[SerializeField] private float height = 10.0f;
	[SerializeField] private float cameraSpeed = 5.0f;
	
	private Vector3 max_offset = new Vector3(10.0f,0.0f,7.0f);
	private Vector3 offset = Vector3.zero;
	
	private void look_at(Vector3 target_position,bool force = false) {
		Vector3 camera_target_position = target_position - camera_direction * distance;
		camera_target_position.y = target_position.y + height;
		Vector3 camera_position = force ? camera_target_position : Vector3.Lerp(transform.position,camera_target_position,cameraSpeed * Time.deltaTime);
		
		float camera_height = camera_position.y - target_position.y;
		float camera_distance = Vector3.Dot(target_position - camera_position,camera_direction);
		float pitch = Mathf.Clamp(Mathf.Atan2(camera_height,camera_distance) * Mathf.Rad2Deg,0.0f,90.0f);
		Quaternion camera_target_rotation = Quaternion.Euler(pitch,0.0f,0.0f);
		transform.position = camera_position;
		transform.rotation = camera_target_rotation;
	}
	
	private void Awake() {
		current_camera = GetComponent<Camera>();
	}
	
	private void Start() {
		if(target == null) return;
		look_at(target.CharacterPosition,true);
	}
	
	private void LateUpdate() {
		if(target == null) return;
		
		Vector3 new_offset = Vector3.zero;
		
		if(!target.IsDead) {
			Vector3 v = GameControls.CusorPosition;
			float hx = 0.5f * Screen.width;
			float hy = 0.5f * Screen.height;
			float x = (Mathf.Clamp(v.x,0.0f,Screen.width) - hx) / hx;
			float y = (Mathf.Clamp(v.y,0.0f,Screen.height) - hy) / hy;
			
			new_offset = new Vector3(max_offset.x * x,max_offset.y,max_offset.z * y);
		}
		
		offset = Vector3.Lerp(offset,new_offset,cameraSpeed * Time.deltaTime);
		look_at(target.CharacterPosition + offset);
	}
	
	public static Camera GetCurrentCamera() { return current_camera; }
}
