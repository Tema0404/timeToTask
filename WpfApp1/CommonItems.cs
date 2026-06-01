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
            Width = 360;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;
            ResizeMode = ResizeMode.NoResize;
            SizeToContent = SizeToContent.WidthAndHeight;
            Background = (Brush)Application.Current.Resources["WindowBackground"];
            BorderBrush = (Brush)Application.Current.Resources["BorderColor"];
            BorderThickness = new Thickness(1);

            var mainPanel = new StackPanel { Margin = new Thickness(16) };

            // Сообщение
            mainPanel.Children.Add(new TextBlock
            {
                Text = message,
                Foreground = (Brush)Application.Current.Resources["PrimaryTextColor"],
                FontSize = 13,
                Margin = new Thickness(0, 0, 0, 12)
            });

            // Поле ввода
            _textBox = new TextBox
            {
                Text = defaultValue,
                Height = 28,
                VerticalContentAlignment = VerticalAlignment.Center,
                Background = (Brush)Application.Current.Resources["InputBackground"],
                Foreground = (Brush)Application.Current.Resources["PrimaryTextColor"],
                BorderBrush = (Brush)Application.Current.Resources["BorderColor"],
                BorderThickness = new Thickness(1),
                CaretBrush = (Brush)Application.Current.Resources["PrimaryTextColor"],
                Margin = new Thickness(0, 0, 0, 16)
            };
            mainPanel.Children.Add(_textBox);

            // Кнопки
            var buttonsPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Right
            };
            var okBtn = new Button
            {
                Content = "OK",
                MinWidth = 70,
                Height = 28,
                Margin = new Thickness(0, 0, 8, 0),
                Style = (Style)Application.Current.Resources["SmallPastelButton"]
            };
            okBtn.Click += (s, e) => { Result = _textBox.Text; DialogResult = true; Close(); };
            var cancelBtn = new Button
            {
                Content = "Отмена",
                MinWidth = 70,
                Height = 28,
                Style = (Style)Application.Current.Resources["SmallPastelButton"]
            };
            cancelBtn.Click += (s, e) => { DialogResult = false; Close(); };
            buttonsPanel.Children.Add(okBtn);
            buttonsPanel.Children.Add(cancelBtn);
            mainPanel.Children.Add(buttonsPanel);

            Content = mainPanel;

            Loaded += (s, e) => _textBox.Focus();

            App.ThemeChanged += OnThemeChanged;
            this.Closed += (s, e) => App.ThemeChanged -= OnThemeChanged;
        }

        private void OnThemeChanged(bool isDark)
        {
            Background = (Brush)Application.Current.Resources["WindowBackground"];
            BorderBrush = (Brush)Application.Current.Resources["BorderColor"];
            if (_textBox != null)
            {
                _textBox.Background = (Brush)Application.Current.Resources["InputBackground"];
                _textBox.Foreground = (Brush)Application.Current.Resources["PrimaryTextColor"];
                _textBox.BorderBrush = (Brush)Application.Current.Resources["BorderColor"];
                _textBox.CaretBrush = (Brush)Application.Current.Resources["PrimaryTextColor"];
            }
            // Обновление цвета сообщения
            if (Content is StackPanel panel && panel.Children[0] is TextBlock prompt)
                prompt.Foreground = (Brush)Application.Current.Resources["PrimaryTextColor"];
        }

        public static string Show(string title, string message, string defaultValue = "")
        {
            var dialog = new InputDialog(title, message, defaultValue);
            return dialog.ShowDialog() == true ? dialog.Result : null;
        }
    }
}
