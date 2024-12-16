using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PatatZaak.Data;
using PatatZaak.Models.Businesslayer;
using PatatZaak.Models.Viewmodels;

namespace PatatZaak.Controllers
{
    public class ProductsController : Controller
    {
        private readonly PatatZaakDB _context;

        // Define Photo as IFormFile for image upload
        public IFormFile Photo { get; set; }

        public ProductsController(PatatZaakDB context)
        {
            _context = context;
        }

        [Authorize(Roles = "admin")]
        public async Task<IActionResult> AdminIndex()
        {
            var products = await _context.Product
                .Select(p => new ProductViewModel
                {
                    ProductId = p.ProductId,
                    ProductName = p.ProductName,
                    ProductPrice = p.ProductPrice,
                    Photopath = p.Photopath,
                }).ToListAsync();

            return View(products);
        }

        // GET: Products
        [AllowAnonymous]
        public IActionResult Index()
        {
            var products = _context.Product.Include(p => p.AvailableAddons).ToList();

            var productViewModels = products.Select(p => new ProductViewModel
            {
                ProductId = p.ProductId,
                ProductName = p.ProductName,
                ProductPrice = p.ProductPrice,
                ProductQuantity = p.ProductQuantity,
                Photopath = p.Photopath,
                QuantityInCart = 0, // Placeholder logic
                AvailableAddons = p.AvailableAddons.ToList()
            }).ToList();

            return View(productViewModels);
        }

        // GET: Products/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Product
                .Include(p => p.Order)
                .FirstOrDefaultAsync(m => m.ProductId == id);
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        // GET: Products/Create
        public IActionResult Create()
        {
            ViewData["OrderId"] = new SelectList(_context.Order, "OrderId", "Ordernumber");
            return View();
        }

        // POST: Products/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ProductId,ProductName,ProductCat,ProductDescription,ProductPoints,ProductDiscount,ProductPrice,ProductQuantity")] Product product, IFormFile Photo)
        {
            if (ModelState.IsValid)
            {
                // Add logging or debugging here to ensure data is correct
                Console.WriteLine($"Product Name: {product.ProductName}");
                Console.WriteLine($"Product Price: {product.ProductPrice}");
                Console.WriteLine($"Product Quantity: {product.ProductQuantity}");

                if (Photo != null && Photo.Length > 0)
                {

                    var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images");
                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }


                    var fileName = Path.GetFileName(Photo.FileName);


                    var filePath = Path.Combine(uploadsFolder, fileName);


                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await Photo.CopyToAsync(stream);
                    }
                    product.Photopath = "/images/" + fileName;

                }

                // Add the product to the context
                _context.Add(product);
                await _context.SaveChangesAsync(); // Save changes to the database

                // Redirect to the Index view
                return RedirectToAction(nameof(Index)); // Redirect to Index
            }

            // If not valid, return to the form with validation errors
            ViewData["OrderId"] = new SelectList(_context.Order, "OrderId", "Ordernumber", product.OrderId);
            return View(product);
        }

        // GET: Products/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Product.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            ViewData["OrderId"] = new SelectList(_context.Order, "OrderId", "Ordernumber", product.OrderId);
            return View(product);
        }

        // POST: Products/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ProductId,ProductName,ProductCat,ProductDescription,ProductPoints,ProductDiscount,Photopath,ProductPrice,ProductQuantity,OrderId")] Product product)
        {
            if (id != product.ProductId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(product);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProductExists(product.ProductId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }

            ViewData["OrderId"] = new SelectList(_context.Order, "OrderId", "Ordernumber", product.OrderId);
            return View(product);
        }

        // GET: Products/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Product
                .Include(p => p.Order)
                .FirstOrDefaultAsync(m => m.ProductId == id);
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        // POST: Products/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var product = await _context.Product.FindAsync(id);
            if (product != null)
            {
                _context.Product.Remove(product);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // POST: Products/AddProductToOrder
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddProductToOrder(int orderId, int productId, int quantity)
        {
            if (quantity <= 0) return BadRequest("Quantity must be greater than zero.");

            var product = await _context.Product.FindAsync(productId);
            if (product == null) return NotFound();

            var order = await _context.Order.Include(o => o.Products).FirstOrDefaultAsync(o => o.OrderId == orderId);
            if (order == null) return NotFound();

            var existingProduct = order.Products.FirstOrDefault(p => p.ProductId == productId);

            if (existingProduct != null)
            {
                existingProduct.ProductQuantity += quantity;
                if (existingProduct.ProductQuantity <= 0)
                {
                    order.Products.Remove(existingProduct); // Verwijder product als de hoeveelheid nul of negatief wordt
                }
            }
            else
            {
                order.Products.Add(new Product
                {
                    ProductId = productId,
                    ProductQuantity = quantity,
                    ProductPrice = product.ProductPrice,
                    ProductName = product.ProductName,
                    ProductDescription = product.ProductDescription
                });
            }

            await _context.SaveChangesAsync();
            return RedirectToAction("Edit", "Orders", new { id = orderId });
        }

        private bool ProductExists(int id)
        {
            return _context.Product.Any(e => e.ProductId == id);
        }
    }
}