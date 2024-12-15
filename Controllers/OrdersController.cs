using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PatatZaak.Data;
using PatatZaak.Models.Businesslayer;
using PatatZaak.Models.Viewmodels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;

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
        public IActionResult Index()
        {
            var orders = _context.Order.Include(o => o.Products).Include(o => o.OrderUser).ToList();
            return View(orders);
        }

        // GET: Orders/Details/5
        public IActionResult Details(int id)
        {
            var order = _context.Order
                .Include(o => o.Products)
                .Include(o => o.OrderUser)
                .FirstOrDefault(o => o.OrderId == id);

            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        // GET: Orders/Create
        public IActionResult Create()
        {
            ViewBag.UserId = new SelectList(_context.User, "Id", "Username");
            return View();
        }

        // POST: Orders/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Order order)
        {
            if (ModelState.IsValid)
            {
                _context.Add(order);
                _context.SaveChanges();
                return RedirectToAction(nameof(Index));
            }

            ViewBag.UserId = new SelectList(_context.User, "Id", "Username", order.UserId);
            return View(order);
        }

        // GET: Orders/Edit/5
        public IActionResult Edit(int id)
        {
            var order = _context.Order.Find(id);
            if (order == null)
            {
                return NotFound();
            }

            ViewBag.UserId = new SelectList(_context.User, "Id", "Username", order.UserId);
            return View(order);
        }

        // POST: Orders/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Order order)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.UserId = new SelectList(_context.User, "Id", "Username", order.UserId);
                return View(order);
            }

            var existingOrder = _context.Order.Find(order.OrderId);
            if (existingOrder == null)
            {
                return NotFound("Order not found");
            }

            existingOrder.Ordernumber = order.Ordernumber;
            existingOrder.OrderStatus = order.OrderStatus;
            existingOrder.UserId = order.UserId;

            _context.Update(existingOrder);
            _context.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        // GET: Orders/Delete/5
        public IActionResult Delete(int id)
        {
            var order = _context.Order
                .Include(o => o.Products)
                .FirstOrDefault(o => o.OrderId == id);

            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        // POST: Orders/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            var order = _context.Order.Find(id);
            if (order == null)
            {
                return NotFound();
            }

            _context.Order.Remove(order);
            _context.SaveChanges();
            return RedirectToAction(nameof(Index));
        }

        // GET: Orders/Confirmation/5
        public IActionResult Confirmation(int id)
        {
            var order = _context.Order
                .Include(o => o.Products)
                .FirstOrDefault(o => o.OrderId == id);

            if (order == null)
            {
                return NotFound("Order not found");
            }

            var viewModel = new ConfirmationVM
            {
                OrderNumber = order.Ordernumber,  // Keep as integer
                TotalPrice = order.TotalPrice,
                Products = order.Products.Select(p => new ProductVM
                {
                    ProductName = p.ProductName,
                    ProductPrice = p.ProductPrice,
                    ProductQuantity = p.ProductQuantity
                }).ToList()
            };

            viewModel.GeneratePickupNumber();  // Generate the pickup number
            viewModel.GeneratePickupTime(order.PickupTime);  // Set pickup time

            return View(viewModel);
        }

        // POST: Orders/UpdateStatus/5
        [HttpPost]
        public IActionResult UpdateStatus(int id)
        {
            var order = _context.Order.Find(id);
            if (order == null)
            {
                return NotFound();
            }

            // Assuming OrderStatus is an integer where:
            // 0 = In Progress, 
            // 1 = Confirmed, 
            // 2 = Ready for Pickup,
            // 3 = Completed

            if (order.OrderStatus == 0)  // In Progress
            {
                order.OrderStatus = 1;  // Confirmed
            }
            else if (order.OrderStatus == 1)  // Confirmed
            {
                order.OrderStatus = 2;  // Ready for Pickup
            }
            else if (order.OrderStatus == 2)  // Ready for Pickup
            {
                order.OrderStatus = 3;  // Completed
            }

            _context.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        [Authorize]
        public async Task<IActionResult> AddToCart(int productId, int quantity)
        {
            // Haal de UserId van de ingelogde gebruiker op
            var userIdString = User.Identity.Name;
            int userId;

            // Controleer of de UserId een geldig geheel getal is
            if (!int.TryParse(userIdString, out userId))
            {
                return Json(new { success = false, message = "Ongeldige gebruiker" });
            }

            // Zoek naar een bestaande order voor deze gebruiker (Winkelwagentje)
            var order = await _context.Order.Include(o => o.Products)
                .FirstOrDefaultAsync(o => o.OrderStatus == 0 && o.UserId == userId); // Zoek naar een order die 'In Progress' is

            if (order == null)
            {
                // Als er geen order bestaat, maak dan een nieuwe aan
                order = new Order
                {
                    UserId = userId,
                    OrderStatus = 0,  // Status 0 = In Progress
                    Products = new List<Product>()
                };
                _context.Order.Add(order); // Voeg de nieuwe order toe aan de database
                await _context.SaveChangesAsync(); // Sla op
            }

            // Zoek het product op basis van productId
            var product = await _context.Product.FindAsync(productId);
            if (product == null)
            {
                return Json(new { success = false, message = "Product niet gevonden" });
            }

            // Controleer of het product al in de winkelwagentje staat
            var existingProduct = order.Products.FirstOrDefault(p => p.ProductId == productId);
            if (existingProduct != null)
            {
                // Als het product al in de winkelwagentje staat, verhoog dan de hoeveelheid
                existingProduct.ProductQuantity += quantity;
            }
            else
            {
                // Als het product nog niet in de winkelwagentje staat, voeg het dan toe
                order.Products.Add(new Product
                {
                    ProductId = productId,
                    ProductQuantity = quantity,
                    ProductPrice = product.ProductPrice,
                    ProductName = product.ProductName
                });
            }

            // Sla de wijzigingen op in de database
            await _context.SaveChangesAsync();

            // Geef een succesbericht terug
            return Json(new { success = true, message = "Product toegevoegd aan winkelwagentje" });
        }

        // GET: Orders/Cart/5
        public IActionResult Cart(int orderId)
        {
            var order = _context.Order.Include(o => o.Products)
                                      .FirstOrDefault(o => o.OrderId == orderId);

            if (order == null)
            {
                return NotFound();
            }

            var viewModel = new CartViewModel
            {
                OrderId = order.OrderId,
                TotalPrice = order.Products.Sum(p => p.ProductPrice * p.ProductQuantity),
                Products = order.Products.Select(p => new CartProductViewModel
                {
                    ProductName = p.ProductName,
                    ProductPrice = p.ProductPrice,
                    ProductQuantity = p.ProductQuantity
                }).ToList()
            };

            return View(viewModel);
        }

        // POST: Orders/UpdateQuantity
        [HttpPost]
        public IActionResult UpdateQuantity(int orderId, int productId, int quantity)
        {
            var order = _context.Order.Include(o => o.Products)
                                      .FirstOrDefault(o => o.OrderId == orderId);

            if (order == null)
            {
                return NotFound();
            }

            var product = order.Products.FirstOrDefault(p => p.ProductId == productId);
            if (product == null)
            {
                return NotFound();
            }

            if (quantity > 0)
            {
                product.ProductQuantity = quantity;
            }
            else
            {
                order.Products.Remove(product);  // Remove product if quantity is 0 or less
            }

            _context.SaveChanges();  // Save changes to the database

            return RedirectToAction("Cart", new { orderId = orderId });
        }

        // POST: Orders/RemoveFromCart
        [HttpPost]
        public IActionResult RemoveFromCart(int productId, int quantity)
        {
            var userIdString = User.Identity.Name;
            int userId;

            if (int.TryParse(userIdString, out userId))
            {
                // Find the order (cart)
                var order = _context.Order.Include(o => o.Products)
                    .FirstOrDefault(o => o.OrderStatus == 0 && o.UserId == userId);

                if (order == null)
                {
                    return NotFound();
                }

                // Find the product in the cart
                var existingProduct = order.Products.FirstOrDefault(p => p.ProductId == productId);
                if (existingProduct != null)
                {
                    if (existingProduct.ProductQuantity > quantity)
                    {
                        existingProduct.ProductQuantity -= quantity; // Decrease the quantity
                    }
                    else
                    {
                        order.Products.Remove(existingProduct); // Remove from cart if quantity is 0
                    }

                    _context.SaveChanges(); // Save changes
                    return Json(new { success = true, message = "Item removed from cart" });
                }
                else
                {
                    return Json(new { success = false, message = "Item not found in cart" });
                }
            }

            return Json(new { success = false, message = "Invalid user ID" });
        }

        // POST: Orders/Checkout/5
        [HttpPost]
        public IActionResult Checkout(int id)
        {
            var order = _context.Order
                .Include(o => o.Products)
                .FirstOrDefault(o => o.OrderId == id);

            if (order == null)
            {
                return NotFound("Order not found");
            }

            var viewModel = new ConfirmationVM
            {
                OrderNumber = order.Ordernumber,
                TotalPrice = order.TotalPrice,
                Products = order.Products.Select(p => new ProductVM
                {
                    ProductName = p.ProductName,
                    ProductPrice = p.ProductPrice,
                    ProductQuantity = p.ProductQuantity
                }).ToList()
            };

            // Generate a valid integer pickup number
            viewModel.PickupNumber = viewModel.GeneratePickupNumber();  // This will now return an int

            viewModel.GeneratePickupTime(order.PickupTime);  // Set pickup time

            return View(viewModel);
        }
    }

}