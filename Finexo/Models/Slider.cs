using Finexo.Models.Common;

namespace Finexo.Models;

public class Slider : BaseEntity
{
    public int Id { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public string Image { get; set; }
    
    
}
