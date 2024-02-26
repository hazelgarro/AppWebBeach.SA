using AppWebBeachSA.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Numerics;
using System.Security.Claims;
using static AppWebBeachSA.Controllers.ClientesController;

namespace AppWebBeachSA.Controllers
{
    public class ClientesController : Controller
    {

        private HotelAPI apiHotel;
        private HttpClient httpClient;
        public static Cliente DatosPersona = new Cliente();
        private static string EmailRestablecer = "";


        public ClientesController()
        {
            apiHotel = new HotelAPI();
            httpClient = apiHotel.Initial();
        }

        public async Task<IActionResult> Index()
        {
            List<Cliente> listado = new List<Cliente>();

            httpClient.DefaultRequestHeaders.Authorization = AutorizacionToken();

            HttpResponseMessage response = await httpClient.GetAsync("Clientes/Listado");

            if (ValidarTransaccion(response.StatusCode) == false)
            {
                return RedirectToAction("Logout", "Clientes");
            }

            if (response.StatusCode == HttpStatusCode.OK)
            {
                var resultados = response.Content.ReadAsStringAsync().Result;

                listado = JsonConvert.DeserializeObject<List<Cliente>>(resultados);
            }

            return View(listado);
        }


        [HttpGet]
        public async Task<IActionResult> Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind] Cliente pCliente)
        {
            pCliente.TipoUsuario = 3;
            pCliente.Estado = 'A';
            pCliente.Restablecer = 0;
            pCliente.Password = "00000";

            var agregar = httpClient.PostAsJsonAsync<Cliente>("Clientes/CrearCuenta", pCliente);

            await agregar;

            var resultado = agregar.Result;

            if (resultado.IsSuccessStatusCode)
            {
                return RedirectToAction("Index", "Home");
            }
            else
            {
                TempData["Mensaje"] = "No se logró registrar el cliente";
                return View(pCliente);
            }
        }


        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {
            var usuario = new Cliente();

            httpClient.DefaultRequestHeaders.Authorization = AutorizacionToken();

            HttpResponseMessage response = await httpClient.GetAsync($"Clientes/Buscar?cedula={id}");

            if (ValidarTransaccion(response.StatusCode) == false)
            {
                return RedirectToAction("Logout", "Clientes");
            }

            if (response.IsSuccessStatusCode)
            {
                var resultado = response.Content.ReadAsStringAsync().Result;

                if (!string.IsNullOrEmpty(resultado))
                {
                    usuario = JsonConvert.DeserializeObject<Cliente>(resultado);
                }
            }

            return View(usuario);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit([Bind] Cliente pCliente)
        {
            httpClient.DefaultRequestHeaders.Authorization = AutorizacionToken();

            var modificar = httpClient.PutAsJsonAsync<Cliente>("Clientes/Modificar", pCliente);
            await modificar;

            var resultado = modificar.Result;

            if (ValidarTransaccion(resultado.StatusCode) == false)
            {
                return RedirectToAction("Logout", "Clientes");
            }

            if (resultado.IsSuccessStatusCode)
            {
                return RedirectToAction("Index");
            }
            else
            {
                TempData["Mensaje"] = "Datos incorrectos";
                return View(pCliente);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Delete(string id)
        {
            var cliente = new Cliente();

            httpClient.DefaultRequestHeaders.Authorization = AutorizacionToken();

            HttpResponseMessage response = await httpClient.GetAsync($"Clientes/Buscar?cedula={id}");

            if (ValidarTransaccion(response.StatusCode) == false)
            {
                return RedirectToAction("Logout", "Clientes");
            }

            if (response.IsSuccessStatusCode)
            {
                var resultado = response.Content.ReadAsStringAsync().Result;

                cliente = JsonConvert.DeserializeObject<Cliente>(resultado);
            }
            return View(cliente);
        }

        [HttpPost]
        [ActionName("Delete")]
        public async Task<IActionResult> DeleteUser(string id)
        {
            httpClient.DefaultRequestHeaders.Authorization = AutorizacionToken();

            HttpResponseMessage response = await httpClient.DeleteAsync($"/Clientes/EliminarCliente?vCedula={id}");

            if (ValidarTransaccion(response.StatusCode) == false)
            {
                return RedirectToAction("Logout", "Clientes");
            }

            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> Details(string id)
        {
            Cliente temp = new Cliente();

            httpClient.DefaultRequestHeaders.Authorization = AutorizacionToken();

            HttpResponseMessage response = await httpClient.GetAsync($"Clientes/Buscar?cedula={id}");

            if (ValidarTransaccion(response.StatusCode) == false)
            {
                return RedirectToAction("Logout", "Clientes");
            }

            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                var resultado = response.Content.ReadAsStringAsync().Result;
                temp = JsonConvert.DeserializeObject<Cliente>(resultado);
            }
            return View(temp);
        }

        public async Task<IActionResult> ObtenerInformacionPersona(string cedula)
        {
            Cliente datosPersona = new Cliente();

            var response = await httpClient.GetAsync("https://apis.gometa.org/cedulas/" + cedula);

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                dynamic data = JObject.Parse(content);

                Console.WriteLine(data.ToString());

                var persona = new Cliente
                {
                    NombreCompleto = data.nombre,
                    TipoCedula = data.results[0].guess_type,
                    Cedula = data.cedula,
                    Telefono = "",
                    Direccion = "",
                    Email = "",
                    Password = "00000",
                    TipoUsuario = 3,
                    Restablecer = 0,
                    FechaRegistro = DateTime.Now,
                    Estado = 'A'
                };

                datosPersona = persona;

            }

            TempData["NombreCompleto"] = datosPersona.NombreCompleto;
            TempData["Cedula"] = datosPersona.Cedula;
            TempData["TipoCedula"] = datosPersona.TipoCedula;
            TempData["Password"] = datosPersona.Password;
            return RedirectToAction("Create", "Clientes");
        }

        [HttpGet]
        public IActionResult ObtenerInformacionPersona()
        {
            return View();
        }


        //Métodos autenticación


        //Login
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(Cliente cliente)
        {

            AutorizacionResponse autorizacion = null;

            if (cliente == null)
            {
                TempData["Mensaje"] = "Usuario o contraseña incorrecta";
                return View();
            }

            HttpResponseMessage response = await httpClient.PostAsync($"Clientes/AutenticarPW?email={cliente.Email}&password={cliente.Password}", null);

            if (response.IsSuccessStatusCode)
            {
                var resultado = response.Content.ReadAsStringAsync().Result;
                autorizacion = JsonConvert.DeserializeObject<AutorizacionResponse>(resultado);
            }

            if (autorizacion != null)
            {
                HttpContext.Session.SetString("token", autorizacion.Token);

                var authProperties = new AuthenticationProperties
                {
                    IsPersistent = true,
                };

                httpClient.DefaultRequestHeaders.Authorization = AutorizacionToken();

                HttpResponseMessage clientInfo = await httpClient.GetAsync($"Clientes/ClienteLogin?email={cliente.Email}");

                if (clientInfo.StatusCode == HttpStatusCode.OK)
                {
                    var resultadoClient = clientInfo.Content.ReadAsStringAsync().Result;

                    Cliente clienteBuscado = JsonConvert.DeserializeObject<Cliente>(resultadoClient);

                    cliente.TipoUsuario = clienteBuscado.TipoUsuario;
                    cliente.Cedula = clienteBuscado.Cedula;
                    cliente.Restablecer = clienteBuscado.Restablecer;
                }

                if (cliente.Restablecer == 0)
                {
                    TempData["EmailRestablecer"] = cliente.Email;
                    TempData["Mensaje"] = "Debe restablecer contraseña";
                    return RedirectToAction("Restablecer");
                }
                else if (cliente.Restablecer == 1)
                {
                    var identity = new ClaimsIdentity(CookieAuthenticationDefaults.AuthenticationScheme);

                    identity.AddClaim(new Claim(ClaimTypes.Name, cliente.Email));
                    identity.AddClaim(new Claim("TipoUsuario", cliente.TipoUsuario.ToString()));
                    identity.AddClaim(new Claim("Cedula", cliente.Cedula.ToString()));

                    var principal = new ClaimsPrincipal(identity);
                    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    TempData["Mensaje"] = "Error al iniciar sesión";
                    return View();
                }
            }
            else
            {
                TempData["Mensaje"] = "Usuario o contraseña incorrecta";
                return View();
            }
        }

        private async Task<bool> VerificarRestablecer(Cliente temp)
        {
            bool verificado = false;

            HttpResponseMessage response = await httpClient.GetAsync($"Clientes/BuscarCorreo?email={temp.Email}");

            if (response.IsSuccessStatusCode)
            {
                var resultado = response.Content.ReadAsStringAsync().Result;
                var cliente = JsonConvert.DeserializeObject<Cliente>(resultado);

                if (cliente.Restablecer == 0)
                {
                    verificado = true;
                }
            }

            return verificado;
        }

        [HttpGet]
        public IActionResult Restablecer()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Restablecer(SeguridadRestablecer pRestablecer)
        {

            string emailRestablecer = TempData["EmailRestablecer"]?.ToString();

            pRestablecer.Email = emailRestablecer;

            if (!string.IsNullOrEmpty(emailRestablecer) && pRestablecer != null)
            {
                var response = await httpClient.PostAsJsonAsync($"/Clientes/Restablecer?email={emailRestablecer}", pRestablecer);


                if (response.IsSuccessStatusCode)
                {
                    var resultado = await response.Content.ReadAsStringAsync();

                    TempData["Mensaje"] = resultado;

                    return RedirectToAction("Login", "Clientes");
                }
                else
                {
                    TempData["Mensaje"] = "Error al restablecer la contraseña";
                }
            }
            else
            {
                TempData["Mensaje"] = "Datos incorrectos";
            }

            return View();
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync();
            HttpContext.Session.SetString("token", "");
            return RedirectToAction("Login", "Clientes");
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

    }
}
