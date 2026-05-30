using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimeToTask
{
    public class UserItem
    {
        public int id {  get; set; }
        public string login {  get; set; }
        public string password {  get; set; }
        public string email { get; set; }
        public UserItem()
        {

        }
        public UserItem(int id, string login, string password, string email)
        {
            this.id = id;
            this.login = login;
            this.password = password;
            this.email = email;
        }
    }
}
