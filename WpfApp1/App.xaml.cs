using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace TimeToTask
{
    /// <summary>
    /// Логика взаимодействия для App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static event System.Action<bool> ThemeChanged;

        public static void NotifyThemeChanged(bool isDark)
        {
            ThemeChanged?.Invoke(isDark);
        }
    }
}
