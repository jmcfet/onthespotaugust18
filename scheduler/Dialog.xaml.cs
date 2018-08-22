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

namespace scheduler
{
    /// <summary>
    /// Interaction logic for Dialog.xaml
    /// </summary>
    public partial class Dialog : Window  //
    {
        public Dialog()
        {
            InitializeComponent();
            Loaded += Dialog_Loaded;
        }

        private void Dialog_Loaded(object sender, RoutedEventArgs e)
        {
            InputTextBox.Focus();
        }

        public bool worked { get; set; }
        private void YesButton_Click(object sender, RoutedEventArgs e)
        {
            worked = false;
            if (InputTextBox.Text == "tennis")
            {
                worked = true;
            }
            this.Close();
            return ;

        }
    }
}
