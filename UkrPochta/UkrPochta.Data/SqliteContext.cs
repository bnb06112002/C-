using SQLite;
using UkrPochta.Data.Entity;
namespace UkrPochta.Data
{
    class SqliteContext
    {
        public SQLiteConnection Db { get; }

        public bool IsOpen { get; private set; }

        public SqliteContext(string pathDb)
        {
            try
            {
                Db = new SQLiteConnection(pathDb, SQLiteOpenFlags.ReadWrite);
#if DEBUG
                Db.Trace = true;
#endif
                IsOpen = true;
            }
            catch (SQLiteException)
            {
                IsOpen = false;
            }
        }



        #region Dictionary

        internal TableQuery<Region> Region => Db.Table<Region>();
        internal TableQuery<District> District => Db.Table<District>();
        internal TableQuery<City> City => Db.Table<City>();
        internal TableQuery<Street> Street => Db.Table<Street>();
        internal TableQuery<House> House => Db.Table<House>();

        #endregion


    }
}






