using InfHelper;
using InfHelper.Models;
using rPrinterManager.src;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace rPrinterManager {
	public class NetworkPrinterInf {

		private Version version;
		public bool classIsPrinter { get { return version.deviceClass == "Printer"; } }
		public List<PrinterIssuer> printerIssuers = new List<PrinterIssuer>();

		public NetworkPrinterInf(string file) {
			InfData data = new InfUtil().ParseFile(file);
			version = new Version(data["Version"]);
			if (version.deviceClass != "Printer") {
				return;
			}
			Manufacturer manufacturer = new Manufacturer(data["Manufacturer"].Keys);
			foreach (ManufacturerEntity manufacturerEntity in manufacturer.manufacturerEntities) {
				List<string> driversNames;
				if (version.Provider == "%OEM%") {
					string subStr = data.Categories.First(el => el.Name == manufacturerEntity.strToFind).Keys[0].Id.Replace("%", "");
					driversNames = data["Strings"].Keys.Where(el => el.Id.StartsWith(subStr)).Select(el => el.KeyValues[0].Value).ToList();
				} else {
					driversNames = data[manufacturerEntity.strToFind].Keys.Select(el => el.KeyValues[0].Value).ToList();
				}

				manufacturerEntity.printerIssuer.printerModels = driversNames.Select(driver => new PrinterModel(driver, file)).ToList();
				manufacturerEntity.printerIssuer.printerModels.Sort((x, y) => x.driverFullName.CompareTo(y.driverFullName));
				manufacturerEntity.printerIssuer.printerModels = manufacturerEntity.printerIssuer.printerModels.GroupBy(x => x.driverFullName)
					.Select(x => x.First())
					.ToList();
				printerIssuers.Add(manufacturerEntity.printerIssuer);
			}
		}
		//InfHelper.Models.Category

	}
}
