using System.Windows;
using WorkWithDataBase;

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
            string login = txtLogin.Text;
            string password = txtPassword.Visibility == Visibility.Visible
                ? txtPassword.Password
                : txtPasswordTextBox.Text;
            string passwordReplay = txtPasswordReplay.Visibility == Visibility.Visible
                ? txtPasswordReplay.Password
                : txtPasswordTextBoxReplay.Text;
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

        #region Переключатели видимости паролей (Глаз)
        private void ToggleFirst_Checked(object sender, RoutedEventArgs e)
        {
            txtPasswordTextBox.Text = txtPassword.Password;

            txtPassword.Visibility = Visibility.Collapsed;
            txtPasswordTextBox.Visibility = Visibility.Visible;

            txtPasswordTextBox.Focus();
            txtPasswordTextBox.SelectionStart = txtPasswordTextBox.Text.Length;
        }

        private void ToggleFirst_Unchecked(object sender, RoutedEventArgs e)
        {
            txtPassword.Password = txtPasswordTextBox.Text;

            txtPasswordTextBox.Visibility = Visibility.Collapsed;
            txtPassword.Visibility = Visibility.Visible;

            txtPassword.Focus();
        }

        private void ToggleReplay_Checked(object sender, RoutedEventArgs e)
        {
            txtPasswordTextBoxReplay.Text = txtPasswordReplay.Password;

            txtPasswordReplay.Visibility = Visibility.Collapsed;
            txtPasswordTextBoxReplay.Visibility = Visibility.Visible;

            txtPasswordTextBoxReplay.Focus();
            if (txtPasswordTextBoxReplay.Text.Length > 0)
                txtPasswordTextBoxReplay.SelectionStart = txtPasswordTextBoxReplay.Text.Length;
        }

        private void ToggleReplay_Unchecked(object sender, RoutedEventArgs e)
        {
            txtPasswordReplay.Password = txtPasswordTextBoxReplay.Text;

            txtPasswordTextBoxReplay.Visibility = Visibility.Collapsed;
            txtPasswordReplay.Visibility = Visibility.Visible;
            
            txtPasswordReplay.Focus();
        }

        #endregion
    }
}