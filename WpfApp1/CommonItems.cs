using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace TimeToTask
{
    public class DayItem : INotifyPropertyChanged
    {
        public DayOfWeek Day { get; set; }
        public string Name { get; set; }

        private bool _isSelected;
        public bool IsSelected
        {
            get => _isSelected;
            set { _isSelected = value; OnPropertyChanged(); }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    public class MonthItem
    {
        public DateTime MonthDate { get; set; }
        public string DisplayName { get; set; }
    }

    public class InputDialog : Window
    {
        private TextBox _textBox;
        public string Result { get; private set; }

        public InputDialog(string title, string message, string defaultValue = "")
        {
            Title = title;
            Width = 350;
            Height = 150;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;
            ResizeMode = ResizeMode.NoResize;
            Background = (Brush)Application.Current.Resources["WindowBackground"];
            BorderBrush = (Brush)Application.Current.Resources["BorderColor"];
            BorderThickness = new Thickness(1);

            Grid grid = new Grid();
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.Margin = new Thickness(10);

            TextBlock prompt = new TextBlock
            {
                Text = message,
                Foreground = (Brush)Application.Current.Resources["PrimaryTextColor"],
                FontSize = 14,
                Margin = new Thickness(0, 0, 0, 8)
            };
            Grid.SetRow(prompt, 0);

            _textBox = new TextBox
            {
                Text = defaultValue,
                Style = (Style)Application.Current.Resources["PastelTextBox"],
                Height = 28
            };
            Grid.SetRow(_textBox, 1);

            StackPanel buttons = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Right,
                Margin = new Thickness(0, 10, 0, 0)
            };
            Button okBtn = new Button { Content = "OK", Width = 70, Style = (Style)Application.Current.Resources["SmallPastelButton"] };
            Button cancelBtn = new Button { Content = "Отмена", Width = 70, Style = (Style)Application.Current.Resources["SmallPastelButton"], Margin = new Thickness(8, 0, 0, 0) };
            okBtn.Click += (s, ev) => { Result = _textBox.Text; DialogResult = true; Close(); };
            cancelBtn.Click += (s, ev) => { DialogResult = false; Close(); };
            buttons.Children.Add(okBtn);
            buttons.Children.Add(cancelBtn);
            Grid.SetRow(buttons, 2);

            grid.Children.Add(prompt);
            grid.Children.Add(_textBox);
            grid.Children.Add(buttons);
            Content = grid;
        }

        public static string Show(string title, string message, string defaultValue = "")
        {
            InputDialog dialog = new InputDialog(title, message, defaultValue);
            dialog.Owner = Application.Current.MainWindow;
            bool? result = dialog.ShowDialog();
            return (result == true) ? dialog.Result : null;
        }
    }
}