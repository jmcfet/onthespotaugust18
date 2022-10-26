using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;

using System.Diagnostics;
using NLog;
using System.Configuration;
using DataAccessLayer.StoreDB;

namespace DataAccessLayer
{
    public class DBAccess
    {
        BCSContext db = new BCSContext();
        Logger logger = LogManager.GetLogger("OnTheSpot");
        StoreContext dbStore;
        

        List<string> connectionNames = new List<string>() { "Store1Context", "Store2Context", "Store3Context", "Store4Context" };
        public DBAccess(int storeid)
        {
           

            string storeconnectionstring = connectionNames[storeid-1];
            //retrieve the connection string from app.config
            ConnectionStringSettingsCollection connections = ConfigurationManager.ConnectionStrings;
            dbStore = new StoreContext(connections[storeconnectionstring].ConnectionString);
           

        }
        
        public OnTheSpot.Models.Employee GetEmployee(int empid)
        {
            Employee emp = null;
            string error = string.Empty;
            try
            {
                emp = dbStore.Employees.Where(e2 => e2.EmployeeID == empid).SingleOrDefault();
            }
            catch (Exception e)
            {
                error = "Critical Error: Could not open Categories DataBase";
            }

            return new OnTheSpot.Models.Employee()
                 {

                     FirstName = emp.FirstName,
                     LastName = emp.LastName
                 };
        }

        public ObservableCollection<OnTheSpot.Models.Category> GetCats(out string error)
        {
            ObservableCollection<OnTheSpot.Models.Category> modelCats = null;
            error = string.Empty;
            try
            {
                List<Category> dbCats = db.Categories.ToList();

                modelCats = new ObservableCollection<OnTheSpot.Models.Category>();
                foreach (Category cat in dbCats)
                {
                   
                    OnTheSpot.Models.Category model = new OnTheSpot.Models.Category()
                    {
                        ID = cat.ID,
                        Description = cat.Description,
                        Name = cat.Name
                    };
                    modelCats.Add(model);
                }
            }
            catch (Exception e)
            {
                error = "Critical Error: Could not open Categories DataBase";
            }

            return modelCats;
        }

        public ObservableCollection<OnTheSpot.Models.Category> GetBCSCats(out string error)
        {
            ObservableCollection<OnTheSpot.Models.Category> modelCats = null;
            //categorys were added for the QCS so need to remove for BCS
            List<string> QCSCats = new List<string>() { "Shirts", "bottoms", "tops", "Household" };
            error = string.Empty;
            try
            {
                List<Category> dbCats = db.Categories.ToList();

                modelCats = new ObservableCollection<OnTheSpot.Models.Category>();
                foreach (Category cat in dbCats)
                {
                    if (QCSCats.Contains(cat.Name))
                        continue;
                    OnTheSpot.Models.Category model = new OnTheSpot.Models.Category()
                    {
                        ID = cat.ID,
                        Description = cat.Description,
                        Name = cat.Name
                    };
                    modelCats.Add(model);
                }
            }
            catch (Exception e)
            {
                if (e.InnerException == null)
                    error = e.Message;
                else 
                    error = e.InnerException.Message;
            }

            return modelCats;
        }

        public List<User> GetUsers()
        {
            return db.Users.ToList();
        }
       
        public void SaveUser(User user)
        {
            db.Users.Add(user);
            db.SaveChanges();
        }
        public void DeleteUser(User user)
        {
         //   List<Item> items = db.Items.Where(i => i.BarCode == barcode).ToList();
            db.Users.Remove(user);
            db.SaveChanges();
        }

        public OnTheSpot.Models.Item getItem(string barcode)
        {
   //        Item item = db.Items.Where(i => i.BarCode == barcode).SingleOrDefault();
            //these will be no record of this barcode if this is the first time the item is scanned
            //when it is categorized there should only be 1 occurance but there seems to a bug where the uncategoryized
            //record is left in the BCS DB, so if we find more than 1 record look for the first that is NOT the uncategorized record
            //we will still throw if there is more than 1 found as we are really messed up
           Item item = null;
           List<Item> items = db.Items.Where(i => i.BarCode == barcode).ToList();
           if (items.Count == 0)
               return null;
           if (items.Count > 1)   //if there are multiple this is ok if all the same category
           {

               List<Category> distinct = items.Select(c => c.Category).Distinct().ToList();
               if (distinct.Count > 1)
                   throw new Exception("barcode cannot have mutliple categories");

           }
           item = items.First();

            OnTheSpot.Models.Item model = new OnTheSpot.Models.Item()
           {
               ID = item.ID,
               BarCode = item.BarCode,
               CustID = item.CustID,
               Note = item.Note,
               picture = item.picture

           };
         //   if (item.CreateDate != null)
                model.CreationDate = item.CreateDate;
           model.Category = new OnTheSpot.Models.Category()
           {
               ID = item.Category.ID,
               Description = item.Category.Description,
               Name = item.Category.Name
           };
            if (item.User != null)
            {
                model.User = new User()
                {
                    Id = item.User.Id,
                    Level = item.User.Level,
                    Name = item.User.Name,
                    Password = item.User.Password
                };
            }
           return model; 
        }

         public ObservableCollection<Item> GetItems()
        {
            List<Item> dbItems = db.Items.ToList();
            ObservableCollection<Item> modelItems = new ObservableCollection<Item>();
            foreach (Item item in dbItems)
            {
                Item model = new Item()
                {
                    ID = item.ID,
                    BarCode = item.BarCode,
                    CustID = item.CustID

                };
                model.Category = new Category()
                {
                    ID = item.Category.ID,
                    Description = item.Category.Description,
                    Name = item.Category.Name
                };
                modelItems.Add(model);
            }
            return modelItems;
        }
         public List<OnTheSpot.Models.Printer> getPrinters()
         {
             List<Printer> dbprinters = db.Printers.ToList();
             
             List<OnTheSpot.Models.Printer> printerModels = new List<OnTheSpot.Models.Printer>();
             foreach (Printer item in dbprinters)
             {
                OnTheSpot.Models.Printer model = new OnTheSpot.Models.Printer()
                 {
                     PrinterName = item.printerName,
                     Store = item.storename
                 };
                printerModels.Add(model);
             }
             return printerModels;            
         }
         public ObservableCollection<OnTheSpot.Models.Bin> GetBins()
         {
             List<Bin> dbBins = db.Bins.ToList();
             ObservableCollection<OnTheSpot.Models.Bin> modelItems = new ObservableCollection<OnTheSpot.Models.Bin>();
             foreach (Bin item in dbBins)
             {
                OnTheSpot.Models.Bin model = new OnTheSpot.Models.Bin()
                 {
                     ID = item.ID,
                     BarCode = item.BarCode,
 //                    Category = item.Category,
                     MaxWeight = item.MaxWeight,
                     PhidgetSlot = item.PhigidSlot

                 };
                 model.Category = new OnTheSpot.Models.Category()
                 {
                     ID = item.Category1.ID,
                     Description = item.Category1.Description,
                     Name = item.Category1.Name
                 };
                 modelItems.Add(model);
             }
             return modelItems;
         }
         public void SaveItem(OnTheSpot.Models.Item item, bool savingPicture = false)
         {
             
        //     logger.Info(string.Format("SaveItem {0}", item.ID));         
             Item dbItem = db.Items.Where(i => i.BarCode == item.BarCode).SingleOrDefault();
             if (dbItem == null)
             {
                
        //         logger.Info(string.Format("Create new"));  
                 dbItem = new Item() { BarCode = item.BarCode, CustID = item.CustID, CreateDate = item.CreationDate };
                 dbItem.CatID = item.Category.ID;
                if (item.User != null)
                    dbItem.User = item.User;
                 db.Items.Add(dbItem);
             }
             else
             {
       //          logger.Info(string.Format("Modify {0}", item.Category.ID));
                if (!savingPicture)
                {
                    if (dbItem.CatID == item.Category.ID)
                        return;
                    dbItem.CatID = item.Category.ID;
                    dbItem.CreateDate = item.CreationDate;
                    if (item.User != null)
                        dbItem.User = item.User;
                }
                else
                    dbItem.picture = item.picture;
             }
             db.SaveChanges();
         }
         public void SaveGssItem(OnTheSpot.Models.GSS gss)
         {

            //       logger.Info(string.Format("SaveItem {0}", gss.ID));
            GSS dbItem = new GSS() { barcode = gss.BarCode, bin = gss.bin, time = gss.CreationDate};
             db.GSSes.Add(dbItem);
             db.SaveChanges();
         }
        
        
        public void saveNote(string heatseal, string note)
         {
             Item item = db.Items.Where(i => i.BarCode == heatseal).SingleOrDefault();
             item.Note = note;
             db.SaveChanges();
         }
        public void saveCustNote(int CustId, string note1)
        {
            NoteForCustomer  note = db.CustomerNotes.Where(i => i.CustID == CustId).SingleOrDefault();
            if (note == null)
            {
                note = new NoteForCustomer() { CustID = CustId, note = note1 };
                db.CustomerNotes.Add(note);
            }
            else
                note.note = note1;
           
            db.SaveChanges();
        }
        public OnTheSpot.Models.Customer GetCustomerInfo(int WorkOrderNumber)
        {
            //Order order = dbStore.Orders.Where(o => o.OrderID == WorkOrderNumber).SingleOrDefault();
            //if (order == null)
            //{
            //    MessageBox("Work order {0} not found in database", WorkOrderNumber);
            //}
            //Customer cust =  dbStore.Customers.Where(c => c.CustomerID == order.CustomerID).SingleOrDefault();
            OnTheSpot.Models.Customer customer = null;
            try
            {
                customer = (from o in dbStore.Orders
                                                where o.OrderID == WorkOrderNumber
                                                join cust in dbStore.Customers on o.CustomerID equals cust.CustomerID
                                                select new OnTheSpot.Models.Customer() { FirstName = cust.FirstName, LastName = cust.LastName, Starch = cust.Starch }).SingleOrDefault();
            }
            catch (Exception e)
            {
            }

            return customer;
        }

         public OnTheSpot.Models.Customer GetCustomer(int id)
         {
            OnTheSpot.Models.Customer cust1 = (from cust in dbStore.Customers
                                         where cust.CustomerID == id
                                         
                                         select new OnTheSpot.Models.Customer() { FirstName = cust.FirstName, LastName = cust.LastName, Starch = cust.Starch }).SingleOrDefault();
            //cannot do a join across databases
            cust1.Note = db.CustomerNotes.Where(c => c.CustID == id).Select(c=>c.note).SingleOrDefault();
            return cust1;

         }

        public bool SaveBins(ObservableCollection<OnTheSpot.Models.Bin> CleaningBins)
        {
            foreach (OnTheSpot.Models.Bin bin in CleaningBins)
            {
                Bin binDB = db.Bins.Where(b => b.ID == bin.ID).SingleOrDefault();
                if (binDB == null)
                {
                    binDB = new Bin();
//                    binDB.Category1 = new Category();
                    db.Bins.Add(binDB);

                }
                binDB.MaxWeight = bin.MaxWeight;
                binDB.BarCode = bin.BarCode;
                binDB.PhigidSlot = bin.PhidgetSlot;
            }
                
            db.SaveChanges();
            return true;
        }
        public void SaveShowPass(int  Save)
        {

            Configuration   con = db.Configurations.First();
            con.ShowPass = Save;
            db.SaveChanges();
        }
        //there is only one config for the system
        public int? getPass()
        {
           
            return  db.Configurations.First().ShowPass;
           
        }
        //we used the barcode to get the cutomer info from assembly database as it knows which store the
        //info we need is 
        public OnTheSpot.Models.InterogatorInfo getInfoForInterogator(OnTheSpot.Models.AutoSortInfo  customerstuff)
        {
            double heatseal = double.Parse(customerstuff.HeatSeal);

            string storeconnectionstring = connectionNames[customerstuff.storeid - 1];
            //retrieve the connection string from app.config
            ConnectionStringSettingsCollection connections = ConfigurationManager.ConnectionStrings;

            StoreContext db = new StoreContext(connections[storeconnectionstring].ConnectionString);

            OnTheSpot.Models.InterogatorInfo info = (from cust in db.Customers
                                                     where cust.CustomerID == customerstuff.CustomerID
                                                     from heat in db.Heatseals
                                                     where heat.HeatsealID == heatseal
                                                     select new OnTheSpot.Models.InterogatorInfo() {  HomePhone = cust.HomePhone, Email= cust.EMail, InvoiceReminder = cust.InvReminder,
                     LastOrder = cust.LastOrder, Notes= cust.Notes, OpenDate = cust.OpenDate, HeatSealMessage = heat.Message }).SingleOrDefault();

            if (info == null)
                return null;
            List<Contact> contacts = (from contact in dbStore.Contacts
                                                      where contact.CustomerID == customerstuff.CustomerID
                                                         select contact).ToList();
            info.CustomerID = customerstuff.CustomerID.ToString();
            info.OpenDateString = info.OpenDate.Value.ToShortDateString();
            info.LastOrderString = info.LastOrder.Value.ToShortDateString();
            info.issues = new List<string>();
            info.resolutions = new List<string>();
            foreach(Contact contact  in contacts)
            {
                info.issues.Add(contact.Issue);
                info.resolutions.Add(contact.Resolution);
            }
            if (info.issues.Count > 0)
                info.currentIssue = info.issues[0];
            if (info.resolutions.Count > 0)
                info.currentResolution = info.resolutions[0];
            return info;
        }


    }


}

   
