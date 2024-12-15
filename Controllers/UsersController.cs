using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages;
using PatatZaak.Models.Businesslayer;
using PatatZaak.Models.Viewmodels;
using PatatZaak.Data;


namespace PatatZaak.Controllers.DataController
{

    public class UsersController : Controller
    {
        private readonly PatatZaakDB _context;
        private readonly PasswordHasher<User> _passwordHasher;

        public UsersController(PatatZaakDB context, ILogger<UsersController> logger)
        {

            _context = context;
            _passwordHasher = new PasswordHasher<User>();
        }

        // GET: Users
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Index()
        {
            var PatatZaakDB = _context.User.Include(u => u.Role);
            return View(await PatatZaakDB.ToListAsync());
        }

        // GET: Users/Details/5
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _context.User
                .Include(u => u.Role)
                .FirstOrDefaultAsync(m => m.UserId == id);
            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }
        [Authorize(Roles = "admin")]
        public async Task<List<UserVM>> AllUsers()
        {
            var PatatZaakDB = _context.User
                .Include(u => u.Role)
                .Select(u => new UserVM
                {
                    UserId = u.UserId,
                    UserName = u.UserName,
                    Password = u.Password,
                    RoleId = u.RoleId,
                    Role = u.Role
                })
                .ToListAsync();

            return await PatatZaakDB;
        }

        // GET: Users/Create
        [Authorize(Roles = "admin")]
        public IActionResult Create()
        {
            var roles = _context.Role // of filter op RoleId: r.RoleId != adminRoleId
                .Select(r => new { r.RoleId, r.RoleName })
                .ToList();

            ViewBag.RoleId = new SelectList(roles, "RoleId", "RoleName");

            return View();
        }

        // POST: Users/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [Authorize(Roles = "admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("UserId,UserName,Password,ConfirmPassword, RoleId")] RegisterVM registerVM)
        {
            if (ModelState.IsValid)
            {
                // Check if the username already exists
                var existingUser = await _context.User
                    .FirstOrDefaultAsync(u => u.UserName == registerVM.UserName);

                if (existingUser != null)
                {
                    // Add a model state error if the username is taken
                    ModelState.AddModelError("UserName", "Deze gebruikersnaam bestaat al.");
                }
                else
                {
                    // Create a new User object from the view model
                    var user = new User
                    {
                        UserName = registerVM.UserName,
                        Password = _passwordHasher.HashPassword(new User(), registerVM.Password), // Hash the password
                        RoleId = registerVM.RoleId
                    };

                    // Add user to the database
                    _context.Add(user);
                    await _context.SaveChangesAsync();
                    return RedirectToAction("Index", "Home");
                }
            }
            // If we got this far, something failed; redisplay the form with the same model
            ViewData["RoleId"] = new SelectList(_context.Role, "RoleId", "RoleName", registerVM.RoleId);
            return View(registerVM); // Pass the RegisterVM back to the view
        }

        // GET: Users/Edit/5
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _context.User.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            // Clear the password field before sending the user data to the view
            user.Password = string.Empty;

            // Populate the roles for dropdown in the view
            ViewData["RoleId"] = new SelectList(_context.Role, "RoleId", "RoleName", user.RoleId);
            return View(user);
        }

        // POST: Users/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [Authorize(Roles = "admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("UserId,UserName,Password,RoleId")] User userVM)
        {
            if (id != userVM.UserId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                // Check if another user already has the same username
                var existingUser = await _context.User
                    .FirstOrDefaultAsync(u => u.UserName == userVM.UserName && u.UserId != userVM.UserId);

                if (existingUser != null)
                {
                    // Add a model state error if the username is taken by another user
                    ModelState.AddModelError("UserName", "Deze gebruikersnaam bestaat al.");
                }
                else
                {
                    try
                    {
                        // Retrieve the user to edit from the database
                        var user = await _context.User.FindAsync(userVM.UserId);
                        if (user == null)
                        {
                            return NotFound();
                        }

                        // Update the user's information
                        user.UserName = userVM.UserName;

                        // Only update the password if it was modified (assuming empty means no change)
                        if (!string.IsNullOrEmpty(userVM.Password))
                        {
                            user.Password = _passwordHasher.HashPassword(user, userVM.Password);
                        }

                        user.RoleId = userVM.RoleId;

                        // Mark the user entity as modified
                        _context.Update(user);
                        await _context.SaveChangesAsync();

                        return RedirectToAction("Index", "Home");
                    }
                    catch (DbUpdateConcurrencyException)
                    {
                        if (!_context.User.Any(u => u.UserId == userVM.UserId))
                        {
                            return NotFound();
                        }
                        else
                        {
                            throw;
                        }
                    }
                }
            }

            // If model validation fails, re-populate role list and return the view
            ViewData["RoleId"] = new SelectList(_context.Role, "RoleId", "RoleName", userVM.RoleId);
            return View(userVM);
        }


        // GET: Users/Delete/5
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _context.User
                .Include(u => u.Role)
                .FirstOrDefaultAsync(m => m.UserId == id);
            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        // POST: Users/Delete/5
        [Authorize(Roles = "admin")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var user = await _context.User.FindAsync(id);
            if (user != null)
            {
                _context.User.Remove(user);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Login()
        {
            return View();
        }

        // POST: Login a user
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginVM model)
        {
            if (ModelState.IsValid)
            {
                var user = await _context.User
                    .Include(u => u.Role)
                    .SingleOrDefaultAsync(u => u.UserName == model.UserName);

                if (user != null)
                {
                    var result = _passwordHasher.VerifyHashedPassword(user, user.Password, model.Password);

                    if (result == PasswordVerificationResult.Success)
                    {
                        var roleName = user.Role?.RoleName;

                        var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, model.UserName),
                    new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                    new Claim(ClaimTypes.Role, roleName ?? "")
                };

                        var claimsIdentity = new ClaimsIdentity(claims, "Cookie");

                        await HttpContext.SignInAsync("Cookies", new ClaimsPrincipal(claimsIdentity));

                        // Redirect based on role
                        if (roleName == "admin")
                        {
                            return RedirectToAction("Dashboard", "Home");  // Admin-specific dashboard
                        }
                        else if (roleName == "worker")
                        {
                            return RedirectToAction("WorkerDashboard", "Home"); // Worker-specific dashboard
                        }
                        else if (roleName == "user") // For users (or other roles you may have)
                        {
                            return RedirectToAction("Index", "Home");  // Default redirect for users
                        }
                        else
                        {
                            return RedirectToAction("Index", "Home");  // Default page for non-admin, non-worker roles
                        }
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, "Naam of wachtwoord is incorrect.");
                    }
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Naam of wachtwoord is incorrect.");
                }
            }
            return View(model);
        }

        // GET: Users/Register
        [HttpGet]
        public IActionResult AccountReg()
        {
            // Voor de dropdownlijst voor rollen
            ViewData["RoleId"] = new SelectList(_context.Role, "RoleId", "RoleName");
            return View();
        }

        // POST: Users/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AccountReg(RegisterVM registerVM)
        {
            if (ModelState.IsValid)
            {
                // Check of de gebruikersnaam al bestaat in de database
                var existingUser = await _context.User
                    .FirstOrDefaultAsync(u => u.UserName == registerVM.UserName);

                if (existingUser != null)
                {
                    // Als de gebruiker al bestaat, voeg een foutmelding toe aan de modelstate
                    ModelState.AddModelError("UserName", "Deze gebruikersnaam bestaat al.");
                }
                else
                {
                    // Maak een nieuwe User object aan
                    var user = new User
                    {
                        UserName = registerVM.UserName,
                        // Wachtwoord hashen
                        Password = _passwordHasher.HashPassword(new User(), registerVM.Password),
                        RoleId = registerVM.RoleId // Rol van de gebruiker
                    };

                    // Voeg de gebruiker toe aan de context (de database)
                    _context.Add(user);
                    await _context.SaveChangesAsync();  // Sla de gebruiker op in de database

                    // Na succesvolle registratie, redirect naar de homepage
                    return RedirectToAction("Index", "Home");
                }
            }

            // Als er fouten zijn, stuur de gebruiker terug naar de registratiepagina
            ViewData["RoleId"] = new SelectList(_context.Role, "RoleId", "RoleName", registerVM.RoleId);
            return View(registerVM);
        }


        //LOGOUT
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync("Cookies");
            return RedirectToAction("Index", "Home"); // Redirect to home or login page after logout
        }

        private bool UserExists(int id)
        {
            return _context.User.Any(e => e.UserId == id);
        }
    }
}