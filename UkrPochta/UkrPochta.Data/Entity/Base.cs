using SQLite;
namespace UkrPochta.Data.Entity
{
    public class Base
    {
        #region Id Ид

        [PrimaryKey, AutoIncrement, Column("id")]
        public int? Id { get; protected set; }

        #endregion
         
        #region Name Название
        [Column("name")]
        public string Name
        { get;set;
           
        }

        #endregion
        #region NameLower Название - нижний регистр       

      //  [Column("namelower")]
        public string NameLower
        {
            get { if (Name == null)
                    return null;
                 else
                   return Name?.ToLower();
            }
           
        }

        #endregion
       
        public override string ToString()
        {
            return string.Format("Id: {0}", Id);
        }
    }
}





