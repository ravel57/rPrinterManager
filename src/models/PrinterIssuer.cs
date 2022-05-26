using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace rPrinterManager.src {
	public class PrinterIssuer {

		public string issuer;
		public List<PrinterModel> printerModels = new List<PrinterModel>();

		public override string ToString() => issuer;

		public PrinterIssuer(string issuer) {
			this.issuer = issuer;
		}

	}
}
