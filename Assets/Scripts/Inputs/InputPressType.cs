namespace AncientMemorial.Inputs {
	public enum InputPressType {
		Down, // the actual frame when value changed to 1
		Hold, // when input value is 1 (not actual frame)
		Up,   // the actual frame when input changed to 0
		None, // when input value is 0 (not actual frame)
		Value // having NOT-BOOL-Value
	}
}