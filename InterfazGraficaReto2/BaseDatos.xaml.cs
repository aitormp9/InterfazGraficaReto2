using System;
using System.Collections.Generic;
using System.Linq;
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
using Npgsql;

namespace InterfazGraficaReto2
{
    public partial class BaseDatos : Page
    {
        public BaseDatos()
        {
            InitializeComponent();
        }

        //public string conexionBD = "Host = 3.233.57.10;" + "Port = 5432;" + "Database = game_db;" + "Username = dam;" + "Password = password;";


        public void llamadaJugadores()
        {
            string conexionBD = "Host = 3.233.57.10;" + "Port = 5432;" + "Database = game_db;" + "Username = dam;" + "Password = password;";
            using (var con = new NpgsqlConnection(conexionBD))
            {
                con.Open();
                string query = "select * from jugadores";
                var llamada = new NpgsqlCommand(query, con);
                MessageBox.Show(llamada.ToString());
            }
        }
        private void llamadaPartidas()
        {
            string conexionBD = "Host = 3.233.57.10;" + "Port = 5432;" + "Database = game_db;" + "Username = dam;" + "Password = password;";
            using (var con = new NpgsqlConnection(conexionBD))
            {
                con.Open();
                string query = "select * from partidas";
                var llamada = new NpgsqlCommand(query, con);
                MessageBox.Show(llamada.ToString());
            }
        }
        private void llamadaRanking()
        {
            string conexionBD = "Host = 3.233.57.10;" + "Port = 5432;" + "Database = game_db;" + "Username = dam;" + "Password = password;";
            using (var con = new NpgsqlConnection(conexionBD))
            {
                con.Open();
                string query = "select * from ranking";
                var llamada = new NpgsqlCommand(query, con);
                MessageBox.Show(llamada.ToString());
            }
        }
    }
}
