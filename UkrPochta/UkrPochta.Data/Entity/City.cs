using SQLite;

namespace UkrPochta.Data.Entity
{
    /// <summary>
    /// Справочник населенных пунктов
    /// </summary>
    [Table("tbl_city")]
    public class City:Base
    {
        #region DistrictId район

        [Column("districtid")]
        public int? DistrictId { get; set; }

        #endregion
    }
}
