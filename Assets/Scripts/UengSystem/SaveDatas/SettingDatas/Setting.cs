using System;

namespace UengSystem.SaveDatas.SettingDatas {
	public class Setting : SaveData<Setting, SettingData, SettingType> {

		// 정적 프로퍼티
		public static bool Loaded = false;

		// 정적 메서드
		public static void Load() {
			Loaded = true;
			LoadFromFile();
			ApplyData();
		}

		public static void Save() {
			SaveToFile();
			ApplyData();
		}
		
		// float 전용 (음량 등)
		public static float GetFloat(SettingType type) {
			return type switch {
				SettingType.Master => dataFile.data.Master,
				SettingType.BGM    => dataFile.data.BGM,
				SettingType.SFX    => dataFile.data.SFX,
				_                  => throw new ArgumentException($"{type}은(는) float 타입 설정이 아니다.")
			};
		}
		
		public static bool GetBool(SettingType type) {
			return type switch {
				SettingType.SlowFX  => dataFile.data.SlowFX,
				SettingType.ShakeFX => dataFile.data.ShakeFX,
				SettingType.ZoomFX  => dataFile.data.ZoomFX,
				_                   => throw new ArgumentException($"{type}은(는) bool 타입 설정이 아니다.")
			};
		}
		
		public static string GetString(SettingType type) {
			return type switch {
				SettingType.playerName => dataFile.data.playerName,
				_                      => throw new ArgumentException($"{type}은(는) string 타입 설정이 아니다.")
			};
		}
		
		public static void SetValue(SettingType type, float value) {
			switch (type) {
				case SettingType.Master: dataFile.data.Master = value; break;
				case SettingType.BGM:    dataFile.data.BGM    = value; break;
				case SettingType.SFX:    dataFile.data.SFX    = value; break;
				default:
					throw new ArgumentException($"{type}은(는) float 타입 설정이 아니다.");
			}
		}
		
		public static void SetValue(SettingType type, bool value) {
			switch (type) {
				case SettingType.SlowFX:  dataFile.data.SlowFX  = value; break;
				case SettingType.ShakeFX: dataFile.data.ShakeFX = value; break;
				case SettingType.ZoomFX:  dataFile.data.ZoomFX  = value; break;
				default:
					throw new ArgumentException($"{type}은(는) bool 타입 설정이 아니다.");
			}
		}
		
		public static void SetValue(SettingType type, string value) {
			switch (type) {
				case SettingType.playerName: dataFile.data.playerName = value; break;
				default:
					throw new ArgumentException($"{type}은(는) string 타입 설정이 아니다.");
			}
		}
		
		public static void ApplyData() {
			float masterVolume = GameManager.valueToDB(dataFile.data.Master);
			float bgmVolume    = GameManager.valueToDB(dataFile.data.BGM);
			float sfxVolume    = GameManager.valueToDB(dataFile.data.SFX);
				
			GameManager.instance.audioMixer.SetFloat("Master", masterVolume);
			GameManager.instance.audioMixer.SetFloat("BGM",    bgmVolume);
			GameManager.instance.audioMixer.SetFloat("SFX",    sfxVolume);
		}
	}
}