using System.Text.RegularExpressions;
using System.Windows;
using WorkWithDataBase;

namespace TimeToTask
{
    public static class LoginVerification
    {
        public static bool Ckeck(string login, PostgreSql connect)
        {
            if (login.Length < 3 || login.Length > 40)
            {
                MessageBox.Show("Длина логина должна быть от 3 до 40 символов.", "Ошибка валидации", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (!Regex.IsMatch(login, @"^[a-zA-Z0-9._-]+$"))
            {
                MessageBox.Show("Логин может содержать только латинские буквы, цифры, точки, дефисы и символы подчеркивания.", "Ошибка валидации", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (connect.IsLoginExists(login))
            {
                MessageBox.Show("Пользователь с таким логином уже зарегистрирован.", "Ошибка регистрации", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            return true;
        }
    }
    public static class PasswordVerification
    {
        public static bool Check(string password)
        {
            if (password.Length < 8 || password.Length > 30)
            {
                MessageBox.Show("Длина пароля должна быть от 8 до 30 символов.", "Ошибка валидации", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            var hasUpperCase = new Regex(@"[A-ZА-ЯЁ]");
            var hasLowerCase = new Regex(@"[a-zа-яё]");
            var hasDecimalDigit = new Regex(@"[0-9]");

            if (!hasUpperCase.IsMatch(password))
            {
                MessageBox.Show("Пароль должен содержать хотя бы одну заглавную букву.", "Ошибка валидации", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            if (!hasLowerCase.IsMatch(password))
            {
                MessageBox.Show("Пароль должен содержать хотя бы одну строчную букву.", "Ошибка валидации", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            if (!hasDecimalDigit.IsMatch(password))
            {
                MessageBox.Show("Пароль должен содержать хотя бы одну цифру.", "Ошибка валидации", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            return true;
        }
        public static bool Equal(string password, string passwordReplay)
        {
            if(password != passwordReplay)
            {
                MessageBox.Show("Пароли не совпадают", "Ошибка валидации", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            return true;
        }
    }
    public static class EmailVerification
    {
        public static bool Check(string email, PostgreSql connect)
        {
            string emailPattern = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$";
            if (!Regex.IsMatch(email, emailPattern))
            {
                MessageBox.Show("Введите корректный адрес электронной почты (например, example@mail.com).", "Ошибка валидации", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (connect.IsEmailExists(email))
            {
                MessageBox.Show("Пользователь с такой электронной почтой уже зарегистрирован.", "Ошибка регистрации", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            return true;
        }
    }
}
