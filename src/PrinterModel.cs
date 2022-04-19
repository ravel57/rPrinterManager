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

		public PrinterModel(string driverShortName, string defaultName, string driverFullName, string driverInfFilePath) {
			this.driverShortName = driverShortName;
			this.defaultName = defaultName;
			this.driverFullName = driverFullName;
			this.driverInfFilePath = driverInfFilePath;
		}

		public override int GetHashCode() {
			return HashCode.Combine(this.driverFullName);
		}

	}
}
