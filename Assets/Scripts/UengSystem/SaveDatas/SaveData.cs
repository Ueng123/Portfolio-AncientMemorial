using UengSystem.Utility;

namespace UengSystem.SaveDatas {
	public abstract class SaveData<INSTANCE, DATATYPE, ENUM> where INSTANCE:SaveData<INSTANCE, DATATYPE, ENUM>, new() {

		// 정적 프로퍼티
		protected static UJSON<DATATYPE> dataFile;

		// 정적 메서드
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