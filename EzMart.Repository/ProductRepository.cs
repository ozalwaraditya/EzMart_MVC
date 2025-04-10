using EzMart.DataAccess.Data;
using EzMart.Models;
using EzMart.Repository.IRepository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EzMart.Repository
{
    public class ProductRepository : Repository<Product>, IProductRepository
    {
        private readonly ApplicationDbContext _context;

        public ProductRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public void Update(Product product)
        {
            var objInDb = _context.Products.FirstOrDefault(u => u.Id == product.Id);
            if (objInDb != null)
            {
                objInDb.Title = product.Title;
                objInDb.ISBN = product.ISBN;
                objInDb.Author = product.Author;
                objInDb.Description = product.Description;
                objInDb.ListPrice = product.ListPrice;
                objInDb.Price = product.Price;
                objInDb.Price50 = product.Price50;
                objInDb.Price100 = product.Price100;
                objInDb.CategoryId = product.CategoryId;

                if (!string.IsNullOrEmpty(product.ImgUrl))
                {
                    objInDb.ImgUrl = product.ImgUrl;
                }
            }
        }
    }
}
