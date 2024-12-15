﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
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

        // GET: Products
        public IActionResult Index()
        {
            // Get the current user ID from User.Identity.Name (assuming it's a string)
            var userIdString = User.Identity.Name;
            int userId;

            // Try to parse the string to an integer (if userId is stored as an integer in the Order table)
            if (int.TryParse(userIdString, out userId))
            {
                // Get all products
                var products = _context.Product.ToList();

                // Get the current cart (Order) for the user
                var order = _context.Order
                                    .Include(o => o.Products)
                                    .FirstOrDefault(o => o.OrderStatus == 0 && o.UserId == userId);  // In Progress order

                // Get the cart items, or an empty list if no order exists
                var cartItems = order?.Products.ToList() ?? new List<Product>();

                // Create a ViewModel to pass to the view
                var viewModel = products.Select(p => new ProductViewModel
                {
                    ProductId = p.ProductId,
                    ProductName = p.ProductName,
                    ProductPrice = p.ProductPrice,
                    ProductQuantity = p.ProductQuantity,
                    Photopath = p.Photopath,
                    QuantityInCart = cartItems.FirstOrDefault(ci => ci.ProductId == p.ProductId)?.ProductQuantity ?? 0
                }).ToList();

                return View(viewModel);
            }
            else
            {
                // Handle the case where the UserId is not a valid integer
                return RedirectToAction("Error");  // Redirect to an error page or show a message
            }
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
        public async Task<IActionResult> Create([Bind("ProductId,ProductName,ProductCat,ProductDescription,ProductPoints,ProductDiscount,Photopath,ProductPrice,ProductQuantity,OrderId")] Product product)
        {
            if (ModelState.IsValid)
            {
                // Handle image upload if a file is selected
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

                _context.Add(product);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

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