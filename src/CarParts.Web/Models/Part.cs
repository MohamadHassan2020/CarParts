using System.ComponentModel.DataAnnotations;

namespace CarParts.Web.Models;

public class Part
{
    public int Id { get; set; }

    [Required, StringLength(50)]
    [Display(Name = "Part Number")]
    public string PartNumber { get; set; } = string.Empty;

    [Required, StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required, StringLength(100)]
    public string Brand { get; set; } = string.Empty;

    public int Quantity { get; set; }

    public decimal Price { get; set; }

    public Guid RowVersion { get; set; } = Guid.NewGuid();
}
