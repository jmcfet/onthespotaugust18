using DataAccessLayer.Models;
using NLog;
using OnTheSpot.Models;
using QCS.Views;
using SUT.PrintEngine.Utils;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Printing;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using static QCS.ViewModels.QCSVM;

namespace QCS
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        QSSVM vm = new QSSVM();
        List<string> testCodes = new List<string>() { "1000492087", "1000508922", "1000446918", "1000446917", "1000446916", "1000446915", "1000446914" };
        int dumcode = 0;
        List<string> buttonLabels = new List<string>() { "Spots", "Repairs", "Repressing","Cleaners" };
        int BarcodeChars = 0;
        DispatcherTimer timer1 = null;
        DispatcherTimer timer2 = null;
        string employeeID;
        bool bClearPressed = false;
        Logger logger = LogManager.GetLogger("BCS");
        BitmapImage bitmap;
        OnTheSpot.Models.Item item;
        string imgName;
        public MainWindow()
        {
            InitializeComponent();
            Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            //make sure we can open database
            vm.OpenBCSandStoreDB();
            vm.GetOurEntities();
            vm.OpenAssemblyDB();
            this.Title = "On the Spot QCS";
            int i = 0;
            //Note.Visibility = Visibility.Visible;
            //Totals.Visibility = Visibility.Visible;
            if (vm.GetShowPass() == 1)
            {
                ShowPass.Content = "Hide pass";
            }
            foreach (object but in buttonsContainer.Children)
            {

                Button button = but as Button;
                if (i < buttonLabels.Count)
                {
                    button.Visibility = Visibility.Visible;
                    button.Content = buttonLabels[i];
                }
                i++;
            }
            if (vm.GetShowPass() == 1)
                PassButton.Visibility = Visibility.Visible;
            timer2 = new DispatcherTimer();
            timer2.Interval = TimeSpan.FromSeconds(1);
            timer2.Tick += new EventHandler(timer2_Tick);
            timer1 = new DispatcherTimer();
            timer1.Interval = TimeSpan.FromSeconds(30);
            timer1.Tick += new EventHandler(timer1_Tick);
            timer1.Start();
            EmployeeID.Focus();
            Barcode.TextChanged += new TextChangedEventHandler(Barcode_TextChanged);
           

            if (vm.DBerrormsg == string.Empty)
            {
                led1.ColorOn = Colors.Green;
                led1.IsActive = true;
                LoadedDone.Visibility = Visibility.Visible;
                LoadingState.Visibility = Visibility.Collapsed;
            }
            else
            {
                DBErrorMsg.Text = vm.DBerrormsg;
                textBlock1.Text = "System Initialization Failed";
                led1.ColorOn = Colors.Red;
                led1.IsActive = true;
            }

            }
        /// <summary>
        /// the barcode scanner works by sending all the scanned chars to the element with the input focus at once so
        /// we set a 1 second timer that will read all the chars in that time span and then grab the expected number of
        /// chars as this is a fixed value for the bar code. this way we will correct some of the garbage scans
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void Barcode_TextChanged(object sender, TextChangedEventArgs e)
        {
            //if we read first char then start a timer to get the rest as it all comes in a burst
            if (BarcodeChars == 0)
            {

                if (bClearPressed)     //if we are manually resetting the barcode then do not start timer
                {
                    bClearPressed = false;
                    return;
                }
                timer2.Start();
                ErrorTxt.Visibility = Visibility.Hidden;
            }
            BarcodeChars++;

        }
        void timer1_Tick(object sender, EventArgs e)
        {
            int i = 0;
            List<DataAccessLayer.QCSInfo> qcs;
            foreach (object but in buttonsContainer.Children)
            {

                Button button = but as Button;
                
                if (i < buttonLabels.Count)
                {
                    button.Visibility = Visibility.Visible;
                    qcs = vm.GetQCSTotal(buttonLabels[i]);
                    button.Content = buttonLabels[i] + "(" +  qcs.Count().ToString() + ")";
                }
                i++;
            } 
                        
            noCat.Content = "no category" + "(" + vm.getUncatCount().ToString() + ")";



        }
            //only this timer is operation now
         void timer2_Tick(object sender, EventArgs e)
        {
            timer2.Stop();

            if (!ProcessBarcode())
                return;
            Mouse.OverrideCursor = Cursors.Wait;
            vm.barcode = Barcode.Text;


            vm.assemblyInfo = null;
            //check that the item is in the Assembly database if not then show message and allow more items
            //to be scanned
            try
            {
                vm.assemblyInfo = vm.getItemInAssemblyDB(Barcode.Text);
                if (vm.assemblyInfo == null)
                {
                    BarcodeChars = 0;
                    Errormsg.Text = string.Format(string.Format("Item has not been marked in {0}", Barcode.Text));
                    ErrorTxt.Visibility = Visibility.Visible;
                    Mouse.OverrideCursor = null;
                    return;
                }
                vm.GetCustomer(vm.assemblyInfo.CustomerID);
                item = vm.GetItemInDB(Barcode.Text);
                if (item == null)
                {
                    Errormsg.Text = string.Format(string.Format("item is not in BCS  {0} .. do not QA ", Barcode.Text));
                    ErrorTxt.Visibility = Visibility.Visible;
                    Mouse.OverrideCursor = null;
                    return;
                }
                if (vm.bLoggedIn)
                    vm.BarcodeEntered = true;
                vm.ShowButtons = true;
                inter.Visibility = Visibility.Visible;
                NoteBox.Visibility = System.Windows.Visibility.Collapsed;
                
                vm.Note = string.Empty;
                if (item.Note != null && item.Note != string.Empty)
                {
                    vm.Note = item.Note;
                    NoteText.Text = item.Note;
                    NoteBox.Visibility = Visibility.Visible;
                }
                DateTime date  = (DateTime)vm.assemblyInfo.Duedate;
                DateTime future = DateTime.Now.AddDays(2);
              //always make the duedate standout
                duedate.Foreground = new SolidColorBrush(Colors.Red); 
                duedate.FontSize = 36;
               
                if (date.Date <= DateTime.Now.Date)
                    duedate.Text = "TODAY";
                else
                    duedate.Text = date.DayOfWeek.ToString() + " " + date.Day;
                vm.Duedate = duedate.Text;
                CustomerName.Text = vm.activeCustomer.FirstName + " " + vm.activeCustomer.LastName;
                //check if this is in Route
                int RFIDlen = vm.assemblyInfo.rfid.Length;
                bool route = false;
                for (int i = 0; i < vm.assemblyInfo.rfid.Length; i++)
                {
                    if (vm.assemblyInfo.rfid[i] != ' ')
                    {
                        route = true;
                        break;
                    }

                }
                if (route)
                    store.Text = vm.store = "Route";
                else
                    store.Text = vm.store = vm.assemblyInfo.storeName;


            }
            catch (Exception e1)
            {
                BarcodeChars = 0;
                Errormsg.Text = string.Format(string.Format("Database logic error {0} {1}", Barcode.Text, e1.Message));
                ErrorTxt.Visibility = Visibility.Visible;
                Mouse.OverrideCursor = null;
                return;
            }
            Mouse.OverrideCursor = null;
            logger.Info(string.Format("all data obtained for {0} ", Barcode.Text));
            //if there is a picture then display it
            if (item.picture == null)
                return;
            picture.Visibility = Visibility.Visible;
            if (employeeID == "1")
            {
                Note.Visibility = Visibility.Visible;
               
            }

            byte[] binaryData = Convert.FromBase64String(item.picture);
            bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.StreamSource = new MemoryStream(binaryData);
            bitmap.EndInit();
            img.Source = bitmap;

        }

        bool ProcessBarcode()
        {
            //test ddd
            double itemCode = 0;
   //         Barcode.Text = testCodes[dumcode++];     //danger   yyyyyyy
            logger.Info("Read bar code " + Barcode.Text);
            if (Barcode.Text == string.Empty)
                return false;
            if (Barcode.Text.Length < 9)
            {
                Errormsg.Text = string.Format("invalid barcode {0}", Barcode.Text);
                ErrorTxt.Visibility = Visibility.Visible;
                Barcode.Text = string.Empty;
                BarcodeChars = 0;
                Barcode.Focus();
                return false;
            }

            if (!double.TryParse(Barcode.Text, out itemCode))       //change
            {

                Errormsg.Text = string.Format("invalid barcode {0}", Barcode.Text);
                ErrorTxt.Visibility = Visibility.Visible;
                Barcode.Text = string.Empty;
                BarcodeChars = 0;
                Barcode.Focus();
                return false;
            }

            return true;

        }
        private void passwordEntered_Click(object sender, RoutedEventArgs e)
        {
            string pass = passwordBox1.Password;
            passwordBox1.Password = string.Empty;
            if (pass == "tennis")
            {
                vm.bLoggedIn = true;

            }
        }
        //look for the user enetering their userid
        private void EmployeeID_KeyUp(object sender, KeyEventArgs e)
        {


            if (e.Key != System.Windows.Input.Key.Enter) return;

            int empid;

            employeeID = EmployeeID.Text;
            if (!Int32.TryParse(employeeID, out empid))
            {
                Errormsg.Text = string.Format("invalid employeeid");
                ErrorTxt.Visibility = Visibility.Visible;
                return;
            }
            OnTheSpot.Models.Employee employee = vm.GetEmployee(empid);
            vm.Employeename = employee.FirstName + " " + employee.LastName;
            if (employee == null)
            {
                Errormsg.Text = string.Format("invalid employeeid");
                ErrorTxt.Visibility = Visibility.Visible;
                return;
            }


            UI.Visibility = Visibility.Visible;
            Login.Visibility = Visibility.Collapsed;
            msg.Text = vm.Employeename + " logged in";
            Loggedin.Visibility = Visibility.Visible;
            if (employeeID == "1")
            {
                
              //  Totals.Visibility = Visibility.Visible;
            }
            Barcode.Focus();



        }
        private void Note_Click(object sender, RoutedEventArgs e)
        {
            GetNote popup = new GetNote(vm);
            popup.ShowDialog();
            if (vm.Note == null ||vm.Note.Count() == 0  )
                return;
            vm.AddNote(Barcode.Text, vm.Note);
            NoteText.Text = vm.Note;
            NoteBox.Visibility = Visibility.Visible;

        }
        private void Log_Click(object sender, RoutedEventArgs e)
        {
            Loggedin.Visibility = Visibility.Collapsed;
            Login.Visibility = Visibility.Visible;
            UI.Visibility = Visibility.Collapsed;
            logout.Visibility = Visibility.Visible;
            ShowPass.Visibility = Visibility.Collapsed;
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {

            Button but = sender as Button;
            string content = but.Content as string;
            //look for count as included in button label after (
            int pos = content.IndexOf('(');
            if (pos == -1)
                pos = content.Count();
            vm.reason = content.Substring(0,pos);

            string message = vm.saveQCS(Barcode.Text, vm.reason);
            if (message != string.Empty)
            {
                Errormsg.Text = message;
                ErrorTxt.Visibility = Visibility.Visible;
                return;
            }
            print popup = new print(vm);
            popup.ShowDialog();

            Barcode.Text = "";
            Barcode.Focus();
            BarcodeChars = 0;
            vm.BarcodeEntered = false;

        }
        private void Clear_Click(object sender, RoutedEventArgs e)
        {
            if (BarcodeChars > 0)
                bClearPressed = true;
            Barcode.Text = "";
            Barcode.Focus();
            BarcodeChars = 0;
            duedate.Text = " ";
            CustomerName.Text = " ";
            store.Text = " ";
            ErrorTxt.Visibility = Visibility.Collapsed;
            NoteBox.Visibility = Visibility.Collapsed;
            picture.Visibility = Visibility.Collapsed;
            

        }
        private void ImgButtonClick(object sender, RoutedEventArgs e)
        {
            ItemImage popup = new ItemImage();
            popup.picture = bitmap;
            popup.ShowDialog();

        }

        private void Integator_Click(object sender, RoutedEventArgs e)
        {
            if (vm.assemblyInfo == null)
            {
                Errormsg.Text = "Bar code was not entered";
                ErrorTxt.Visibility = Visibility.Visible;
                Barcode.Focus();
                return;
            }
            InterogatorInfo info = vm.getInfoForInterogator();
            if (info == null)
            {
                Errormsg.Text = "Customer database issue";
                ErrorTxt.Visibility = Visibility.Visible;
                Barcode.Focus();
                return;
            }
            Interogator popup = new Interogator(info);
            popup.ShowDialog();
        }

        private void Pass_Click(object sender, RoutedEventArgs e)
        {
            string label = ShowPass.Content as string;
            if (label == "Show Pass")
            {
                PassButton.Visibility = Visibility.Visible;
                ShowPass.Content = "Hide Pass";
                vm.SaveShowPass(1);
            }
            else
            {
                ShowPass.Content = "Show Pass";
                PassButton.Visibility = Visibility.Collapsed;
                vm.SaveShowPass(0);
            }

        }

        private void Totals_Click(object sender, RoutedEventArgs e)
        {
            totalsGrid.Visibility = Visibility.Visible;
        }

        private void SpotsTotal_Click(object sender, RoutedEventArgs e)
        {
            Details.ItemsSource = vm.GetQCSTotal("Spots");
        }

        private void Missing_Click(object sender, RoutedEventArgs e)
        {
            timer1.Stop();
            List<MissingItems>  items = vm.getMissingItems();
            if (items.Count == 0)
            {
                MessageBox.Show("no items to report");
                return;
            }
            timer1.Start();
            var dataTable = new DataTable();

            AddColumn(dataTable, "store", typeof(string));
            AddColumn(dataTable, "type", typeof(string));
            AddColumn(dataTable, "description", typeof(string));
            AddColumn(dataTable, "CustomerName", typeof(string));
            int row = 0;
            string store1 = items[0].store;
            foreach (MissingItems item in items)
            {
                var dataRow = dataTable.NewRow();
                //if change in store than add a line of blanks for seperation
                if (store1 != item.store)
                {
                    dataRow[0] = row.ToString();
                    dataRow[1] = "  ";
                    dataRow[2] = "  ";
                    dataRow[3] = "  ";
                    row = 0;
                    dataTable.Rows.Add(dataRow);
                    dataRow = dataTable.NewRow();
                }
                row++;
                dataRow[0] = item.store;
                dataRow[1] = item.qcsType;
                dataRow[2] = item.description;
                dataRow[3] = item.CustomerName;
                dataTable.Rows.Add(dataRow);
                store1 = item.store;
            }
            var columnWidths = new List<double>() { 80, 80, 300, 150 };
            var ht = new HeaderTemplate();
            var headerTemplate = XamlWriter.Save(ht);
            var printControl = PrintControlFactory.Create(dataTable, columnWidths, headerTemplate);
            printControl.ShowPrintPreview();
            

        }
        private void AddColumn(DataTable dataTable, string columnName, Type type)
        {
            var dataColumn = new DataColumn(columnName, type);
            dataTable.Columns.Add(dataColumn);
        }
    }
}
