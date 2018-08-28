using DataAccessLayer;
using DataAccessLayer.Data;
using DataAccessLayer.Models;
using OutlookCalendar.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
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

namespace scheduler
{
    /// <summary>
    /// Interaction logic for Sched.xaml
    /// </summary>
    public partial class Sched : UserControl
    {
        List<DataAccessLayer.Models.Appointment> allAppts;
        ObservableCollection<DataAccessLayer.Models.Appointment> today;
        CalendarAccess dbAccess;
        bool superuser = false;
        private DispatcherTimer DateChanging;
        private DateTime _yesturday = DateTime.Today;
        bool editMode = false;
        mybutton selectedDayButton = null;
        //   ObservableCollection<DataAccessLayer.Models.Appointment> events;
        public Sched()
        {
            InitializeComponent();
            Loaded += Sched_Loaded;
        }

        private void Sched_Loaded(object sender, RoutedEventArgs e)
        {
            // Register the Bubble Event Handler when calendar closes
            AddHandler(OutlookCalendar.Controls.Calendar.CalCosing,
                new RoutedEventHandler(CalendarClosed));
            dbAccess = new CalendarAccess();
            string error;
            //get all appointments
            allAppts = dbAccess.GetAppointments(out error);

            //filter those for today
            List<DataAccessLayer.Models.Appointment> temp = allAppts.Where(a => a.StartTime.ToShortDateString() == DateTime.Now.ToShortDateString()).ToList();
            today = new ObservableCollection<DataAccessLayer.Models.Appointment>(temp) ;
            lvDataBinding.ItemsSource = today;
            

            //populate the right pane with button for the next 6 days and highlite
            //any that have appointments
            populateRightPane();
            DateChanging = new DispatcherTimer();
            DateChanging.Interval = TimeSpan.FromHours(1);
            DateChanging.Tick += new EventHandler(timer_Tick);
            DateChanging.Start();

        }

        void populateRightPane()
        {
            DateTime now = DateTime.Now;
            future.Children.Clear();
            //populate the right pane with button for the next 6 days and highlite
            //any that have appointments
            for (int i = 1; i < 7; i++)
            {
                DateTime t1 = now.AddDays(i);
                mybutton dayevent = new mybutton();
                dayevent.Date = t1;
                dayevent.Content = t1.DayOfWeek.ToString() + " " + t1.Day.ToString();
                dayevent.Click += dayevent_Click;

                List<DataAccessLayer.Models.Appointment> thisday = allAppts.Where(a => a.StartTime.ToShortDateString() == t1.ToShortDateString()).ToList();

                if (thisday.Count() > 0)
                    dayevent.Foreground = new SolidColorBrush(Colors.Red);
                future.Children.Add(dayevent);
            }
            if (superuser)  //if super user then add button to select a date
            {

                mybutton dayevent = new mybutton();
                dayevent.Content = "Date Picker";
                dayevent.Click += ShowCalendar_Click; ;
                future.Children.Add(dayevent);
            }

        }

        private void ShowCalendar_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Controls.Calendar MonthlyCalendar = new System.Windows.Controls.Calendar();
            //  MonthlyCalendar.CalendarStyle = Microsoft.Windows.Controls.CalendarMode.Month;

            //   MonthlyCalendar.SelectionMode = CalendarSelectionMode.SingleRange;
            MonthlyCalendar.IsTodayHighlighted = true;
            MonthlyCalendar.SelectedDatesChanged += MonthlyCalendar_SelectedDatesChanged;
            //  MonthlyCalendar.DisplayDateStart =  DateTime.Now;
            mybutton dayevent = future.Children[future.Children.Count - 1] as mybutton;
            dayevent.Visibility = Visibility.Collapsed;
            future.Children.Add(MonthlyCalendar);

        }

        private void MonthlyCalendar_SelectedDatesChanged(object sender, SelectionChangedEventArgs e)
        {
            DateTime picked = (sender as System.Windows.Controls.Calendar).SelectedDate.Value;
            cal.Appointments = allAppts;   //set control appointments to all as it will filter
            cal.CurrentDate = picked;
            cal.Visibility = Visibility.Visible;
            future.Visibility = Visibility.Collapsed;

        }

        private void CalendarClosed(object sender,
              RoutedEventArgs e)
        {
            populateRightPane();
            future.Visibility = Visibility.Visible;
        }
        //the user has clicked on a day in the right grid. open a day scheuduler
        //for the selected day and show appointments if there are any except if in edit mode
        //then we show all appts for the selected day in left side so the user can delete events
        private void dayevent_Click(object sender, RoutedEventArgs e)
        {

            selectedDayButton = sender as mybutton;
            DateTime thisDate = (DateTime)(sender as mybutton).Date;
            cal.Appointments = allAppts;   //set control appointments to all as it will filter
            //currentdate is a dependency property on the calendar that will cause filtering by CurrentDate
            cal.CurrentDate = thisDate;
            if (!editMode)
            {
                cal.Visibility = Visibility.Visible;
                future.Visibility = Visibility.Collapsed;
            }
            else
            {
                List<DataAccessLayer.Models.Appointment> temp = allAppts.Where(a => a.StartTime.ToShortDateString() == thisDate.ToShortDateString()).ToList();
                today = new ObservableCollection<DataAccessLayer.Models.Appointment>(temp);
                lvDataBinding.ItemsSource = today;
              
                head.Text = thisDate.DayOfWeek.ToString() + " " + thisDate.Day.ToString();
            }
        }

        private void Calendar_AddAppointment(object sender, RoutedEventArgs e)
        {
            if (!superuser)
                return;
            DataAccessLayer.Models.Appointment appointment = new DataAccessLayer.Models.Appointment();
            appointment.Subject = "";
            double Hour = (e.OriginalSource as CalendarTimeslotItem).Hour;
            appointment.StartTime = new DateTime(cal.CurrentDate.Year, cal.CurrentDate.Month, cal.CurrentDate.Day, (int)Hour, 0, 0);
            appointment.EndTime = new DateTime(cal.CurrentDate.Year, cal.CurrentDate.Month, cal.CurrentDate.Day, (int)Hour + 1, 0, 0); ;

            AddAppointmentWindow aaw = new AddAppointmentWindow();
            aaw.DataContext = appointment;
            aaw.ShowDialog();
            if (appointment.Subject.Count() == 0)   
                return;
            allAppts.Add(appointment);
            dbAccess.SaveAppts(appointment);
            cal.Appointments = Filters.ByDate(allAppts, cal.CurrentDate).ToList();
            //we might have added an appointment for today so refresh the left side
            List<DataAccessLayer.Models.Appointment> today = allAppts.Where(a => a.StartTime.ToShortDateString() == DateTime.Now.ToShortDateString()).ToList();
            lvDataBinding.ItemsSource = today;
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {

            Dialog about1 = new Dialog();
            about1.ShowDialog();
            superuser = false;
            if (about1.worked == true)
            { 
                superuser = true;
                EditMode.Visibility = Visibility.Visible;
                login.Source = new BitmapImage(new Uri("Superman_shield.svg.png", UriKind.Relative));
                populateRightPane();
            }
        }
        //if we are in a new day and the app has not been restarted then refreash the left side to show events for today.
        void timer_Tick(object sender, EventArgs e)
        {
            DateTime target = DateTime.Today;
            if (_yesturday != target)
            {
                IEnumerable<DataAccessLayer.Models.Appointment> Appointments = Filters.ByDate(allAppts, target).ToList();
                lvDataBinding.ItemsSource = Appointments;
                _yesturday = DateTime.Today;
            }
        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            List<DataAccessLayer.Models.Appointment> toRemove = new List<DataAccessLayer.Models.Appointment>();  
            foreach (object o in lvDataBinding.SelectedItems)
            {

                DataAccessLayer.Models.Appointment appt = o as DataAccessLayer.Models.Appointment;
                dbAccess.DeleteAppointment(appt.Subject,appt.StartTime);
                toRemove.Add(appt);
            }
            toRemove.ForEach(t => allAppts.Remove(t));
            toRemove.ForEach(t => today.Remove(t));
            if (today.Count == 0)
            {
               
                if (selectedDayButton != null)
                    selectedDayButton.Foreground = new SolidColorBrush(Colors.Black);
                     
            }
            

            //we deleted an appointment for today so refresh the left side
            lvDataBinding.ItemsSource = today;
            cal.Appointments = today;
        }

        private void EditMode_Click(object sender, RoutedEventArgs e)
        {
            if (EditMode.Content.ToString() == "OFF")
            {
                Delete.Visibility = Visibility.Hidden;
                EditMode.Content = "Edit";
                editMode = false;
                //restore todaylv
                lvDataBinding.ItemsSource = today;
                head.Text = "Today";
            }
            else
            {
                Delete.Visibility = Visibility.Visible;
                EditMode.Content = "OFF";
                editMode = true;
            }
        }
    }
    class mybutton:Button
    {
        public mybutton()
        {
            this.SetResourceReference(StyleProperty, typeof(Button));
        }
        public DateTime Date { get; set; }
    }
}
