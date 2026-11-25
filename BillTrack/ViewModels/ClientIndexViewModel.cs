namespace BillTrack.ViewModels
{
    public class ClientIndexViewModel
    {
        public string? Search { get; set; }
        public List<ClientListItemViewModel> Clients { get; set; } = new();
    }
}
