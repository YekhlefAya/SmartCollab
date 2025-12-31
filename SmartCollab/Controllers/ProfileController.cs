using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SmartCollab.Models;
using System;
using System.IO;
using System.Threading.Tasks;

namespace SmartCollab.Controllers
{
    [Authorize]
    public class ProfileController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;

        public ProfileController(UserManager<User> userManager, SignInManager<User> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        // GET: /Profile/Edit
        [HttpGet]
        public async Task<IActionResult> Edit()
        {
            // Récupérer l'utilisateur actuellement connecté
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return NotFound();

            return View(user);
        }

        // POST: /Profile/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(User model, IFormFile avatar)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return NotFound();

            // Mettre à jour les champs
            user.FirstName = model.FirstName;
            user.LastName = model.LastName;
            user.Email = model.Email;
            user.PhoneNumber = model.PhoneNumber;
            user.Position = model.Position;
            user.Department = model.Department;
            user.City = model.City;
     

            // Gérer l'upload de l'avatar
            if (avatar != null && avatar.Length > 0)
            {
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "avatars");
                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                var fileName = $"{Guid.NewGuid()}_{avatar.FileName}";
                var filePath = Path.Combine(uploadsFolder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await avatar.CopyToAsync(stream);
                }

                // Sauvegarder le chemin relatif dans la base
                user.ProfilePicturePath = $"/uploads/avatars/{fileName}";
            }

            // Mettre à jour l'utilisateur dans la base
            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                    ModelState.AddModelError("", error.Description);

                return View(user);
            }

            // Rafraîchir le cookie si email ou autre info a changé
            await _signInManager.RefreshSignInAsync(user);

            TempData["SuccessMessage"] = "Profil mis à jour avec succès !";
            return RedirectToAction("Index"); // Retour à la vue du profil
        }

        // GET: /Profile
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return NotFound();

            return View(user);
        }
    }
}
