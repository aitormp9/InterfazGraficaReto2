using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Text.Json.Serialization;
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
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ScottPlot;
using ScottPlot.TickGenerators;

namespace InterfazGraficaReto2
{
    public class Jugador // creación de la clase Jugador
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public string Email { get; set; }

        public Jugador() { }
    }

    public class Partida // creación de la clase Partida
    {
        public string NombreJugador { get; set; }
        public DateTime Fecha { get; set; }
        public int Duracion { get; set; }
        public int Puntuacion { get; set; }
        public Partida() { }
    }
    public class Ranking    // creación de la clase Ranking
    {
        public int Puesto { get; set; }
        public string NombreJugador { get; set; }
        public int Puntuacion { get; set; }

        public Ranking() { }
    }
    public partial class MainWindow : Window
    {                                           // creaciones de los ObservableCollection de cada clase
        public ObservableCollection<Jugador> JugadorOC { get; set; }
        public ObservableCollection<Partida> PartidaOC { get; set; }
        public ObservableCollection<Ranking> RankingOC { get; set; }

        public MainWindow()
        {                   // llamadas a los ObservableCollection
            JugadorOC = new ObservableCollection<Jugador>();
            PartidaOC = new ObservableCollection<Partida>();
            RankingOC = new ObservableCollection<Ranking>();

            InitializeComponent();
            // al iniciar el proyecto se abre la tabla de Partidas
            TablasCB.SelectedIndex = 1; 
            ComboBoxItem tablaSeleccionada = (ComboBoxItem)TablasCB.SelectedItem;
            String resultadoCB = tablaSeleccionada.Content.ToString();
            ElegirTabla(resultadoCB);
            // llamadas a las funciones de Api y Grafico para que se ejecuten al iniciar la aplicación
            LlamadaApi();
            DibujarGrafico();
        }

        private void DibujarGrafico()
        {
            double[] duracion = PartidaOC.Select(d => (double)d.Duracion).ToArray();


            if (duracion.Length == 0)
            {
                Grafico.Plot.Clear();
                Grafico.Refresh();
                return;
            }
            var hist = ScottPlot.Statistics.Histogram.WithBinCount(10, duracion);
            double binSize = hist.FirstBinSize;
            double halfBin = binSize / 2;
            double[] yPuntuacion = new double[hist.Bins.Length];
            for(int i= 0; i < hist.Bins.Length; i++)
            {
                double min = hist.Bins[i] - halfBin;
                double max = hist.Bins[i] + halfBin;
                var puntuaciones = PartidaOC.Where(d => d.Duracion >= min && d.Duracion <= max).Select(p=>p.Puntuacion);
                yPuntuacion[i] = puntuaciones.Any() ? puntuaciones.Average() : 0; 
            }
            var barras = Grafico.Plot.Add.Bars(hist.Bins, yPuntuacion);
            foreach(var bar in barras.Bars)
            {
                bar.Size = hist.FirstBinSize * 0.8;
            }
            Grafico.Plot.XLabel("duracion");
            Grafico.Plot.YLabel("puntuacion");
            Grafico.Plot.Axes.Margins(bottom: 0);
            
            Grafico.Refresh();
        }

        private void VerInformes(object sender, RoutedEventArgs e)  // al hacer click en el botón de ver informes, se redirige a la ventana Informes
        {
            Informes ventanaInformes = new Informes();
            ventanaInformes.ShowDialog();
        }

        private void TablasCB_SelectionChanged(object sender, SelectionChangedEventArgs e)  // función para el correcto funcionamiento del ComboBox
        {

            ComboBoxItem tablaSeleccionada = (ComboBoxItem)TablasCB.SelectedItem;

            String resultadoCB = tablaSeleccionada.Content.ToString();

            ElegirTabla(resultadoCB);
        }

        private void ElegirTabla(String resultadoCB)        // función para enseñar la tabla en función de lo elegido en el ComboBox
        {
            foreach (var col in Tabla.Columns)      //  escondiendo todas las columnas de las tablas
            {
                col.Visibility = Visibility.Collapsed;  

            }
            FechaDP.Visibility = Visibility.Hidden;
            LabelFecha.Visibility = Visibility.Hidden;
            switch (resultadoCB)                                        // dependiendo de la tabla elegida, se muestran unas columnas u otras
            {
                case "Jugadores":

                    Tabla.ItemsSource = JugadorOC;
                    Tabla.Columns[4].Visibility = Visibility.Visible;
                    Tabla.Columns[5].Visibility = Visibility.Visible;
                    Tabla.Columns[6].Visibility = Visibility.Visible;
                    Tabla.Items.Refresh();
                    Tabla.UpdateLayout();
                    break;
                case "Partidas":
                    FechaDP.Visibility = Visibility.Visible;
                    LabelFecha.Visibility = Visibility.Visible;
                    Tabla.ItemsSource = PartidaOC;
                    Tabla.Columns[0].Visibility = Visibility.Visible;
                    Tabla.Columns[1].Visibility = Visibility.Visible;
                    Tabla.Columns[2].Visibility = Visibility.Visible;
                    Tabla.Columns[3].Visibility = Visibility.Visible;
                    Tabla.Items.Refresh();
                    Tabla.UpdateLayout();
                    break;
                case "Ranking":
                    Tabla.ItemsSource = RankingOC;

                    Tabla.Columns[7].Visibility = Visibility.Visible;
                    Tabla.Columns[8].Visibility = Visibility.Visible;
                    Tabla.Columns[9].Visibility = Visibility.Visible;
                    Tabla.Items.Refresh();
                    Tabla.UpdateLayout();
                    break;
            }
        }
        private async void FiltradoFecha(object sender, SelectionChangedEventArgs e)    // función para filtrar la fecha seleccionada
        {
            List<Partida> partidaEliminar = new List<Partida>();

            DateTime? fechaSeleccionada = FechaDP.SelectedDate;

            if (!fechaSeleccionada.HasValue) return;

            await LlamadaApi();

            if (fechaSeleccionada.HasValue)         
            {
                
                foreach (var partida in PartidaOC)      // si la fecha seleccionada coincide con las fechas de las partidas, se muestran
                {
                    if (fechaSeleccionada.Value.Date != partida.Fecha.Date) {   
                        partidaEliminar.Add(partida);
                    }
                }
                foreach(var partida in partidaEliminar)                
                {
                    PartidaOC.Remove(partida);
                    Tabla.Items.Refresh();
                }
            }
            
        }

        private async Task LlamadaApi()     // función para hacer las llamadas a las apis
        {
            PartidaOC.Clear();          // limpieza de las tablas 
            JugadorOC.Clear();
            RankingOC.Clear();
            try
            {
                var api = new Api();        // llamadas a las apis mediante su nombre
                var respuestaPartidas = await api.apiGet("partidas");       
                var respuestaJugadores = await api.apiGet("jugadores");
                var respuestaRanking = await api.apiGet("ranking");

                var listaPartidas = JsonConvert.DeserializeObject<JArray>(respuestaPartidas); // pasamos las respuestas de las apis a formato json
                var listaJugadores = JsonConvert.DeserializeObject<JArray>(respuestaJugadores);
                var listaRanking = JsonConvert.DeserializeObject<JArray>(respuestaRanking);

                foreach (var item in listaPartidas)     // paso de variables de las apis a la lista
                {
                    string NombreJugador = item["nombre"].ToString();   
                    string Fecha = item["fecha"].ToString();
                    string Duracion = item["duracion"].ToString();
                    string Puntuacion = item["score"].ToString();

                    
                    DateTime fecha = DateTime.Parse(Fecha);
                    int duracion = int.Parse(Duracion);
                    int puntuacion = int.Parse(Puntuacion);

                    PartidaOC.Add(new Partida { NombreJugador = NombreJugador, Fecha = fecha, Duracion = duracion, Puntuacion = puntuacion });
                }

                foreach (var item in listaJugadores)
                {
                    int Id = int.Parse(item["id"].ToString());
                    string Nombre = item["nombre"].ToString();
                    string Email = item["email"].ToString();


                    JugadorOC.Add(new Jugador { Id = Id, Nombre = Nombre, Email = Email });
                }

                foreach (var item in listaRanking)
                {
                    int Puesto = int.Parse(item["puesto"].ToString());
                    string NombreJugador = item["nombre"].ToString();
                    int Puntuacion = int.Parse(item["score"].ToString());


                    RankingOC.Add(new Ranking { Puesto = Puesto, NombreJugador = NombreJugador, Puntuacion = Puntuacion });
                }
                DibujarGrafico();

            }
            catch (Exception e) // control de excepciones
            {
                MessageBox.Show("Excepcion: " + e.Message);
            }
        }
    }  
}
