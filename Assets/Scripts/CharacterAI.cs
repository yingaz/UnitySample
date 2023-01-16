using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class CharacterAI : Character {
	
	private class AutoMovement {
		
		public enum MoveResult {
			Done,
			Fail,
		}
		
		public delegate void OnMoveEnd(MoveResult result);
		
		private static readonly float destination_eps = 0.1f;
		
		private CharacterAI character = null;
		private Vector3 destination = Vector3.zero;
		private OnMoveEnd on_move_end_callback = null;
		
		private NavMeshPath path = new NavMeshPath();
		private NavMeshQueryFilter filter = new NavMeshQueryFilter() { areaMask = ~0, agentTypeID = 0 };
		private int current_index = -1;
		
		private bool is_moving = false;
		
		private void calc_path() {
			current_index = -1;
			NavMesh.CalculatePath(character.CharacterPosition,destination,filter,path);
		}
		
		public AutoMovement(CharacterAI character) {
			this.character = character;
		}
		
		public void MoveTo(Vector3 destination,OnMoveEnd callback) {
			on_move_end_callback = callback;
			this.destination = destination;
			calc_path();
			is_moving = true;
		}
		
		public void Update() {
			if(!is_moving) return;
			if(path.status != NavMeshPathStatus.PathComplete || path.corners.Length == 0) {
				is_moving = false;
				on_move_end_callback?.Invoke(MoveResult.Fail);
				return;
			}
			
			if(current_index < 0) current_index = 0;
			
			Vector3 position = character.CharacterPosition;
			Vector3 direction = Vector3.forward;
			while(true) {
				Vector3 point = path.corners[current_index];
				direction = point - position;
				
				if(Vector3.SqrMagnitude(direction) > destination_eps) break;
				current_index++;
				if(current_index >= path.corners.Length) {
					is_moving = false;
					on_move_end_callback?.Invoke(MoveResult.Done);
					return;
				}
			}
			
			character.move(Vector3.Normalize(direction));
		}
		
		public void Stop() {
			is_moving = false;
			current_index = -1;
			on_move_end_callback = null;
			character.move(Vector3.zero);
		}
	}
	
	private static RaycastHit[] visiable_hits = new RaycastHit[3];
	private static Collider[] look_colliders = new Collider[3];
	
	[SerializeField] private float respawnTime = 3.0f;
	[SerializeField] private float lookingRadius = 15.0f;
	[SerializeField] private float visiableDistance = 15.0f;
	
	private AutoMovement auto_movement = null;
	private float respawn_timer = 0.0f;
	
	private Character attack_target = null;
	
	private void on_move_end(AutoMovement.MoveResult result) {
		auto_movement.MoveTo(Arena.GetRandomPoint(),on_move_end);
	}
	
	private bool is_visiable(Character character) {
		Vector3 direction = Vector3.Normalize(character.CharacterCenter - CharacterCenter);
		RaycastHit hit;
		if(Physics.Raycast(CharacterCenter + direction * CharacterRadius,direction,out hit,visiableDistance,GameLayer.HitLayer)) {
			if(hit.collider.gameObject == character.gameObject) return true;
		}
		
		return false;
	}
	
	private void looking_target() {
		int count = Physics.OverlapSphereNonAlloc(CharacterPosition,lookingRadius,look_colliders,1 << GameLayer.Character);
		for(int i = 0; i < count; i++) {
			if(look_colliders[i].gameObject == gameObject) continue;
			Character character = look_colliders[i].gameObject.GetComponent<Character>();
			if(character == null || character.IsDead) continue;
			attack_target = character;
			return;
		}
	}
	
	private bool in_target(Vector3 direction) {
		Vector3 current_dir = CurrentDirection;
		current_dir.y = 0.0f;
		current_dir.Normalize();
		
		direction.y = 0.0f;
		direction.Normalize();
		
		float a = Mathf.Rad2Deg * Mathf.Acos(Mathf.Clamp(Vector3.Dot(current_dir,direction),0.0f,1.0f - Mathf.Epsilon)) * Mathf.Sign(Vector3.Cross(current_dir,direction).y);
		return a < 5.0f;
	}
	
	protected override void onDead() {
		base.onDead();
		respawn_timer = 0.0f;
		auto_movement.Stop();
		attack_target = null;
	}
	
	protected override void onRespawn() {
		base.onRespawn();
		auto_movement.MoveTo(Arena.GetRandomPoint(),on_move_end);
	}
	
	protected override void onHit(Character from) {
		base.onHit(from);
		attack_target = from;
	}
	
	protected override void Awake() {
		base.Awake();
		auto_movement = new AutoMovement(this);
		auto_movement.MoveTo(Arena.GetRandomPoint(),on_move_end);
	}
	
	protected override void Update() {
		auto_movement.Update();
		
		if(IsDead) {
			respawn_timer += Time.deltaTime;
			if(respawn_timer > respawnTime) Respawn();
		} else {
			if(attack_target != null) {
				if(attack_target.IsDead) {
					attack_target = null;
					rotateBody(MoveDirection);
				} else {
					if(is_visiable(attack_target)) {
						Vector3 attack_direction = Vector3.Normalize(attack_target.CharacterPosition - CharacterPosition);
						if(in_target(attack_direction)) {
							setWeaponTargetDirection(attack_direction);
							fire();
						}
						rotateBody(attack_direction);
						if(GetWeaponAmmo() == 0) reloadWeapon();
					} else {
						attack_target = null;
						rotateBody(MoveDirection);
					}
				}
			} else {
				looking_target();
				reloadWeapon();
				rotateBody(MoveDirection);
			}
		}
		
		base.Update();
	}
}
