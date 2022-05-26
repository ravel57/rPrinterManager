using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace rPrinterManager {
	public class Version {

		public string deviceClass;
		public string Provider;

		public Version(InfHelper.Models.Category version) {
			deviceClass = version["Class"].PrimitiveValue;
			Provider = version["Provider"].PrimitiveValue;
		}


	}
}
