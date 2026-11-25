namespace BillTrack.ViewModels
{
    public class ClientListItemViewModel
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? CompanyName { get; set; }
        public string? Phone { get; set; }
        public int InvoiceCount { get; set; }
    }
}
