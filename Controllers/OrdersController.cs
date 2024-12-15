using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PatatZaak.Data;
using PatatZaak.Models.Businesslayer;
using PatatZaak.Models.Viewmodels;

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
            // Haal de order op uit de database
            var order = _context.Order
                .Include(o => o.Products)
                .FirstOrDefault(o => o.OrderId == id);

            // Controleer of de order bestaat
            if (order == null)
            {
                return NotFound("Order not found");
            }

            // Maak het viewmodel aan en vul het met de orderdata
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

            // Genereer het ophaalnummer en de ophaaltijd
            viewModel.GeneratePickupNumber();  // Genereer het ophaalnummer
            viewModel.GeneratePickupTime(order.PickupTime);  // Genereer de pickup time

            // Retourneer het viewmodel naar de view
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

            if (order.OrderStatus < 2)
            {
                order.OrderStatus++;
                _context.SaveChanges();
            }

            return RedirectToAction(nameof(Index));
        }
    }
}