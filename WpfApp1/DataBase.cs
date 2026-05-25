using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using WpfApp1;
using System.Linq;

namespace WorkWithDataBase
{
    public interface DataBase
    {
        void ConnectionReader();
    }
    public class PostgreSql : DataBase
    {

        public PostgreSql()
        {
            
        }
        private string connectionString = "Server=localhost; port=5432; Database=timeToTask; User Id=postgres; Password=postgre;";

        private void DoCommand(string InputCommand)
        {
            using (NpgsqlConnection sqlConnection = new NpgsqlConnection(connectionString))
            {
                sqlConnection.Open();
                using (NpgsqlCommand command = new NpgsqlCommand(InputCommand, sqlConnection))
                {
                    command.CommandType = CommandType.Text;
                    command.ExecuteNonQuery();
                }
            }
        }
        private int ExeScalarQuery(string query)
        {
            using (NpgsqlConnection sqlConnection = new NpgsqlConnection(connectionString))
            {
                sqlConnection.Open();
                using (NpgsqlCommand command = new NpgsqlCommand(query, sqlConnection))
                {
                    command.CommandType = CommandType.Text;
                    object res = command.ExecuteScalar();

                    if (res == null || res == DBNull.Value)
                        return 0;
                    return Convert.ToInt32(res);
                }
            }
        }
        public void ConnectionReader()
        {

        }
        public void AddNewUser(string login, string mail, string password)
        {
            string query = $"INSERT INTO users (login, mail, password) VALUES ('{login}', '{mail}', '{password}')";
            DoCommand(query);
        }
        public void AddTask(int uid, string nameTask, string description, DateTime? appointmentDay)
        {
            string query = $"INSERT INTO tasks (user_id, name_task, description, appointment_day) " +
                $"VALUES ({uid}, '{nameTask}', '{description}', '{appointmentDay}')";
            DoCommand(query);
        }
        //public List<TaskItem> GetTasksByUser(int uid)
        //{
        //    List<TaskItem> tasks = new List<TaskItem>();
        //    using (NpgsqlConnection sqlConnection = new NpgsqlConnection(connectionString))
        //    {
        //        sqlConnection.Open();
        //        string query = $"SELECT id, name_task, description, appointment_day FROM tasks WHERE user_id = {uid}";
        //        using (NpgsqlCommand cmd = new NpgsqlCommand(query, sqlConnection))
        //        {
        //            using (NpgsqlDataReader reader = cmd.ExecuteReader())
        //            {
        //                while (reader.Read())
        //                {
        //                    tasks.Add(new TaskItem
        //                    {
        //                        Id = reader.GetInt32(0),
        //                        Description = reader.GetString(1),
        //                        Note = reader.IsDBNull(2) ? null : reader.GetString(2),
        //                        Date = reader.GetDateTime(3),
        //                        IsCompleted = reader.GetBoolean(4),
        //                        IsPermanent = reader.GetBoolean(5),
        //                    });
        //                }
        //            }
        //        }
        //    }
        //    return tasks;
        //}
        public void DeleteTask(int uid, int idTask)
        {
            string query = $"DELETE FROM tasks WHERE user_id = {uid} and id = {idTask}";
            DoCommand(query);
        }
        private bool isUser(string login, string password)
        {
            using (NpgsqlConnection sqlConnection = new NpgsqlConnection(connectionString))
            {
                sqlConnection.Open();
                string query = $"SELECT COUNT(*) FROM users WHERE login = '{login}' and password = '{password}'";
                using (NpgsqlCommand cmd = new NpgsqlCommand(query, sqlConnection))
                {
                    long count = (long)cmd.ExecuteScalar();
                    return count > 0;
                }
            }
        }
        public int GetUserId(string login, string password)
        {
            if (!isUser(login, password))
                return -1;
            using (NpgsqlConnection sqlConnection = new NpgsqlConnection(connectionString))
            {
                sqlConnection.Open();
                string query = $"SELECT id FROM users WHERE login = '{login}' AND password = '{password}'";
                using (NpgsqlCommand cmd = new NpgsqlCommand(query, sqlConnection))
                {
                    object result = cmd.ExecuteScalar();
                    return Convert.ToInt32(result);
                }
            }
        }
        public string GetUserEmail(int uid)
        {
            using (NpgsqlConnection sqlConnection = new NpgsqlConnection(connectionString))
            {
                sqlConnection.Open();
                string query = $"SELECT mail FROM users WHERE id = {uid}";
                using (NpgsqlCommand cmd = new NpgsqlCommand(query, sqlConnection))
                {
                    object result = cmd.ExecuteScalar();
                    return result.ToString();
                }
            }
        }
        public void isComplete(int uid, int idTask)
        {
            using (NpgsqlConnection sqlConnetion = new NpgsqlConnection(connectionString))
            {
                sqlConnetion.Open();
                string query = $"UPDATE tasks SET completed = TRUE WHERE user_id = {uid} and id = {idTask}";
                using (NpgsqlCommand cmd = new NpgsqlCommand(query, sqlConnetion))
                {
                    cmd.ExecuteNonQuery();
                }
            }
        }

        //Метод для проверки существования пароля в бд(для валидации)
        public bool IsLoginExists(string login)
        {
            using (NpgsqlConnection sqlConnection = new NpgsqlConnection(connectionString))
            {
                sqlConnection.Open();
                string query = "SELECT COUNT(*) FROM users WHERE LOWER(\"login\") = LOWER(@login)";
                using (NpgsqlCommand cmd = new NpgsqlCommand(query, sqlConnection))
                {
                    cmd.Parameters.AddWithValue("@login", login);
                    long count = Convert.ToInt64(cmd.ExecuteScalar());
                    return count > 0;
                }
            }
        }
        //Метод для проверки существования почты в бд(для валидации)
        public bool IsEmailExists(string email)
        {
            using (NpgsqlConnection sqlConnection = new NpgsqlConnection(connectionString))
            {
                sqlConnection.Open();
                string query = "SELECT COUNT(*) FROM users WHERE LOWER(\"mail\") = LOWER(@email)";
                using (NpgsqlCommand cmd = new NpgsqlCommand(query, sqlConnection))
                {
                    cmd.Parameters.AddWithValue("@email", email);

                    var result = cmd.ExecuteScalar();
                    long count = Convert.ToInt64(result);
                    return count > 0;
                }
            }
        }
        public int AddFullTask(int uid, string nameTask, string description, DateTime? appointmentDay,
            bool isPermanent, string recurringDays, DateTime? month, string excludedDates)
        {
            string query = $@"
        INSERT INTO tasks (user_id, name_task, description, appointment_day, permanent, recurring_days, month, excluded_dates)
        VALUES ({uid}, '{nameTask}', '{description}', {(appointmentDay.HasValue ? $"'{appointmentDay.Value:yyyy-MM-dd}'" : "NULL")},
                {isPermanent}, {(recurringDays != null ? $"'{recurringDays}'" : "NULL")},
                {(month.HasValue ? $"'{month.Value:yyyy-MM-dd}'" : "NULL")},
                {(excludedDates != null ? $"'{excludedDates}'" : "NULL")})
        RETURNING id";
            using (var conn = new NpgsqlConnection(connectionString))
            {
                conn.Open();
                using (var cmd = new NpgsqlCommand(query, conn))
                {
                    return Convert.ToInt32(cmd.ExecuteScalar());
                }
            }
        }
        public void UpdateTaskDescription(int taskId, string newDescription)
        {
            string query = $"UPDATE tasks SET description = '{newDescription}' WHERE id = {taskId}";
            DoCommand(query);
        }
        public void UpdateTaskNote(int taskId, string note)
        {
            string query = $"UPDATE tasks SET description = '{note}' WHERE id = {taskId}";
            DoCommand(query);
        }

        public void AddCompletion(int taskId, DateTime date)
        {
            string query = $"INSERT INTO task_completions (task_id, completion_date) VALUES ({taskId}, '{date:yyyy-MM-dd}') ON CONFLICT DO NOTHING";
            DoCommand(query);
        }
        public void RemoveCompletion(int taskId, DateTime date)
        {
            string query = $"DELETE FROM task_completions WHERE task_id = {taskId} AND completion_date = '{date:yyyy-MM-dd}'";
            DoCommand(query);
        }
        public void AddExcludedDate(int taskId, DateTime date)
        {
            // Обновляем поле excluded_dates в таблице tasks
            string query = $@"
        UPDATE tasks SET excluded_dates = CONCAT(COALESCE(excluded_dates, ''), '{date:yyyy-MM-dd},')
        WHERE id = {taskId}";
            DoCommand(query);
        }
        public void DeleteTask(int taskId)
        {
            string query = $"DELETE FROM tasks WHERE id = {taskId}";
            DoCommand(query);
        }
        public List<TaskItem> GetTasksByUser(int uid)
        {
            var tasks = new List<TaskItem>();
            using (var conn = new NpgsqlConnection(connectionString))
            {
                conn.Open();
                // Загружаем задачи
                string query = $@"
            SELECT id, name_task, description, appointment_day, permanent, recurring_days, month, excluded_dates
            FROM tasks WHERE user_id = {uid}";
                using (var cmd = new NpgsqlCommand(query, conn))
                {
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var task = new TaskItem
                            {
                                Id = reader.GetInt32(0),
                                Description = reader.GetString(1),
                                Note = reader.IsDBNull(2) ? null : reader.GetString(2),
                                Date = reader.IsDBNull(3) ? null : (DateTime?)reader.GetDateTime(3),
                                IsPermanent = reader.GetBoolean(4),
                                RecurringDays = reader.IsDBNull(5)
                                    ? new List<DayOfWeek>()
                                    : reader.GetString(5)
                                        .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                                        .Select(s => s.Trim())
                                        .Where(s => Enum.TryParse<DayOfWeek>(s, out _))
                                        .Select(s => (DayOfWeek)Enum.Parse(typeof(DayOfWeek), s))
                                        .ToList(),
                                Month = reader.IsDBNull(6) ? (DateTime?)null : reader.GetDateTime(6),
                                ExcludedDates = reader.IsDBNull(7)
                                    ? new List<DateTime>()
                                    : reader.GetString(7).Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                                        .Select(d => DateTime.Parse(d)).ToList()
                            };
                            tasks.Add(task);
                        }
                    }

                    var taskIds = tasks.Select(t => t.Id).ToList();
                    if (taskIds.Any())
                    {
                        string compQuery = $@"
                SELECT task_id, completion_date FROM task_completions
                WHERE task_id = ANY(ARRAY[{string.Join(",", taskIds)}])";
                        using (var cmd2 = new NpgsqlCommand(compQuery, conn))
                        {
                            using (var compReader = cmd2.ExecuteReader())
                            {
                                while (compReader.Read())
                                {
                                    int tid = compReader.GetInt32(0);
                                    DateTime dt = compReader.GetDateTime(1);
                                    var task = tasks.First(t => t.Id == tid);
                                    task.CompletedDates.Add(dt);
                                }
                            }
                        }
                    }
                }
                return tasks;
            }
        }
    }
}