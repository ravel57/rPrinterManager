using InfHelper;
using InfHelper.Models;
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
	public class Computer {

		public BindingList<Printer> printers { get; private set; }
		public List<PrinterModel> printerModels = new List<PrinterModel>();
		private ManagementScope managementScope = null;

		public Computer(string computerName) {
			string driverpath = @"\\rmfs01\Distro\Drivers\Printers";
			printerModels.Add(new PrinterModel("HP 425", "HP LaserJet MFP M425", "HP LaserJet 400 MFP M425 PCL 6", driverpath + @"\HP\LJPro-MFP-M425_driver_only_15188_1\hpcm425u.inf"));
			printerModels.Add(new PrinterModel("HP 426", "HP LaserJet Pro MFP M426f", "HP LaserJet Pro MFP M426-M427 PCL 6", driverpath + @"\HP\HP_LJ_Pro_MFP_M426\hpma532a_x64.inf"));
			printerModels.Add(new PrinterModel("HP 428", "HP LaserJet M428f", "HP LaserJet Pro M428f-M429f PCL-6 (V4)", driverpath + @"\HP\HP 428\HPeSCLScan.INF"));
			printerModels.Add(new PrinterModel("RICOH 2011", "RICOH MP C2011", "RICOH MP C2011 PCL 6", driverpath + @"\Ricoh\MP C2011\disk1\OEMSETUP.INF"));
			printerModels.Add(new PrinterModel("RICOH 2000", "RICOH MP C2000", "RICOH PCL6 V4 UniversalDriver V4.12", driverpath + @"\Ricoh\2000\disk1\r4600.inf"));
			
			CreateManagementScope(computerName);
			updatePrinters();
		}

		public void addPrinter(string driverName, string ip, string name) {
			Printer printer = new Printer(ip, name, printerModels.First(p => p.driverShortName == driverName), managementScope);
			printer.InstallPrinterWMI();


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

		private void CreateManagementScope(string computerName) {
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

		public void updatePrinters() {
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



	}
}
