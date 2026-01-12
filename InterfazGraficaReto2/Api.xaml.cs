using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace InterfazGraficaReto2
{
    /// <summary>
    /// Lógica de interacción para Api.xaml
    /// </summary>
    public partial class Api : Page     
    {
        private string url = "http://localhost:8080/api/v1/";       // llamada a la api mediante url
        private readonly HttpClient _httpclient;

        public Api()
        {
            InitializeComponent();
            _httpclient = new HttpClient();
            _httpclient.BaseAddress = new Uri(url);
        }
        public async Task<string> apiGet(string endpoint)   // función para recibir la información de la api
        {
            HttpResponseMessage resultado = await this._httpclient.GetAsync(endpoint);
            string respuesta = await resultado.Content.ReadAsStringAsync();
            if (!resultado.IsSuccessStatusCode)
            {
                throw new HttpRequestException($"Get{endpoint} failed: {resultado.StatusCode}, {respuesta}");
            }
            return respuesta;
        }
        
    }
}
