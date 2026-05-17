using System;
using System.Collections.Generic;
using System.Text;

namespace WorkWithDataBase
{
    public class UserTask
    {
        public int Id { get; set; }
        public string NameTask { get; set; }
        public string Description { get; set; }
        public DateTime? AppointmentDay { get; set; }
        public bool Completed { get; set; }
        public bool Repeat { get; set; }
    }
}
