using BCS.Views;
using NLog;
using OnTheSpot.Models;
using OnTheSpot.ViewModels;
using Phidgets;
using Phidgets.Events;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace BCS
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml  testing
    /// </summary>
    public partial class MainWindow : Window
    {
        BCSandGSSVM vm;
        static InterfaceKit ifKit;
        DispatcherTimer PhigTimer;
        Cursor oldcursor = null;
        Logger logger = LogManager.GetLogger("BCS");
       
        public MainWindow()
        {
            InitializeComponent();
            Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            
            vm =  new BCSandGSSVM(true);
            this.Title = "On the Spot BCS";
            passwordBox1.Visibility = Visibility.Visible;
            label4.Visibility = Visibility.Visible;
            label1.Visibility = Visibility.Visible;
            passwordEntered.Visibility = Visibility.Visible;
            led1.Visibility = Visibility.Visible;
           
            vm.bControllerOn = false;
            logger.Info("BCS start");

            PhigTimer = new DispatcherTimer();
            PhigTimer.Interval = TimeSpan.FromSeconds(5);
            PhigTimer.Tick += new EventHandler(timer_Tick);
            PhigTimer.Start();
            ifKit = new InterfaceKit();

            //Hook the basica event handlers
            ifKit.Attach += new AttachEventHandler(PhidgitController_Attach);
            oldcursor = this.Cursor;
            this.Cursor = Cursors.Wait;
            //Open the object for device connections
            ifKit.open();
            
            DataContext = vm;
           
        }
       
        //for the system to be totally operational we must get a response from the controller and
        //have database connectivity. If we have just database working then allow degraded operation
        void timer_Tick(object sender, EventArgs e)
        {
            //phidgit controller might be offline in which case warn the user and check database
            //operation as we can allow partial operation if the DB is operational
            logger.Info("timer_Tick");
            PhigTimer.IsEnabled = false;
            CheckSystemState(false);



        }
        //phidgit controller has signalled that it is good to go
        void PhidgitController_Attach(object sender, Phidgets.Events.AttachEventArgs e)
        {
            logger.Info("PhidgitController_Attach");
            PhigTimer.IsEnabled = false;
            CheckSystemState(true);

        }
        void PhidgitController_Detach(object sender, Phidgets.Events.DetachEventArgs e)
        {
            logger.Info("PhidgitController_Detach");
            vm.bControllerOn = false;
            //            if (vm.errormsg == string.Empty)

        }

       
        void CheckSystemState(bool PhidgitState)
        {
            logger.Info("CheckSystemState start");
            if (PhidgitState || vm.bSimulatePhigetsMode)
            {
                this.Dispatcher.Invoke(new Action(delegate ()    //use dispatcher to update UI
                {
                    textBlock1.Text = "Phidget operational .. waiting for database connection";
                    vm.bControllerOn = true;
                    if (!vm.bSimulatePhigetsMode)
                    {
                        led1.ColorOn = Colors.Green;
                        led1.IsActive = true;
                    }
                    label1.Background = Brushes.Green;
                    DBErrorMsg.Text = string.Empty;


                }));
            }
            else
            {

                DBErrorMsg.Text = "Phidgit controller not operational";
                textBlock1.Text = "System Initialization Failed";
                led1.ColorOn = Colors.Red;
                led1.IsActive = true;  //test

            }


            vm.OpenBCSandStoreDB();
            vm.OpenAssemblyDB();
            vm.GetOurEntities();
            


            this.Dispatcher.Invoke(new Action(delegate ()     //use dispatcher to update UI
            {
                  this.Cursor = oldcursor;
                if (vm.DBerrormsg == string.Empty)
                {
                    led2.ColorOn = Colors.Green;
                    led2.IsActive = true;
                    
                    textBlock1.Text = "System Initialization Complete";
                    
                    LoadedDone.Visibility = Visibility.Visible;
                    LoadingState.Visibility = Visibility.Collapsed;
                    
                    ShowBCSUI();


                }
                else   //system is not in running state
                {
                    DBErrorMsg.Text = vm.DBerrormsg;
                    textBlock1.Text = "System Initialization Failed";
                   
                        Errormsg.Text = vm.DBerrormsg;
                        ErrorTxt.Visibility = Visibility.Visible;
                    //    ErrorTxt.Background = new SolidColorBrush(Colors.Red);
                        //  MessageBox.Show(vm.DBerrormsg);

                    
                    led2.ColorOn = Colors.Red;
                    led2.IsActive = true;
                    
                }
                

            }));
            logger.Info("CheckSystemState end");

        }
       

        private void version_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("On The Spot BCS version 1.8 Jan 1 2018");
        }

        private void passwordEntered_Click(object sender, RoutedEventArgs e)
        {
            string pass = passwordBox1.Password;
            passwordBox1.Password = string.Empty;
            if (pass == "tennis")
            {
                vm.bLoggedIn = true;

                vm.ClassifyButtonText = "Admin Off";
                vm.ReClassifyButtonText = "ReClassify On";
                vm.QuickReClassifyButtonText = "Quick reclassify On";
                vm.BatchButtonText = "Batch On";
                BatchBCS.Visibility = Visibility.Visible;
                QuickReClassify.Visibility = Visibility.Visible;
                Classify.Visibility = Visibility.Visible;
                if (ifKit == null)
                    ifKit = new Phidgets.InterfaceKit();

                vm.bControllerOn = false;
                logger.Info("BCS start");
                ReClassify.Visibility = Visibility.Visible;

                SetFocusBarcode();


            }

        }
        private void Classify_Click(object sender, RoutedEventArgs e)
        {
            vm.bLoggedIn = false;
            ReClassify.Visibility = Visibility.Collapsed;
            Classify.Visibility = Visibility.Collapsed;
            QuickReClassify.Visibility = Visibility.Collapsed;
        }
        //allows admin person to reclassify without BCS feedback
        private void QickClassify_Click(object sender, RoutedEventArgs e)
        {

            if (vm.QuickReClassifyButtonText == "Quick Reclassify Off")
                vm.QuickReClassifyButtonText = "Quick Reclassify On";
            else
                vm.QuickReClassifyButtonText = "Quick Reclassify Off";
            if (vm.bSimulatePhigetsMode == false)
            {
                vm.bSimulatePhigetsMode = true;
            //later    if (registerView == null)
          //      {
           //         registerView = new RegisterItem();
               //     Views.Children.Add(registerView);
                //    if (vm.bLoggedIn)
                //    {
                //        ReClassify.Visibility = Visibility.Visible;
                //        Classify.Visibility = Visibility.Visible;
                //    }
                //}
            }
      //later      registerView.SetFocusBarcode();

        }
        private void BatchBCS_Click(object sender, RoutedEventArgs e)
        {

            if (vm.BatchButtonText == "Batch Off")
            {
                vm.BatchButtonText = "Batch On";
                BatchOff();
                ButRow1.Children.Clear();
                ButRow2.Children.Clear();
                ReClassify.Visibility = Visibility.Visible;
                QuickReClassify.Visibility = Visibility.Visible;
                CustomerNote.Visibility = Visibility.Hidden;
            }
            else
            {
                vm.BatchButtonText = "Batch Off";
                vm.bSimulatePhigetsMode = true;
             
                Barcode.IsEnabled = false;
                ShowCategoryButtons();
                Errormsg.Text = "select a bin";
                ErrorTxt.Visibility = Visibility.Visible;
                ErrorTxt.Background = new SolidColorBrush(Colors.Gray);
                ReClassify.Visibility = Visibility.Collapsed;
                QuickReClassify.Visibility = Visibility.Collapsed;

            }

            SetFocusBarcode();

        }
        //show buttons for all the categorys.
        public void ShowCategoryButtons()
        {
            int ii = 0;
            ButRow1.Visibility = Visibility.Visible;   //batch renders these inVisible
            ButRow2.Visibility = Visibility.Visible;
            ButRow1.Children.Clear();
            ButRow2.Children.Clear();
            foreach (Category cat in vm.CleaningCats)
            {
                if (cat.ID == vm.unknownCat.ID)
                    continue;
                ii++;
                Button but = new Button() { Content = cat.Name, Height = ButtonHeight, Width = ButtonWidth, Visibility = Visibility.Visible };
                but.Margin = new Thickness(0, 5, 5, 5);

                but.Click += new RoutedEventHandler(but_Click);
                if (ii < 5)
                    ButRow1.Children.Add(but);
                else
                    ButRow2.Children.Add(but);
            }
        }

        //let an admin change the items classification
        private void Reclassify_click(object sender, RoutedEventArgs e)
        {
            vm.ReClassifyButtonText = "ReClassify Off";
            SetFocusBarcode();

        }
        public void BatchOff()
        {
            foreach (string barcode in vm.scancodes)
            {
                Item item = vm.GetItemInDB(barcode);
                item.Category = catForBatch;
                vm.SaveItem(item);
            }
            ErrorTxt.Visibility = Visibility.Hidden;
            Codes.Visibility = Visibility.Collapsed;
            ErrorCodes.Visibility = Visibility.Collapsed;  //test

            CustomerName.Text = " ";
            vm.scancodes.Clear();
            ClearInput();

        }


        //all the ducks are lined up so show the BCS UI and start classifying
        DispatcherTimer timer2 = null;
        AutoSortInfo assemblyInfo = null;
        
        int BarcodeChars = 0;
        int ButtonHeight = 100;
        int ButtonWidth = 140;
       
        bool bNextDay = false;
        Bin TargetBin = null;
        string CleaningCat = string.Empty;
        Category catForBatch = null;
        Item item = null;
        void ShowBCSUI()
        {
            timer2 = new DispatcherTimer();
            timer2.Interval = TimeSpan.FromSeconds(1);
            timer2.Tick += new EventHandler(timer2_Tick);
            vm.bReceivedAlready = false;
            TopGrid.Visibility = Visibility.Visible;
            Barcode.Focus();
            turnoffLights();
            Barcode.TextChanged += new TextChangedEventHandler(Barcode_TextChanged);
            ifKit.InputChange += new Phidgets.Events.InputChangeEventHandler(PhidgitController_InputChange);
            ifKit.SensorChange += new Phidgets.Events.SensorChangeEventHandler(PhidgitController_SensorChange);
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

            if (BarcodeChars == 0)
            {
                Debug.WriteLine("Timer start");

                timer2.Start();
                if (vm.BatchButtonText != "Batch Off")
                    ErrorTxt.Visibility = Visibility.Hidden;
            }
            BarcodeChars++;

        }
        List<string> testCodes = new List<string>() { "1000437154", "1000437155", "1000446919", "1000446918", "1000446917", "1000446916", "1000446915", "1000446914" };
        int dumcode = 0;
        public void SetFocusBarcode()
        {

            Keyboard.Focus(Barcode);

        }
        bool ProcessBarcode()
        {

            double itemCode = 0;


       //     Barcode.Text = testCodes[dumcode++];      //DANGER
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
        void timer2_Tick(object sender, EventArgs e)
        {

            timer2.Stop();
            if (!ProcessBarcode())
                return;
            string barcode = Barcode.Text;

            //check that the item is in the Assembly database if not then show message and allow more items
            //to be scanned
            try
            {
                assemblyInfo = vm.getItemInAssemblyDB(barcode);
                if (assemblyInfo == null)
                {
                    ClearInput();
                    Errormsg.Text = string.Format(string.Format("Item has not been marked in {0}", barcode));
                    ErrorTxt.Background = new SolidColorBrush(Colors.Red);
                    ErrorTxt.Visibility = Visibility.Visible;
                    if (vm.BatchButtonText == "Batch Off")
                    {
                        vm.errorcodes.Add(barcode);
                        ErrorCodes.Visibility = Visibility.Visible;
                    }
                    return;
                }
                vm.GetCustomer(assemblyInfo.CustomerID);
                NoteButton.Visibility = Visibility.Visible;
                NoteButton.Content = "Add Note";
                CustomerNote.Visibility = Visibility.Hidden;
                if (vm.activeCustomer.Note != null)
                {
                    CustomerNote.Text = vm.activeCustomer.Note;
                    CustomerNote.Visibility = Visibility.Visible;
                    NoteButton.Content = "Edit Note";
                    System.Media.SystemSounds.Beep.Play();
                }
                CustomerName.Text = vm.activeCustomer.FirstName + " " + vm.activeCustomer.LastName;
                
                    //check if the item is in BCS Codes
                    item = vm.GetItemInDB(barcode);
                    //if we are in batch mode then it must be already in BCS 
                    if (item == null && vm.BatchButtonText == "Batch Off")
                    {
                        vm.errorcodes.Add(barcode);
                        ErrorCodes.Visibility = Visibility.Visible;
                        ErrorTxt.Background = new SolidColorBrush(Colors.Red);
                        Errormsg.Text = string.Format(string.Format("item {0} not in BCS", barcode));
                        ErrorTxt.Visibility = Visibility.Visible;
                        ClearInput();
                        return;
                    }
                
            }
            catch (Exception e1)
            {
                BarcodeChars = 0;
                Errormsg.Text = string.Format(string.Format("Database logic error {0} {1}", barcode, e1.Message));
                ErrorTxt.Visibility = Visibility.Visible;
                return;
            }
            logger.Info(string.Format("all data obtained for {0} ", barcode));
            if (vm.BatchButtonText == "Batch Off")
            {
                vm.scancodes.Add(barcode);
                Codes.Visibility = Visibility.Visible;
                Errormsg.Text = string.Format("Scan {0} items and hit Batch off when finished", catForBatch.Name);
                ErrorTxt.Background = new SolidColorBrush(Colors.Gray);
                ClearInput();
                return;
            }
            

            bool route = false;
            for (int i = 0; i < assemblyInfo.rfid.Length; i++)
            {
                if (assemblyInfo.rfid[i] != ' ')
                {
                    route = true;
                    break;
                }

            }
            if (!route)     //only look for next day if this is not a routed customer
                bNextDay = vm.getNextdayBin(assemblyInfo);

            if (bNextDay)
            {
                ButRow1.Children.Clear();
                ButRow1.Visibility = Visibility.Visible;
                logger.Info("new item");
                item = new Item() { BarCode = barcode, CustID = 0, ID = -1, CreationDate = DateTime.Now };
                Button but = new Button() { Content = "Next Day Work", Height = ButtonHeight, Width = ButtonWidth, Visibility = Visibility.Visible };
                but.Margin = new Thickness(0, 5, 5, 5);
                //             but.Click += new RoutedEventHandler(but_Click);
                item.Category = vm.CleaningCats.Where(c => c.ID == 11).Single();
                ButRow1.Children.Add(but);
                SetupActions();
                return;
            }
            //a new item set the category to Unclassified or laundry
            if (item == null)
            {

                ButRow1.Children.Clear();
                ButRow1.Visibility = Visibility.Visible;
                logger.Info("new item");
                item = new Item() { BarCode = barcode, CustID = 0, ID = -1, CreationDate = DateTime.Now };
                Button but = new Button() { Content = "Unclassified Work", Height = ButtonHeight, Width = ButtonWidth, Visibility = Visibility.Visible };
                but.Margin = new Thickness(0, 5, 5, 5);
                but.Click += new RoutedEventHandler(but_Click);
                if (assemblyInfo.dept != "Laundry")     //must be unclassifield dry cleaning
                {
                    logger.Info("dry cleaning");
                    item.Category = vm.CleaningCats.Where(c => c.Name.StartsWith("Unclassified")).Single();
                }
                else
                {
                    logger.Info("laundry");
                    if (vm.activeCustomer.Starch == "H" || vm.activeCustomer.Starch == "M")
                    {
                        but.Content = "Starched Shirts";

                    }
                    else
                    {
                        but.Content = "No Starch Shirts";

                    }
                    //get category so we can place in the right bin
                    item.Category = vm.CleaningCats.Where(c => c.Name == but.Content.ToString()).Single();
                }
                ButRow1.Children.Add(but);
                SetupActions();

            }
            else
            {
                //known item but first check if it is unknown as it needs to be categorized before proceeding
                ButRow1.Children.Clear();
                ButRow2.Children.Clear();
                ButRow1.Visibility = Visibility.Visible;
                if (item.Category.ID == vm.unknownCat.ID || vm.ReClassifyButtonText == "ReClassify Off" || vm.QuickReClassifyButtonText == "Quick Reclassify Off")
                {
                    logger.Info("Classify");

                    vm.ReClassifyButtonText = "ReClassify On";    //this is a one time deal


                    if (!vm.bLoggedIn)
                    {
                        Errormsg.Text = "Classify is a adminstrator action";
                        ErrorTxt.Visibility = Visibility.Visible;
                        Barcode.Text = string.Empty;
                        BarcodeChars = 0;
                        return;
                    }
                    ButRow2.Visibility = Visibility.Visible;
                    int ii = 0;
                    string[] QCSCats = new string[] { "shirts", "bottoms", "tops", "household" };
                    foreach (Category cat in vm.CleaningCats)
                    {
                        if (cat.ID == vm.unknownCat.ID)
                            continue;
                        ii++;
                        Button but = new Button() { Content = cat.Name, Height = ButtonHeight, Width = ButtonWidth, Visibility = Visibility.Visible };
                        but.Margin = new Thickness(0, 5, 5, 5);

                        but.Click += new RoutedEventHandler(but_Click);
                        if (ii < 5)
                            ButRow1.Children.Add(but);
                        else
                            ButRow2.Children.Add(but);
                    }
                }
                else    //allow the garment to be placed in a bin
                {
                    Button but = new Button() { Height = ButtonHeight, Width = ButtonWidth, Visibility = Visibility.Visible, IsEnabled = false };
                    if (assemblyInfo.dept != "Laundry")     //if this is  dry cleaning
                    {
                        Category cat = vm.CleaningCats.Where(c => c.ID == item.Category.ID).SingleOrDefault();
                        but.Content = cat.Name;
                    }
                    else    //laundry then get the starch from the customer info as they might have changed in fabricare
                    {
                        if (vm.activeCustomer.Starch == "H" || vm.activeCustomer.Starch == "M")
                            but.Content = "Starched Shirts";

                        else
                            but.Content = "No Starch Shirts";
                        //set category so we can place in the right bin in fabricare setting
                        item.Category = vm.CleaningCats.Where(c => c.Name == but.Content.ToString()).Single();
                    }
                    ButRow1.Children.Add(but);
                    SetupActions();


                }
            }



        }

        void but_Click(object sender, RoutedEventArgs e)
        {


            string catName = (string)(sender as Button).Content;
            catForBatch = vm.CleaningCats.Where(c => c.Name == catName).Single();

            if (vm.BatchButtonText == "Batch Off")
            {
                Errormsg.Text = string.Format("Start scaning {0} items and hit Batch off when finished", catName);
                Barcode.IsEnabled = true;
                ButRow1.Visibility = Visibility.Collapsed;
                ButRow2.Visibility = Visibility.Collapsed;


                return;
            }
            item.Category = catForBatch;
            if (vm.QuickReClassifyButtonText == "Quick Reclassify Off")    //if in QuickClassy mode then do not wait for feedback
            {
                vm.SaveItem(item);
                ClearItemFields();
                CustomerName.Text = "";

                vm.QuickReClassifyButtonText = "Quick Reclassify On";
                return;
            }
            SetupActions();
        }

        private void Next_Click(object sender, RoutedEventArgs e)
        {

            //          Action.Text = "Place in Lighted bin";
            //         Weight.Visibility = Visibility.Hidden;
            //turn on the right light
            ifKit.outputs[TargetBin.PhidgetSlot] = true;
            vm.bReceivedAlready = false;
            Wizard.Visibility = Visibility.Hidden;
        }
        void ClearInput()
        {
            Barcode.Text = "";
            Barcode.Focus();
            BarcodeChars = 0;
            NoteButton.Visibility = Visibility.Hidden;
        }
        void ClearItemFields()
        {
            grid1.Visibility = Visibility.Hidden;
            ButRow1.Visibility = Visibility.Collapsed;
            ButRow2.Visibility = Visibility.Collapsed;
            Barcode.Text = "";
            BarcodeChars = 0;     //reenable the timer for reading barcode
            NoteButton.Visibility = Visibility.Hidden;
            Barcode.Focus();

        }
        void SetupActions()
        {

            grid1.Visibility = Visibility.Visible;
            Category cat = item != null ? item.Category : null;
            //each bin represents a category of cleaning 
            TargetBin = vm.getBin(cat, assemblyInfo);
            if (TargetBin == null )
            {
                if (cat != null)
                    MessageBox.Show(string.Format("Bin not found for category {0}", item.Category.Name));
                else
                    MessageBox.Show(string.Format("Bin not found for customer {0} Call Manager!", assemblyInfo.CustomerID));
                return;
            }
            Action.Text = "Place in Lighted bin";
            logger.Info("turn on bin light" + TargetBin.PhidgetSlot.ToString());
            if (ifKit.Attached)
            {
                turnoffLights();    //in case on is in the wrong state
                ifKit.outputs[TargetBin.PhidgetSlot] = true;
            }
            vm.bReceivedAlready = false;
            Wizard.Visibility = Visibility.Hidden;
            if (vm.BCSMode)
            {
                if (item.Category.ID != 11)     //do not save next day info as next time the item might NOT be next day
                    vm.SaveItem(item);
            }
            else
            {
                GSS gssItem = new GSS() { CreationDate = DateTime.Now, bin = TargetBin.PhidgetSlot, BarCode = TargetBin.BarCode };
                vm.SaveItemGSS(gssItem);
            }
        }

        //a piece of laundry has been dropped into a bin, the index tells us which one
        void PhidgitController_InputChange(object sender, Phidgets.Events.InputChangeEventArgs e)
        {
            logger.Info(string.Format("input change {0}", e.Index));

            int slot = e.Index;
            if (TargetBin == null)
                return;
            //{
            //    this.Dispatcher.Invoke(new Action(delegate ()
            //    {
            //        ClearItemFields();
            //        Errormsg.Text = "Target slot unknown  ... Please rescan item";
            //        ErrorTxt.Visibility = Visibility.Visible;

            //    }));
            //    return;

            //}
            //is item in the right slot
            if (slot != TargetBin.PhidgetSlot)
            {

                this.Dispatcher.Invoke(new Action(delegate ()
                {

                    Errormsg.Text = "Please place item in right bin";
                    ErrorTxt.Visibility = Visibility.Visible;
                }));
            }
            else
                this.Dispatcher.Invoke(new Action(delegate ()
                {
                    //this event is fired multiple times 
                    if (vm.bReceivedAlready == false)
                    {
                        ErrorTxt.Visibility = Visibility.Hidden;
                        logger.Info("write possibile changes to database");
                        if (vm.BCSMode)
                        {
                            if (item.Category.ID != 11)     //do not save next day info as next time the item might NOT be next day)
                                vm.SaveItem(item);
                        }
                        else
                        {
                            GSS gssItem = new GSS() { CreationDate = DateTime.Now, bin = TargetBin.PhidgetSlot, BarCode = TargetBin.BarCode };
                            vm.SaveItemGSS(gssItem);
                        }
                        ClearItemFields();
                        CustomerName.Text = "";

                    }
                    ifKit.outputs[TargetBin.PhidgetSlot] = false;
                    vm.bReceivedAlready = true;

                }));

        }
        void PhidgitController_SensorChange(object sender, Phidgets.Events.SensorChangeEventArgs e)
        {
            Debug.WriteLine("sensor change {0}", e.Index);
        }
        private void Cleartext_Click(object sender, RoutedEventArgs e)
        {
            Barcode.Text = "";
            Barcode.Focus();
            BarcodeChars = 0;

        }
        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            //check if the light was turned on
            if (ifKit.Attached)
            {
                if (ifKit.outputs[TargetBin.PhidgetSlot] == true)
                    ifKit.outputs[TargetBin.PhidgetSlot] = false;
            }
            ClearItemFields();
        }
        private void turnoffLights()
        {
            if (ifKit.outputs.Count > 0)
            {
                for (int i = 0; i < 8; i++)
                    ifKit.outputs[i] = false;
            }
        }

        private void Note_Click(object sender, RoutedEventArgs e)
        {
            if (assemblyInfo == null)
                return;
            GetNote popup = new GetNote(vm);
            popup.ShowDialog();
            if (vm.Note == null || vm.Note.Count() == 0)
                return;
            vm.AddNote(assemblyInfo.CustomerID, vm.Note);
            CustomerNote.Text = vm.Note;
            CustomerNote.Visibility = Visibility.Visible;
        }
    }
}
