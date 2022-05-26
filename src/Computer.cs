using InfHelper;
using InfHelper.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Management;
using System.Net.NetworkInformation;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;

namespace rPrinterManager {
	public static class Computer {

		private static ManagementScope managementScope = null;
		private static string printerModelsFile = "printerModelsFile.json";

		public static BindingList<Printer> printers { get; private set; }
		public static List<PrinterModel> printerModels = new List<PrinterModel>();

		//public Computer(string computerName) {
		//	string printerModelsStr = File.ReadAllText(printerModelsFile);
		//	printerModels = JsonConvert.DeserializeObject<List<PrinterModel>>(printerModelsStr);
		//	CreateManagementScope(computerName);
		//	updatePrinters();
		//}

		public static void init(string computerName) {
			try {
				string printerModelsStr = File.ReadAllText(printerModelsFile);
				printerModels = JsonConvert.DeserializeObject<List<PrinterModel>>(printerModelsStr);
			} catch (FileNotFoundException ex) {
			} catch (Exception ex) {
				MessageBox.Show(ex.Message);
			}
			CreateManagementScope(computerName);
			updatePrinters();
		}

		public static void addPrinter(string driverName, string ip, string name) {
			Printer printer = new Printer(ip, name, 
											printerModels.First(p => p.driverShortName == driverName), 
											managementScope);
			printer.InstallPrinterWMI();
			updatePrinters();
		}

		public static void editPrinter(Printer printer, string newName) {
			//Printer printer = new Printer(ip, name, 
			//								printerModels.First(p => p.driverShortName == driverName), 
			//								managementScope);
			//printer.InstallPrinterWMI();
		}


		public static bool IsValidateIP(string address) {
			string Pattern = @"^([1-9]|[1-9][0-9]|1[0-9][0-9]|2[0-4][0-9]|25[0-5])(\.([0-9]|[1-9][0-9]|1[0-9][0-9]|2[0-4][0-9]|25[0-5])){3}$";
			Regex check = new Regex(Pattern);
			if (!string.IsNullOrEmpty(address)) {
				return check.IsMatch(address, 0);
			} else
				return false;
		}

		public static void execProcess(string proces) {
			System.Diagnostics.Process process = new System.Diagnostics.Process();
			System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
			startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
			startInfo.FileName = "cmd.exe";
			startInfo.Arguments = $"/c start \"\" \"{proces}\"";
			process.StartInfo = startInfo;
			process.Start();
		}

		public static bool ping(string pcName) {
			Ping ping = new Ping();
			PingOptions pingOptions = new PingOptions();
			pingOptions.DontFragment = true;
			pingOptions.Ttl = 5;
			string data = "fff";
			byte[] buffer = Encoding.ASCII.GetBytes(data);
			try {
				return (ping.Send(pcName, 120, buffer, pingOptions).Status == IPStatus.Success);
			} catch (System.Net.NetworkInformation.PingException) {
				return false;
			}
		}

		private static void CreateManagementScope(string computerName) {
			var wmiConnectionOptions = new ConnectionOptions();
			wmiConnectionOptions.Impersonation = ImpersonationLevel.Impersonate;
			wmiConnectionOptions.Authentication = AuthenticationLevel.Default;
			wmiConnectionOptions.EnablePrivileges = true;
			// required to load/install the driver.
			// Supposed equivalent to VBScript objWMIService.Security_.Privileges.AddAsString "SeLoadDriverPrivilege", True 
			string path = "\\\\" + computerName + "\\root\\cimv2";
			managementScope = new ManagementScope(path, wmiConnectionOptions);
			managementScope.Connect();
		}

		private static void updatePrinters() {
			ObjectQuery query = new ObjectQuery("SELECT * FROM Win32_Printer");
			ManagementObjectSearcher searcher = new ManagementObjectSearcher(managementScope, query);
			var searcherResult = searcher.Get();
			printers = new BindingList<Printer>();
			foreach (var printer in searcherResult) {
				//var tdfgdg = printer.GetPropertyValue("DriverName").ToString();
				//bool dfgd = printer.GetPropertyValue("DriverName").ToString() == printerModels[4].driverFullName;
				printers.Add(new Printer(
						printer.GetPropertyValue("PortName").ToString(),
						printer.GetPropertyValue("Name").ToString(),
						printerModels.FirstOrDefault(pr => printer.GetPropertyValue("DriverName").ToString() == pr.driverFullName),
						//new PrinterModel(printer.GetPropertyValue("DriverName").ToString()),
						managementScope
					));
			}
		}

		public static void AddPrinterModels(PrinterModel printerModel) {
			printerModels.Add(printerModel);
		}

		public static void savePrinterModels() {
			File.WriteAllText(printerModelsFile, JsonConvert.SerializeObject(printerModels));
		}

	}
}
