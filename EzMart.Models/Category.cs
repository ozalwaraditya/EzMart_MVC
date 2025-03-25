using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace EzMart.Models
{
    public class Category
    {
        [Key]
        public int Id { get; set; }
        [Required]
        //These are used for server side validations;
        [MaxLength(50)]
        [DisplayName("Category Name")]
        public string Name { get; set; }
        [Range(1, 100, ErrorMessage = "Custom Error Message")]
        public int DisplayOrder { get; set; }
    }
}
