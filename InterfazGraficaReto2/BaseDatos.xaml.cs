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
using Microsoft.Reporting.Map.WebForms.BingMaps;
using Npgsql;
using OpenTK.Graphics.ES20;

namespace InterfazGraficaReto2
{
    public partial class BaseDatos : Page
    {                                           // conexión a la BD mediante IP, puerto, nombre DB, usuario y contraseña
        public string conexion = "Host = 3.233.57.10;" + "Port = 5432;" + "Database = game_db;" + "Username = dam;" + "Password = password;";
        public BaseDatos()
        {
            InitializeComponent();
        }
        
        public string conexionBD()
        {
            string con = conexion;
            return con;
        }
    }
}
