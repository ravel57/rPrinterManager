using rPrinterManager.src;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace rPrinterManager {
	public class ManufacturerEntity {

		public PrinterIssuer printerIssuer;
		public string strToFind;

		public ManufacturerEntity(string printerIssuer, string str2) {
			this.printerIssuer = new PrinterIssuer(printerIssuer);
			this.strToFind = new StringBuilder(printerIssuer).Append('.').Append(str2).ToString();
		}
	}
}
