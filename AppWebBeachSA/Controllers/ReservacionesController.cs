using AppWebBeachSA.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Net;

namespace AppWebBeachSA.Controllers
{
    public class ReservacionesController : Controller
    {
        private HotelAPI hotelAPI;

        private HttpClient client;

        public static TipoCambio tipoCambio = null;

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

            TempData["tipoCambio"] = null;

            extraerTipoCambio();

            List<Reservacion> listado = new List<Reservacion>();

            HttpResponseMessage response = await client.GetAsync("/Reservaciones/ListaReservas");

            if (response.StatusCode == HttpStatusCode.OK)
            {
                var resultados = response.Content.ReadAsStringAsync().Result;

                listado = JsonConvert.DeserializeObject<List<Reservacion>>(resultados);
            }

            ReservacionPaqueteLista listas = new ReservacionPaqueteLista();

            listas.ListaReservaciones = listado;

            listas.ListaPaquetes = await GetPaquetes();

            return View(listas);
        }

        public async Task<IActionResult> ListadoCliente()
        {
            var cedulaClaim = User.Claims.FirstOrDefault(c => c.Type == "Cedula");
            var cedula = cedulaClaim.Value;

            var reservaciones = await ObtenerTodasLasReservaciones();
            var reservacionesCliente = reservaciones.Where(r => r.CedulaCliente == cedula).ToList();

            ReservacionPaqueteLista listas = new ReservacionPaqueteLista();
            listas.ListaReservaciones = reservacionesCliente;
            listas.ListaPaquetes = await GetPaquetes();

            return View(listas);

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
        public async Task<IActionResult> Create([Bind] Reservacion pReservacion)
        {
            pReservacion.Id = await GetNumReserva();
            pReservacion.Estado = 'A';

            HttpResponseMessage resultado = await client.PostAsJsonAsync<Reservacion>("/Reservaciones/AgregarReserva", pReservacion);

            if (resultado.StatusCode == HttpStatusCode.OK)
            {
                if (pReservacion.TipoPago.Equals("Cheque"))
                {
                    TempData["NumeroReserva"] = pReservacion.Id;
                    TempData["EnviarCorreo"] = "true";
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
        /// Metodo para enviar los datos de la reservacion que se va a editar
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var reserva = new Reservacion();

            HttpResponseMessage response = await client.GetAsync($"Reservaciones/BuscarReserva?id={id}");

            if (response.StatusCode == HttpStatusCode.OK)
            {
                var resultado = response.Content.ReadAsStringAsync().Result;

                reserva = JsonConvert.DeserializeObject<Reservacion>(resultado);
            }

            ViewBag.ListaPaquetes = await GetPaquetes();
            return View(reserva);
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit([Bind] Reservacion pReserva)
        {
            pReserva.Estado = 'A';
            var modificar = client.PutAsJsonAsync<Reservacion>("/Reservaciones/Editar", pReserva);
            await modificar;

            var resultado = modificar.Result;

            if (resultado.StatusCode == HttpStatusCode.OK)
            {
                if (pReserva.TipoPago.Equals("Cheque"))
                {
                    var cheque = new Cheque();

                    HttpResponseMessage response = await client.GetAsync($"Cheques/Consultar?Id={pReserva.Id}");

                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        var chequeResultado = response.Content.ReadAsStringAsync().Result;

                        cheque = JsonConvert.DeserializeObject<Cheque>(chequeResultado);
                    }

                    if (cheque.IdCheque == 0)
                    {
                        TempData["NumeroReserva"] = pReserva.Id;
                        TempData["EnviarCorreo"] = "false";
                        return RedirectToAction("Create", "Cheques");
                    }
                    else
                    {
                        return RedirectToAction("Edit", "Cheques", new { id = pReserva.Id });
                    }

                }
                else
                {
                    return RedirectToAction("Index", "Home");
                }
            }
            else
            {
                TempData["Mensaje"] = "No se ha podido modificar la reservación.";
                return View(pReserva);
            }
        }



        [HttpGet]
        public async Task<IActionResult> Delete(int? id)
        {
            var reserva = new Reservacion();

            HttpResponseMessage mensaje = await client.GetAsync($"Reservaciones/BuscarReserva?id={id}");

            if (mensaje.StatusCode == HttpStatusCode.OK)
            {
                var resultado = mensaje.Content.ReadAsStringAsync().Result;

                reserva = JsonConvert.DeserializeObject<Reservacion>(resultado);
            }

            return View(reserva);
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var reserva = new Reservacion();

            HttpResponseMessage mensaje = await client.GetAsync($"Reservaciones/BuscarReserva?id={id}");

            if (mensaje.StatusCode == HttpStatusCode.OK)
            {
                var resultado = mensaje.Content.ReadAsStringAsync().Result;

                reserva = JsonConvert.DeserializeObject<Reservacion>(resultado);
            }

            if (reserva.TipoPago.Equals("Cheque"))
            {
                DeleteCheques(reserva.Id);
            }

            await client.DeleteAsync($"/Reservaciones/Eliminar?id={id}");

            return RedirectToAction("Index", "Home");
        }



        public async void DeleteCheques(int id)
        {
            HttpResponseMessage response = await client.DeleteAsync($"/Cheques/Eliminar?Id={id}");
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

        private async void extraerTipoCambio()
        {

            HttpResponseMessage response = client.GetAsync("https://apis.gometa.org/tdc/tdc.json").Result;

            if (response.IsSuccessStatusCode)
            {
                var result = response.Content.ReadAsStringAsync().Result;
                tipoCambio = JsonConvert.DeserializeObject<TipoCambio>(result);
                decimal precio = tipoCambio.venta;
                TempData["tipoCambio"] = precio;
                //TempData.Keep("tipoCambio");
            }


        }

        public async Task<List<Reservacion>> ObtenerTodasLasReservaciones()
        {
            TempData["tipoCambio"] = null;

            extraerTipoCambio();

            List<Reservacion> listado = new List<Reservacion>();

            HttpResponseMessage response = await client.GetAsync("/Reservaciones/ListaReservas");

            if (response.StatusCode == HttpStatusCode.OK)
            {
                var resultados = response.Content.ReadAsStringAsync().Result;

                listado = JsonConvert.DeserializeObject<List<Reservacion>>(resultados);
            }

            return listado;
        }

    }
}
