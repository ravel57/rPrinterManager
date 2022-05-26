using System;
using System.Collections.Generic;
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
using System.ComponentModel;
using System.Net.NetworkInformation;
using System.Data;
using System.IO;

namespace rPrinterManager {
	/// <summary>
	/// Логика взаимодействия для MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window {

		//private Computer computer;
		//private BindingList<Printer> printers;

		//private enum ScanComputerType : int {
		//	local = 0,
		//	remote = 1
		//}
		//private ScanComputerType scanComputerType = ScanComputerType.local;


		public MainWindow() {
			InitializeComponent();
		}

		private void Window_Loaded(object sender, RoutedEventArgs e) {
			try {
				Directory.SetCurrentDirectory(Directory.GetParent(Environment.GetCommandLineArgs()[0]).FullName);
				if (Environment.GetCommandLineArgs().Count() > 1) {
					remoteComputerName.Text = Environment.GetCommandLineArgs()[1];
					setRemote();
				} else {
					setLocal();
				}
				//Computer.printers.ListChanged += Printers_ListChanged;
			} catch (Exception ex) {
				MessageBox.Show(ex.Message);
			}
		}




		private void setLocal() {
			localComputer_rb.IsChecked = true;
			//scanComputerType = ScanComputerType.local;
			//computer = new Computer("localhost");
			Computer.init("localhost");
			printers_dg.ItemsSource = Computer.printers;
		}

		private void localCompute_RadioButton_Checked(object sender, RoutedEventArgs e) {
			setLocal();
		}


		private void setRemote() {
			remoteComputer_rb.IsChecked = true;
			if (!string.IsNullOrEmpty(remoteComputerName.Text) && Computer.ping(remoteComputerName.Text)) {
				//computer = new Computer(remoteComputerName.Text);
				Computer.init(remoteComputerName.Text);
				printers_dg.ItemsSource = Computer.printers;
			}
			//scanComputerType = ScanComputerType.remote;
		}

		private void TextBox_TextChanged(object sender, TextChangedEventArgs e) {
			setRemote();
		}

		private void remoteComputer_radioButton_Checked(object sender, RoutedEventArgs e) {
			setRemote();
		}
		private void remoteComputerName_PreviewMouseDown(object sender, MouseButtonEventArgs e) {
			setRemote();
		}


		private void Printers_ListChanged(object sender, ListChangedEventArgs e) {
			MessageBox.Show("");
			//	switch (e.ListChangedType) {
			//		case ListChangedType.Reset:
			//			break;
			//		case ListChangedType.ItemAdded:
			//			break;
			//		case ListChangedType.ItemDeleted:
			//			break;
			//		case ListChangedType.ItemMoved:
			//			break;
			//		case ListChangedType.ItemChanged:
			//			break;
			//		case ListChangedType.PropertyDescriptorAdded:
			//			break;
			//		case ListChangedType.PropertyDescriptorDeleted:
			//			break;
			//		case ListChangedType.PropertyDescriptorChanged:
			//			break;
			//			//throw new NotImplementedException();
			//	}
		}

		private void newPrinter_btn_Click(object sender, RoutedEventArgs e) {
			EditPrinterWindow editPrinterWindow = new EditPrinterWindow(/*computer*/);
			editPrinterWindow.ShowDialog();
			printers_dg.ItemsSource = Computer.printers;
		}



		private void Row_DoubleClick(object sender, MouseButtonEventArgs e) {
			var t = ((sender as DataGridRow).Item as Printer);
			string ip = t.ip;
			if (ip.ToUpper().StartsWith("IP_"))
				ip = ip.Remove(0, 3);
			if (ip.Contains("_"))
				ip = ip.Remove(ip.IndexOf("_"), ip.Length - ip.IndexOf("_"));
			if (Computer.IsValidateIP(ip))
				Computer.execProcess($"http://{ip}");
		}

		private void HiddenEditButton_Click(object sender, RoutedEventArgs e) {
			EditPrinterWindow editPrinterWindow = new EditPrinterWindow(/*computer,*/ ((Button)e.Source).DataContext as Printer);
			editPrinterWindow.Show();
		}

		private void deletePrinter_btn_Click(object sender, RoutedEventArgs e) {

		}
	}
}