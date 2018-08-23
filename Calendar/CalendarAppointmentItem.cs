using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace OutlookCalendar.Controls
{      
    public class CalendarAppointmentItem : ButtonBase
    {    
        static CalendarAppointmentItem()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(CalendarAppointmentItem), new FrameworkPropertyMetadata(typeof(CalendarAppointmentItem)));
        }

        #region StartTime/EndTime

        public static readonly DependencyProperty StartTimeProperty =
            TimeslotPanel.StartTimeProperty.AddOwner(typeof(CalendarAppointmentItem));

        public bool StartTime
        {
            get { return (bool)GetValue(StartTimeProperty); }
            set { SetValue(StartTimeProperty, value); }
        }

        public static readonly DependencyProperty EndTimeProperty =
            TimeslotPanel.EndTimeProperty.AddOwner(typeof(CalendarAppointmentItem));

        public bool EndTime
        {
            get { return (bool)GetValue(EndTimeProperty); }
            set { SetValue(EndTimeProperty, value); }
        }

        #endregion

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            UIElement container = VisualTreeHelper.GetParent(this) as UIElement;
            Point relativeLocationInContainer = this.TranslatePoint(new Point(0, 0), container);
            //find the timeslot that was clicked (44 is fixed height of item)
            //Hour = relativeLocationInContainer.Y / 44;

            //RaiseAddAppointmentEvent();


        }
    }
}
