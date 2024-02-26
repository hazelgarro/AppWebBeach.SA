using AppWebBeachSA.Models;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using System.Net;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;

namespace AppWebBeachSA.Controllers
{
    public class EmpleadosController : Controller
    {
        private HotelAPI hotelApi = null;
        private HttpClient httpClient = null;

        public EmpleadosController()
        {
            hotelApi = new HotelAPI();
            httpClient = hotelApi.Initial();
        }

        public async Task<IActionResult> Index()
        {
            var lista = new List<Empleado>();
            httpClient.DefaultRequestHeaders.Authorization = AutorizacionToken();

            HttpResponseMessage response = await httpClient.GetAsync("/Empleados/Listado");

            if (ValidarTransaccion(response.StatusCode) == false)
            {
                return RedirectToAction("Logout", "");
            }

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadAsStringAsync();
                lista = JsonConvert.DeserializeObject<List<Empleado>>(result);
            }
            else
            {
                TempData["Mensaje"] = "No se logro cargar los empleados verifique la API";
            }
            return View(lista);
        }//end Index

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }//end create

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(List<IFormFile> files, [Bind] Empleado empleado)
        {
            empleado.ID = 0;
            empleado.TipoUsuario = 2;
            var agregar = httpClient.PostAsJsonAsync<Empleado>("/Empleados/Agregar", empleado);
            httpClient.DefaultRequestHeaders.Authorization = AutorizacionToken();

            await agregar;

            var resultado = agregar.Result;

            if (resultado.IsSuccessStatusCode)
            {
                return RedirectToAction("Index");
            }
            else
            {
                TempData["Mensaje"] = "No se logró registrar el empleado " + empleado.NombreCompleto;
                return View(empleado);
            }

        }//end create

        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {

            var user = new Empleado();

            httpClient.DefaultRequestHeaders.Authorization = AutorizacionToken();

            HttpResponseMessage response = await httpClient.GetAsync($"/Empleados/Consultar?ID={id}");

            if (ValidarTransaccion(response.StatusCode) == false)
            {
                return RedirectToAction("Logout", "");
            }

            //Se todo fue correcto
            if (response.IsSuccessStatusCode)
            {
                //se realiza la lectura de los datos en formato JSON
                var resultado = response.Content.ReadAsStringAsync().Result;

                //Se convierte el  JSON  en  un  Object
                user = JsonConvert.DeserializeObject<Empleado>(resultado);
            }

            return View(user);
        }//end edit

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit([Bind] Empleado user)
        {
            httpClient.DefaultRequestHeaders.Authorization = AutorizacionToken();

            var modificar = httpClient.PutAsJsonAsync<Empleado>("/Empleados/Modificar", user);
            await modificar;

            var resultado = modificar.Result;

            if (resultado.IsSuccessStatusCode)
            {
                return RedirectToAction("Index");
            }
            else
            {
                TempData["Mensaje"] = "Datos incorrectos";
                return View(user);
            }
        }//end edit

        [HttpGet]
        public async Task<IActionResult> Details(string id)
        {
            Empleado temp = new Empleado();

            httpClient.DefaultRequestHeaders.Authorization = AutorizacionToken();

            HttpResponseMessage response = await httpClient.GetAsync($"/Empleados/Consultar?ID={id}");

            if (ValidarTransaccion(response.StatusCode) == false)
            {
                return RedirectToAction("Logout", "");
            }

            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                var resultado = response.Content.ReadAsStringAsync().Result;
                temp = JsonConvert.DeserializeObject<Empleado>(resultado);
            }
            return View(temp);
        }//end details

        [HttpGet]
        public async Task<IActionResult> Delete(string id)
        {
            httpClient.DefaultRequestHeaders.Authorization = AutorizacionToken();

            var user = new Empleado();

            HttpResponseMessage mensaje = await httpClient.GetAsync($"/Empleados/Consultar?ID={id}");

            if (ValidarTransaccion(mensaje.StatusCode) == false)
            {
                return RedirectToAction("Logout", "");
            }

            if (mensaje.IsSuccessStatusCode)//Si todo está correcto
            {
                //Se realiza lectura datos en formato JSON
                var resultado = mensaje.Content.ReadAsStringAsync().Result;

                //se convierte el JSON en  un  object
                user = JsonConvert.DeserializeObject<Empleado>(resultado);
            }

            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("Delete")]
        public async Task<IActionResult> DeleteUsuario(string id)
        {
            httpClient.DefaultRequestHeaders.Authorization = AutorizacionToken();

            HttpResponseMessage response = await httpClient.DeleteAsync($"/Empleados/Eliminar?ID={id}");

            if (ValidarTransaccion(response.StatusCode) == false)
            {
                return RedirectToAction("Logout", "");
            }

            return RedirectToAction("Index");
        }


        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login([Bind] Empleado empleado)
        {
            AutorizacionResponse autorizacion = null;

            if (empleado == null)
            {
                TempData["Mensaje"] = "Usuario o contraseña incorrecta";

                return View();
            }

            HttpResponseMessage response = await httpClient.PostAsync($"Empleados/AutenticarPW?email={empleado.Email}&password={empleado.Password}", null);

            if (response.IsSuccessStatusCode)
            {
                var resultado = response.Content.ReadAsStringAsync().Result;

                autorizacion = JsonConvert.DeserializeObject<AutorizacionResponse>(resultado);
            }

            if (autorizacion != null)
            {
                HttpContext.Session.SetString("token", autorizacion.Token);

                HttpResponseMessage empleInfo = await httpClient.GetAsync($"Empleados/EmpleadoLogin?email={empleado.Email}");

                if (empleInfo.StatusCode == HttpStatusCode.OK)
                {
                    var resultadoEmple = empleInfo.Content.ReadAsStringAsync().Result;

                    Empleado empleadoBuscado = JsonConvert.DeserializeObject<Empleado>(resultadoEmple);

                    empleado.TipoUsuario = empleadoBuscado.TipoUsuario;
                }

                if (empleado.TipoUsuario > 0)
                {
                    var identity = new ClaimsIdentity(CookieAuthenticationDefaults.AuthenticationScheme);

                    identity.AddClaim(new Claim(ClaimTypes.Name, empleado.Email));
                    identity.AddClaim(new Claim("TipoUsuario", empleado.TipoUsuario.ToString()));

                    var principal = new ClaimsPrincipal(identity);

                    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);
                }

                return RedirectToAction("Index", "Home");
            }
            else
            {
                TempData["Mensaje"] = "Intente de nuevo";

                return View(empleado);
            }
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync();
            HttpContext.Session.SetString("token", "");
            return RedirectToAction("Login", "Empleados");
        }



        private AuthenticationHeaderValue AutorizacionToken()
        {
            var token = HttpContext.Session.GetString("token");

            AuthenticationHeaderValue autorizacion = null;

            if (token != null && token.Length != 0)
            {
                autorizacion = new AuthenticationHeaderValue("Bearer", token);
            }//end if

            return autorizacion;
        }//end AutorizacionToken


        private bool ValidarTransaccion(HttpStatusCode resultado)
        {
            if (resultado == HttpStatusCode.Unauthorized)
            {
                TempData["MensajeSesion"] = "Su sesion no es valida o ha expirado";
                return false;
            }
            else
            {
                TempData["MensajeSesion"] = null;
                return true;
            }

        }//end ValidarTransaccion
    }//end class
}//end namespace
