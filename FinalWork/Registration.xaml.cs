using Client_db;
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
    /// Interaction logic for Registration.xaml
    /// </summary>
    public partial class Registration : Window
    {
        
        public Registration()
        {
            InitializeComponent();
        }

        private void Button_Okay(object sender, RoutedEventArgs e)
        {
            string name = Name.Text;
            string login = Login.Text;
            string password = Password.Text;

            

            if(name == "" || login == "" || password == "")
            {
                MessageBox.Show("Please write all");
                return;
            }
            using(Client_db.Clients_db db = new Client_db.Clients_db())
            {
                var user = db.Clients.FirstOrDefault(x => x.Login == login);

                if(user != null)
                {
                    MessageBox.Show("This Login exists");
                    return;
                }

                if (login.Length <= 10)
                {
                    MessageBox.Show("login must be 10 symbols");
                    return;
                }

                if (!login.EndsWith("@gmail.com"))
                {
                    MessageBox.Show("Incorrect login");
                    return;
                }

                if(password.Length <= 8)
                {
                    MessageBox.Show("password must be > 8 symbol");
                    return;
                }

                Client client = new Client()
                {
                    Name = name,
                    Login = login,
                    Password = password,
                    IP = "Not Connected"
                };

                db.Clients.Add(client);
                db.SaveChanges();
            }

            MessageBox.Show("Registration was successful");

            Window_Clients clients = new Window_Clients(login);
            clients.ShowDialog();
            this.Close();            
        }
    }
}
