using Fiorello.Helpers;
using Fiorello.Models;
using Fiorello.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Fiorello.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        public AccountController(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }
        public IActionResult Register()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterVM registerVM)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }
            AppUser appUser = new AppUser
            {
                Name=registerVM.Name,
                Surname=registerVM.Surname,
                UserName=registerVM.Username,
                Email=registerVM.Email
            };
            IdentityResult identityResult= await _userManager.CreateAsync(appUser, registerVM.Password);
            if (!identityResult.Succeeded)
            {
                foreach (IdentityError error in identityResult.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
                return View();
            }
            await _signInManager.SignInAsync(appUser, true);
            await _userManager.AddToRoleAsync(appUser, Helper.Roles.Member.ToString());
            return RedirectToAction("Index", "Home");
        }
        public IActionResult Login()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginVM loginVM)
        {
            AppUser appUser = await _userManager.FindByNameAsync(loginVM.Username);
            if (appUser==null)
            {
                ModelState.AddModelError("", "UserName or Password is wrong !");
                return View();
            }
            Microsoft.AspNetCore.Identity.SignInResult signInResult = await _signInManager.PasswordSignInAsync(appUser, loginVM.Password,true,true);
            if (signInResult.IsLockedOut)
            {
                ModelState.AddModelError("", "Error cix get");
                return View();
            }
            if (!signInResult.Succeeded)
            {
                ModelState.AddModelError("", "UserName or Password is wrong !");
                return View();
            }
            return RedirectToAction("Index", "Home");
        }
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index","Home");
        }
        //public async Task CreateRoles()
        //{
        //    if (!await _roleManager.RoleExistsAsync(Helper.Roles.Admin.ToString()))
        //    {
        //        await _roleManager.CreateAsync(new IdentityRole { Name = Helper.Roles.Admin.ToString() });
        //    }
        //    if (!await _roleManager.RoleExistsAsync(Helper.Roles.Member.ToString()))
        //    {
        //        await _roleManager.CreateAsync(new IdentityRole { Name = Helper.Roles.Member.ToString() });
        //    }
        //}
    }
}
