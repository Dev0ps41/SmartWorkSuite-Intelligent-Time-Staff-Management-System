using System.Windows;
using System.Windows.Controls;

namespace EmployerTimeManagement
{
    public partial class ChangePasswordControl : UserControl
    {
        public ChangePasswordControl()
        {
            InitializeComponent();
        }

        private void SavePassword_Click(object sender, RoutedEventArgs e)
        {
            // Εδώ λογική αλλαγής κωδικού
            MessageBox.Show("Ο κωδικός άλλαξε επιτυχώς!");
        }
    }
}
