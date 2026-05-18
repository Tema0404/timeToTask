using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

            //Добавить проверки корректности пароля, почты(нет повтора), логина(нет повтора)

            PostgreSql connect = new PostgreSql();
            connect.AddNewUser(login, mail, password);
            int newUid = connect.GetUserId(login, password);
            UserItem newUser = new UserItem(newUid, login, password, mail);

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
    }
}
