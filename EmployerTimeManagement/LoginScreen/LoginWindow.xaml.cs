using System.Windows;

namespace EmployerTimeManagement.LoginScreen
{
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            string username = UsernameTextBox.Text.Trim();
            string password = PasswordBox.Password;

            if (username == "1" && password == "1")
            {
                MessageBox.Show("✅ Επιτυχής σύνδεση!");
                this.DialogResult = true;
                this.Close();
            }

            else
            {
                ErrorTextBlock.Visibility = Visibility.Visible;
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
    }
}