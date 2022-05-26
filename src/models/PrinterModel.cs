using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace rPrinterManager {
	public class PrinterModel {

		public string driverShortName;
		public string driverFullName;
		public string driverInfFilePath;
		public string defaultName;

		public override string ToString() => driverShortName;
		
		[JsonConstructor]
		public PrinterModel(string driverShortName, string defaultName, string driverFullName, string driverInfFilePath) {
			this.driverShortName = driverShortName;
			this.defaultName = defaultName;
			this.driverFullName = driverFullName;
			this.driverInfFilePath = driverInfFilePath;
		}

		public PrinterModel(string driverName, string driverInfFilePath) {
			this.driverShortName = driverName.Replace(" PCL 6", "").Replace(" PS", "").Replace(" KX", "");
			this.defaultName = driverName.Replace(" PCL 6", "").Replace(" PS","").Replace(" KX","");
			this.driverFullName = driverName;
			this.driverInfFilePath = driverInfFilePath;
		}

	}
}
