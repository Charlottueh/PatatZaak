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
using System.Speech.Synthesis;
using static NuGet.Packaging.PackagingConstants;


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
        public IActionResult Confirmation()
        {
            // Get the current user ID
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out int userId))
            {
                return RedirectToAction("Login", "Account");
            }

            // Fetch the user's uncompleted order (status 0 means uncompleted)
            var order = _context.Order
                .Include(o => o.Products)
                .FirstOrDefault(o => o.OrderStatus == 0 && o.UserId == userId);

            if (order == null || order.Products == null || !order.Products.Any())
            {
                return RedirectToAction("Index", "Home"); // Redirect if no order is found
            }

            // Mark the order as completed (status 1 means completed)
            order.OrderStatus = 1; // Assuming 1 means completed
            _context.SaveChanges();

            // Create the view model for confirmation
            var viewModel = new ConfirmationVM
            {
                OrderNumber = order.Ordernumber,  // Order number
                TotalPrice = order.TotalPrice,    // Total price of the order
                Products = order.Products.Select(p => new ProductVM
                {
                    ProductName = p.ProductName,
                    ProductPrice = p.ProductPrice,
                    ProductQuantity = p.ProductQuantity
                }).ToList(),
                PickupNumber = GeneratePickupNumber()  // Generate the pickup number
            };

            // Generate pickup time (30 minutes after the order's pickup time)
            viewModel.GeneratePickupTime(order.PickupTime);

            // Return the confirmation view
            return View(viewModel);
        }

        // Helper method to generate a pickup number (unique)
        private int GeneratePickupNumber()
        {
            return DateTime.Now.Ticks.GetHashCode(); // Unique pickup number using DateTime ticks
        }

        // POST: Orders/UpdateStatus/
        [HttpPost]
        public IActionResult UpdateStatus(int id)
        {
            var order = _context.Order.Find(id);
            if (order == null)
            {
                return NotFound();
            }

            // Update the status based on the current status
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

            GenerateSpeech(order.OrderId);  // Call to generate speech

            
            return RedirectToAction("Index", "Orders");

        }

        [Authorize]
        public async Task<IActionResult> AddToCart(int productId, int quantity)
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out int userId))
            {
                return Json(new { success = false, message = "Ongeldige gebruiker" });
            }

            var order = await _context.Order
                .Include(o => o.Products)
                .FirstOrDefaultAsync(o => o.OrderStatus == 0 && o.UserId == userId);

            if (order == null)
            {
                order = new Order
                {
                    UserId = userId,
                    OrderStatus = 0,
                    Products = new List<Product>()
                };
                _context.Order.Add(order);
                await _context.SaveChangesAsync();
            }

            var product = await _context.Product.FirstOrDefaultAsync(p => p.ProductId == productId);
            if (product == null)
            {
                return Json(new { success = false, message = "Product niet gevonden" });
            }

            // Check if the product already exists in the cart
            var existingProduct = order.Products.FirstOrDefault(p => p.ProductId == productId);
            if (existingProduct != null)
            {
                existingProduct.ProductQuantity += quantity;
            }
            else
            {
                // Add only the reference to the product, not a new instance
                order.Products.Add(product);

                // Update the quantity directly
                product.ProductQuantity = quantity;
            }

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Product succesvol toegevoegd aan de winkelwagen!";
            return RedirectToAction("Cart"); // Toon direct de winkelwagen
        }

        // GET: Orders/Cart/5
        public IActionResult Cart()
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out int userId))
            {
                return RedirectToAction("Login", "Account");
            }

            var order = _context.Order
                .Include(o => o.Products)
                .FirstOrDefault(o => o.OrderStatus == 0 && o.UserId == userId);

            if (order == null || order.Products == null || !order.Products.Any())
            {
                return View(new CartViewModel { Products = new List<CartProductViewModel>(), TotalPrice = 0 });
            }

            var products = order.Products.Select(p => new CartProductViewModel
            {
                ProductId = p.ProductId,
                ProductName = p.ProductName,
                ProductPrice = p.ProductPrice,
                ProductQuantity = p.ProductQuantity
            }).ToList();

            var cart = new CartViewModel
            {
                Products = products,
                TotalPrice = order.TotalPrice
            };

            return View(cart);
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
        public IActionResult Checkout()
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out int userId))
            {
                return RedirectToAction("Login", "Account");
            }

            // Fetch the order associated with the user and status 0 (uncompleted orders)
            var order = _context.Order
                .Include(o => o.Products)
                .FirstOrDefault(o => o.OrderStatus == 0 && o.UserId == userId);

            if (order == null || order.Products == null || !order.Products.Any())
            {
                return RedirectToAction("Index", "Home");  // Redirect if no order is found
            }

            // Mark the order as completed (you can also adjust the status here)
            order.OrderStatus = 1;  // Assuming 1 means completed
            _context.SaveChanges();

            // Redirect to the Confirmation page, passing the order id
            return RedirectToAction("Confirmation", new { id = order.OrderId });
        }

        // Method to generate speech and return it as an audio file
        public void GenerateSpeech(int orderId)
        {
            var order = _context.Order.Find(orderId);

            // Initialize the speech synthesizer
            var speech = new SpeechSynthesizer();

            // Set the voice to "Microsoft David Desktop" or your preferred voice
            speech.SelectVoice("Microsoft David Desktop"); // Change to the desired voice
            speech.Volume = 100; // Set volume level (0-100)
            speech.Rate = 0; // Set speech rate (-10 to 10, 0 is normal speed)

            // Prepare the message to be spoken
            var message = $"The order {order.OrderId} is ready for pickup.";
            // Make sure the speech has finished speaking and the file has been written
            speech.Speak(message); // This will generate the WAV file

        }
    }
}

