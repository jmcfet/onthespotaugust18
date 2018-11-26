using DataAccessLayer;
using OnTheSpot.ViewModels;
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

namespace BCS
{
    /// <summary>
    /// Interaction logic for Login.xaml
    /// </summary>
    public partial class Login : Window
    {
        BCSandGSSVM vm;
        List<User> users = null;
        public Login(BCSandGSSVM vm)
        {
            this.vm = vm;
            users = vm.GetUsers();
            InitializeComponent();
        } 

        private void Login_Click(object sender, RoutedEventArgs e)
        {
            string userName = Name.Text;
            User user = users.Where(u => u.Name == userName && u.Password == Password.Text).SingleOrDefault();
            if (user == null)
            {
                MessageBox.Show("Invalid userName or password");
            }
            vm.UserLevel = user.Level;
            this.Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
