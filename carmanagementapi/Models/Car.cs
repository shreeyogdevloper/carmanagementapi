using System.ComponentModel.DataAnnotations;

namespace carmanagementapi.Models
{
    public class Car
    {
        public int Id { get; set; }

 
        [Required]
        public string Brand { get; set; }

        [Required]
        public string Class { get; set; }

        [Required]
        public string ModelName { get; set; }

        [Required]
        [StringLength(10, MinimumLength = 10)]
        [RegularExpression(@"^[a-zA-Z0-9]*$", ErrorMessage = "Only alphanumeric characters are allowed.")]
        public string ModelCode { get; set; }

        [Required]
        public string Description { get; set; }

        [Required]
        public string Features { get; set; }

        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "Price must be a positive number.")]
        public decimal Price { get; set; }

        [Required]
        public DateTime DateOfManufacturing { get; set; }

        public bool Active { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Sort order must be a positive number.")]
        public int SortOrder { get; set; }

        [Required]
        public List<string> Images { get; set; } = new List<string>();
    }

}
