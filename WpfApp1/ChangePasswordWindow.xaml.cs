using System;
using System.Windows;
using System.Windows.Controls;
using WorkWithDataBase;

namespace WpfApp1
{
    public partial class ChangePasswordWindow : Window
    {
        private int _userId;
        private PostgreSql _db;

        public ChangePasswordWindow(int userId)
        {
            InitializeComponent();
            _userId = userId;
            _db = new PostgreSql();

            // Инициализируем текстовые поля, чтобы синхронизировать их со скрытыми PasswordBox
            txtOldPassword.Visibility = Visibility.Collapsed;
            txtNewPassword.Visibility = Visibility.Collapsed;
        }

        // Синхронизация данных при ручном вводе в скрытый PasswordBox
        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (sender == pwdOldPassword)
                txtOldPassword.Text = pwdOldPassword.Password;
            else if (sender == pwdNewPassword)
                txtNewPassword.Text = pwdNewPassword.Password;
        }

        #region Переключатели видимости паролей (Глаз)

        private void ToggleOld_Checked(object sender, RoutedEventArgs e)
        {
            pwdOldPassword.Visibility = Visibility.Collapsed;
            txtOldPassword.Visibility = Visibility.Visible;
            txtOldPassword.Focus();
            if (txtOldPassword.Text.Length > 0)
                txtOldPassword.SelectionStart = txtOldPassword.Text.Length;
        }

        private void ToggleOld_Unchecked(object sender, RoutedEventArgs e)
        {
            txtOldPassword.Visibility = Visibility.Collapsed;
            pwdOldPassword.Visibility = Visibility.Visible;
            pwdOldPassword.Password = txtOldPassword.Text;
            pwdOldPassword.Focus();
        }

        private void ToggleNew_Checked(object sender, RoutedEventArgs e)
        {
            pwdNewPassword.Visibility = Visibility.Collapsed;
            txtNewPassword.Visibility = Visibility.Visible;
            txtNewPassword.Focus();
            if (txtNewPassword.Text.Length > 0)
                txtNewPassword.SelectionStart = txtNewPassword.Text.Length;
        }

        private void ToggleNew_Unchecked(object sender, RoutedEventArgs e)
        {
            txtNewPassword.Visibility = Visibility.Collapsed;
            pwdNewPassword.Visibility = Visibility.Visible;
            pwdNewPassword.Password = txtNewPassword.Text;
            pwdNewPassword.Focus();
        }

        #endregion

        // Кнопка отмены
        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        // Сохранение и валидация
        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            // Берем актуальный текст из TextBox (так как они всегда синхронизированы)
            string oldPassword = txtOldPassword.Text.Trim();
            string newPassword = txtNewPassword.Text.Trim();

            if (string.IsNullOrEmpty(oldPassword) || string.IsNullOrEmpty(newPassword))
            {
                MessageBox.Show("Пожалуйста, заполните все поля ввода.", "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (oldPassword == newPassword)
            {
                MessageBox.Show("Новый пароль не должен совпадать со старым.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                // Проверяем корректность старого пароля через метод авторизации в вашей БД
                var user = _db.AuthorizeUser(_userId.ToString(), oldPassword);

                if (user != null)
                {
                    // Вызываем метод смены пароля в вашем классе PostgreSql
                    _db.ChangePassword(_userId, newPassword);

                    MessageBox.Show("Пароль успешно обновлен!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                    this.Close();
                }
                else
                {
                    MessageBox.Show("Текущий пароль введен неверно.", "Ошибка авторизации", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при работе с базой данных: {ex.Message}", "Критическая ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}