using System.IO;
using Newtonsoft.Json;
using UnityEngine;

namespace UengSystem.Utility {
	public class UJSON<T> {

		// 인스턴스 프로퍼티
		private string path;
		public  T      data;

		// 정적 메서드
		public static T LoadData(string path) {
			string _path = Path.Combine(Application.streamingAssetsPath, path);
			string jsonText = File.ReadAllText(_path);
			
			return JsonConvert.DeserializeObject<T>(jsonText);
		}

		// 인스턴스 메서드
		public UJSON(string path) {
			this.path = Path.Join(Application.streamingAssetsPath, path);
			LoadData();
		}
		
		public T LoadData() {
			string jsonText = File.ReadAllText(path);
			data = JsonConvert.DeserializeObject<T>(jsonText);
			return data;
		}

		public void SaveData() {
			File.WriteAllText(path, JsonConvert.SerializeObject(data));
		}
	}
}