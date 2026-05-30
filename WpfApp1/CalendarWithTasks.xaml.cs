using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
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
        private static bool _globalDarkTheme = false;

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

            _isDarkTheme = _globalDarkTheme;
            RefreshCalendar();
            LoadTasksFromDb();
            RefreshCalendar();
        }

        private void LoadTasksFromDb()
        {
            _allTasks.Clear();
            var tasksFromDb = _db.GetTasksByUser(_currentUser.id);
            foreach (var t in tasksFromDb)
                _allTasks.Add(t);
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
                    Width = 10,
                    Height = 10,
                    Margin = new Thickness(0, 4, 0, 0),
                    Fill = GetColorForDate(date)
                };
                sp.Children.Add(dayText);
                sp.Children.Add(dot);
                btn.Content = sp;

                if (date == DateTime.Today)
                    btn.BorderBrush = new SolidColorBrush(Color.FromRgb(0xFF, 0x9E, 0x64));

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

            int completed = tasks.Count(t => t.IsCompletedOnDate(date));

            if (date.Date > DateTime.Today)
            {
                if (completed == 0)
                    return (Brush)FindResource("AccentColor");
                if (completed == tasks.Count)
                    return new SolidColorBrush(Color.FromRgb(0x9E, 0xCE, 0x6A)); 
                return new SolidColorBrush(Color.FromRgb(0xE0, 0xAF, 0x68)); 
            }

            if (completed == tasks.Count)
                return new SolidColorBrush(Color.FromRgb(0x9E, 0xCE, 0x6A));
            if (completed == 0)
                return new SolidColorBrush(Color.FromRgb(0xF7, 0x76, 0x8E)); 
            return new SolidColorBrush(Color.FromRgb(0xE0, 0xAF, 0x68));    
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
                    Background = (Brush)FindResource("PanelBackground"),
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

            Button noteBtn = new Button { Content = "📄", Style = (Style)FindResource("IconButton"), Tag = task, ToolTip = "Заметка" };
            noteBtn.Click += ShowNote_Click;
            rightPanel.Children.Add(noteBtn);

            Button editBtn = new Button { Content = "✎", Style = (Style)FindResource("IconButton"), Tag = task, ToolTip = "Редактировать" };
            editBtn.Click += EditTask_Click;
            rightPanel.Children.Add(editBtn);

            Button deleteBtn = new Button { Content = "🗑", Style = (Style)FindResource("IconButton"), Tag = task, ToolTip = "Удалить" };
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

            int newId = _db.AddFullTask(
                _currentUser.id,
                desc,
                desc,
                isPermanent ? null : (DateTime?)_selectedDate,
                isPermanent,
                recurringDaysStr,
                month,
                null
            );

            var task = new TaskItem
            {
                Id = newId,
                Description = desc,
                Note = null,
                Date = isPermanent ? null : _selectedDate,
                IsPermanent = isPermanent,
                RecurringDays = isPermanent ? _dayItems.Where(d => d.IsSelected).Select(d => d.Day).ToList() : new List<DayOfWeek>(),
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

        private void BackToMainMenu_Click(object sender, RoutedEventArgs e)
        {
            MainWindow mainWin = new MainWindow(_currentUser);
            mainWin.Show();
            this.Close();
        }
    }
}