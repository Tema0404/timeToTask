using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using WpfApp1;
using System.Linq;

namespace WorkWithDataBase
{
    public class PostgreSql
    {
        private string connectionString = "Server=localhost; port=5432; Database=timeToTask; User Id=postgres; Password=postgre;";
        public void AddNewUser(string login, string mail, string password)
        {
            string query = "INSERT INTO users (login, mail, password) VALUES (@login, @mail, @password)";
            using (var sqlConnection = new NpgsqlConnection(connectionString))
            {
                sqlConnection.Open();
                using(var command = new NpgsqlCommand(query, sqlConnection))
                {
                    command.Parameters.AddWithValue("@login", login);
                    command.Parameters.AddWithValue("@mail", mail);
                    command.Parameters.AddWithValue("@password", password);

                    command.ExecuteNonQuery();
                }
            }
        }
        public void AddTask(int uid, string nameTask, string description, DateTime? appointmentDay)
        {
            string query = "INSERT INTO tasks (user_id, name_task, description, appointment_day) " +
                $"VALUES (@uid, @nameTask, @description, @appointmentDay)";
            using(var sqlConnection = new NpgsqlConnection(connectionString))
            {
                sqlConnection.Open();
                using(var command = new NpgsqlCommand(query, sqlConnection))
                {
                    command.Parameters.AddWithValue("@uid", uid);
                    command.Parameters.AddWithValue("@nameTask", nameTask);
                    command.Parameters.AddWithValue("@description", description);
                    command.Parameters.AddWithValue("@appointmentDay", appointmentDay);

                    command.ExecuteNonQuery();
                }
            }
        }
        public void DeleteTask(int uid, int idTask)
        {
            string query = "DELETE FROM tasks WHERE user_id = @uid and id = @idTask";
            using(var sqlConnection = new NpgsqlConnection(connectionString))
            {
                sqlConnection.Open();
                using(var command = new NpgsqlCommand(query, sqlConnection))
                {
                    command.Parameters.AddWithValue("@uid", uid);
                    command.Parameters.AddWithValue("@idTask", idTask);

                    command.ExecuteNonQuery();
                }
            }
        }
        private bool isUser(string login, string password)
        {
            string query = "SELECT COUNT(*) FROM users WHERE login = @login and password = @password";
            using (NpgsqlConnection sqlConnection = new NpgsqlConnection(connectionString))
            {
                sqlConnection.Open();
                using (NpgsqlCommand command = new NpgsqlCommand(query, sqlConnection))
                {
                    command.Parameters.AddWithValue("@login", login);
                    command.Parameters.AddWithValue("@password", password);

                    long count = (long)command.ExecuteScalar();
                    return count > 0;
                }
            }
        }
        public int GetUserId(string login, string password)
        {
            if (!isUser(login, password))
                return -1;

            string query = "SELECT id FROM users WHERE login = @login AND password = @password";

            using (NpgsqlConnection sqlConnection = new NpgsqlConnection(connectionString))
            {
                sqlConnection.Open();
                using (NpgsqlCommand command = new NpgsqlCommand(query, sqlConnection))
                {
                    command.Parameters.AddWithValue("@login", login);
                    command.Parameters.AddWithValue ("@password", password);

                    object result = command.ExecuteScalar();
                    return Convert.ToInt32(result);
                }
            }
        }
        public string GetUserEmail(int uid)
        {
            string query = "SELECT mail FROM users WHERE id = @uid";
            using (NpgsqlConnection sqlConnection = new NpgsqlConnection(connectionString))
            {
                sqlConnection.Open();
                using (NpgsqlCommand command = new NpgsqlCommand(query, sqlConnection))
                {
                    command.Parameters.AddWithValue("@uid", uid);

                    object result = command.ExecuteScalar();
                    return result.ToString();
                }
            }
        }
        public void isComplete(int uid, int idTask)
        {
            string query = "UPDATE tasks SET completed = TRUE WHERE user_id = @uid and id = @idTask";
            using (NpgsqlConnection sqlConnetion = new NpgsqlConnection(connectionString))
            {
                sqlConnetion.Open();
                using (NpgsqlCommand command = new NpgsqlCommand(query, sqlConnetion))
                {
                    command.Parameters.AddWithValue("@uid", uid);
                    command.Parameters.AddWithValue("@idTask", idTask);

                    command.ExecuteNonQuery();
                }
            }
        }
        public bool IsLoginExists(string login)
        {
            string query = "SELECT COUNT(*) FROM users WHERE LOWER(\"login\") = LOWER(@login)";
            using (NpgsqlConnection sqlConnection = new NpgsqlConnection(connectionString))
            {
                sqlConnection.Open();
                using (NpgsqlCommand command = new NpgsqlCommand(query, sqlConnection))
                {
                    command.Parameters.AddWithValue("@login", login);

                    long count = Convert.ToInt64(command.ExecuteScalar());
                    return count > 0;
                }
            }
        }
        public bool IsEmailExists(string email)
        {
            string query = "SELECT COUNT(*) FROM users WHERE LOWER(\"mail\") = LOWER(@email)";
            using (NpgsqlConnection sqlConnection = new NpgsqlConnection(connectionString))
            {
                sqlConnection.Open();
                using (NpgsqlCommand command = new NpgsqlCommand(query, sqlConnection))
                {
                    command.Parameters.AddWithValue("@email", email);

                    var result = command.ExecuteScalar();
                    long count = Convert.ToInt64(result);
                    return count > 0;
                }
            }
        }
        public int AddFullTask(int uid, string nameTask, string description, DateTime? appointmentDay,
            bool isPermanent, string recurringDays, DateTime? month, string excludedDates)
        {
            string query = @"
                INSERT INTO tasks (user_id, name_task, description, appointment_day, permanent, recurring_days, month, excluded_dates)
                VALUES (@uid, @nameTask, @description, @appointmentDay,
                @isPermanent, @recurringDays,
                @month, @excludedDates) RETURNING id";
            using (var connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();
                using (var command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@uid", uid);
                    command.Parameters.AddWithValue("@nameTask", nameTask);
                    command.Parameters.AddWithValue("@description", description);
                    command.Parameters.AddWithValue("@appointmentDay",
                        appointmentDay.HasValue ? (object)appointmentDay.Value : DBNull.Value);
                    command.Parameters.AddWithValue("@isPermanent", isPermanent);
                    command.Parameters.AddWithValue("@recurringDays",
                        recurringDays ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@month",
                        month.HasValue ? (object)month.Value : DBNull.Value);
                    command.Parameters.AddWithValue("@excludedDates",
                        excludedDates ?? (object)DBNull.Value);

                    var res = Convert.ToInt32(command.ExecuteScalar());
                    return res;
                }
            }
        }
        public void UpdateTaskDescription(int taskId, string newDescription)
        {
            string query = "UPDATE tasks SET description = @newDescription WHERE id = @taskId";
            using(var connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();
                using(var command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@newDescription", newDescription);
                    command.Parameters.AddWithValue("@taskId", taskId);

                    command.ExecuteNonQuery();
                }
            }
        }
        public void UpdateTaskNote(int taskId, string note)
        {
            string query = "UPDATE tasks SET description = @note WHERE id = @taskId";
            using(var connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();
                using(var command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@note", note);
                    command.Parameters.AddWithValue("@taskId", taskId);

                    command.ExecuteNonQuery();
                }
            }
        }

        public void AddCompletion(int taskId, DateTime date)
        {
            string query = "INSERT INTO task_completions (task_id, completion_date) VALUES (@taskId, @date) ON CONFLICT DO NOTHING";
            using (var connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();
                using (var command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@taskId", taskId);
                    command.Parameters.AddWithValue("@date", date.Date);

                    command.ExecuteNonQuery();
                }
            }
        }
        public void RemoveCompletion(int taskId, DateTime date)
        {
            string query = "DELETE FROM task_completions WHERE task_id = @taskId AND completion_date = @date";
            using(var connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();
                using(var command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@taskId", taskId);
                    command.Parameters.AddWithValue("@date", date.Date);

                    command.ExecuteNonQuery();
                }
            }
        }
        public void AddExcludedDate(int taskId, DateTime date)
        {
            string formattedDate = date.Date.ToString("yyyy-MM-dd");
            string query = @"
                UPDATE tasks SET excluded_dates = CONCAT(COALESCE(excluded_dates, ''), @date)
                WHERE id = @taskId";
            using(var connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();
                using(var command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@date", formattedDate);
                    command.Parameters.AddWithValue("@taskId", taskId);

                    command.ExecuteNonQuery();
                }
            }
        }
        public void DeleteTask(int taskId)
        {
            string query = "DELETE FROM tasks WHERE id = @taskId";
            using(var connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();
                using(var command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@taskId", taskId);

                    command.ExecuteNonQuery();
                }
            }
        }
        public List<TaskItem> GetTasksByUser(int uid)
        {
            var tasks = new List<TaskItem>();
            string query = @"
            SELECT id, name_task, description, appointment_day, permanent, recurring_days, month, excluded_dates
            FROM tasks WHERE user_id = @uid";
            using (var connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();
                
                using (var command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@uid", uid);

                    using (var reader = command.ExecuteReader())
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
                        string compQuery = @"
                            SELECT task_id, completion_date FROM task_completions
                            WHERE task_id = ANY(ARRAY[@taskIds])";
                        using (var command2 = new NpgsqlCommand(compQuery, connection))
                        {
                            command2.Parameters.AddWithValue("@taskIds", taskIds.ToArray());

                            using (var compReader = command2.ExecuteReader())
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
                        foreach (var task in tasks)
                        {
                            if (!task.IsPermanent && task.Date.HasValue)
                            {
                                task.IsCompleted = task.CompletedDates.Contains(task.Date.Value);
                            }
                        }
                    }
                }
                return tasks;
            }
        }
        public object AuthorizeUser(string userId, string password)
        {
            if (!int.TryParse(userId, out int uid))
                return null;
            string query = "SELECT id FROM users WHERE id = @uid AND password = @password";
            using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();
                using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@uid", uid);
                    command.Parameters.AddWithValue("@password", password);

                    object result = command.ExecuteScalar();

                    return result;
                }
            }
        }
        public void ChangePassword(int uid, string newPassword)
        {
            string query = "UPDATE users SET password = @newPassword WHERE id = @uid";
            using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();
                using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@newPassword", newPassword);
                    command.Parameters.AddWithValue("@uid", uid);

                    command.ExecuteNonQuery();
                }
            }
        }
        public void DeleteUser(int uid)
        {
            string deleteTasksQuery = "DELETE FROM tasks WHERE user_id = @uid";
            using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();
                using (NpgsqlCommand command = new NpgsqlCommand(deleteTasksQuery, connection))
                {
                    command.Parameters.AddWithValue("@uid", uid);

                    command.ExecuteNonQuery();
                }

                string deleteUserQuery = "DELETE FROM users WHERE id = @uid";
                using (NpgsqlCommand command = new NpgsqlCommand(deleteUserQuery, connection))
                {
                    command.Parameters.AddWithValue("@uid", uid);

                    command.ExecuteNonQuery();
                }
            }
        }
    }
}