using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PatatZaak.Data;
using PatatZaak.Models.Businesslayer;
using PatatZaak.Models.Viewmodels;
using System;
using System.Collections.Generic;
using System.Linq;

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

        // GET: Orders/AddToCart/5
        public IActionResult AddToCart(int productId, int quantity)
        {
            // Get the user ID from User.Identity.Name (assuming it returns the user ID as a string)
            var userIdString = User.Identity.Name;
            int userId;

            // Try to parse the string to an integer
            if (int.TryParse(userIdString, out userId))
            {
                var order = _context.Order.Include(o => o.Products)
                                   .FirstOrDefault(o => o.OrderStatus == 0 && o.UserId == userId);  // In Progress status

                if (order == null)
                {
                    order = new Order
                    {
                        UserId = userId,
                        OrderStatus = 0,  // Should remain an integer
                        Products = new List<Product>()  // Assuming Product exists
                    };
                    _context.Order.Add(order);
                    _context.SaveChanges();  // Save new order to database
                }

                var product = _context.Product.Find(productId);
                if (product == null)
                {
                    return NotFound();
                }

                var existingProduct = order.Products.FirstOrDefault(p => p.ProductId == productId);
                if (existingProduct != null)
                {
                    existingProduct.ProductQuantity += quantity;
                }
                else
                {
                    order.Products.Add(new Product
                    {
                        ProductId = productId,
                        ProductQuantity = quantity,
                        ProductPrice = product.ProductPrice,
                        ProductName = product.ProductName
                    });
                }

                _context.SaveChanges();  // Save changes to the database

                return RedirectToAction("Cart", new { orderId = order.OrderId });
            }
            else
            {
                return NotFound("Invalid User ID");
            }
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
        public IActionResult RemoveFromCart(int orderId, int productId)
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

            order.Products.Remove(product);
            _context.SaveChanges();  // Save changes to the database

            return RedirectToAction("Cart", new { orderId = orderId });
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