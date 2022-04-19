using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace rPrinterManager.src {
	class PrinterIssuer {

		public string issuer;
		public List<PrinterModel> printerModels;

		public PrinterIssuer(string issuer) {
			this.issuer = issuer;
			printerModels = new List<PrinterModel>();
		}

		public override string ToString() {
			return issuer;
		}

	}
}
