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

            HttpResponseMessage response = await httpClient.GetAsync("Clientes/Listado");

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
                return RedirectToAction("Index");
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

            HttpResponseMessage response = await httpClient.GetAsync($"Clientes/Buscar?cedula={id}");

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
            var modificar = httpClient.PutAsJsonAsync<Cliente>("Clientes/Modificar", pCliente);
            await modificar;

            var resultado = modificar.Result;

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

            HttpResponseMessage response = await httpClient.GetAsync($"Clientes/Buscar?cedula={id}");

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
            HttpResponseMessage response = await httpClient.DeleteAsync($"/Clientes/EliminarCliente?vCedula={id}");

            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> Details(string id)
        {
            Cliente temp = new Cliente();

            HttpResponseMessage response = await httpClient.GetAsync($"Clientes/Buscar?cedula={id}");

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
            var response = await httpClient.PostAsJsonAsync("/Clientes/Login", cliente);

            if (response.IsSuccessStatusCode)
            {
                var resultado = await response.Content.ReadAsStringAsync();

                if (resultado == "Debe restablecer contraseña")
                {
                    TempData["EmailRestablecer"] = cliente.Email;
                    TempData["Mensaje"] = "Debe restablecer contraseña";
                    return RedirectToAction("Restablecer");
                }
                else if (resultado == "Ha iniciado sesión")
                {
                    // Obtener el token del API después de iniciar sesión
                    var tokenResponse = await httpClient.PostAsync($"/Clientes/AutenticarPW?email={cliente.Email}&password={cliente.Password}", null);

                    if (tokenResponse.IsSuccessStatusCode)
                    {
                        var tokenResult = await tokenResponse.Content.ReadAsStringAsync();
                        var autorizacion = JsonConvert.DeserializeObject<AutorizacionResponse>(tokenResult);

                        if (autorizacion != null)
                        {
                            // Almacenar el token en la sesión
                            HttpContext.Session.SetString("token", autorizacion.Token);

                            // Redireccionar al usuario a la página de inicio
                            return RedirectToAction("Index", "Home");
                        }
                    }

                    TempData["Mensaje"] = "Error al obtener el token de autenticación";
                    return View();
                }
                else
                {
                    TempData["Mensaje"] = "Error al iniciar sesión";
                    return View();
                }
            }
            else
            {
                TempData["Mensaje"] = "Error al iniciar sesión total";
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

                    if (resultado == "Restablecer contraseña: Éxito")
                    {
                        TempData["Mensaje"] = "Contraseña restablecida con éxito";
                    }
                    else
                    {
                        TempData["Mensaje"] = resultado;
                    }

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
            }

            return autorizacion;
        }

    }
}
