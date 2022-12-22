using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Fiorello.Models
{
    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Image { get; set; }
        public int Price { get; set; }
        [NotMapped]
        public IFormFile Photo { get; set; }
        public Category Category { get; set; }
        public int CategoryId { get; set; }
        public bool IsDeactive { get; set; }
        public ProductDetail ProductDetail { get; set; }
    }
}
