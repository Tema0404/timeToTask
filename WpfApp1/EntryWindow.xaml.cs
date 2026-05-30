using System.Windows;
using WorkWithDataBase;
using WpfApp1;

namespace TimeToTask
{
    public partial class EntryWindow : Window
    {
        public EntryWindow()
        {
            InitializeComponent();
        }

        private void RegistrtionHyperLink_Click(object sender, RoutedEventArgs e)
        {
            RegistrationWindow registrationWindow = new RegistrationWindow();
            registrationWindow.Show();
            Application.Current.MainWindow = registrationWindow;
            this.Close();
        }

        private void btnEnterAcc_Click(object sender, RoutedEventArgs e)
        {
            string login = txtLogin.Text.Trim();
            string password = txtPassword.Visibility == Visibility.Visible
                ? txtPassword.Password
                : txtPasswordReveal.Text;
            if (string.IsNullOrEmpty(login) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Пожалуйста, заполните все поля для входа!", "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            PostgreSql connect = new PostgreSql();
            int uid = -1;
            uid = connect.GetUserId(login, password);
            if (uid == -1)
            {
                MessageBox.Show(
                    "Неверный логин или пароль",
                    "Ошибка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
                return;
            }

            UserItem currentUser = new UserItem(
                uid,
                login,
                password,
                connect.GetUserEmail(uid)
            );

            MainWindow mainWindow = new MainWindow(currentUser);
            mainWindow.Show();
            Application.Current.MainWindow = mainWindow;
            this.Close();
        }
        private void btnTogglePassword_Click(object sender, RoutedEventArgs e)
        {
            if (txtPassword.Visibility == Visibility.Visible)
            {
                txtPasswordReveal.Text = txtPassword.Password;
                txtPassword.Visibility = Visibility.Collapsed;
                txtPasswordReveal.Visibility = Visibility.Visible;
                btnTogglePassword.Content = "🙈";
                txtPasswordReveal.Focus();
                txtPasswordReveal.SelectionStart = txtPasswordReveal.Text.Length;
            }
            else
            {
                txtPassword.Password = txtPasswordReveal.Text;
                txtPasswordReveal.Visibility = Visibility.Collapsed;
                txtPassword.Visibility = Visibility.Visible;
                btnTogglePassword.Content = "👁";
                txtPassword.Focus();
            }
        }
    }
}
