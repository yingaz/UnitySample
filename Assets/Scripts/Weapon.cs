using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum WeaponMode {
	Single,
	Auto,
}

public class Weapon : MonoBehaviour {
	
	[SerializeField] private Transform spawnTransform = null;
	[SerializeField] private AmmoType ammoType = AmmoType.Bullet;
	[SerializeField,Range(1,600)] private int rate = 10;
	[SerializeField,Range(0.0f,30.0f)] private float scatterAngle = 10.0f;
	[SerializeField] private int magazine = 60;
	[SerializeField] private float reloadTime = 0.5f;
	
	private Character owner = null;
	
	private WeaponMode mode = WeaponMode.Auto;
	private int ammo = 0;
	private Vector3 direction = Vector3.forward;
	
	private bool is_fire = false;
	private bool is_reloading = false;
	private float reload_time = 0.0f;
	float step_time = 0.0f;
	float spawn_time = 0.0f;
	
	private static Vector3 rotate_vector(Vector3 v,Vector3 axis,float angle) {
		Quaternion rotate_quat = Quaternion.AngleAxis(angle,axis);
		Matrix4x4 rotate_matrix = Matrix4x4.Rotate(rotate_quat);
		return rotate_matrix.MultiplyVector(v);
	}
	
	private void spawn_bullet(float dt) {
		Vector3 cross = Vector3.Cross(direction,spawnTransform.up);
		Vector3 dir = rotate_vector(rotate_vector(direction,cross,Random.Range(-scatterAngle,scatterAngle)),direction,Random.Range(0.0f,360.0f));
		Bullet bullet = AmmoManager.Create(ammoType);
		bullet.Init(spawnTransform.position,dir,owner,dt);
	}
	
	private void Awake() {
		ammo = magazine;
		step_time = 1.0f / (float)rate;
	}
	
	private void Update() {
		
		if(is_fire && !is_reloading && ammo > 0) {
			spawn_time += Time.deltaTime;
			switch(mode) {
				case WeaponMode.Single:
					spawn_bullet(spawn_time);
					spawn_time -= step_time;
					ammo--;
					break;
				case WeaponMode.Auto:
					while(spawn_time > 0.0f && ammo > 0) {
						spawn_bullet(spawn_time);
						spawn_time -= step_time;
						ammo--;
					}
					break;
			}
			is_fire = false;
			if(ammo == 0) spawn_time = 0.0f;
		} else {
			spawn_time = Mathf.Min(0.0f,spawn_time + Time.deltaTime);
		}
		
		if(is_reloading) {
			reload_time += Time.deltaTime;
			if(reload_time > reloadTime) {
				reload_time = 0.0f;
				is_reloading = false;
				ammo += owner.GetAmmo(ammoType,magazine - ammo);
			}
		}
	}
	
	public void Fire() {
		if(is_fire) return;
		if(is_reloading || ammo == 0) return;
		is_fire = true;
	}
	
	public void Reload() {
		if(is_reloading) return;
		if(ammo == magazine) return;
		reload_time = 0.0f;
		is_reloading = true;
	}
	
	public void SetDirection(Vector3 direction) {
		this.direction = direction;
	}
	
	public void SetOwner(Character owner) {
		this.owner = owner;
	}
	
	public void SwitchMode() {
		mode = mode == WeaponMode.Auto ? WeaponMode.Single : WeaponMode.Auto;
	}
	
	public WeaponMode Mode { get { return mode; } }
	public int Ammo { get { return ammo; } }
	public bool IsReloading { get { return is_reloading; } }
	public int Magazine { get { return magazine; } }
}
