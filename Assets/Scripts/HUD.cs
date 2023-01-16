using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HUD : MonoBehaviour {
	
	private class ValueChecker<TYPE> {
		
		public delegate void OnChange(TYPE v);
		
		private TYPE value = default;
		private OnChange callback = null;
		
		public ValueChecker(TYPE value,OnChange callback) {
			this.value = value;
			this.callback = callback;
			callback?.Invoke(this.value);
		}
		
		public void Check(TYPE v) {
			if(value.Equals(v)) return;
			value = v;
			callback?.Invoke(value);
		}
	}
	
	[SerializeField] private Player player = null;
	[SerializeField] private TMP_Text ammoText = null;
	[SerializeField] private TMP_Text weaponModeText = null;
	[SerializeField] private TMP_Text hpText = null;
	[SerializeField] private Button respawnButton = null;
	
	private ValueChecker<WeaponMode> weapon_mode_checker = null;
	private ValueChecker<int> hp_checker = null;
	private ValueChecker<bool> death_checker = null;
	private ValueChecker<int> ammo_checker = null;
	
	private void update_weapon_mode(WeaponMode mode) {
		weaponModeText.text = mode == WeaponMode.Auto ? "Auto" : "Single";
	}
	
	private void update_hp(int hp) {
		hpText.text = hp.ToString();
	}
	
	private void update_respawn_button(bool mode) {
		respawnButton.gameObject.SetActive(mode);
	}
	
	private void update_ammo(int ammo) {
		ammoText.text = string.Format("{0}/{1}",player.GetWeaponAmmo(),player.GetWeaponMagazine());
	}
	
	private void on_respawn_click() {
		if(player == null) return;
		player.Respawn();
	}
	
	private void Awake() {
		respawnButton.onClick.AddListener(on_respawn_click);
		if(player == null) return;
		
		weapon_mode_checker = new ValueChecker<WeaponMode>(player.WeaponMode,update_weapon_mode);
		hp_checker = new ValueChecker<int>(Mathf.FloorToInt(player.HP),update_hp);
		death_checker = new ValueChecker<bool>(player.IsDead,update_respawn_button);
		ammo_checker = new ValueChecker<int>(player.GetWeaponAmmo(),update_ammo);
	}
	
	private void Update() {
		if(player == null) return;
		
		weapon_mode_checker.Check(player.WeaponMode);
		hp_checker.Check(Mathf.FloorToInt(player.HP));
		death_checker.Check(player.IsDead);
		ammo_checker.Check(player.GetWeaponAmmo());
	}
}
