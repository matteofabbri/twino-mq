using System.ComponentModel.DataAnnotations;

namespace ProductService.Domain
{
	public class ProductCategory
	{
		[Key]
		public int Id { get; set; }
		public string Name { get; set; }
	}
}