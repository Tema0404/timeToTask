using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using Npgsql;

namespace WorkWithDataBase
{
    public interface DataBase
    {
        abstract public void ConnectionReader();
    }
    public class PostgreSql : DataBase
    {
        public PostgreSql(string connectionString)
        {
            this.connectionString = connectionString;
        }
        private string connectionString;

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
        public List<UserTask> GetTasksByUser(int uid)
        {
            List<UserTask> tasks = new List<UserTask>();
            using (NpgsqlConnection sqlConnection = new NpgsqlConnection(connectionString))
            {
                sqlConnection.Open();
                string query = $"SELECT id, name_task, description, appointment_day FROM tasks WHERE user_id = {uid}";
                using (NpgsqlCommand cmd = new NpgsqlCommand(query, sqlConnection))
                {
                    using (NpgsqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            tasks.Add(new UserTask
                            {
                                Id = reader.GetInt32(0),
                                NameTask = reader.GetString(1),
                                Description = reader.IsDBNull(2) ? null : reader.GetString(2),
                                AppointmentDay = reader.IsDBNull(3) ? null : reader.GetDateTime(3),
                                Completed = reader.GetBoolean(4),
                                Repeat = reader.GetBoolean(5),
                            });
                        }
                    }
                }
            }
            return tasks;
        }
        public void DeleteTask(int uid, int idTask)
        {
            string query = $"DELETE FROM tasks WHERE user_id = {uid} and id = {idTask}";
            DoCommand(query);
        }
        public bool isUser(string login, string password)
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
    }
}
