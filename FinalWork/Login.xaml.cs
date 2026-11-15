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

namespace FinalWork
{
    /// <summary>
    /// Interaction logic for Login.xaml
    /// </summary>
    public partial class Login : Window
    {
        public Login()
        {
            InitializeComponent();
        }

        private void Button_Join(object sender, RoutedEventArgs e)
        {
            string login = LoginBox.Text;
            string password = Password.Text;

            using(Client_db.Clients_db db = new Client_db.Clients_db())
            {
                if(login == "" ||  password == "")
                {
                    MessageBox.Show("Write All");
                    return;
                }

                var loginn = db.Clients.FirstOrDefault(c => c.Login == login);

                if (loginn != null)
                {
                    var passwordd = db.Clients.FirstOrDefault(c => c.Password == password && login == c.Login);

                    if (passwordd != null)
                    {
                        Window_Clients clients = new Window_Clients(login);

                        clients.ShowDialog();

                        this.Close();
                        return;
                    }
                }

                MessageBox.Show("login not exists (Registration is required)!!!");
                Registration registration = new Registration();
                registration.ShowDialog();
                this.Close();

            }
        }
    }
}
