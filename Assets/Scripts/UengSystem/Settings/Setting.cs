using System;
using System.IO;
using AncientMemorial;
using Newtonsoft.Json;
using UnityEngine;

namespace UengSystem.Settings {
	public class Setting {
		public static SettingData data;

		public static string GetValueString(SettingType settingType) {
			return settingType switch {
				SettingType.playerName   => data.playerName,
				_                        => throw new ArgumentOutOfRangeException(nameof(settingType), settingType, "NAHHHH")
			};
		}
		
		public static float GetValueFloat(SettingType settingType) {
			return settingType switch {
				SettingType.Master => data.Master,
				SettingType.BGM => data.BGM,
				SettingType.SFX => data.SFX,
				_ => throw new ArgumentOutOfRangeException(nameof(settingType), settingType, "NAHHHH")
			};
		}

		public static void SetValueString(SettingType settingType, string value) {
			
			Debug.Log($"[SETTING] {settingType} changed to {value}");
			
			switch (settingType) {
				case SettingType.playerName:
					data.playerName = value;
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(settingType), settingType, "NAHHHH");
			}
		}

		public static void SetValueFloat(SettingType settingType, float value) {
			
			Debug.Log($"[SETTING] {settingType} changed to {value}");
			
			switch (settingType) {
				case SettingType.Master:
					data.Master = value;
					break;
				case SettingType.BGM:
					data.BGM = value;
					break;
				case SettingType.SFX:
					data.SFX = value;
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(settingType), settingType, "NAHHHH");
			}
		}
		
		public static void LoadData() {
			string path = Path.Combine(Application.streamingAssetsPath, $"SettingData.json");
			string jsonText = File.ReadAllText(path);
			
			data = JsonConvert.DeserializeObject<SettingData>(jsonText);
			ApplyData();
		}

		public static void SaveData() {
			string path     = Path.Combine(Application.streamingAssetsPath, $"SettingData.json");
			File.WriteAllText(path, JsonConvert.SerializeObject(data));
			
			ApplyData();
		}

		public static void ApplyData() {
			float masterVolume = GameManager.valueToDB(data.Master);
			float bgmVolume    = GameManager.valueToDB(data.BGM);
			float sfxVolume    = GameManager.valueToDB(data.SFX);
				
			GameManager.instance.audioMixer.SetFloat("Master", masterVolume);
			GameManager.instance.audioMixer.SetFloat("BGM",    bgmVolume);
			GameManager.instance.audioMixer.SetFloat("SFX",    sfxVolume);
		}
	}
}