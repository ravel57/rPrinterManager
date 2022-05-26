using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Management;
using System.Text;
using System.Windows;

namespace rPrinterManager {

	public class Printer /*: INotifyPropertyChanged*/ {


		private ManagementScope managementScope = null;

		public string ip { get; set; }
		public string name { get; set; }
		public PrinterModel printerModel { get; set; }
		public bool marked = false;

		//public event PropertyChangedEventHandler PropertyChanged;
		//protected virtual void onPropertyChanged(string propertyName = "") {
		//	PropertyChanged ?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		//}



		public Printer(string ip, string name, PrinterModel printerModel, ManagementScope managementScope) {
			this.ip = ip;
			this.name = name;
			this.printerModel = printerModel;
			this.managementScope = managementScope;
		}




		private bool CheckPrinterPort() {
			//Query system for Operating System information
			ObjectQuery query = new ObjectQuery("SELECT * FROM Win32_TCPIPPrinterPort");
			ManagementObjectSearcher searcher = new ManagementObjectSearcher(managementScope, query);

			ManagementObjectCollection queryCollection = searcher.Get();
			foreach (ManagementObject m in queryCollection) {
				if (m["Name"].ToString() == ip)
					return true;
			}
			return false;
		}

		private bool CreatePrinterPort() {
			if (CheckPrinterPort()) {
				return true;
			}
			var printerPortClass = new ManagementClass(managementScope, new ManagementPath("Win32_TCPIPPrinterPort"), new ObjectGetOptions());
			printerPortClass.Get();
			var newPrinterPort = printerPortClass.CreateInstance();
			newPrinterPort.SetPropertyValue("Name", ip);
			newPrinterPort.SetPropertyValue("Protocol", 1);
			newPrinterPort.SetPropertyValue("HostAddress", ip);
			newPrinterPort.SetPropertyValue("PortNumber", 9100);    // default=9100
			newPrinterPort.SetPropertyValue("SNMPEnabled", false);  // true?
			newPrinterPort.Put();
			return true;
		}

		private bool CreatePrinterDriver(string driverInfFile) {
			var endResult = false;
			// Inspired from https://msdn.microsoft.com/en-us/library/aa384771%28v=vs.85%29.aspx?f=255&MSPPError=-2147217396
			// and http://microsoft.public.win32.programmer.wmi.narkive.com/y5GB15iF/adding-printer-driver-using-system-management
			//string printerDriverInfPath = IOUtils.FindInfFile(printerDriverFolderPath);
			var printerDriverClass = new ManagementClass(managementScope, new ManagementPath("Win32_PrinterDriver"), new ObjectGetOptions());
			var printerDriver = printerDriverClass.CreateInstance();
			printerDriver.SetPropertyValue("Name", printerModel.driverFullName);
			printerDriver.SetPropertyValue("FilePath", Directory.GetParent(Path.GetFullPath(driverInfFile)).FullName + '\\');
			printerDriver.SetPropertyValue("InfName", driverInfFile);

			// Obtain in-parameters for the method
			using (ManagementBaseObject inParams = printerDriverClass.GetMethodParameters("AddPrinterDriver")) {
				inParams["DriverInfo"] = printerDriver;
				// Execute the method and obtain the return values.            

				using (ManagementBaseObject result = printerDriverClass.InvokeMethod("AddPrinterDriver", inParams, null)) {
					// result["ReturnValue"]
					uint errorCode = (uint)result.Properties["ReturnValue"].Value;
					// or directly result["ReturnValue"]
					// https://msdn.microsoft.com/en-us/library/windows/desktop/ms681386(v=vs.85).aspx
					switch (errorCode) {
						case 0:
							//Trace.TraceInformation("Successfully connected printer.");
							endResult = true;
							break;
						case 5:
							MessageBox.Show("Access Denied.");
							break;
						//case 87:
						//	MessageBox.Show("Invalid .INF file path or file don't contains printer.");
						//	break;
						case 123:
							MessageBox.Show("The filename, directory name, or volume label syntax is incorrect.");
							break;
						case 1801:
							MessageBox.Show("Invalid Printer Name.");
							break;
						case 1930:
							MessageBox.Show("Incompatible Printer Driver.");
							break;
						case 3019:
							MessageBox.Show("The specified printer driver was not found on the system and needs to be downloaded.");
							break;
					}
				}
			}
			return endResult;
		}

		private bool CreatePrinter() {
			var printerClass = new ManagementClass(managementScope,
												   new ManagementPath("Win32_Printer"), 
												   new ObjectGetOptions());
			printerClass.Get();
			var printer = printerClass.CreateInstance();
			printer.SetPropertyValue("DriverName", printerModel.driverFullName);
			printer.SetPropertyValue("PortName", ip);
			printer.SetPropertyValue("Name", name);
			printer.SetPropertyValue("DeviceID", name);
			//printer.SetPropertyValue("Location", "Front Office");
			printer.SetPropertyValue("Network", true);
			printer.SetPropertyValue("Shared", false);
			printer.Put();
			return true;
		}


		public void InstallPrinterWMI(/*string printerDriverPath*/) {
			string printerDriverPath = this.printerModel.driverInfFilePath;
			bool printePortCreated = false, printeDriverCreated = false, printeCreated = false;
			try {
				printePortCreated = CreatePrinterPort();
				//printeDriverCreated = CreatePrinterDriver(printerDriverPath);
				printeDriverCreated = CreatePrinterDriver(printerModel.driverInfFilePath);
				printeCreated = CreatePrinter();
			} catch (ManagementException err) {
				if (printePortCreated) {
					// RemovePort
				}
				MessageBox.Show("An error occurred while trying to execute the WMI method: " + err.Message);
			}
		}

		public void unmapPrinter() {
			ConnectionOptions options = new ConnectionOptions();
			options.EnablePrivileges = true;
			ManagementScope scope = new ManagementScope(ManagementPath.DefaultPath, options);
			scope.Connect();

			ManagementClass win32Printer = new ManagementClass("Win32_Printer");
			ManagementObjectCollection printers = win32Printer.GetInstances();
			foreach (ManagementObject printer in printers) {
				printer.Delete();
			}
		}

	}
}
