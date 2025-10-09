using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SQLite;
using Learnix.Models;

namespace Learnix.Services
{
    public class DatabaseService
    {
        private readonly SQLiteAsyncConnection _db;

        public DatabaseService(string dbPath)
        {
            _db = new SQLiteAsyncConnection(dbPath);
            _db.CreateTableAsync<User>().GetAwaiter().GetResult() ;
            
        }

        public Task<int> AddUser(User user)
        {
            return _db.InsertAsync(user);
        }

        public Task<User> GetUser(string email, string password)
        {
            return _db.Table<User>()
                      .Where(u => u.Email == email && u.Password == password)
                      .FirstOrDefaultAsync();
        }

        public Task<User> UserEmailCheck(string email) 
        { 
            return _db.Table<User>().Where(u => u.Email == email).FirstOrDefaultAsync();
        
        
        }
    }
}
