namespace InternManager.DTO.task
{
    public class TaskReviewResponseDTO
    {
        public int id { get; set; }
        public string comment { get; set; } = string.Empty;
        public DateOnly ngay_danh_gia { get; set; }
        public string reviewer { get; set; } = string.Empty;
        public int task_id { get; set; }
        public string tieu_de { get; set; } = string.Empty;
        public string noi_dung { get; set; } = string.Empty;
        public int progress { get; set; }
        public string statusTask { get; set; } = string.Empty;
        public string username { get; set; } = string.Empty;
        public string leader { get; set; } = string.Empty;
    }
}
