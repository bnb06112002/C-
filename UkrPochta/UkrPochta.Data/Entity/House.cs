using SQLite;


namespace UkrPochta.Data.Entity
{
    /// <summary>
    /// Справочник домов
    /// </summary>
    [Table("tbl_house")]
    public class House:Base
    {
        #region StreetId Улица

        [Column("streetid")]
        public int? StreetId { get; set; }

        #endregion
        #region дом

        [Column("house")]
        public string NHouse { get; set; }

        #endregion
        #region почтовый индекс

        [Column("postindex")]
        public string PostIndex { get; set; }

        #endregion
    }
}
