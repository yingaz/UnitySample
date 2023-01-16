using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody),typeof(CapsuleCollider))]
public abstract class Character : MonoBehaviour {
	
	private class CharacterAnimatorController {
		
		public enum MovementMode {
			None	= 0,
			Forward	= 1 << 0,
			Back	= 1 << 1,
			Left	= 1 << 2,
			Right	= 1 << 3,
		}
		
		private static readonly int turn_id = Animator.StringToHash("Turn");
		private static readonly int death_id = Animator.StringToHash("Death");
		
		private static readonly KeyValuePair<byte,int>[] modes = new KeyValuePair<byte,int>[] {
			new KeyValuePair<byte,int>((byte)MovementMode.Forward,Animator.StringToHash("Forward")),
			new KeyValuePair<byte,int>((byte)MovementMode.Back,Animator.StringToHash("Back")),
			new KeyValuePair<byte,int>((byte)MovementMode.Left,Animator.StringToHash("Left")),
			new KeyValuePair<byte,int>((byte)MovementMode.Right,Animator.StringToHash("Right")),
		};
		
		private Animator animator = null;
		
		public CharacterAnimatorController (Animator animator) {
			this.animator = animator;
		}
		
		public void SetMovementMode(MovementMode mode) {
			if(animator == null) return;
			byte mask = (byte)mode;
			for(int i = 0; i < modes.Length; i++) {
				KeyValuePair<byte,int> pair = modes[i];
				animator.SetBool(pair.Value,(mask & pair.Key) > 0);
			}
		}
		
		public void SetIdleRotation(bool mode) {
			if(animator == null) return;
			animator.SetBool(turn_id,mode);
		}
		
		public void SetDeath(bool mode) {
			if(animator == null) return;
			animator.SetBool(death_id,mode);
			if(mode) {
				SetMovementMode(MovementMode.None);
				SetIdleRotation(false);
			}
		}
	}
	
	[SerializeField] private float health = 100.0f;
	[SerializeField] private float moveSpeed = 5.0f;
	[SerializeField] private float rotationSpeed = 360.0f;
	[SerializeField] private Transform rotationTransform = null;
	[SerializeField] private Transform hand = null;
	[SerializeField] private Weapon defaultWeaponPrefab = null;
	
	private float hp = 0.0f;
	
	private Vector3 move_direction = Vector3.zero;
	private Vector3 body_direction = Vector3.forward;
	
	private Rigidbody body = null;
	private CapsuleCollider character_collider = null;
	private float current_body_angle = 0.0f;
	private CharacterAnimatorController animator_controller = null;
	
	private Weapon weapon = null;
	
	private bool rotate_body(float target_body_angle,float speed) {
		if(Mathf.Abs(current_body_angle - target_body_angle) < Mathf.Epsilon) return true;
		current_body_angle = Mathf.MoveTowardsAngle(current_body_angle,target_body_angle,speed * Time.deltaTime);
		rotationTransform.rotation = Quaternion.AngleAxis(current_body_angle,Vector3.up);
		return Mathf.Abs(current_body_angle - target_body_angle) < Mathf.Epsilon;
	}
	
	protected void move(Vector3 direction) {
		move_direction = direction;
	}
	
	protected void rotateBody(Vector3 direction) {
		body_direction = direction;
	}
	
	protected void setWeaponTargetDirection(Vector3 direction) {
		if(weapon == null) return;
		weapon.SetDirection(direction);
	}
	
	protected void fire() {
		if(weapon == null) return;
		weapon.Fire();
	}
	
	protected void reloadWeapon() {
		if(weapon == null) return;
		weapon.Reload();
	}
	
	protected void switchWeaponMode() {
		if(weapon == null) return;
		weapon.SwitchMode();
	}
	
	protected virtual void onDead() {
		animator_controller.SetDeath(true);
	}
	
	protected virtual void onRespawn() {
		if(defaultWeaponPrefab != null) SetWeapon(Instantiate(defaultWeaponPrefab).GetComponent<Weapon>());
	}
	
	protected virtual void onHit(Character from) {
		
	}
	
	protected virtual void Awake() {
		hp = health;
		body = GetComponent<Rigidbody>();
		character_collider = GetComponent<CapsuleCollider>();
		animator_controller = new CharacterAnimatorController(GetComponentInChildren<Animator>());
		if(defaultWeaponPrefab != null) SetWeapon(Instantiate(defaultWeaponPrefab).GetComponent<Weapon>());
		// set random position on scene load
		Vector3 spawn_point = Arena.GetRandomPoint();
		transform.position = spawn_point;
		body.position = spawn_point;
	}
	
	protected virtual void Update() {
		
		if(IsDead) return;
		
		float target_body_angle = Vector3.SignedAngle(Vector3.forward,body_direction,Vector3.up);
		animator_controller.SetIdleRotation(Mathf.Abs(current_body_angle - target_body_angle) > 0.5f);
		
		if(Vector3.SqrMagnitude(move_direction) > Mathf.Epsilon) {
			body.MovePosition(body.position + move_direction * moveSpeed * Time.deltaTime);
			CharacterAnimatorController.MovementMode mode = CharacterAnimatorController.MovementMode.None;
			float a = Mathf.Rad2Deg * Mathf.Acos(Mathf.Clamp(Vector3.Dot(move_direction,body_direction),0.0f,1.0f - Mathf.Epsilon)) * Mathf.Sign(Vector3.Cross(move_direction,body_direction).y);
			if(a >= -135.0f && a < -45.0f) mode = CharacterAnimatorController.MovementMode.Right;
			else if(a >= -45.0f && a < 45.0f) mode = CharacterAnimatorController.MovementMode.Forward;
			else if(a >= 45.0f && a < 135.0f) mode = CharacterAnimatorController.MovementMode.Left;
			else mode = CharacterAnimatorController.MovementMode.Back;
			animator_controller.SetMovementMode(mode);
		} else {
			animator_controller.SetMovementMode(CharacterAnimatorController.MovementMode.None);
		}
		
		rotate_body(target_body_angle,rotationSpeed);
	}
	
	public WeaponMode WeaponMode {
		get {
			if(weapon == null) return WeaponMode.Auto;
			return weapon.Mode;
		}
	}
	
	public Vector3 CharacterCenter { get { return body.position + new Vector3(0.0f,character_collider.height * 0.5f,0.0f); } }
	public Vector3 CharacterPosition { get { return body.position; } }
	public Vector3 TargetBodyDirection { get { return body_direction; } }
	public Vector3 MoveDirection { get { return move_direction; } }
	public Vector3 CurrentDirection { get { return rotationTransform.forward; } }
	public bool IsDead { get { return hp <= 0.0f; } }
	public float HP { get { return hp; } }
	public float MaxHP { get { return health; } }
	public float CharacterRadius { get { return character_collider.radius; } }
	
	public void SetWeapon(Weapon weapon) {
		
		if(this.weapon != null) {
			this.weapon.SetOwner(null);
			Destroy(this.weapon.gameObject);
			this.weapon = null;
		}
		
		this.weapon = weapon;
		weapon.SetOwner(this);
		weapon.transform.SetParent(hand);
		weapon.transform.localPosition = Vector3.zero;
		weapon.transform.LookAt(hand.transform.position + hand.up,-hand.right);
	}
	
	// the infinite ammo
	public int GetAmmo(AmmoType type,int count) {
		return count;
	}
	
	public void Hit(float damage,Character from) {
		if(IsDead) return;
		onHit(from);
		hp -= damage;
		if(IsDead) onDead();
	}
	
	public int GetWeaponMagazine() {
		if(weapon == null) return 0;
		return weapon.Magazine;
	}
	
	public int GetWeaponAmmo() {
		if(weapon == null) return 0;
		return weapon.Ammo;
	}
	
	public void Respawn() {
		hp = health;
		animator_controller.SetDeath(false);
		Vector3 spawn_point = Arena.GetRandomPoint();
		transform.position = spawn_point;
		body.position = spawn_point;
		onRespawn();
	}
}
