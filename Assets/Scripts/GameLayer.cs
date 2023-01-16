using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GameLayer {
	public static readonly int Character = 6;
	public static readonly int Ground = 7;
	public static readonly int HitLayer = 1 << GameLayer.Character | 1 << GameLayer.Ground;
}
