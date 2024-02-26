using AppWebBeachSA.Models;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Net;

namespace AppWebBeachSA.Controllers
{
    public class PaquetesController : Controller
    {
        private HotelAPI hotelAPI;
        private HttpClient client;

        public PaquetesController()
        {
            hotelAPI = new HotelAPI();
            client = hotelAPI.Initial();
        }


        //Lista
        public async Task<IActionResult> Index(string buscar)
        {
            List<Paquete> lista = new List<Paquete>();

            client.DefaultRequestHeaders.Authorization = AutorizacionToken();

            HttpResponseMessage response = await client.GetAsync("Paquetes/Listado");

            if (ValidarTransaccion(response.StatusCode) == false)
            {
                return RedirectToAction("Logout", "Clientes");
            }

            if (response.IsSuccessStatusCode)
            {
                var resultado = await response.Content.ReadAsStringAsync();

                lista = JsonConvert.DeserializeObject<List<Paquete>>(resultado);
            }

            return View(lista);
        }

        //Agregar
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind] Paquete paquete)
        {
            paquete.ID = 0;
            paquete.FechaRegistro = DateTime.Now;

            client.DefaultRequestHeaders.Authorization = AutorizacionToken();

            var agregar = client.PostAsJsonAsync<Paquete>("Paquetes/Agregar", paquete);
            await agregar;

            var resultado = agregar.Result;

            if (ValidarTransaccion(resultado.StatusCode) == false)
            {
                return RedirectToAction("Logout", "Clientes");
            }

            if (resultado.IsSuccessStatusCode)
            {
                return RedirectToAction("Index", "Paquetes");
            }
            else
            {
                TempData["Mensaje"] = "No se logró registrar el paquete";

                return View(paquete);
            }
        }


        //Editar
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var paquete = new Paquete();

            client.DefaultRequestHeaders.Authorization = AutorizacionToken();

            HttpResponseMessage response = await client.GetAsync($"Paquetes/Consultar?ID={id}");

            if (ValidarTransaccion(response.StatusCode) == false)
            {
                return RedirectToAction("Logout", "Clientes");
            }

            if (response.IsSuccessStatusCode)
            {
                var resultado = response.Content.ReadAsStringAsync().Result;

                paquete = JsonConvert.DeserializeObject<Paquete>(resultado);
            }

            return View(paquete);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit([Bind] Paquete paquete)
        {

           client.DefaultRequestHeaders.Authorization = AutorizacionToken();

            var modificar = client.PutAsJsonAsync<Paquete>("/Paquetes/Modificar", paquete);
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

                return View(paquete);
            }
        }

        //Eliminar
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var paquete = new Paquete();

            client.DefaultRequestHeaders.Authorization = AutorizacionToken();

            HttpResponseMessage mensaje = await client.GetAsync($"/Paquetes/Consultar?ID={id}");

            if (ValidarTransaccion(mensaje.StatusCode) == false)
            {
                return RedirectToAction("Logout", "Clientes");
            }

            if (mensaje.IsSuccessStatusCode)
            {
                var resultado = mensaje.Content.ReadAsStringAsync().Result;

                //conversion json a obj
                paquete = JsonConvert.DeserializeObject<Paquete>(resultado);
            }

            return View(paquete);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("Delete")]
        public async Task<IActionResult> DeletePaquete(int id)
        {
            client.DefaultRequestHeaders.Authorization = AutorizacionToken();

            HttpResponseMessage response = await client.DeleteAsync($"/Paquetes/Eliminar?ID={id}");

            if (ValidarTransaccion(response.StatusCode) == false)
            {
                return RedirectToAction("Logout", "Clientes");
            }

            return RedirectToAction("Index");
        }

        //Detalles
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var paquete = new Paquete();

            client.DefaultRequestHeaders.Authorization = AutorizacionToken();

            HttpResponseMessage respuesta = await client.GetAsync($"/Paquetes/Consultar?ID={id}");

            if (ValidarTransaccion(respuesta.StatusCode) == false)
            {
                return RedirectToAction("Logout", "Clientes");
            }

            if (respuesta.IsSuccessStatusCode)
            {
                var resultado = respuesta.Content.ReadAsStringAsync().Result;

                paquete = JsonConvert.DeserializeObject<Paquete>(resultado);
            }

            return View(paquete);
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


        private bool ValidarTransaccion(HttpStatusCode resultado)
        {
            //Se vencio el token por ende debe hacer cerrar sesion
            if (resultado == HttpStatusCode.Unauthorized)
            {
                TempData["MensajeSesion"] = "Su sesion ha expirado o no es válida";
                return false;
            }
            else
            {
                TempData["MensajeSesion"] = null;
                return true;
            }
        }
    }
}
