using Microsoft.AspNetCore.Mvc;
using x86Cinemas.Data;
using x86Cinemas.Models;
using Microsoft.EntityFrameworkCore;
using x86Cinemas.ViewsModels;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Newtonsoft.Json;

namespace x86Cinemas.Controllers
{
    public class AccesoController : Controller
    {
        private readonly AppDbContext _appDbContext;
        public AccesoController(AppDbContext appDbContext) 
        {
            _appDbContext = appDbContext;
        }

        /// captcha <summary>
        private async Task<bool> ValidateCaptcha(string captchaResponse)
        {
            using (var httpClient = new HttpClient())
            {
                var response = await httpClient.GetStringAsync($"https://www.google.com/recaptcha/api/siteverify?secret=6LdVvHEqAAAAAEueLUYVdVL0ld7HhTp9QPVtTB64&response={captchaResponse}");
                dynamic jsonData = Newtonsoft.Json.JsonConvert.DeserializeObject(response);
                return jsonData.success == "true";
            }
        }


        [HttpGet]
        public IActionResult Registrarse()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Registrarse(UsuarioVM modelo)
        {
            
            if (modelo.Clave != modelo.ConfirmarClave)
            {
                ViewData["Mensaje"] = "Las contraseñas no coinciden";
                return View();
            }
            var captchaResponse = Request.Form["g-recaptcha-response"];
            if (!await ValidateCaptcha(captchaResponse))
            {
                ViewData["Mensaje"] = "El usuario o la contraseña no son el correcto o el catpcha";
                return View();
            }
                Usuarios usuario = new Usuarios()
            {
                NombreCompleto = modelo.NombreCompleto,
                Correo = modelo.Correo,
                Clave = modelo.Clave,
                Administrador =  "No"
                };
            await _appDbContext.Usuarios.AddAsync(usuario);
            await _appDbContext.SaveChangesAsync();

            if (usuario.ID != 0) return RedirectToAction("Login", "Acceso");
            ViewData["Mensaje"] = "Usuario no se pudo crear";

			return View();
        }
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginVM modelo)
        {
            Usuarios? usuario_registrado = await _appDbContext.Usuarios.Where
                                           (u => u.Correo == modelo.Correo &&
                                            u.Clave == modelo.Clave).FirstOrDefaultAsync();
            var captchaResponse = Request.Form["g-recaptcha-response"];
            if (!await ValidateCaptcha(captchaResponse)){
                ViewData["Mensaje"] = "El usuario o la contraseña no son el correcto o el catpcha";
                return View();
            }
            if (usuario_registrado == null ) 
            {
                ViewData["Mensaje"] = "El usuario o la contraseña no son el correcto o el catpcha";
                return View();
            }// LA SESSION DEL USUARIO
            List<Claim> claims = new List<Claim>()
            {
                new Claim(ClaimTypes.Name, usuario_registrado.NombreCompleto)
            };

            ClaimsIdentity claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            AuthenticationProperties properties = new AuthenticationProperties()
            {
                AllowRefresh = true,
            };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                properties
                );
            if (usuario_registrado.Administrador=="Yes") {
                return RedirectToAction("Privacy", "Home");
            }

            return RedirectToAction("Index", "Home");
        }
    }
}
