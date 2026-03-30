using System.Collections.Generic;
using AncientMemorial;
using AncientMemorial.Entities;
using UnityEngine;

namespace AncientMemorial {
	public class DataStorage : Manager<DataStorage> {

		public List<EntityData> Entities;
		public Dictionary<string, LayerMask> layerMask = new () {
			{ "Default", 0   },
			{ "Map"    , 3   },
		};
		
		public override void ManagerUpdate() { }
		public override void ManagerFixedUpdate() { }
	}
}