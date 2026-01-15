using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
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
using Microsoft.ReportingServices.ReportProcessing.ReportObjectModel;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Npgsql;
using ScottPlot;
using ScottPlot.Colormaps;
using ScottPlot.TickGenerators;
using ScottPlot.Statistics;
using ScottPlot.AxisLimitManagers;

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
        
        public string conn = "";        // inicialización del fichero BD
        public BaseDatos bd;
       
        public MainWindow()
        {                   // llamadas a los ObservableCollection
            JugadorOC = new ObservableCollection<Jugador>();
            PartidaOC = new ObservableCollection<Partida>();
            RankingOC = new ObservableCollection<Ranking>();

            bd = new BaseDatos();

            InitializeComponent();
            // al iniciar el proyecto se abre la tabla de Partidas
            TablasCB.SelectedIndex = 1; 
            ComboBoxItem tablaSeleccionada = (ComboBoxItem)TablasCB.SelectedItem;
            String resultadoCB = tablaSeleccionada.Content.ToString();
            ElegirTabla(resultadoCB);

            // llamadas a las funciones de la BD y Grafico para que se ejecuten al iniciar la aplicación
            LlamadaJugadores();
            LlamadaPartidas();
            LlamadaRanking();
            DibujarGrafico();
        }
        public void LlamadaJugadores()      // función para la muestra de datos de la tabla Jugadores
        {
            JugadorOC.Clear();
            using (NpgsqlConnection con = new NpgsqlConnection(bd.conexionBD()))
            {
                con.Open();
                string query = "select * from jugadores";       // consulta a ejecutar
                var llamada = new NpgsqlCommand(query, con);

                using (var reader = llamada.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        JugadorOC.Add(new Jugador       // introducción de datos al Observable Collection de Jugador
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("id")),
                            Nombre = reader.GetString(reader.GetOrdinal("nombre")),
                            Email = reader.GetString(reader.GetOrdinal("email"))
                        });
                    }
                }
            }
        }
        public async Task LlamadaPartidas()         // función para la muestra de datos de la tabla Partidas
        {
            PartidaOC.Clear();
            using (var con = new NpgsqlConnection(bd.conexionBD())) // conexión con la base de datos
            {
                con.Open();
                string query = "select * from jugadores as j inner join jugadores_partidas as jp on j.id = jp.jugador_id " +    // consulta a ejecutar
                    "inner join partidas as p on jp.partida_id = p.id;";
                var llamada = new NpgsqlCommand(query, con);

                using (var reader = llamada.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        PartidaOC.Add(new Partida       // introducción de datos al Observable Collection de Partida
                        {
                            NombreJugador = reader.GetString(reader.GetOrdinal("nombre")),
                            Fecha = reader.GetDateTime(reader.GetOrdinal("fecha")),
                            Duracion = reader.GetInt32(reader.GetOrdinal("duracion")),
                            Puntuacion = reader.GetInt32(reader.GetOrdinal("score"))
                        });
                    }
                }
            }
        }
        public void LlamadaRanking()        // función para la muestra de datos de la tabla Ranking
        {
            RankingOC.Clear();
            using (var con = new NpgsqlConnection(bd.conexionBD()))
            {
                con.Open();
                string query = "select j.id, j.nombre, sum(jp.score) as total_score from jugadores_partidas as jp " +   // consulta a ejecutar
                    "inner join jugadores as j on j.id = jp.jugador_id group by j.id, j.nombre order by total_score desc;";
                var llamada = new NpgsqlCommand(query, con);
                int puesto = 0;

                using (var reader = llamada.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        puesto += 1;
                        RankingOC.Add(new Ranking       // introducción de datos al Observable Collection de Ranking
                        {
                            Puesto = puesto,
                            NombreJugador = reader.GetString(reader.GetOrdinal("nombre")),
                            Puntuacion = reader.GetInt32(reader.GetOrdinal("total_score"))
                        });
                    }
                }
            }
        }

        private void DibujarGrafico()
        {
            if (!PartidaOC.Any())
            {
                Grafico.Plot.Clear();
                Grafico.Refresh();
                return;
            }

            double[] duraciones = PartidaOC.Select(d => (double)d.Duracion).ToArray();
            double[] puntuaciones = PartidaOC.Select(d => (double)d.Puntuacion).ToArray();

            Grafico.Plot.Clear();

            var barras = Grafico.Plot.Add.Bars(duraciones, puntuaciones);

            foreach (var bar in barras.Bars)
                bar.Size = 5; 

            Grafico.Plot.Title("Gráfico por puntuaciones");
            Grafico.Plot.XLabel("Duración");
            Grafico.Plot.YLabel("Puntuación");

            double maxY = puntuaciones.Max();
            Grafico.Plot.Axes.SetLimitsY(0, maxY * 1.05);
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

            await LlamadaPartidas();

            if (fechaSeleccionada.HasValue)         
            {
                foreach (var partida in PartidaOC)      // si la fecha seleccionada coincide con las fechas de las partidas, se muestran
                {
                    if (fechaSeleccionada.Value.Date != partida.Fecha.Date) {   
                        partidaEliminar.Add(partida);
                    }
                }
                foreach(var partida in partidaEliminar)        // se eliminan de la tabla las partidas que no sean de la fecha seleccionada        
                {
                    PartidaOC.Remove(partida);
                    Tabla.Items.Refresh();
                }
            }
            
        }
        private void AyudaPDF(object sender, RoutedEventArgs e)     // al hacer click en el botón de Ayuda, se abre la documentación para el usuario en el navegador
        {
            string ruta = @"..\..\Documentación de usuario para GAME HUB.pdf";
            try
            {
                Process.Start(new ProcessStartInfo(ruta)
                {
                    UseShellExecute = true
                });

            }catch(Exception err)   // control de excepciones
            {
                MessageBox.Show("Excepción" + err.Message);
            }
        }
    }  
}
