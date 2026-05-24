using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using WorkWithDataBase;
using WpfApp1;

namespace TimeToTask
{
    public partial class MainWindow : Window
    {
        private UserItem _currentUser;
        private PostgreSql _db;
        private ObservableCollection<TaskItem> _allTasks = new ObservableCollection<TaskItem>();
        private bool _isDarkTheme = false;
        private static bool _globalDarkTheme = false;

        private ObservableCollection<DayItem> _dayItems;
        private System.Collections.Generic.List<MonthItem> _months;
        private DateTime _today = DateTime.Today;

        public bool IsClosed { get; private set; }

        public MainWindow(UserItem user)
        {
            InitializeComponent();
            
            App.ThemeChanged += OnGlobalThemeChanged;
            this.Closed += (s, e) => App.ThemeChanged -= OnGlobalThemeChanged;
            _currentUser = user;
            _db = new PostgreSql();
            DateText.Text = _today.ToString("dd MMMM yyyy, dddd");

            App.ThemeChanged += OnGlobalThemeChanged;
            this.Closed += (s, e) => App.ThemeChanged -= OnGlobalThemeChanged;

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

            _months = new System.Collections.Generic.List<MonthItem>();
            for (int i = -1; i <= 12; i++)
            {
                DateTime monthStart = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1).AddMonths(i);
                _months.Add(new MonthItem { MonthDate = monthStart, DisplayName = monthStart.ToString("MMMM yyyy") });
            }
            MonthComboBox.ItemsSource = _months;
            MonthComboBox.DisplayMemberPath = "DisplayName";
            MonthComboBox.SelectedValuePath = "MonthDate";

            // Настройка анимации для кнопки темы
            ThemeToggleBtn.RenderTransform = new RotateTransform();
            ThemeToggleBtn.RenderTransformOrigin = new Point(0.5, 0.5);

            _isDarkTheme = _globalDarkTheme;
            ThemeToggleBtn.IsChecked = _isDarkTheme;
            ApplyTheme(_isDarkTheme);
            LoadTasks();
            this.Closed += (s, e) => IsClosed = true;
        }



        protected override void OnActivated(EventArgs e)
        {
            base.OnActivated(e);
            LoadTasks();

            // Синхронизация темы при возврате в окно
            if (_isDarkTheme != _globalDarkTheme)
            {
                ApplyTheme(_globalDarkTheme);
                ThemeToggleBtn.IsChecked = _globalDarkTheme;
            }
        }

        private void LoadTasks()
        {
            _allTasks.Clear();
            var tasksFromDb = _db.GetTasksByUser(_currentUser.id);
            foreach (var t in tasksFromDb)
                _allTasks.Add(t);
            RefreshTodayTasks();
        }

        private void RefreshTodayTasks()
        {
            var todayTasks = _allTasks.Where(t => IsTaskForDate(t, _today)).ToList();
            foreach (var task in todayTasks)
            {
                task.IsCompleted = task.IsCompletedOnDate(_today);
            }
            TasksItemsControl.ItemsSource = todayTasks;
        }

        private bool IsTaskForDate(TaskItem task, DateTime date)
        {
            if (task.IsPermanent)
            {
                if (task.Month.HasValue && new DateTime(date.Year, date.Month, 1) != task.Month.Value)
                    return false;
                if (task.ExcludedDates.Contains(date.Date))
                    return false;
                return task.RecurringDays.Contains(date.DayOfWeek);
            }
            return task.Date == date;
        }

        private void TaskCheckBox_Changed(object sender, RoutedEventArgs e)
        {
            var cb = sender as CheckBox;
            var task = cb?.Tag as TaskItem;
            if (task == null) return;

            if (cb.IsChecked == true)
            {
                task.MarkCompleted(_today);
                _db.AddCompletion(task.Id, _today);
            }
            else
            {
                task.MarkNotCompleted(_today);
                _db.RemoveCompletion(task.Id, _today);
            }
            RefreshTodayTasks();
        }

        private void RemoveTaskFromDay_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            var task = btn?.Tag as TaskItem;
            if (task == null) return;

            if (task.IsPermanent)
            {
                if (!task.ExcludedDates.Contains(_today.Date))
                {
                    task.ExcludedDates.Add(_today.Date);
                    _db.AddExcludedDate(task.Id, _today);
                }
            }
            else
            {
                _db.DeleteTask(task.Id);
                _allTasks.Remove(task);
            }
            RefreshTodayTasks();
        }

        private void ShowNote_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            var task = btn?.Tag as TaskItem;
            if (task == null) return;

            string result = InputDialog.Show("Введите заметку", "Заметка к задаче", task.Note ?? "");
            if (result != null)
            {
                task.Note = result;
                _db.UpdateTaskNote(task.Id, result);
                RefreshTodayTasks();
            }
        }

        private void EditTask_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            var task = btn?.Tag as TaskItem;
            if (task == null) return;

            string result = InputDialog.Show("Редактировать задачу", "Новое описание", task.Description);
            if (!string.IsNullOrWhiteSpace(result))
            {
                task.Description = result;
                _db.UpdateTaskDescription(task.Id, result);
                RefreshTodayTasks();
            }
        }

        private void DeleteTask_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            var task = btn?.Tag as TaskItem;
            if (task == null) return;

            if (MessageBox.Show($"Полностью удалить задачу «{task.Description}»?",
                "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                _db.DeleteTask(task.Id);
                _allTasks.Remove(task);
                RefreshTodayTasks();
            }
        }

        private void ShowAddPanel_Click(object sender, RoutedEventArgs e)
        {
            AddTaskPanel.Visibility = Visibility.Visible;
            TaskTextBox.Text = "Название задачи";
            PermanentCheckBox.IsChecked = true;
            DayOfWeekComboBox.Visibility = Visibility.Visible;
            MonthComboBox.Visibility = Visibility.Visible;

            foreach (var d in _dayItems) d.IsSelected = false;
            var selectedDay = _dayItems.FirstOrDefault(d => d.Day == _today.DayOfWeek);
            if (selectedDay != null) selectedDay.IsSelected = true;
            UpdateComboBoxText();
            MonthComboBox.SelectedValue = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
        }

        private void CancelAddTask_Click(object sender, RoutedEventArgs e)
        {
            AddTaskPanel.Visibility = Visibility.Collapsed;
        }

        private void AddTask_Click(object sender, RoutedEventArgs e)
        {
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
                isPermanent ? null : (DateTime?)_today,
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
                Date = isPermanent ? null : (DateTime?)_today,
                IsPermanent = isPermanent,
                RecurringDays = isPermanent ? _dayItems.Where(d => d.IsSelected).Select(d => d.Day).ToList() : new System.Collections.Generic.List<DayOfWeek>(),
                Month = month,
                ExcludedDates = new System.Collections.Generic.List<DateTime>()
            };

            _allTasks.Add(task);
            AddTaskPanel.Visibility = Visibility.Collapsed;
            RefreshTodayTasks();
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

        private void btnToCalendar_Click(object sender, RoutedEventArgs e)
        {
            this.Hide();
            CalendarWithTasks calendar = new CalendarWithTasks(_currentUser, this);
            calendar.Show();
        }

        // Анимация переключения темы
        private async void AnimateThemeSwitch(bool toDark)
        {
            var btn = ThemeToggleBtn;
            for (int i = 0; i <= 10; i++)
            {
                btn.RenderTransform = new RotateTransform(i * 36);
                await System.Threading.Tasks.Task.Delay(15);
            }
            btn.Content = toDark ? "🌙" : "☀️";
            btn.RenderTransform = new RotateTransform(0);
        }

        private void ThemeToggleBtn_Checked(object sender, RoutedEventArgs e)
        {
            SetTheme(true);
        }

        private void ThemeToggleBtn_Unchecked(object sender, RoutedEventArgs e)
        {
            SetTheme(false);
        }

        private void ApplyTheme(bool isDark)
        {
            _isDarkTheme = isDark;
            _globalDarkTheme = isDark;
            var resources = Application.Current.Resources;

            if (isDark)
            {
                // Основные цвета
                resources["WindowBackground"] = new SolidColorBrush(Color.FromRgb(0x1A, 0x1B, 0x26));
                resources["PrimaryTextColor"] = new SolidColorBrush(Color.FromRgb(0xF5, 0xF5, 0xF7));
                resources["SecondaryTextColor"] = new SolidColorBrush(Color.FromRgb(0xC0, 0xC2, 0xE0));
                resources["WeekendTextColor"] = new SolidColorBrush(Color.FromRgb(0xFF, 0xB8, 0x6B));
                resources["ButtonBackground"] = new SolidColorBrush(Color.FromRgb(0x2C, 0x2E, 0x3E));
                resources["ButtonHover"] = new SolidColorBrush(Color.FromRgb(0x3B, 0x3E, 0x55));
                resources["AccentColor"] = new SolidColorBrush(Color.FromRgb(0xFF, 0x9E, 0x64));
                resources["AccentHover"] = new SolidColorBrush(Color.FromRgb(0xFF, 0xB5, 0x82));
                resources["InputBackground"] = new SolidColorBrush(Color.FromRgb(0x2C, 0x2E, 0x3E));
                resources["BorderColor"] = new SolidColorBrush(Color.FromRgb(0x41, 0x48, 0x68));
                resources["PopupBackground"] = new SolidColorBrush(Color.FromRgb(0x2C, 0x2E, 0x3E));
                resources["PanelBackground"] = new SolidColorBrush(Color.FromRgb(0x24, 0x26, 0x35));
                resources["DayButtonBackground"] = new SolidColorBrush(Color.FromRgb(0x2C, 0x2E, 0x3E));
                resources["DayButtonBorder"] = new SolidColorBrush(Color.FromRgb(0x41, 0x48, 0x68));
                resources["DayButtonHover"] = new SolidColorBrush(Color.FromRgb(0x3B, 0x3E, 0x55));
                resources["IconButtonHover"] = new SolidColorBrush(Color.FromRgb(0x3B, 0x3E, 0x55));
            }
            else
            {
                // Светлая пастельная схема
                resources["WindowBackground"] = new SolidColorBrush(Color.FromRgb(0xFF, 0xF5, 0xF0));
                resources["PrimaryTextColor"] = new SolidColorBrush(Color.FromRgb(0x5D, 0x53, 0x4C));
                resources["SecondaryTextColor"] = new SolidColorBrush(Color.FromRgb(0x8B, 0x7D, 0x6B));
                resources["WeekendTextColor"] = new SolidColorBrush(Color.FromRgb(0xCD, 0x85, 0x3F));
                resources["ButtonBackground"] = new SolidColorBrush(Color.FromRgb(0xEE, 0xD9, 0xC4));
                resources["ButtonHover"] = new SolidColorBrush(Color.FromRgb(0xDD, 0xBD, 0x9A));
                resources["AccentColor"] = new SolidColorBrush(Color.FromRgb(0xCD, 0x85, 0x3F));
                resources["AccentHover"] = new SolidColorBrush(Color.FromRgb(0xB8, 0x6A, 0x44));
                resources["InputBackground"] = new SolidColorBrush(Color.FromRgb(0xFF, 0xFB, 0xF5));
                resources["BorderColor"] = new SolidColorBrush(Color.FromRgb(0xD2, 0xBB, 0xA5));
                resources["PopupBackground"] = new SolidColorBrush(Color.FromRgb(0xFF, 0xFB, 0xF5));
                resources["PanelBackground"] = new SolidColorBrush(Color.FromRgb(0xFF, 0xF0, 0xE7));
                resources["DayButtonBackground"] = new SolidColorBrush(Color.FromRgb(0xFF, 0xFB, 0xF5));
                resources["DayButtonBorder"] = new SolidColorBrush(Color.FromRgb(0xD2, 0xBB, 0xA5));
                resources["DayButtonHover"] = new SolidColorBrush(Color.FromRgb(0xF5, 0xE5, 0xD5));
                resources["IconButtonHover"] = new SolidColorBrush(Color.FromRgb(0xEE, 0xD9, 0xC4));
            }

            
            this.Background = (Brush)resources["WindowBackground"];
            
            RefreshTodayTasks();
        }

        private void OnGlobalThemeChanged(bool isDark)
        {
            // если текущее состояние темы уже совпадает – не обновляем, чтобы избежать лишних вызовов
            if (_isDarkTheme == isDark) return;

            // обновляем состояние кнопки (без вызова события, чтобы не зациклиться)
            ThemeToggleBtn.Checked -= ThemeToggleBtn_Checked;
            ThemeToggleBtn.Unchecked -= ThemeToggleBtn_Unchecked;
            ThemeToggleBtn.IsChecked = isDark;
            ThemeToggleBtn.Checked += ThemeToggleBtn_Checked;
            ThemeToggleBtn.Unchecked += ThemeToggleBtn_Unchecked;

            // применяем тему
            ApplyTheme(isDark);
        }

        private void SetTheme(bool isDark)
        {
            if (_isDarkTheme == isDark) return;

            // Анимация (если есть)
            AnimateThemeSwitch(isDark);

            // Применяем тему в текущем окне
            ApplyTheme(isDark);

            // Оповещаем все остальные окна
            App.NotifyThemeChanged(isDark);
        }

    }
}