using DMS.Utilities.MailSender;
using System.DirectoryServices.AccountManagement;
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

namespace MailSender
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        string loggedUser = UserPrincipal.Current.DisplayName;
        string emailReceiver = "";
        List<string> emailReceivers = new List<string> { "", "", "" };
        EmailSender emailSender = new EmailSender();
       
        public MainWindow()
        {
            InitializeComponent();
            InitializeContactList();
            lblFullName.Content = $"Witaj, {loggedUser} !!";
            txtTitle.Text = "Default title to change";
            txtBody.Text = 
                $@"Default body to change 
...{"\n\r"}
Regards {"\n\r"}
{loggedUser}";
        }

        private void InitializeContactList()
        {
            foreach (var item in emailReceivers)
            {
                emailsList.Items.Add(item);
            }
        }
        private async void SendEmail_Click(object sender, RoutedEventArgs e)
        {
           await emailSender.SendEmail(loggedUser, txtTitle.Text, txtBody.Text, new EmailParams());
        }
    }
}