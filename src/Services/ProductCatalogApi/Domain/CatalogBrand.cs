using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ProductCatalogApi.Domain
{
    public class CatalogBrand
    {
        [Key]
        [Required]
        public int id { get; set; }
        [Required]
        [MaxLength(100)]
        public string Brand { get; set; }
    }
}
