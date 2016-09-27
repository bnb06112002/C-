using SQLite;

namespace UkrPochta.Data.Entity
{
    /// <summary>
    /// Справочник районов
    /// </summary>
    [Table("tbl_district")]
    public class District:Base
    {
         #region RegionId Область

            [Column("regionid")]
            public int? RegionId { get; set; }

            #endregion
       
      
    }
}
