﻿namespace AppWebBeachSA.Models
{
    public class HotelAPI
    {

        public HttpClient Initial()
        {
            //objeto HttpClient
            var client = new HttpClient();

            //URL API
            //client.BaseAddress = new Uri("https://localhost:7094/");
            client.BaseAddress = new Uri("https://www.apihotelbeachsag1.somee.com/");

            return client;
        }

    }
}
