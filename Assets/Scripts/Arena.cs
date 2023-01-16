using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Arena : MonoBehaviour {
	
	private static Arena instance = null;
	
	[SerializeField] private Transform[] points;
	
	private void Awake() { instance = this; }
	
	#if UNITY_EDITOR
		
		private void OnDrawGizmos() {
			if(Application.isPlaying) return;
			if(points == null) return;
			Gizmos.color = Color.yellow;
			
			for(int i = 0; i < points.Length; i++) {
				if(points[i] == null) continue;
				Gizmos.DrawSphere(points[i].position,1.0f);
			}
		}
		
	#endif
	
	public static Vector3 GetRandomPoint() {
		if(instance == null || instance.points == null || instance.points.Length == 0) return Vector3.zero;
		return instance.points[Random.Range(0,instance.points.Length)].position;
	}
}
