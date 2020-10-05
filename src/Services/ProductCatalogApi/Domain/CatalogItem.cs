using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace ProductCatalogApi.Domain
{
    [Table("Catalog")]
    public class CatalogItem
    {
        [Key]
        [Required]

        public int Id { get; set; }
        [Required]
        [MaxLength(50)]
        public string Name { get; set; }
        public string Description { get; set; }
        [Required]
        [MaxLength(50)]
        public decimal Price { get; set; }
        public string PictureFileName { get; set; }
        public string PictureUrl { get; set; }

        public int CatalogTypeId { get; set; }
        public int CatalogBrandId { get; set; }

        //navigation properties
        public CatalogBrand CatalogBrand { get; set; }
        public CatalogType CatalogType { get; set; }
    }
}
