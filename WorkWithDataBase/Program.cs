using WorkWithDataBase;

PostgreSql connect = new PostgreSql("Server=localhost; port=5432; Database=timeToTask; User Id=postgres; Password=postgre;");
//connect.AddNewUser("Ranil2", "ranil2@mail.ru", "ranil2007");

connect.AddTask(1, "NewTask", "my second task", new DateTime(2026, 5, 20));

//var listTasks = connect.GetTasksByUser(1);
//foreach (var task in listTasks)
//{
//    Console.WriteLine($"{task.NameTask} — {task.AppointmentDay}");
//}

//connect.DeleteTask(1, 1);

//Console.WriteLine(connect.isUser("artem2", "password123"));