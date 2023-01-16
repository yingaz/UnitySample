using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour {
	
	[SerializeField] private float damage = 10.0f;
	[SerializeField] private float speed = 60.0f;
	[SerializeField] private AmmoType type = AmmoType.Bullet;
	[SerializeField] private float lifeTime = 10.0f;
	
	private Character owner = null;
	private Vector3 direction = Vector3.forward;
	private float life_time = 0.0f;
	
	private bool check_hit(float distance,ref Character character,out Vector3 point) {
		point = Vector3.zero;
		RaycastHit hit;
		if(Physics.Raycast(transform.position,direction,out hit,distance,GameLayer.HitLayer)) {
			point = hit.point;
			GameObject game_object = hit.collider.gameObject;
			if(game_object.layer == GameLayer.Ground) {
				return true;
			} else if(game_object.layer == GameLayer.Character) {
				character = game_object.GetComponent<Character>();
				if(character == owner) return false;
				if(character.IsDead) return false;
				return true;
			}
		}
		return false;
	}
	
	private void clear() {
		life_time = 0.0f;
		owner = null;
		gameObject.SetActive(false);
		AmmoManager.Clear(this);
	}
	
	private void Update() {
		life_time += Time.deltaTime;
		if(life_time > lifeTime) {
			clear();
			return;
		}
		
		float distance = speed * Time.deltaTime;
		Character character = null;
		Vector3 point;
		if(check_hit(distance,ref character,out point)) {
			if(character != null) character.Hit(damage,owner);
			clear();
			// do some effects (explosion, blood, ...) here in the hit point
			return;
		}
		
		transform.position = transform.position + direction * distance;
	}
	
	public void Init(Vector3 position,Vector3 direction,Character owner,float dt) {
		this.owner = owner;
		this.direction = direction;
		transform.position = position + direction * dt * speed;
		transform.LookAt(position + direction);
		gameObject.SetActive(true);
	}
	
	public AmmoType Type { get { return type; } }
}
