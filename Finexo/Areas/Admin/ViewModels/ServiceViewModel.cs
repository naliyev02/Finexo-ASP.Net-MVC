using System.ComponentModel.DataAnnotations;

namespace Finexo.Areas.Admin.ViewModels
{
    public class ServiceViewModel
    {
        public int Id { get; set; }
        [Required]
        public string Title { get; set; }
        [Required]
        public string Description { get; set; }
        public IFormFile? Image { get; set; }
        public string? CreatedBy { get; set; }
        public string? ModifiedBy { get; set; }

    }
}
