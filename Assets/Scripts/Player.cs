using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : Character {
	
	protected override void Update() {
		
		if(!IsDead) {
			Vector3 move_direction = Vector3.zero;
			
			if(GameControls.GetActionState(ControlAction.Left) > 0.0f) move_direction += Vector3.left;
			if(GameControls.GetActionState(ControlAction.Right) > 0.0f) move_direction += Vector3.right;
			if(GameControls.GetActionState(ControlAction.Up) > 0.0f) move_direction += Vector3.forward;
			if(GameControls.GetActionState(ControlAction.Down) > 0.0f) move_direction += Vector3.back;
			
			move(Vector3.Normalize(move_direction));
			Vector3 target_direction = Vector3.Normalize(GameControls.CursorHitPosition - CharacterPosition);
			rotateBody(target_direction);
			setWeaponTargetDirection(target_direction);
			
			if(GameControls.ClearActionState(ControlAction.Reload) > 0.0f) reloadWeapon();
			if(GameControls.ClearActionState(ControlAction.SwitchMode) > 0.0f) switchWeaponMode();
			
			float fire_state = 0.0f;
			switch(WeaponMode) {
				case WeaponMode.Single : fire_state = GameControls.ClearActionState(ControlAction.Fire); break;
				case WeaponMode.Auto : fire_state = GameControls.GetActionState(ControlAction.Fire); break;
			}
			
			if(fire_state > 0.0f) fire();
		}
		
		base.Update();
	}
}
