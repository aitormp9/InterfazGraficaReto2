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

namespace InterfazGraficaReto2
{
    public class Jugador
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public string Email { get; set; }

        public Jugador() { }
    }

    public class Partida
    {
        public string NombreJugador { get; set; }
        public DateTime Fecha { get; set; }
        public int Duracion { get; set; }
        public int Puntuacion { get; set; }
        public Partida() { }
    }
    public class Ranking
    {
        public int Puesto { get; set; }
        public string NombreJugador { get; set; }
        public int Puntuacion { get; set; }

        public Ranking() { }
    }
    public partial class MainWindow : Window
    {
        public ObservableCollection<Jugador> JugadorOC { get; set; }
        public ObservableCollection<Partida> PartidaOC { get; set; } = new ObservableCollection<Partida>();
        public ObservableCollection<Ranking> RankingOC { get; set; }

        public MainWindow()
        {
            JugadorOC = new ObservableCollection<Jugador>();
            RankingOC = new ObservableCollection<Ranking>();
            this.DataContext = this;

            InitializeComponent();
            TablasCB.SelectedIndex = 0;
            ComboBoxItem tablaSeleccionada = (ComboBoxItem)TablasCB.SelectedItem;
            String resultadoCB = tablaSeleccionada.Content.ToString();
            ElegirTabla(resultadoCB);
            LlamadaApi();
            DibujarGrafico();
        }

        private void DibujarGrafico()
        {
            double[] puntuaciones = PartidaOC.Select(p => (double)p.Puntuacion).ToArray();

            if (puntuaciones.Length == 0)
            {
                Grafico.Plot.Clear();
                Grafico.Refresh();
                return;
            }
            var hist = ScottPlot.Statistics.Histogram.WithBinCount(20, puntuaciones);
            Grafico.Plot.Clear();
            Grafico.Plot.Add.Histogram(hist).BarWidthFraction = 0.8;

            Grafico.Refresh();
        }



        private void VerInformes(object sender, RoutedEventArgs e)
        {
            Informes ventanaInformes = new Informes();
            ventanaInformes.ShowDialog();
        }

        private void TablasCB_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            ComboBoxItem tablaSeleccionada = (ComboBoxItem)TablasCB.SelectedItem;

            String resultadoCB = tablaSeleccionada.Content.ToString();

            ElegirTabla(resultadoCB);
        }

        private void ElegirTabla(String resultadoCB)
        {
            foreach (var col in Tabla.Columns)
            {
                col.Visibility = Visibility.Collapsed;

            }
            FechaDP.Visibility = Visibility.Hidden;
            LabelFecha.Visibility = Visibility.Hidden;
            switch (resultadoCB)
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
        private async void FiltradoFecha(object sender, SelectionChangedEventArgs e)
        {
            List<Partida> partidaEliminar = new List<Partida>();

            DateTime? fechaSeleccionada = FechaDP.SelectedDate;

            if (!fechaSeleccionada.HasValue) return;

            await LlamadaApi();

            if (fechaSeleccionada.HasValue)
            {
                
                foreach (var partida in PartidaOC)
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

        private async Task LlamadaApi()
        {
            PartidaOC.Clear();
            JugadorOC.Clear();
            RankingOC.Clear();
            try
            {
                var api = new Api();
                var respuestaPartidas = await api.apiGet("partidas");
                var respuestaJugadores = await api.apiGet("jugadores");
                var respuestaRanking = await api.apiGet("ranking");

                var listaPartidas = JsonConvert.DeserializeObject<JArray>(respuestaPartidas);
                var listaJugadores = JsonConvert.DeserializeObject<JArray>(respuestaJugadores);
                var listaRanking = JsonConvert.DeserializeObject<JArray>(respuestaRanking);

                foreach (var item in listaPartidas)
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

            }
            catch (Exception e)
            {
                MessageBox.Show("Excepcion: " + e.Message);
            }
        }
    }  
}
