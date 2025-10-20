namespace AttaEduSystem.Models.Entities
{
    public class BaseEntity<TCid, TUid, TSid>
    {
        public TCid? CreatedBy { get; set; }
        public DateTime? CreatedTime { get; set; }
        public TUid? UpdatedBy { get; set; }
        public DateTime? UpdatedTime { get; set; }
        public TSid? Status { get; set; }
    }
}
