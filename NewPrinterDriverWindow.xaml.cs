using InfHelper;
using InfHelper.Models;
using Microsoft.Win32;
using rPrinterManager.src;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
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
	/// Логика взаимодействия для NewPrinterDriverWindow.xaml
	/// </summary>
	public partial class NewPrinterDriverWindow : Window {

		private string path;
		List<PrinterIssuer> printerIssuers = new List<PrinterIssuer>();


		public NewPrinterDriverWindow() {
			InitializeComponent();
		}

		void checkPrinterFromFolder() {
			var helper = new InfUtil();
			List<string> driversNames = null;
			string[] files = Directory.GetFiles(path, "*.inf");
			printerInfFiles_lb.Items.Clear();
			printerIssuers_lb.Items.Clear();
			//string aaa = file.Remove(0, path.Length + 1);
			foreach (string file in files) {
				InfData data = helper.ParseFile(file);
				var infVerson = data["Version"];
				var infVersonClass = infVerson["Class"];
				if (infVersonClass.PrimitiveValue != "Printer") {
					continue;
				}
				var infVersonManufacturer = data["Manufacturer"];
				for (int i = 0; i < infVersonManufacturer.Keys.Count; i++) {
					var infPrinterIssuer = infVersonManufacturer.Keys[i];
					string toFind = infPrinterIssuer.KeyValues[0].Value + '.' + infPrinterIssuer.KeyValues[1].Value;
					if (infVerson["Provider"].PrimitiveValue == "%OEM%") {
						string toFindInStrings = data.Categories.First(el => el.Name == toFind).Keys[0].Id.Replace("%", "");
						var infStrings = data["Strings"];
						driversNames = infStrings.Keys.Where(el => el.Id.StartsWith(toFindInStrings)).Select(el => el.KeyValues[0].Value).ToList();
					} else {
						driversNames = data[toFind].Keys.Select(el => el.KeyValues[0].Value).ToList();
					}
					PrinterIssuer printerIssuer = new PrinterIssuer(infPrinterIssuer.KeyValues[0].Value);
					foreach (var driver in driversNames) {
						printerIssuer.printerModels.Add(new PrinterModel(driver, driver, driver, file));
					}
					printerIssuer.printerModels.Sort((x, y) => x.driverFullName.CompareTo(y.driverFullName));
					printerIssuer.printerModels = printerIssuer.printerModels.GroupBy(x => x.driverFullName).Select(x => x.First()).ToList();
					printerIssuers.Add(printerIssuer);
				}
				//	if (printerIssuer.printerModels.Count > 0) {
				//		printerInfFiles_lb.Items.Add(file.Remove(0, path.Length + 1));
				//		printerIssuers_lb.Items.Add(infPrinterIssuer.KeyValues[0].Value);
				//		printerDrivers_lb.ItemsSource = printerIssuer.printerModels.Select(el => el.driverFullName).ToList();
				//	}
			}
			if (printerIssuers.Count > 1)
				printerIssuers_lb.ItemsSource = printerIssuers.Select(el => el.issuer).ToList();
			else
				printerIssuers_lb.Items.Clear();
			printerIssuers_lb.SelectedIndex = 0;

		}

		private void browseInfFile_btn_Click(object sender, RoutedEventArgs e) {
			OpenFileDialog openFileDialog = new OpenFileDialog();
			openFileDialog.Filter = "INF files (*.inf)|*.inf";
			if (openFileDialog.ShowDialog() == true) {
				this.path = Directory.GetParent(System.IO.Path.GetFullPath(openFileDialog.FileName)).FullName + '\\';
				checkPrinterFromFolder();
			}
		}

		private void printerInfFiles_lb_SelectionChanged(object sender, SelectionChangedEventArgs e) {

		}

		private void printerIssuers_lb_SelectionChanged(object sender, SelectionChangedEventArgs e) {

		}

		private void printerDrivers_lb_SelectionChanged(object sender, SelectionChangedEventArgs e) {

		}

	}
}
