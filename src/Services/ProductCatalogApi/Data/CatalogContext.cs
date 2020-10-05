using Microsoft.EntityFrameworkCore;
using ProductCatalogApi.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProductCatalogApi.Data
{
    public class CatalogContext : DbContext
    {
        public CatalogContext(DbContextOptions<CatalogContext> dbContextOptions) : base(dbContextOptions)
        {

        }

        protected override void OnModelCreating(ModelBuilder modelbuilder)
        {
            modelbuilder.UseHiLo("catalog_hilo").Entity<CatalogBrand>();
            modelbuilder.UseHiLo("catalog_type_hilo").Entity<CatalogType>();
            modelbuilder.UseHiLo("catalog_brand_hilo").Entity<CatalogItem>();
        }

        public DbSet<CatalogType> CatalogTypes { get; set; }
        public DbSet<CatalogBrand> CatalogBrands { get; set; }
        public DbSet<CatalogItem> CatalogItems { get; set; }
    }
}
