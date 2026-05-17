using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace WpfApp1
{
    public class TaskItem : INotifyPropertyChanged
    {
        public int Id {  get; set; }
        public string Description { get; set; }
        public DateTime? Date { get; set; }
        public bool IsPermanent { get; set; }
        public List<DayOfWeek> RecurringDays { get; set; } = new List<DayOfWeek>();
        public string Note { get; set; }
        public DateTime? Month { get; set; }
        public List<DateTime> ExcludedDates { get; set; } = new List<DateTime>();
        public List<DateTime> CompletedDates { get; set; } = new List<DateTime>();

        private bool _isCompleted;
        public bool IsCompleted
        {
            get => _isCompleted;
            set { _isCompleted = value; OnPropertyChanged(); }
        }

        public bool IsCompletedOnDate(DateTime date)
        {
            if (ExcludedDates.Contains(date.Date))
                return false;
            if (!IsPermanent && Date.HasValue && Date.Value.Date == date.Date)
                return IsCompleted;
            if (IsPermanent)
            {
                if (Month.HasValue && new DateTime(date.Year, date.Month, 1) != Month.Value)
                    return false;
                if (RecurringDays.Contains(date.DayOfWeek))
                    return CompletedDates.Any(d => d.Date == date.Date);
            }
            return false;
        }

        public void MarkCompleted(DateTime date)
        {
            if (ExcludedDates.Contains(date.Date))
                return;
            if (!IsPermanent && Date.HasValue && Date.Value.Date == date.Date)
                IsCompleted = true;
            else if (IsPermanent && RecurringDays.Contains(date.DayOfWeek) &&
                     !CompletedDates.Any(d => d.Date == date.Date))
                CompletedDates.Add(date.Date);
        }

        public void MarkNotCompleted(DateTime date)
        {
            if (ExcludedDates.Contains(date.Date))
                return;
            if (!IsPermanent && Date.HasValue && Date.Value.Date == date.Date)
                IsCompleted = false;
            else if (IsPermanent && RecurringDays.Contains(date.DayOfWeek))
            {
                var existing = CompletedDates.FirstOrDefault(d => d.Date == date.Date);
                if (existing != default)
                    CompletedDates.Remove(existing);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
