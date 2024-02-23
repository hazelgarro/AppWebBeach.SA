using AppWebBeachSA.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net;
using System.Net.Http;
using System.Numerics;
using static AppWebBeachSA.Controllers.ClientesController;

namespace AppWebBeachSA.Controllers
{
    public class ClientesController : Controller
    {

        private HotelAPI apiHotel;
        private HttpClient httpClient;
        public static Cliente DatosPersona = new Cliente();

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

    }
}
