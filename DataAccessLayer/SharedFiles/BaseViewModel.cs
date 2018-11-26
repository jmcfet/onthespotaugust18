using System;
using System.Linq;
using System.Text;
using OnTheSpot.Models;

using System.Collections.ObjectModel;

using System.Configuration;
using System.Collections.Generic;
using NLog;
using System.ComponentModel;
using System.Windows;
using DataAccessLayer;

namespace OnTheSpot.ViewModels
{
    
    public class BaseViewModel:INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected DBAccess db;
        AssemblyAccess AssemblyDB;
        public bool bControllerOn = false;
        public string WindowID;
        private bool _bTurnOnRegister = true;
        private bool _bLoggedIn = false;

        Logger logger = LogManager.GetLogger("OnTheSpot");
        
        public bool bLoggedIn
        {
            get { return _bLoggedIn; }
            set
            {
                if (_bLoggedIn != value)
                {
                    _bLoggedIn = value;
                    NotifyPropertyChanged("bLoggedIn");
                }
            }
        }
        private OnTheSpot.Models.Bin _bin;
        public OnTheSpot.Models.Bin bin
        {
            get { return _bin; }
            set
            {
                if (_bin != value)
                {
                    _bin = value;
                    NotifyPropertyChanged("bin");
                }
            }
        }
        
        private string _ClassifyButtonText;
        public string ClassifyButtonText
        {
            get { return _ClassifyButtonText; }
            set
            {
                if (_ClassifyButtonText != value)
                {
                    _ClassifyButtonText = value;
                    NotifyPropertyChanged("ClassifyButtonText");
                }
            }
        }
        private string _ReClassifyButtonText;
        public string ReClassifyButtonText
        {
            get { return _ReClassifyButtonText; }
            set
            {
                if (_ReClassifyButtonText != value)
                {
                    _ReClassifyButtonText = value;
                    NotifyPropertyChanged("ReClassifyButtonText");
                }
            }
        }
        private string _QuickReClassifyButtonText;
        public string QuickReClassifyButtonText
        {
            get { return _QuickReClassifyButtonText; }
            set
            {
                if (_QuickReClassifyButtonText != value)
                {
                    _QuickReClassifyButtonText = value;
                    NotifyPropertyChanged("QuickReClassifyButtonText");
                }
            }
        }
        private string _batchButtonText;
        public string BatchButtonText
        {
            get { return _batchButtonText; }
            set
            {
                if (_batchButtonText != value)
                {
                    _batchButtonText = value;
                    NotifyPropertyChanged("BatchButtonText");
                }
            }
        }
        bool barcodeEntered;

        public bool BarcodeEntered
        {
            get { return barcodeEntered; }
            set
            {
                if (barcodeEntered != value)
                {
                    barcodeEntered = value;
                    NotifyPropertyChanged("BarcodeEntered");
                }
            }
        }
        public OnTheSpot.Models.Category cat = null;
        public bool bReceivedAlready = false;
        public ObservableCollection<string> scancodes { get; set; }
        public ObservableCollection<string> errorcodes { get; set; }
        public ObservableCollection<OnTheSpot.Models.Bin> CleaningBins{ get; set; }
        public ObservableCollection<OnTheSpot.Models.Item> Items { get; set; }
        public ObservableCollection<OnTheSpot.Models.Category> CleaningCats { get; set; }
        public ObservableCollection<string> stores { get; set; }
        public Customer activeCustomer { get; set; }
        public OnTheSpot.Models.Category unknownCat;
        public string DBerrormsg = string.Empty;
        public BaseViewModel()
        {
           
            stores = new ObservableCollection<string>();

            stores.Add("store1");
            stores.Add("store2");
            stores.Add("store3");
            stores.Add("store4");
            scancodes = new ObservableCollection<string>();
            errorcodes = new ObservableCollection<string>();
            BarcodeEntered = false;

        }
        public bool bShowWorkorder = false;
        //there is a database per store
        public void OpenBCSandStoreDB()
        {

            int storeNum = 1;
            db = new DBAccess(storeNum);
   
        }

        public void OpenAssemblyDB()
        {
          
            AssemblyDB = new AssemblyAccess();
           
        }

        public List<User> GetUsers()
        {
            return db.GetUsers();
        }

        public void Saveuser(User user)
        {
            db.SaveUser(user);
        }
        public void Deleteuser(User user)
        {
            db.DeleteUser(user);
        }

        public void GetOurEntities()
        {
            logger.Info("GetOurEntities ");
          //  try
           // {
                CleaningCats = db.GetBCSCats(out DBerrormsg);
           // }
            if (DBerrormsg.Count() > 2)
            {
                
                return;
            }
 //           Items = db.GetItems();
            CleaningBins = db.GetBins();
            unknownCat = CleaningCats.Where(c => c.Name.StartsWith("Unclassified")).Single();
        }

        public void AddBin()
        {
            CleaningBins.Add(bin);

        }

        public void AddCat()
        {
            CleaningCats.Add(cat);
   //         persister.Cats.Add(cat);
        }

        public void SaveBins()
        {
            db.SaveBins(CleaningBins);

        }

        public AutoSortInfo getItemInAssemblyDB(string code)
        {
            List<string> storeNames = new List<string>() { "Haille", "Millhopper", "Westgate", "HuntersCrossing" };
            AutoSortInfo info =  AssemblyDB.GetCustomerInfoFromAssembly(code);
            if (info != null)
                info.storeName = storeNames[info.storeid-1];
            return info;
        }

        public OnTheSpot.Models.Item GetItemInDB(string code)
        {
            return db.getItem(code);
            

        }

        public List<OnTheSpot.Models.Printer> getPrinters()
        {
            return db.getPrinters();
        }

        public OnTheSpot.Models.Category getCat(string name)
        {

            return CleaningCats.Where(i => i.Name == name).SingleOrDefault();

        }
       

        public void SaveItem(OnTheSpot.Models.Item item,bool savingPicture=false)
        {
            //Item item1 = Items.Where(i => i.BarCode == item.BarCode).SingleOrDefault();
            //if (item1 == null)
            //    Items.Add(item);
            db.SaveItem(item, savingPicture);
        }

        //public void GetCustomer(int orderID)
        //{
        //    activeCustomer =  db.GetCustomerInfo(orderID);
        //}
        //unfortunately the customer info is saved in a store specific database so we
        //have to look in all stores to find the info
        public bool GetCustomer(int customerID)
        {
             string indexString = customerID.ToString();
            int storeID = Int32.Parse(indexString[0].ToString());
            if (storeID < 0 || storeID > 4)
                return false;
                     
            DBAccess db = new DBAccess(storeID);
            activeCustomer = db.GetCustomer(customerID);
            if (activeCustomer != null) return false;
            return true;
            
        }

      
        public void SaveShowPass(int  ShowPass)
        {
            db.SaveShowPass(ShowPass);
        }
       
        public int GetShowPass()
        {
            int? pass = db.getPass();
            return (int)pass;
        }
        protected void NotifyPropertyChanged(string propertyName)
        {
            if (this.PropertyChanged != null)
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

    }
    

        

}
