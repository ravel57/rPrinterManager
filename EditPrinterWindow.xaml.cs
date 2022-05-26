using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace rPrinterManager {
	/// <summary>
	/// Логика взаимодействия для EditPrinterWindow.xaml
	/// </summary>
	public partial class EditPrinterWindow : Window {
		//public 
		//List<Printer.PrinterModel> printerModels = new List<Printer.PrinterModel>();
		//private string selectedDriver;

		private Printer printer;
		public EditPrinterWindow() {
			InitializeComponent();
			this.addPrinter_b.Content = "Добавить";
			printerIP.Text = getLocalIpSubNetwork();
			driverList_comboBox.ItemsSource = getDriverNames();
		}
		public EditPrinterWindow(Printer printer) {
			InitializeComponent();
			this.addPrinter_b.Content = "Изменить";
			this.printer = printer;
			driverList_comboBox.ItemsSource = getDriverNames();
			this.driverList_comboBox.SelectedItem = printer.printerModel.driverShortName;
			//driverList_cb.ItemsSource = new List<string>().Add(printer.printerModel.driverName);
			this.driverList_comboBox.IsEnabled = false;
			this.printerIP.Text = printer.ip;
			this.printerName_tb.Text = printer.name;
			//driverList_cb.ItemsSource = computer.printerModels.First();
		}

		List<string> getDriverNames() {
			return Computer.printerModels.Select(o => o.driverShortName).ToList();
		}

		string getLocalIpSubNetwork() {
			string ip = Dns.GetHostEntry(Dns.GetHostName()).AddressList
				.First(arrd => arrd.AddressFamily == AddressFamily.InterNetwork)
				.ToString();
			int pos = 0;
			for (int i = 0; i < 3; i++)
				pos = ip.IndexOf(".", pos + 1);
			return ip.Remove(pos + 1);
		}

		private void printerIP_TextChanged(object sender, TextChangedEventArgs e) {
			//driverList_cb.SelectedItem = getDriverName((sender as TextBox).Text);
			if (printerIP.Text.Contains(",")) {
				int cursorPosition = printerIP.SelectionStart;
				printerIP.Text = printerIP.Text.Replace(',', '.');
				printerIP.Select(cursorPosition, 0);
			}
			getDriverName((sender as TextBox).Text);
			//driverList_cb.SelectedItem = Task.Run(() => getDriverName()).Result;
		}


		private async void getDriverName(string ip) {
			if (Computer.IsValidateIP(ip) && Computer.ping(ip)) {
				await Task.Run(() => {
					try {
						string hostName = Dns.GetHostEntryAsync(ip).Result.HostName;
						WebClient webClient = new WebClient();
						//webClient.Encoding = Encoding.UTF8;
						string source = webClient.DownloadString($"http://{ip}");
						//printerName_tb.Dispatcher.Invoke(() => { printerName_tb.Text = source; });
						string title = Regex.Match(source, @"\<title\b[^>]*\>\s*(?<Title>[\s\S]*?)\</title\>", RegexOptions.IgnoreCase).Groups["Title"].Value;
						if (hostName != null && title != null) {
							string driverName = "";
							if (hostName.StartsWith("RNP") & title == "Web Image Monitor")
								driverName = Computer.printerModels.First(p => p.driverShortName.Contains("RICOH")).driverShortName;
							if (hostName.StartsWith("HP") & title == "")
								driverName = Computer.printerModels.First(p => p.driverShortName.Contains("HP 428")).driverShortName;
							if (title.Contains("HP LaserJet MFP M426fdn"))
								driverName = Computer.printerModels.First(p => p.driverShortName.Contains("HP 426")).driverShortName;
							if (title.Contains("HP LaserJet 400 MFP M425"))
								driverName = Computer.printerModels.First(p => p.driverShortName.Contains("HP 425")).driverShortName;
							//if (driverList_cb.Dispatcher.CheckAccess()) driverList_cb.SelectedItem = driverName; else
							driverList_comboBox.Dispatcher.Invoke(() => { driverList_comboBox.SelectedItem = driverName; });
						}
					} catch (Exception e) { }
				});
			}
			//return "";

		}



		private void driverList_cb_SelectionChanged(object sender, SelectionChangedEventArgs e) {
			printerName_tb.Text = Computer.printerModels
				.First(printe => printe.driverShortName == e.AddedItems[0].ToString())
				.defaultName;
		}

		private void addPrinter_b_Click(object sender, RoutedEventArgs e) {
			if (printer == null) {
				Computer.addPrinter(driverList_comboBox.SelectedItem.ToString(),
									printerIP.Text, 
									printerName_tb.Text);
			} else {
				Computer.editPrinter(printer, printerName_tb.Text);
			}
			//Computer.updatePrinters();
			this.Close();
		}

		private void addDriver_btn_Click(object sender, RoutedEventArgs e) {
			NewPrinterDriverWindow newPrinterDriverWindow = new NewPrinterDriverWindow();
			newPrinterDriverWindow.ShowDialog();
			driverList_comboBox.ItemsSource = getDriverNames();
		}
	}
}
