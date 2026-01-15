using Microsoft.ReportingServices.Interfaces;
using Microsoft.Win32;
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
using System.Windows.Shapes;
using System.IO;

namespace InterfazGraficaReto2
{
    
    /// <summary>
    /// Lógica de interacción para Informes.xaml
    /// </summary>
    public partial class Informes : Window
    {
        public Informes()
        {
            InitializeComponent();
        }

        private void ExportarInforme(object sender, RoutedEventArgs e)  // función para exportar informe
        {
            // if para que salte un error si no se ha elegido nada del ComboBox antes de exportar
            if (CBextension.SelectedItem == null || CBInforme.SelectedItem == null)     
            {
                MessageBox.Show("Selecciona un informe o tipo de extensión", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            else
            {
                string nombreInforme = ((ComboBoxItem)CBInforme.SelectedItem).Content.ToString().Replace(" ", "_");

                string extension = ((ComboBoxItem)CBextension.SelectedItem).Content.ToString().ToLower();   
                SaveFileDialog saveFileDialog = new SaveFileDialog();                                       // guardado del informe
                saveFileDialog.Filter = $"{extension.ToUpper()} files|*.{extension}";
                saveFileDialog.FileName = $"{nombreInforme}.{extension}";

                if (saveFileDialog.ShowDialog() == true)
                {
                    string contenido = "prueba";
                    File.WriteAllText(saveFileDialog.FileName, contenido);
                    MessageBox.Show("Informe guardado correctamente");
                }
            }
        }
    }
}
