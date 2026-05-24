using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
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

        private void btnRegistration_Click(object sender, RoutedEventArgs e)
        {
            string login = txtgLogin.Text;
            string password = txtPassword.Password;
            string mail = txtMail.Text;
            PostgreSql connect = new PostgreSql();

            if (login.Length < 3 || login.Length > 40)
            {
                MessageBox.Show("Длина логина должна быть от 3 до 40 символов.", "Ошибка валидации", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!Regex.IsMatch(login, @"^[a-zA-Z0-9._-]+$"))
            {
                MessageBox.Show("Логин может содержать только латинские буквы, цифры, точки, дефисы и символы подчеркивания.", "Ошибка валидации", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (connect.IsLoginExists(login))
            {
                MessageBox.Show("Пользователь с таким логином уже зарегистрирован.", "Ошибка регистрации", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (password.Length < 8 || password.Length > 30)
            {
                MessageBox.Show("Длина пароля должна быть от 8 до 30 символов.", "Ошибка валидации", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var hasUpperCase = new Regex(@"[A-ZА-ЯЁ]");
            var hasLowerCase = new Regex(@"[a-zа-яё]");
            var hasDecimalDigit = new Regex(@"[0-9]");
            var hasSpecialChar = new Regex(@"[!@#$%^&*()_+=\[{\]};:<>|./?,-]");

            if (!hasUpperCase.IsMatch(password))
            {
                MessageBox.Show("Пароль должен содержать хотя бы одну заглавную букву.", "Ошибка валидации", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (!hasLowerCase.IsMatch(password))
            {
                MessageBox.Show("Пароль должен содержать хотя бы одну строчную букву.", "Ошибка валидации", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (!hasDecimalDigit.IsMatch(password))
            {
                MessageBox.Show("Пароль должен содержать хотя бы одну цифру.", "Ошибка валидации", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (!hasSpecialChar.IsMatch(password))
            {
                MessageBox.Show("Пароль должен содержать хотя бы один специальный символ (например, !, @, #, $, %).", "Ошибка валидации", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string emailPattern = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$";
            if (!Regex.IsMatch(mail, emailPattern))
            {
                MessageBox.Show("Введите корректный адрес электронной почты (например, example@mail.com).", "Ошибка валидации", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (connect.IsEmailExists(mail))
            {
                MessageBox.Show("Пользователь с такой электронной почтой уже зарегистрирован.", "Ошибка регистрации", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            connect.AddNewUser(login, mail, password);
            int newUid = connect.GetUserId(login, password);
            UserItem newUser = new UserItem(newUid, login, password, mail);

            MessageBox.Show(
                "Ваш аккаунт успешно создан",
                "Успешная регистрация",
                MessageBoxButton.OK,
                MessageBoxImage.Information
            );

            CalendarWithTasks calendarWindow = new CalendarWithTasks(newUser, null);
            calendarWindow.Show();
            Application.Current.MainWindow = calendarWindow;
            this.Close();
        }
        #region Логика кнопки-глазика для пароля
        private void btnTogglePassword_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            btnTogglePassword.Content = "🙈";
            txtPasswordTextBox.Text = txtPassword.Password;
            txtPassword.Visibility = Visibility.Collapsed;
            txtPasswordTextBox.Visibility = Visibility.Visible;
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
            if (txtPasswordTextBox.Visibility == Visibility.Visible)
            {
                btnTogglePassword.Content = "👁";
                txtPassword.Password = txtPasswordTextBox.Text;

                txtPasswordTextBox.Visibility = Visibility.Collapsed;
                txtPassword.Visibility = Visibility.Visible;

                txtPassword.Focus();
            }
        }

        #endregion
    }
}