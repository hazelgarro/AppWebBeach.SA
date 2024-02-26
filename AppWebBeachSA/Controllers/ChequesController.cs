﻿using AppWebBeachSA.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Net;
using System.Text;
using System.Net.Http;
using System.Text.Json;
using JsonSerializer = System.Text.Json.JsonSerializer;
using System.Net.Http.Json;

namespace AppWebBeachSA.Controllers
{
    public class ChequesController : Controller
    {
        private HotelAPI hotelAPI;

        private HttpClient client;

        /// <summary>
        /// Metodo constructor
        /// </summary>
        public ChequesController()
        {
            hotelAPI = new HotelAPI();

            client = hotelAPI.Initial();
        }



        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind] Cheque pCheque)
        {
            pCheque.IdCheque = await GetNumCheque();
            pCheque.IdReservacion = await GetNumReserva();

            bool enviarCorreo = TempData["EnviarCorreo"] as string == "true";

            ChequeEnvioEmail chequeEnvioEmail = new ChequeEnvioEmail();
            chequeEnvioEmail.IdCheque = pCheque.IdCheque;
            chequeEnvioEmail.NumeroCheque = pCheque.NumeroCheque;
            chequeEnvioEmail.NombreBanco = pCheque.NombreBanco;
            chequeEnvioEmail.IdReservacion = pCheque.IdReservacion;
            chequeEnvioEmail.EnvioEmail = enviarCorreo;

            var response = await client.PostAsJsonAsync<ChequeEnvioEmail>("/Reservaciones/AgregarCheque", chequeEnvioEmail);

            if (response.StatusCode == HttpStatusCode.OK)
            {
                return RedirectToAction("Index", "Home");
            }
            else
            {
                TempData["Mensaje"] = "No se ha podido registrar el cheque.";
                return View(pCheque);
            }
        }



        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var cheque = new Cheque();

            HttpResponseMessage response = await client.GetAsync($"Cheques/Consultar?Id={id}");

            if (response.StatusCode == HttpStatusCode.OK)
            {
                var resultado = response.Content.ReadAsStringAsync().Result;

                cheque = JsonConvert.DeserializeObject<Cheque>(resultado);
            }

            return View(cheque);
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit([Bind] Cheque pCheque)
        {
            var modificar = client.PutAsJsonAsync<Cheque>("/Cheques/Modificar", pCheque);
            await modificar;

            var resultado = modificar.Result;

            if (resultado.StatusCode == HttpStatusCode.OK)
            {
                return RedirectToAction("Index", "Home");
            }
            else
            {
                TempData["Mensaje"] = "No se ha podido modificar el cheque.";
                return View(pCheque);
            }
        }



        public async Task<int> GetNumCheque()
        {
            int ultimoId = 0;
            List<Cheque> listado = new List<Cheque>();

            HttpResponseMessage response = await client.GetAsync("/Cheques/Listado");

            if (response.StatusCode == HttpStatusCode.OK)
            {
                var resultados = response.Content.ReadAsStringAsync().Result;

                listado = JsonConvert.DeserializeObject<List<Cheque>>(resultados);
            }

            foreach (var item in listado)
            {
                ultimoId = item.IdCheque;
            }

            return ultimoId + 1;
        }



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

            return ultimoId;
        }
    }
}
