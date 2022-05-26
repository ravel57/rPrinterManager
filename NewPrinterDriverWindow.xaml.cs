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

		//List<PrinterIssuer> printerIssuers = new List<PrinterIssuer>();
		private List<InfFile> infFiles = new List<InfFile>();
		private InfFile selectedInfFile;
		private PrinterIssuer selectedPrinterIssuers;
		private PrinterModel selectedPrinterModel;

		public NewPrinterDriverWindow() {
			InitializeComponent();
			browseInfFile_btn_Click(new object(), new RoutedEventArgs());
		}

		private void browseInfFile_btn_Click(object sender, RoutedEventArgs e) {
			OpenFileDialog openFileDialog = new OpenFileDialog();
			openFileDialog.Filter = "INF files (*.inf)|*.inf";
			if (openFileDialog.ShowDialog() == true) {
				string path = Directory.GetParent(System.IO.Path.GetFullPath(openFileDialog.FileName)).FullName + '\\';
				infFiles = Directory.GetFiles(path, "*.inf").Select(el => new InfFile(el)).ToList();
				foreach (InfFile infFile in infFiles) {
					NetworkPrinterInf inf = new NetworkPrinterInf(infFile.path);
					if (inf.classIsPrinter && inf.printerIssuers.Sum(el => el.printerModels.Count) > 0)
						infFile.printerIssuers.AddRange(inf.printerIssuers);
				}
				infFiles = infFiles.Where(el => el.printerIssuers.Count > 0).ToList();
				if (infFiles.Count > 0) {
					printerInfFiles_listBox.ItemsSource = infFiles.Select(el => el.path.Replace(path, ""));
					printerInfFiles_listBox.SelectedIndex = 0;
				} else {
					printerIssuers_listBox.ItemsSource = new List<string>();
				}
			}
		}

		private void printerInfFiles_listBox_SelectionChanged(object sender, SelectionChangedEventArgs e) {
			if (printerInfFiles_listBox.SelectedIndex >= 0) {
				selectedInfFile = infFiles[printerInfFiles_listBox.SelectedIndex];
				printerIssuers_listBox.ItemsSource = selectedInfFile.printerIssuers.Select(el => el.issuer).ToList();
				printerIssuers_listBox.SelectedIndex = 0;
				printerIssuers_listBox_SelectionChanged();
			}
		}

		private void printerIssuers_listBox_SelectionChanged(object sender, SelectionChangedEventArgs e) {
			printerIssuers_listBox_SelectionChanged();
		}

		private void printerIssuers_listBox_SelectionChanged() {
			if (printerIssuers_listBox.SelectedIndex >= 0) {
				selectedPrinterIssuers = selectedInfFile.printerIssuers[printerIssuers_listBox.SelectedIndex];
				printerDrivers_listBox.ItemsSource = selectedPrinterIssuers.printerModels.Select(el => el.driverFullName).ToList();
				printerDrivers_listBox.SelectedIndex = 0;
				printerDrivers_listBox_SelectionChanged();
			}
		}

		private void printerDrivers_listBox_SelectionChanged(object sender, SelectionChangedEventArgs e) {
			printerDrivers_listBox_SelectionChanged();
		}

		private void printerDrivers_listBox_SelectionChanged() {
			if (printerDrivers_listBox.SelectedIndex >= 0) {
				selectedPrinterModel = selectedPrinterIssuers.printerModels[printerDrivers_listBox.SelectedIndex];
				driverFullName_textBox.Text = selectedPrinterModel.driverFullName;
				driverShortName_textBox.Text = selectedPrinterModel.driverShortName;
				driverDefaultName_textBox.Text = selectedPrinterModel.defaultName;
				driverInfFilePath_textBox.Text = selectedPrinterModel.driverInfFilePath;
				addPrinter_button.IsEnabled = true;
			}
		}

		private void addPrinter_button_Click(object sender, RoutedEventArgs e) {
			Computer.AddPrinterModels(new PrinterModel(driverShortName_textBox.Text, 
														driverDefaultName_textBox.Text, 
														driverFullName_textBox.Text,
														driverInfFilePath_textBox.Text));
			Computer.savePrinterModels();
			Close();
		}
	}
}
