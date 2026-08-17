using UengSystem.Settings;

namespace UengSystem.GameSaveDatas {
	public abstract class SaveData<INSTANCE, DATATYPE, ENUM> where INSTANCE:SaveData<INSTANCE, DATATYPE, ENUM>, new() {
		protected static UJSON<DATATYPE> dataFile;
		
		protected static void LoadFromFile() {
			dataFile?.LoadData();
			dataFile??=new UJSON<DATATYPE>($"{typeof(DATATYPE).Name}.json");
		}

		protected static void SaveToFile() {
			dataFile?.SaveData();
			dataFile??=new UJSON<DATATYPE>($"{typeof(DATATYPE).Name}.json");
		}
	}
}