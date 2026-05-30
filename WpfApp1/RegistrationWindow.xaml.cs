using System.Windows;
using System.Windows.Input;
using WorkWithDataBase;
using WpfApp1;

namespace TimeToTask
{
    public partial class RegistrationWindow : Window
    {
        public RegistrationWindow()
        {
            InitializeComponent();
        }
        private void EntryHyperLink_Click(object sender, RoutedEventArgs e)
        {
            EntryWindow entryWindow = new EntryWindow();
            entryWindow.Show();
            Application.Current.MainWindow = entryWindow;
            this.Close();
        }

        private void btnRegistration_Click(object sender, RoutedEventArgs e)
        {
            string login = txtgLogin.Text;
            string password = txtPassword.Password;
            string passwordReplay = txtPasswordReplay.Password;
            string email = txtMail.Text;
            PostgreSql connect = new PostgreSql();

            if (!LoginVerification.Ckeck(login, connect)) return;
            if (!PasswordVerification.Equal(password, passwordReplay)) return;
            if (!PasswordVerification.Check(password)) return;
            if (!EmailVerification.Check(email, connect)) return;
            

            connect.AddNewUser(login, email, password);
            int newUid = connect.GetUserId(login, password);
            UserItem newUser = new UserItem(newUid, login, password, email);

            MessageBox.Show(
                "Ваш аккаунт успешно создан",
                "Успешная регистрация",
                MessageBoxButton.OK,
                MessageBoxImage.Information
            );

            CalendarWithTasks calendarWindow = new CalendarWithTasks(newUser);
            calendarWindow.Show();
            Application.Current.MainWindow = calendarWindow;
            this.Close();
        }
        #region Логика кнопки-глазика для пароля
        private void btnTogglePassword_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            btnTogglePassword.Content = "🙈";
            btnTogglePasswordReplay.Content = "🙈";

            txtPasswordTextBox.Text = txtPassword.Password;
            txtPasswordTextBoxReplay.Text = txtPasswordReplay.Password;

            txtPassword.Visibility = Visibility.Collapsed;
            txtPasswordReplay.Visibility = Visibility.Collapsed;
            txtPasswordTextBox.Visibility = Visibility.Visible;
            txtPasswordTextBoxReplay.Visibility = Visibility.Visible;
            txtPasswordTextBox.Focus();

        }

        private void btnTogglePassword_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            ResetPasswordVisibility();
        }

        private void btnTogglePassword_MouseLeave(object sender, MouseEventArgs e)
        {
            ResetPasswordVisibility();
        }

        private void ResetPasswordVisibility()
        {
            if (txtPasswordTextBox.Visibility == Visibility.Visible || 
                txtPasswordTextBoxReplay.Visibility == Visibility.Visible)
            {
                btnTogglePassword.Content = "👁";
                btnTogglePasswordReplay.Content = "👁";

                txtPassword.Password = txtPasswordTextBox.Text;
                txtPasswordReplay.Password = txtPasswordTextBoxReplay.Text;

                txtPasswordTextBox.Visibility = Visibility.Collapsed;
                txtPasswordTextBoxReplay.Visibility = Visibility.Collapsed;

                txtPassword.Visibility = Visibility.Visible;
                txtPasswordReplay.Visibility = Visibility.Visible;

                txtPassword.Focus();
            }
        }

        #endregion
    }
}