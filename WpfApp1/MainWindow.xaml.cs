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
using WpfApp1;

namespace TimeToTask
{
    
    public partial class MainWindow : Window
    {
        private UserItem _currentUser = new UserItem();
        public MainWindow(UserItem user)
        {
            InitializeComponent();
            _currentUser = user;
            tbCurrentDate.Text = DateTime.Now.ToShortDateString();
        }

        private void btnToCalendar_Click(object sender, RoutedEventArgs e)
        {
            CalendarWithTasks daysWindow = new CalendarWithTasks(_currentUser);
            daysWindow.Show();
            Application.Current.MainWindow = daysWindow;
            this.Close();
        }
    }
}
