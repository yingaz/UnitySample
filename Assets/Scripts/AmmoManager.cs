using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum AmmoType {
	Bullet = 0,
}

public class AmmoManager : MonoBehaviour {
	
	[System.Serializable]
	private class AmmoPrefab {
		
		private List<Bullet> pool = new List<Bullet>();
		
		public AmmoType Type;
		public Bullet Prefab;
		
		public Bullet Create() {
			Bullet result = null;
			if(pool.Count == 0) {
				result = GameObject.Instantiate(Prefab).GetComponent<Bullet>();
			} else {
				result = pool[pool.Count - 1];
				pool.RemoveAt(pool.Count - 1);
			}
			
			return result;
		}
		
		public void Clear(Bullet bullet) {
			pool.Add(bullet);
		}
	}
	
	private static AmmoManager instance = null;
	
	private void Awake() {
		instance = this;
	}
	
	/**
	 * The best way: set in a editor ammos array sorted by AmmoType.
	 * Then we can take prefab by cast enum to index
	 */
	[SerializeField,Tooltip("Just sort by AmmoType like: Bullet, ...")] private AmmoPrefab[] ammos = null;
	
	public static Bullet Create(AmmoType type) {
		if(instance == null) return null;
		return instance.ammos[(int)type].Create();
	}
	
	public static void Clear(Bullet bullet) {
		if(instance == null) return;
		instance.ammos[(int)bullet.Type].Clear(bullet);
	}
}
