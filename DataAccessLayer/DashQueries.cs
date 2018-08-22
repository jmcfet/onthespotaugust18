using DataAccessLayer.Models;
using DataAccessLayer.StoreDB;
using NLog;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccessLayer
{
   
    public class DashQueries
    {
        BCSContext dbBCS = new BCSContext();
        Logger logger = LogManager.GetLogger("OnTheSpot");
        StoreContext dbStore1, dbStore2, dbStore3, dbStore4, db4OTS;
        DataAccessLayer.AssemblyDB.Assembly dbassembly = new DataAccessLayer.AssemblyDB.Assembly();
        List<string> connectionNames = new List<string>() { "Store1Context", "Store2Context", "Store3Context", "Store4Context" };
        public DashQueries()
        {
            ConnectionStringSettingsCollection connections = ConfigurationManager.ConnectionStrings;
            string StoreConnectionString = connections["Store1Context"].ConnectionString;
            dbStore1 = new StoreContext(StoreConnectionString);
            StoreConnectionString = connections["Store2Context"].ConnectionString;
            dbStore2 = new StoreContext(StoreConnectionString);
            StoreConnectionString = connections["Store3Context"].ConnectionString;
            dbStore3 = new StoreContext(StoreConnectionString);
            StoreConnectionString = connections["Store4Context"].ConnectionString;
            dbStore4 = new StoreContext(StoreConnectionString);

        }
        public List<CustomerInfo> getCPRCounts()
        {
            StoreContext dbStore;
            List<string> storeNames = new List<string>() { "Haile", "Millhopper", "Westgate", "HuntersCrossing" };
            List<CustomerInfo> storecounts = new List<CustomerInfo>();

            for (int storeid = 0; storeid < 4; storeid++)
            {
                dbStore = dbStore1;
                switch (storeid)
                {
                    case 1:
                        dbStore = dbStore2;
                        break;
                    case 2:
                        dbStore = dbStore3;
                        break;
                    case 3:
                        dbStore = dbStore4;
                        break;
                }

                List<CustomerInfo> info = FindInvoicesToCheck(dbStore, storeNames[storeid]);
                storecounts.AddRange(info);



            }
            return storecounts;
        }
        StoreContext GetDBStore(int storeid)
        {
            StoreContext dbStore = dbStore1;
            switch (storeid)
            {
                case 1:
                    dbStore = dbStore1;
                    break;
                case 2:
                    dbStore = dbStore2;
                    break;
                case 3:
                    dbStore = dbStore3;
                    break;
                case 4:
                    dbStore = dbStore4;
                    break;
            }
            return dbStore;
        }
        public List<missingPieceInfo> FindMissingOrders(string storeName)
        {


            List<missingPieceInfo> miss = new List<missingPieceInfo>();
            List<missingPieceInfo> missFilteredbyCustomerid = new List<missingPieceInfo>();
            StoreContext dbStore;

            //get all invoices for the last day
            List<DataAccessLayer.AssemblyDB.Invoice> allInvoices = (from inv in dbassembly.Invoices
                                                                    where DbFunctions.TruncateTime(inv.MarkInDate) >= DateTime.Today
                                                                    select inv).ToList();

            //group these by orderid
            var invsGroupedByOrderID = from inv2 in allInvoices
                                       group inv2 by inv2.OrderID into groupedinvs
                                       select groupedinvs;
            //Invoices are now grouped by orderid so at look at each group of invoices 
            foreach (var AssemblyInvGroup in invsGroupedByOrderID)
            {
                int storeid = AssemblyInvGroup.First().StoreID;
                dbStore = GetDBStore(storeid);

                //find all the orderdetail objects with this orderid and group by orderid
                var ordersGroup = from order in dbStore.OrderDetails
                                  where AssemblyInvGroup.Key == order.OrderID
                                  group order by order.OrderID into groupedby
                                  select groupedby;

                if (ordersGroup.Count() > 0)
                {

                    foreach (var orderDetailGroup in ordersGroup)
                    {

                        int piecesInOrderDetails = (int)orderDetailGroup.Sum(o => o.Pieces);
                        OrderDetail orderDetail = orderDetailGroup.First();
                        List<AssemblyDB.AutoSort> inAutoPieces = (from auto in dbassembly.AutoSorts
                                                                  where auto.CustomerID == orderDetail.CustomerID
                                                                  && DbFunctions.TruncateTime(auto.InvoiceDate) >= DateTime.Today
                                                                  group auto by auto.ArticleCode into groupbyarticle
                                                                  select groupbyarticle.FirstOrDefault()).ToList();

                        //ignore number pieces mismatch if the item not marked in
                        if (piecesInOrderDetails != inAutoPieces.Count && inAutoPieces.Count > 0)
                        {
                            missingPieceInfo info = new missingPieceInfo()
                            {
                                orderid = AssemblyInvGroup.Key,
                                numInvoiced = inAutoPieces.Count.ToString(),
                                numOrders = piecesInOrderDetails,
                                storeid = orderDetail.StoreID,
                                customerid = orderDetail.CustomerID,
                                date = orderDetail.DueDate.ToString()
                            };

                            miss.Add(info);


                        }
                    }


                }
            }


            //we have the possible mismatches between orderdetails pieces and autosort now group by customerid. there 
            //might be multiple mismatches for same customer in which case we need to look at group as a whole
            //    
            var groupedbyCust = miss.GroupBy(g => g.customerid).ToList();
            foreach (var missGroup in groupedbyCust)
            {
                //these customer might have multiple orders 
                if (missGroup.Count() > 1)
                {
                    if ((int)missGroup.Sum(o => o.numOrders) != Int32.Parse(missGroup.First().numInvoiced))
                    {
                        missGroup.First().numInvoiced = string.Format("Mult work orders {0}", missGroup.First().numInvoiced);
                        missFilteredbyCustomerid.Add(missGroup.First());
                    }
                }
                else
                    missFilteredbyCustomerid.Add(missGroup.First());
            }

            return missFilteredbyCustomerid.OrderBy(o => o.storeid).ToList(); ;
        }

        private List<CustomerInfo> FindInvoicesToCheck(StoreContext dbStore, string storeName)
        {
            var q1 = from inv in dbStore.Invoices
                     where inv.BaggerMemo != null && inv.PickupDate == null && inv.Rack != null && inv.Rack.ToLower() != "bagged"
                     from cust in dbStore.Customers
                     where inv.CustomerID == cust.CustomerID
                     select new CustomerInfo() { storeName = storeName, FirstName = cust.FirstName, LastName = cust.LastName, rack = inv.Rack, invoiceID = inv.InvoiceID, invmemo = cust.InvReminder, baggermemo = inv.BaggerMemo };

            List<CustomerInfo> invinfo = q1.ToList();

            //now get all the processed invoices that passed
            List<int> processed = (from c in dbBCS.CPRs
                                   where c.state == 1 && c.store == storeName
                                   select c.invoiceid).ToList();
            List<int> invPaided = (from i in dbStore.InvPaids
                                   select i.InvoiceID).ToList();
            //if invoice was processed then remove
            invinfo = (from inv in invinfo
                       where !processed.Contains(inv.invoiceID)
                       orderby inv.rack
                       select inv).ToList();


            return invinfo;

        }
        // find orders lost on the conveyor due to a missing rack location.
        public List<OrdersLostOnRacktoMissingRackLocationData> OrdersLostOnRacktoMissingRackLocation()
        {
            StoreContext CurrentContext = null;

            List<OrdersLostOnRacktoMissingRackLocationData> ListData = new List<OrdersLostOnRacktoMissingRackLocationData>();
            for (int i = 1; i <= 4; i++)
            {
                CurrentContext = GetDBStore(i);
                DateTime prev = DateTime.Today.AddDays(-7).Date;
                var invs = from inv in CurrentContext.Invoices
                           
                           where DbFunctions.TruncateTime(inv.DueDate) >= prev.Date
                           && inv.Rack == null && inv.DepartmentID != 5 && inv.PaidAmount == 0
                           select inv;

                foreach (Invoice inv in invs)
                {
                    //might have been voided so check in InvoiceDetail
                    InvoiceDetail detail = (from id in CurrentContext.InvoiceDetails
                                            where id.InvoiceID == inv.InvoiceID
                                            && id.Voided == true
                                            select id).FirstOrDefault();
                    if (detail == null)   //nope then we want it
                    {
                        OrdersLostOnRacktoMissingRackLocationData data = new OrdersLostOnRacktoMissingRackLocationData()
                        {
                            dueDate = inv.DueDate,
                            invoiceID = inv.InvoiceID,
                            storeID = i
                        };
                        ListData.Add(data);
                    }

                }

            }
            return ListData.OrderBy(o => o.storeID).ToList();
        }
        public List<ShirtInfo> getItemCount(string type, int plusdays)
        {
            DateTime dueDate = DateTime.Now.AddDays(plusdays);
            if (dueDate.DayOfWeek == DayOfWeek.Friday)    //if Friday move to Monday      
                dueDate.AddDays(2);
            List<int> ids = (from cat in dbBCS.Categories
                             where cat.Name == type
                             join fab in dbBCS.OTISIdsToFabIds on cat.ID equals fab.CatID
                             select fab.FabID).ToList();

            var q1 = from inv in dbassembly.InvoiceDetails
                     where ids.Contains(inv.GarmentID)
                     join auto in dbassembly.AutoSorts on inv.ArticleCode equals auto.ArticleCode
                     where DbFunctions.TruncateTime(auto.DueDate) >= DateTime.Today && DbFunctions.TruncateTime(auto.DueDate) <= dueDate
                     && auto.Status == "R"
                     select new ShirtInfo { articleID = inv.ArticleCode, invoiceID = inv.InvoiceID, dueDate = auto.DueDate };


            return q1.ToList();

        }
        public List<GarmentIds> getTypes()
        {


            List<DataAccessLayer.AssemblyDB.InvoiceDetail> distinctPeople = dbassembly.InvoiceDetails
                .GroupBy(p => p.GarmentID)
                .Select(g => g.FirstOrDefault())
                .ToList();
            List<GarmentIds> gids = new List<GarmentIds>();
            foreach (DataAccessLayer.AssemblyDB.InvoiceDetail inv in distinctPeople)
            {
                GarmentIds g1 = new GarmentIds() { garmentID = inv.GarmentID, desc = inv.DetailDesc };
                gids.Add(g1);
            }

            return gids;
        }
    }
}


