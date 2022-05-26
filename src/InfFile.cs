using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace rPrinterManager.src {
	public class InfFile {
		public List<PrinterIssuer> printerIssuers = new List<PrinterIssuer>();
		public string path;

		public InfFile(string file) {
			this.path = file;
		}

	}
}
