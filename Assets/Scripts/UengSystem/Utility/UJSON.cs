using System.IO;
using Newtonsoft.Json;
using UnityEngine;

namespace UengSystem.Settings {
	public class UJSON<T> {
		private string path;
		public  T      data;

		public static T LoadData(string path) {
			string _path = Path.Combine(Application.streamingAssetsPath, $"{typeof(T).Name}.json");
			string jsonText = File.ReadAllText(_path);
			
			return JsonConvert.DeserializeObject<T>(jsonText);
		}

		public UJSON(string path) {
			this.path = Path.Join(Application.streamingAssetsPath, $"{typeof(T).Name}.json");
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