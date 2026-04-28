using Learnix.Models;
using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Learnix.Services
{
    public class DatabaseService 
    {
        private readonly SQLiteAsyncConnection _db;

        public DatabaseService(string dbPath)
        {
            _db = new SQLiteAsyncConnection(dbPath);
            _db.CreateTableAsync<User>().Wait();
        }

        public Task<int> AddUser(User user)
        {
            return _db.InsertAsync(user);
        }

        public async Task<User?> GetUserByEmail(string email)
        {
            return await _db.Table<User>().FirstOrDefaultAsync(u => u.Email == email);
        }


    }
}
