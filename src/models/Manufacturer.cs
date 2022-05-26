using InfHelper.Models;
using rPrinterManager.src;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace rPrinterManager {
	public class Manufacturer {

		public List<ManufacturerEntity> manufacturerEntities = new List<ManufacturerEntity>();

		public Manufacturer(List<Key> manufacturer) {
			manufacturerEntities = manufacturer.Select(el =>
				new ManufacturerEntity(el.KeyValues[0].Value, el.KeyValues[1].Value)
			).ToList();
		}
	}
}
