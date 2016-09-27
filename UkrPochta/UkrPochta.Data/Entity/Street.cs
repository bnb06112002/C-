using SQLite;


namespace UkrPochta.Data.Entity
{
    /// <summary>
    /// Справочник Улиц
    /// </summary>
    [Table("tbl_street")]
    public class Street:Base
    {
        #region CityId населённый пункт

        [Column("cityid")]
        public int? CityId { get; set; }

        #endregion
    }
}
