using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SQLite;
using System.Data.Entity;

namespace BackupProject.Core
{
    //class Person
    //{
    //    public int Id { get; set; }
    //    public string Name { get; set; }
    //}

    //class MyContext : DbContext
    //{
    //    public DbSet<Person> Persons { get; set; }
    //}

    //class Program
    //{
    //    static void Main(string[] args)
    //    {
    //        using (var db = new MyContext())
    //        {
    //            var person = new Person() { Name = "John" };
    //            db.Persons.Add(person);
    //            db.SaveChanges();
    //        }
    //    }
    //}

    public class WatchDatabase : IDisposable
    {
        private SQLiteConnection _connection;

        public WatchDatabase()
        {
            SQLiteConnection.CreateFile("WatchDb.db");
            _connection = new SQLiteConnection("Data Source=MaBaseDeDonnees.sqlite;Version=3;");
            _connection.Open();
            //http://stackoverflow.com/questions/38557170/simple-example-using-system-data-sqlite-with-entity-framework-6
        }



        public void Dispose()
        {
            _connection.Close();
        }
    }
}
