using System.ComponentModel.DataAnnotations;

namespace CarParts.Web.Models;

public class PartInputModel
{
    [Required, StringLength(50)]
    [Display(Name = "Part Number")]
    public string PartNumber { get; set; } = string.Empty;

    [Required, StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required, StringLength(100)]
    public string Brand { get; set; } = string.Empty;

    [Range(0, 100_000)]
    public int Quantity { get; set; }

    [Range(0.01, 9_999_999.99, ErrorMessage = "Price must be between 0.01 and 9,999,999.99.")]
    public decimal Price { get; set; }
}
