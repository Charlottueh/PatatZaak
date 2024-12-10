using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PatatZaak.Data;
using PatatZaak.Models.Businesslayer;

namespace PatatZaak.Controllers
{
    public class OrdersController : Controller
    {
        private readonly PatatZaakDB _context;

        public OrdersController(PatatZaakDB context)
        {
            _context = context;
        }

        // GET: Orders
        public async Task<IActionResult> Index()
        {
            var patatZaakDB = _context.Order.Include(o => o.OrderUser);
            return View(await patatZaakDB.ToListAsync());
        }

        // GET: Orders/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var order = await _context.Order
                .Include(o => o.OrderUser)
                .FirstOrDefaultAsync(m => m.OrderId == id);
            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        // GET: Orders/Create
        public IActionResult Create()
        {
            ViewData["UserId"] = new SelectList(_context.User, "UserId", "Password");
            return View();
        }

        // POST: Orders/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddProductToOrder(int orderId, int productId, int quantity)
        {
            if (quantity <= 0) return BadRequest("Quantity must be greater than zero.");

            var order = _context.Order.Include(o => o.Products).FirstOrDefault(o => o.OrderId == orderId);
            if (order == null) return NotFound();

            var product = _context.Product.Find(productId);
            if (product == null) return NotFound();

            var existingProduct = order.Products.FirstOrDefault(p => p.ProductId == productId);

            if (existingProduct != null)
            {
                existingProduct.ProductQuantity += quantity;

                if (existingProduct.ProductQuantity <= 0)
                {
                    // Als de hoeveelheid nul of negatief wordt, verwijder het product
                    order.Products.Remove(existingProduct);
                }
            }
            else
            {
                if (quantity > 0)
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
            }

            _context.SaveChanges();
            return RedirectToAction("Edit", new { id = orderId });
        }

        // GET: Orders/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var order = await _context.Order.FindAsync(id);
            if (order == null)
            {
                return NotFound();
            }
            ViewData["UserId"] = new SelectList(_context.User, "UserId", "Password", order.UserId);
            return View(order);
        }

        // POST: Orders/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("OrderId,Ordernumber,OrderStatus,UserId")] Order order)
        {
            if (id != order.OrderId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(order);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!OrderExists(order.OrderId))
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
            ViewData["UserId"] = new SelectList(_context.User, "UserId", "Password", order.UserId);
            return View(order);
        }

        // GET: Orders/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var order = await _context.Order
                .Include(o => o.OrderUser)
                .FirstOrDefaultAsync(m => m.OrderId == id);
            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        // POST: Orders/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var order = await _context.Order.FindAsync(id);
            if (order != null)
            {
                _context.Order.Remove(order);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool OrderExists(int id)
        {
            return _context.Order.Any(e => e.OrderId == id);
        }
    }
}
