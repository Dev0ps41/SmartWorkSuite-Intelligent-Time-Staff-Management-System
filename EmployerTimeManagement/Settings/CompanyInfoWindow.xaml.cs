using EmployerTimeManagement.Data;
using EmployerTimeManagement.Models;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace EmployerTimeManagement.Settings
{
    public partial class CompanyInfoControl : UserControl
    {
        private readonly AppDbContext _context;
        private CompanyInfo _companyInfo;

        public CompanyInfoControl()
        {
            InitializeComponent();
            _context = new AppDbContext();
            LoadCompanyInfo();
        }

        private void LoadCompanyInfo()
        {
            _companyInfo = _context.CompanyInfos.FirstOrDefault();

            if (_companyInfo != null)
            {
                txtName.Text = _companyInfo.EmployerName;
                txtAFM.Text = _companyInfo.AFM;
                txtPhone.Text = _companyInfo.Phone;
                txtEmail.Text = _companyInfo.Email;
                txtBranchId.Text = _companyInfo.BranchId;
                txtComment.Text = _companyInfo.Comment;

                // ΕΡΓΑΝΗ πεδία
                txtErganiUsername.Text = _companyInfo.ErganiUsername;
                txtErganiPassword.Password = _companyInfo.ErganiPassword;
                txtErganiAA.Text = _companyInfo.ErganiAA;
                txtApiKey.Text = _companyInfo.ErganiApiKey;

                chkUseProduction.IsChecked = _companyInfo.UseProductionApi;
            }
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (_companyInfo == null)
            {
                _companyInfo = new CompanyInfo();
                _context.CompanyInfos.Add(_companyInfo);
            }

            _companyInfo.EmployerName = txtName.Text;
            _companyInfo.AFM = txtAFM.Text;
            _companyInfo.Phone = txtPhone.Text;
            _companyInfo.Email = txtEmail.Text;
            _companyInfo.BranchId = txtBranchId.Text;
            _companyInfo.Comment = txtComment.Text;

            // ΕΡΓΑΝΗ πεδία
            _companyInfo.ErganiUsername = txtErganiUsername.Text;
            _companyInfo.ErganiPassword = txtErganiPassword.Password;
            _companyInfo.ErganiAA = txtErganiAA.Text;
            _companyInfo.ErganiApiKey = txtApiKey.Text;

            _companyInfo.UseProductionApi = chkUseProduction.IsChecked ?? false;

            _context.SaveChanges();

            MessageBox.Show("Τα στοιχεία αποθηκεύτηκαν επιτυχώς!", "Επιτυχία", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}
