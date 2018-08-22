using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using DataAccessLayer.Models;

namespace OutlookCalendar.Controls
{
    public class Calendar : Control
    {
        static Calendar()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(Calendar), new FrameworkPropertyMetadata(typeof(Calendar)));
            CommandManager.RegisterClassCommandBinding(typeof(Calendar), new CommandBinding(Back, new ExecutedRoutedEventHandler(OnExecutedBack), new CanExecuteRoutedEventHandler(OnCanExecuteBack)));
            CommandManager.RegisterClassCommandBinding(typeof(Calendar), new CommandBinding(NextDay, new ExecutedRoutedEventHandler(OnExecutedNextDay), new CanExecuteRoutedEventHandler(OnCanExecuteNextDay)));
            CommandManager.RegisterClassCommandBinding(typeof(Calendar), new CommandBinding(PreviousDay, new ExecutedRoutedEventHandler(OnExecutedPreviousDay), new CanExecuteRoutedEventHandler(OnCanExecutePreviousDay)));
            
        }

        #region AddAppointment

        public static readonly RoutedEvent AddAppointmentEvent =
            CalendarTimeslotItem.AddAppointmentEvent.AddOwner(typeof(CalendarDay));

        public event RoutedEventHandler AddAppointment
        {
            add
            {
                AddHandler(AddAppointmentEvent, value);
            }
            remove
            {
                RemoveHandler(AddAppointmentEvent, value);
            }
        }

        #endregion

        #region Appointments

        public static readonly DependencyProperty AppointmentsProperty =
            DependencyProperty.Register("Appointments", typeof(IEnumerable<Appointment>), typeof(Calendar),
            new FrameworkPropertyMetadata(null, new PropertyChangedCallback(Calendar.OnAppointmentsChanged)));

        public IEnumerable<Appointment> Appointments
        {
            get { return (IEnumerable<Appointment>)GetValue(AppointmentsProperty); }
            set { SetValue(AppointmentsProperty, value); }
        }

        private static void OnAppointmentsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((Calendar)d).OnAppointmentsChanged(e);
        }

        protected virtual void OnAppointmentsChanged(DependencyPropertyChangedEventArgs e)
        {
            FilterAppointments();
        }

        #endregion

        #region CurrentDate

        /// <summary>
        /// CurrentDate Dependency Property
        /// </summary>
        public static readonly DependencyProperty CurrentDateProperty =
            DependencyProperty.Register("CurrentDate", typeof(DateTime), typeof(Calendar),
                new FrameworkPropertyMetadata((DateTime)DateTime.Now,
                    new PropertyChangedCallback(OnCurrentDateChanged)));

        /// <summary>
        /// Gets or sets the CurrentDate property.  This dependency property 
        /// indicates ....
        /// </summary>
        public DateTime CurrentDate
        {
            get { return (DateTime)GetValue(CurrentDateProperty); }
            set { SetValue(CurrentDateProperty, value); }
        }

        /// <summary>
        /// Handles changes to the CurrentDate property.
        /// </summary>
        private static void OnCurrentDateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((Calendar)d).OnCurrentDateChanged(e);
        }

        /// <summary>
        /// Provides derived classes an opportunity to handle changes to the CurrentDate property.
        /// </summary>
        protected virtual void OnCurrentDateChanged(DependencyPropertyChangedEventArgs e)
        {
            FilterAppointments();
        }

        #endregion

        private void FilterAppointments()
        {   
            DateTime byDate = CurrentDate;
            CalendarDay day = this.GetTemplateChild("day") as CalendarDay;
            day.ItemsSource = Appointments.ByDate(byDate);
            CalendarLedger test = this.GetTemplateChild("Slots") as CalendarLedger;
            //         day.ItemsSource = Appointments;
            TextBlock dayHeader = this.GetTemplateChild("dayHeader") as TextBlock;
            dayHeader.Text =    byDate.Date.ToLongDateString();

        }
      
    // This creates a static property on the UserControl, SettingsConfirmedEvent, which 
    // will be used by the Window, or any control up the Visual Tree, that wants to 
    // handle the event. This is the Custom Routed Event for more info on the 
    // RegisterRoutedEvent method
    // https://msdn.microsoft.com/en-us/library/ms597876(v=vs.100).aspx

        public static readonly RoutedEvent CalCosing =
        EventManager.RegisterRoutedEvent("CalCosing", RoutingStrategy.Bubble,
        typeof(RoutedEventHandler), typeof(Calendar));

        // Create RoutedEventHandler
        // This adds the Custom Routed Event to the WPF Event System and allows it to be 
        // accessed as a property from within xaml if you so desire

        public event RoutedEventHandler SettingConfirmed
        {
            add { AddHandler(CalCosing, value); }
            remove { RemoveHandler(CalCosing, value); }
        }
        #region NextDay/PreviousDay

        public static readonly RoutedCommand NextDay = new RoutedCommand("NextDay", typeof(Calendar));
        public static readonly RoutedCommand PreviousDay = new RoutedCommand("PreviousDay", typeof(Calendar));
        public static readonly RoutedCommand Back = new RoutedCommand("Back", typeof(Calendar));

        private static void OnCanExecuteBack(object sender, CanExecuteRoutedEventArgs e)
        {
            ((Calendar)sender).OnCanExecuteBack(e);
        }

        private static void OnExecutedBack(object sender, ExecutedRoutedEventArgs e)
        {
            ((Calendar)sender).OnExecutedBack(e);
        }

        private static void OnCanExecuteNextDay(object sender, CanExecuteRoutedEventArgs e)
        {
            ((Calendar)sender).OnCanExecuteNextDay(e);
        }

        private static void OnExecutedNextDay(object sender, ExecutedRoutedEventArgs e)
        {
            ((Calendar)sender).OnExecutedNextDay(e);
        }

        protected virtual void OnCanExecuteNextDay(CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
            e.Handled = false;
        }

        protected virtual void OnCanExecuteBack(CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
            e.Handled = false;
        }

        protected virtual void OnExecutedNextDay(ExecutedRoutedEventArgs e)
        {
            CurrentDate += TimeSpan.FromDays(1);
            e.Handled = true;
        }
        protected virtual void OnExecutedBack(ExecutedRoutedEventArgs e)
        {
            this.Visibility = Visibility.Collapsed;
            // Raise the custom routed event, this fires the event from the UserControl
            RaiseEvent(new RoutedEventArgs(Calendar.CalCosing));
        }
        private static void OnCanExecutePreviousDay(object sender, CanExecuteRoutedEventArgs e)
        {
            ((Calendar)sender).OnCanExecutePreviousDay(e);
        }

        private static void OnExecutedPreviousDay(object sender, ExecutedRoutedEventArgs e)
        {
            ((Calendar)sender).OnExecutedPreviousDay(e);
        }

        protected virtual void OnCanExecutePreviousDay(CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
            e.Handled = false;
        }

        protected virtual void OnExecutedPreviousDay(ExecutedRoutedEventArgs e)
        {
            CurrentDate -= TimeSpan.FromDays(1);
            e.Handled = true;
        }

        #endregion
    }
}
