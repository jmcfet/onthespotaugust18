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
    /// Interaction logic for CreateUsers.xaml
    /// </summary>
    public partial class CreateUsers : Window
    {
        List<User> users = null;
        BCSandGSSVM vm;
        public CreateUsers(BCSandGSSVM vm)
        {
            this.vm = vm;
            InitializeComponent();
            users = vm.GetUsers();
        }

        private void Create_Click(object sender, RoutedEventArgs e)
        {
            User user = new User()
            {
                Name = Name.Text,
                Password = Password.Text
            };
            if (Name.Text.Count() == 0)
            {
                MessageBox.Show("user Name cannot be blank");
                return;
            }
            if (Password.Text.Count() == 0)
            {
                MessageBox.Show("password cannot be blank");
                return;
            }
            User user1 = users.Where(u => u.Name == Name.Text ).SingleOrDefault();
            if (user1 != null)
            {
                MessageBox.Show("User already exists");
            }
            else
            {
                user.Level = 2;
                vm.Saveuser(user);
            }
            UserInfo.Visibility = Visibility.Hidden;
            PassInfo.Visibility = Visibility.Hidden;
            Create.Visibility = Visibility.Hidden;
            Name.Text = "";
            Password.Text = "";
            CreateUser.Focus();

        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {

        }

        private void CreateUser_Click(object sender, RoutedEventArgs e)
        {
            UserInfo.Visibility = Visibility.Visible;
            PassInfo.Visibility = Visibility.Visible;
            Create.Visibility = Visibility.Visible;
        }

        private void DeleteUser_Click(object sender, RoutedEventArgs e)
        {
            UserInfo.Visibility = Visibility.Visible;
            Delete.Visibility = Visibility.Visible;
        }
        //delete a user
        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            User user1 = users.Where(u => u.Name == Name.Text).SingleOrDefault();
            if (user1 == null)
            {
                MessageBox.Show("User does not exist");
                return;
            }
            vm.Deleteuser(user1);
            UserInfo.Visibility = Visibility.Hidden;
            PassInfo.Visibility = Visibility.Hidden;
            Delete.Visibility = Visibility.Hidden;
            Name.Text = "";
        }
        //test
        private void Done_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
