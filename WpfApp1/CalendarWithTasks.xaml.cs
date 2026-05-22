using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using WorkWithDataBase;
using WpfApp1;

namespace TimeToTask
{
    public partial class CalendarWithTasks : Window
    {
        private DateTime _currentMonth;
        private DateTime? _selectedDate;
        private readonly ObservableCollection<TaskItem> _allTasks = new ObservableCollection<TaskItem>();
        private readonly ObservableCollection<DayItem> _dayItems;
        private readonly List<MonthItem> _months;
        private bool _isDarkTheme = false;

        private PostgreSql _db;

        private UserItem _currentUser;
        public CalendarWithTasks(UserItem user)
        {
            InitializeComponent();
            _currentUser = user;
            _db = new PostgreSql();
            _currentMonth = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
            _selectedDate = null;

            _dayItems = new ObservableCollection<DayItem>
            {
                new DayItem { Day = DayOfWeek.Monday, Name = "Пн" },
                new DayItem { Day = DayOfWeek.Tuesday, Name = "Вт" },
                new DayItem { Day = DayOfWeek.Wednesday, Name = "Ср" },
                new DayItem { Day = DayOfWeek.Thursday, Name = "Чт" },
                new DayItem { Day = DayOfWeek.Friday, Name = "Пт" },
                new DayItem { Day = DayOfWeek.Saturday, Name = "Сб" },
                new DayItem { Day = DayOfWeek.Sunday, Name = "Вс" }
            };
            DayOfWeekComboBox.ItemsSource = _dayItems;

            _months = new List<MonthItem>();
            for (int i = 0; i <= 12; i++)
            {
                DateTime monthStart = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1).AddMonths(i);
                _months.Add(new MonthItem { MonthDate = monthStart, DisplayName = monthStart.ToString("MMMM yyyy") });
            }
            MonthComboBox.ItemsSource = _months;
            MonthComboBox.DisplayMemberPath = "DisplayName";
            MonthComboBox.SelectedValuePath = "MonthDate";

            ApplyTheme(false);
            RefreshCalendar();
            
            LoadTasksFromDb();
            ApplyTheme(false);
            RefreshCalendar();
        }
        private void LoadTasksFromDb()
        {
            _allTasks.Clear();
            var tasksFromDb = _db.GetTasksByUser(_currentUser.id);
            foreach (var t in tasksFromDb)
                _allTasks.Add(t);
        }

        private void ApplyTheme(bool isDark)
        {
            _isDarkTheme = isDark;
            var resources = Application.Current.Resources;

            if (isDark)
            {
                resources["WindowBackground"] = new SolidColorBrush(Color.FromRgb(0x2B, 0x2B, 0x2B));
                resources["PrimaryTextColor"] = new SolidColorBrush(Color.FromRgb(0xE0, 0xE0, 0xE0));
                resources["SecondaryTextColor"] = new SolidColorBrush(Color.FromRgb(0xA0, 0xA0, 0xA0));
                resources["WeekendTextColor"] = new SolidColorBrush(Color.FromRgb(0xFF, 0x99, 0x66));
                resources["NavButtonBackground"] = new SolidColorBrush(Color.FromRgb(0x3D, 0x3D, 0x3D));
                resources["NavButtonHover"] = new SolidColorBrush(Color.FromRgb(0x55, 0x55, 0x55));
                resources["AccentButtonBackground"] = new SolidColorBrush(Color.FromRgb(0x4D, 0x4D, 0x4D));
                resources["AccentButtonHover"] = new SolidColorBrush(Color.FromRgb(0x66, 0x66, 0x66));
                resources["InputBackground"] = new SolidColorBrush(Color.FromRgb(0x3D, 0x3D, 0x3D));
                resources["BorderColor"] = new SolidColorBrush(Color.FromRgb(0x55, 0x55, 0x55));
                resources["DayButtonBackground"] = new SolidColorBrush(Color.FromRgb(0x3D, 0x3D, 0x3D));
                resources["DayButtonBorder"] = new SolidColorBrush(Color.FromRgb(0x55, 0x55, 0x55));
                resources["DayButtonHover"] = new SolidColorBrush(Color.FromRgb(0x55, 0x55, 0x55));
                resources["PopupBackground"] = new SolidColorBrush(Color.FromRgb(0x3D, 0x3D, 0x3D));
                resources["PopupBorder"] = new SolidColorBrush(Color.FromRgb(0x55, 0x55, 0x55));
                resources["AddPanelBackground"] = new SolidColorBrush(Color.FromRgb(0x4D, 0x4D, 0x4D));
                resources["IconButtonHover"] = new SolidColorBrush(Color.FromRgb(0x4D, 0x4D, 0x4D));
                resources["ToggleBackground"] = new SolidColorBrush(Color.FromRgb(0x4D, 0x4D, 0x4D));
                resources["ToggleBorder"] = new SolidColorBrush(Color.FromRgb(0x80, 0x80, 0x80));
                resources["ToggleThumb"] = new SolidColorBrush(Color.FromRgb(0x2B, 0x2B, 0x2B));
                resources["ToggleThumbBorder"] = new SolidColorBrush(Color.FromRgb(0x80, 0x80, 0x80));
                resources["ToggleIconColor"] = new SolidColorBrush(Color.FromRgb(0xFF, 0xD7, 0x00));
            }
            else
            {
                resources["WindowBackground"] = new SolidColorBrush(Color.FromRgb(0xFF, 0xF5, 0xF0));
                resources["PrimaryTextColor"] = new SolidColorBrush(Color.FromRgb(0x5D, 0x53, 0x4C));
                resources["SecondaryTextColor"] = new SolidColorBrush(Color.FromRgb(0x8B, 0x7D, 0x6B));
                resources["WeekendTextColor"] = new SolidColorBrush(Color.FromRgb(0xCD, 0x85, 0x3F));
                resources["NavButtonBackground"] = new SolidColorBrush(Color.FromRgb(0xEE, 0xD9, 0xC4));
                resources["NavButtonHover"] = new SolidColorBrush(Color.FromRgb(0xDD, 0xBD, 0x9A));
                resources["AccentButtonBackground"] = new SolidColorBrush(Color.FromRgb(0xEE, 0xD9, 0xC4));
                resources["AccentButtonHover"] = new SolidColorBrush(Color.FromRgb(0xE4, 0xC8, 0xA8));
                resources["InputBackground"] = new SolidColorBrush(Color.FromRgb(0xFF, 0xFB, 0xF5));
                resources["BorderColor"] = new SolidColorBrush(Color.FromRgb(0xD2, 0xBB, 0xA5));
                resources["DayButtonBackground"] = new SolidColorBrush(Color.FromRgb(0xFF, 0xFB, 0xF5));
                resources["DayButtonBorder"] = new SolidColorBrush(Color.FromRgb(0xD2, 0xBB, 0xA5));
                resources["DayButtonHover"] = new SolidColorBrush(Color.FromRgb(0xF5, 0xE5, 0xD5));
                resources["PopupBackground"] = new SolidColorBrush(Color.FromRgb(0xFF, 0xFB, 0xF5));
                resources["PopupBorder"] = new SolidColorBrush(Color.FromRgb(0xE6, 0xD3, 0xC2));
                resources["AddPanelBackground"] = new SolidColorBrush(Color.FromRgb(0xFF, 0xF0, 0xE7));
                resources["IconButtonHover"] = new SolidColorBrush(Color.FromRgb(0xEE, 0xD9, 0xC4));
                resources["ToggleBackground"] = new SolidColorBrush(Color.FromRgb(0xEE, 0xD9, 0xC4));
                resources["ToggleBorder"] = new SolidColorBrush(Color.FromRgb(0xD2, 0xBB, 0xA5));
                resources["ToggleThumb"] = new SolidColorBrush(Color.FromRgb(0xFF, 0xF5, 0xF0));
                resources["ToggleThumbBorder"] = new SolidColorBrush(Color.FromRgb(0xD2, 0xBB, 0xA5));
                resources["ToggleIconColor"] = new SolidColorBrush(Color.FromRgb(0xFF, 0xA5, 0x00));
            }
            RefreshCalendar();
        }

        private void ThemeToggleBtn_Checked(object sender, RoutedEventArgs e)
        {
            ApplyTheme(true);
        }

        private void ThemeToggleBtn_Unchecked(object sender, RoutedEventArgs e)
        {
            ApplyTheme(false);
        }

        private void RefreshCalendar()
        {
            MonthYearText.Text = _currentMonth.ToString("MMMM yyyy");
            DaysGrid.Children.Clear();

            DateTime firstDay = new DateTime(_currentMonth.Year, _currentMonth.Month, 1);
            int daysInMonth = DateTime.DaysInMonth(_currentMonth.Year, _currentMonth.Month);
            int offset = ((int)firstDay.DayOfWeek - 1 + 7) % 7;

            for (int i = 0; i < offset; i++)
                DaysGrid.Children.Add(new Button { IsEnabled = false, Opacity = 0 });

            for (int day = 1; day <= daysInMonth; day++)
            {
                DateTime date = new DateTime(_currentMonth.Year, _currentMonth.Month, day);
                Button btn = new Button { Tag = date, Style = (Style)FindResource("DayButton") };

                StackPanel sp = new StackPanel { HorizontalAlignment = HorizontalAlignment.Center };
                TextBlock dayText = new TextBlock
                {
                    Text = day.ToString(),
                    FontSize = 15,
                    Foreground = (Brush)FindResource("PrimaryTextColor"),
                    HorizontalAlignment = HorizontalAlignment.Center
                };
                Ellipse dot = new Ellipse
                {
                    Width = 7,
                    Height = 7,
                    Margin = new Thickness(0, 2, 0, 0),
                    Fill = GetColorForDate(date)
                };
                sp.Children.Add(dayText);
                sp.Children.Add(dot);
                btn.Content = sp;

                if (date == DateTime.Today)
                    btn.BorderBrush = new SolidColorBrush(Color.FromRgb(0xCD, 0x85, 0x3F));

                btn.Click += DayButton_Click;
                DaysGrid.Children.Add(btn);
            }
        }

        private Brush GetColorForDate(DateTime date)
        {
            var tasks = GetTasksForDate(date);
            if (tasks.Count == 0)
            {
                if (date.Date < DateTime.Today)
                    return Brushes.Transparent;
                return (Brush)FindResource("SecondaryTextColor");
            }

            if (date.Date > DateTime.Today)
                return (Brush)FindResource("PrimaryTextColor");

            int completed = tasks.Count(t => t.IsCompletedOnDate(date));
            if (completed == tasks.Count)
                return new SolidColorBrush(Color.FromRgb(0x6B, 0x8E, 0x6B));
            if (completed == 0)
                return new SolidColorBrush(Color.FromRgb(0xC0, 0x6B, 0x6B));
            return new SolidColorBrush(Color.FromRgb(0xD9, 0xB4, 0x6A));
        }

        private List<TaskItem> GetTasksForDate(DateTime date)
        {
            return _allTasks.Where(t =>
            {
                if (t.IsPermanent)
                {
                    if (date.Date < DateTime.Today)
                        return false;
                    if (t.Month.HasValue && new DateTime(date.Year, date.Month, 1) != t.Month.Value)
                        return false;
                    if (t.ExcludedDates.Contains(date.Date))
                        return false;
                    return t.RecurringDays.Contains(date.DayOfWeek);
                }
                return t.Date == date;
            }).ToList();
        }

        private void DayButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is DateTime date)
            {
                _selectedDate = date;
                PopupDateText.Text = date.ToString("dd MMMM yyyy, dddd");
                FillTasksPanel(date);

                bool isPast = date.Date < DateTime.Today;
                ShowAddPanelButton.Visibility = isPast ? Visibility.Collapsed : Visibility.Visible;
                AddTaskPanel.Visibility = Visibility.Collapsed;

                TaskPopup.PlacementTarget = btn;
                TaskPopup.IsOpen = true;
            }
        }

        private void FillTasksPanel(DateTime date)
        {
            TasksPanel.Children.Clear();
            var tasks = GetTasksForDate(date);

            foreach (var task in tasks)
            {
                Border taskBorder = new Border
                {
                    Background = (Brush)FindResource("AddPanelBackground"),
                    CornerRadius = new CornerRadius(8),
                    Margin = new Thickness(0, 3, 0, 3),
                    Padding = new Thickness(8, 6, 8, 6),
                    Child = CreateTaskRow(task, date)
                };
                TasksPanel.Children.Add(taskBorder);
            }
        }

        private UIElement CreateTaskRow(TaskItem task, DateTime date)
        {
            Grid grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            StackPanel leftPanel = new StackPanel { Orientation = Orientation.Horizontal };
            CheckBox cb = new CheckBox
            {
                IsChecked = task.IsCompletedOnDate(date),
                Tag = task,
                VerticalAlignment = VerticalAlignment.Center,
                Foreground = (Brush)FindResource("PrimaryTextColor")
            };
            cb.Checked += TaskCheckBox_Changed;
            cb.Unchecked += TaskCheckBox_Changed;

            TextBlock desc = new TextBlock
            {
                Text = task.Description,
                Margin = new Thickness(6, 0, 0, 0),
                VerticalAlignment = VerticalAlignment.Center,
                Foreground = (Brush)FindResource("PrimaryTextColor"),
                TextWrapping = TextWrapping.Wrap
            };
            leftPanel.Children.Add(cb);
            leftPanel.Children.Add(desc);
            Grid.SetColumn(leftPanel, 0);

            StackPanel rightPanel = new StackPanel { Orientation = Orientation.Horizontal };

            Button removeDayBtn = new Button
            {
                Content = "✕",
                Style = (Style)FindResource("IconButton"),
                Tag = task,
                ToolTip = "Убрать из этого дня"
            };
            removeDayBtn.Click += RemoveTaskFromDay_Click;
            rightPanel.Children.Add(removeDayBtn);

            Button noteBtn = new Button { Content = "📝", Style = (Style)FindResource("IconButton"), Tag = task };
            noteBtn.Click += ShowNote_Click;
            rightPanel.Children.Add(noteBtn);

            Button editBtn = new Button { Content = "✏️", Style = (Style)FindResource("IconButton"), Tag = task };
            editBtn.Click += EditTask_Click;
            rightPanel.Children.Add(editBtn);

            Button deleteBtn = new Button { Content = "🗑️", Style = (Style)FindResource("IconButton"), Tag = task };
            deleteBtn.Click += DeleteTask_Click;
            rightPanel.Children.Add(deleteBtn);

            Grid.SetColumn(rightPanel, 1);
            grid.Children.Add(leftPanel);
            grid.Children.Add(rightPanel);

            if (!string.IsNullOrWhiteSpace(task.Note))
            {
                grid.RowDefinitions.Add(new RowDefinition());
                grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                TextBlock noteText = new TextBlock
                {
                    Text = task.Note,
                    FontStyle = FontStyles.Italic,
                    Foreground = (Brush)FindResource("SecondaryTextColor"),
                    Margin = new Thickness(20, 4, 0, 0),
                    TextWrapping = TextWrapping.Wrap
                };
                Grid.SetRow(noteText, 1);
                Grid.SetColumnSpan(noteText, 2);
                grid.Children.Add(noteText);
            }
            return grid;
        }

        private void TaskCheckBox_Changed(object sender, RoutedEventArgs e)
        {
            if (sender is CheckBox cb && cb.Tag is TaskItem task && _selectedDate.HasValue)
            {
                if (cb.IsChecked == true)
                {
                    task.MarkCompleted(_selectedDate.Value);
                    _db.AddCompletion(task.Id, _selectedDate.Value);
                }
                else
                {
                    task.MarkNotCompleted(_selectedDate.Value);
                    _db.RemoveCompletion(task.Id, _selectedDate.Value);
                }
                RefreshCalendar();
            }
        }

        private void RemoveTaskFromDay_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is TaskItem task && _selectedDate.HasValue)
            {
                if (task.IsPermanent)
                {
                    if (!task.ExcludedDates.Contains(_selectedDate.Value.Date))
                    {
                        task.ExcludedDates.Add(_selectedDate.Value.Date);
                        _db.AddExcludedDate(task.Id, _selectedDate.Value);
                    }
                }
                else
                {
                    _db.DeleteTask(task.Id);
                    _allTasks.Remove(task);
                }
                FillTasksPanel(_selectedDate.Value);
                RefreshCalendar();
            }
        }

        private void ShowNote_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is TaskItem task)
            {
                string result = InputDialog.Show("Введите заметку", "Заметка к задаче", task.Note ?? "");
                if (result != null)
                {
                    task.Note = result;
                    _db.UpdateTaskNote(task.Id, result);
                    if (_selectedDate.HasValue) FillTasksPanel(_selectedDate.Value);
                }
            }
        }

        private void EditTask_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is TaskItem task)
            {
                string result = InputDialog.Show("Редактировать задачу", "Новое описание", task.Description);
                if (!string.IsNullOrWhiteSpace(result))
                {
                    task.Description = result;
                    _db.UpdateTaskDescription(task.Id, result);
                    if (_selectedDate.HasValue) FillTasksPanel(_selectedDate.Value);
                }
            }
        }

        private void DeleteTask_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is TaskItem task)
            {
                if (MessageBox.Show($"Полностью удалить задачу «{task.Description}»?",
                    "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    _db.DeleteTask(task.Id);
                    _allTasks.Remove(task);
                    if (_selectedDate.HasValue) FillTasksPanel(_selectedDate.Value);
                    RefreshCalendar();
                }
            }
        }

        private void ShowAddPanel_Click(object sender, RoutedEventArgs e)
        {
            AddTaskPanel.Visibility = Visibility.Visible;
            ShowAddPanelButton.Visibility = Visibility.Collapsed;
            TaskTextBox.Text = "Название задачи";
            PermanentCheckBox.IsChecked = true;
            DayOfWeekComboBox.Visibility = Visibility.Visible;
            MonthComboBox.Visibility = Visibility.Visible;

            foreach (var d in _dayItems) d.IsSelected = false;
            if (_selectedDate.HasValue)
            {
                var dayOfWeek = _selectedDate.Value.DayOfWeek;
                var selectedDay = _dayItems.FirstOrDefault(d => d.Day == dayOfWeek);
                if (selectedDay != null) selectedDay.IsSelected = true;
            }
            UpdateComboBoxText();
            MonthComboBox.SelectedValue = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
        }

        private void CancelAddTask_Click(object sender, RoutedEventArgs e)
        {
            AddTaskPanel.Visibility = Visibility.Collapsed;
            ShowAddPanelButton.Visibility = Visibility.Visible;
        }

        private void AddTask_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedDate == null) return;
            string desc = TaskTextBox.Text.Trim();
            if (string.IsNullOrWhiteSpace(desc) || desc == "Название задачи")
            {
                MessageBox.Show("Введите название задачи.");
                return;
            }

            bool isPermanent = PermanentCheckBox.IsChecked == true;
            DateTime? month = isPermanent ? (MonthComboBox.SelectedValue as DateTime?) : null;
            string recurringDaysStr = isPermanent
                ? string.Join(",", _dayItems.Where(d => d.IsSelected).Select(d => d.Day.ToString()))
                : null;
            string excludedDatesStr = null; // в начале нет исключений

            int newId;

            try
            {
                newId = _db.AddFullTask(
                    _currentUser.id,
                    desc,
                    desc, // описание и имя пока одинаковы, можно разделить
                    isPermanent ? (DateTime?)null : _selectedDate,
                    isPermanent,
                    recurringDaysStr,
                    month,
                    excludedDatesStr
                );
            }
            catch(Exception ex)
            {
                MessageBox.Show($"Ошибка при добавлении задачи: {ex.Message}");
                return;
            }

            var task = new TaskItem
            {
                Id = newId,
                Description = desc,
                Note = null,
                Date = isPermanent ? null : _selectedDate,
                IsPermanent = isPermanent,
                RecurringDays = isPermanent
                    ? _dayItems.Where(d => d.IsSelected).Select(d => d.Day).ToList()
                    : new List<DayOfWeek>(),
                Month = month,
                ExcludedDates = new List<DateTime>()
            };

            _allTasks.Add(task);
            FillTasksPanel(_selectedDate.Value);
            AddTaskPanel.Visibility = Visibility.Collapsed;
            ShowAddPanelButton.Visibility = _selectedDate.Value.Date < DateTime.Today ? Visibility.Collapsed : Visibility.Visible;
            RefreshCalendar();

            TaskPopup.IsOpen = false;
        }

        private void TaskTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (TaskTextBox.Text == "Название задачи")
                TaskTextBox.Clear();
        }

        private void PermanentCheckBox_Changed(object sender, RoutedEventArgs e)
        {
            bool isChecked = PermanentCheckBox.IsChecked == true;
            DayOfWeekComboBox.Visibility = isChecked ? Visibility.Visible : Visibility.Collapsed;
            MonthComboBox.Visibility = isChecked ? Visibility.Visible : Visibility.Collapsed;
        }

        private void DayCheckBox_Changed(object sender, RoutedEventArgs e) => UpdateComboBoxText();
        private void UpdateComboBoxText()
        {
            var selected = _dayItems.Where(d => d.IsSelected).Select(d => d.Name);
            DayOfWeekComboBox.Text = selected.Any() ? string.Join(", ", selected) : "Выберите дни";
        }

        private void MonthComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e) { }

        private void PreviousMonth_Click(object sender, RoutedEventArgs e)
        {
            _currentMonth = _currentMonth.AddMonths(-1);
            RefreshCalendar();
        }

        private void NextMonth_Click(object sender, RoutedEventArgs e)
        {
            _currentMonth = _currentMonth.AddMonths(1);
            RefreshCalendar();
        }
    }

    // ---------- Модели данных ----------


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

    // Простое диалоговое окно для ввода текста
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