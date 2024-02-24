using AppWebBeachSA.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Net;

namespace AppWebBeachSA.Controllers
{
    public class ReservacionesController : Controller
    {
        private HotelAPI hotelAPI;

        private HttpClient client;

        /// <summary>
        /// Metodo constructor
        /// </summary>
        public ReservacionesController()
        {
            hotelAPI = new HotelAPI();

            client = hotelAPI.Initial();
        }

        /// <summary>
        /// Metodo que muestra la vista de la lista de reservaciones
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> Listado()
        {
            List<Reservacion> listado = new List<Reservacion>();

            HttpResponseMessage response = await client.GetAsync("/Reservaciones/ListaReservas");

            if (response.StatusCode == HttpStatusCode.OK)
            {
                var resultados = response.Content.ReadAsStringAsync().Result;

                listado = JsonConvert.DeserializeObject<List<Reservacion>>(resultados);
            }

            return View(listado);
        }



        /// <summary>
        /// Muestra la vista de buscar cliente
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public IActionResult ConfirmarCliente()
        {
            return View();
        }



        /// <summary>
        /// Envia la cedula y retorna los datos del objeto
        /// </summary>
        /// <param name="pCedula"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmarCliente(string pCedula)
        {
            Cliente cliente = new Cliente();

            HttpResponseMessage response = await client.GetAsync($"Clientes/Buscar?cedula={pCedula}");

            if (response.StatusCode == HttpStatusCode.OK)
            {
                var resultados = response.Content.ReadAsStringAsync().Result;

                cliente = JsonConvert.DeserializeObject<Cliente>(resultados);
            }

            return View(cliente);
        }



        /// <summary>
        /// Muestra la vista de crear reservacion
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            ViewBag.ListaPaquetes = await GetPaquetes();
            return View();
        }



        /// <summary>
        /// Envia los datos de la reservacion
        /// </summary>
        /// <param name="files"></param>
        /// <param name="pReservacion"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(List<IFormFile> files, [Bind] Reservacion pReservacion)
        {
            pReservacion.Id = await GetNumReserva();
            pReservacion.Estado = 'A';

            var agregar = client.PostAsJsonAsync<Reservacion>("/Reservaciones/AgregarReserva", pReservacion);
            await agregar;

            var resultado = agregar.Result;

            if (resultado.StatusCode == HttpStatusCode.OK)
            {
                if (pReservacion.TipoPago.Equals("Cheque"))
                {
                    return RedirectToAction("Create", "Cheques");
                }
                else
                {
                    return RedirectToAction("Index", "Home");
                }
            }
            else
            {
                TempData["Mensaje"] = "No se ha podido registrar la reservación.";
                return View(pReservacion);
            }
        }



        /// <summary>
        /// Metodo para retornar la lista de paquetes
        /// </summary>
        /// <returns></returns>
        public async Task<List<Paquete>> GetPaquetes()
        {
            List<Paquete> listado = new List<Paquete>();

            HttpResponseMessage response = await client.GetAsync("/Paquetes/Listado");

            if (response.StatusCode == HttpStatusCode.OK)
            {
                var resultados = response.Content.ReadAsStringAsync().Result;

                listado = JsonConvert.DeserializeObject<List<Paquete>>(resultados);
            }

            return listado;
        }



        /// <summary>
        /// Metodo para crear el id para conocer su valor
        /// </summary>
        /// <returns></returns>
        public async Task<int> GetNumReserva()
        {
            int ultimoId = 0;
            List<Reservacion> listado = new List<Reservacion>();

            HttpResponseMessage response = await client.GetAsync("/Reservaciones/ListaReservas");

            if (response.StatusCode == HttpStatusCode.OK)
            {
                var resultados = response.Content.ReadAsStringAsync().Result;

                listado = JsonConvert.DeserializeObject<List<Reservacion>>(resultados);
            }

            foreach (var item in listado)
            {
                ultimoId = item.Id;
            }

            return ultimoId + 1;
        }
    }
}
