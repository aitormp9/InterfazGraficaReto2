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
using FastReport;
using FastReport.Export.PdfSimple;
using FastReport.Export.OoXML;
using FastReport.Export.Csv;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using System.Threading;


namespace InterfazGraficaReto2
{
    public partial class Informes : Window
    {
        public Informes()
        {
            InitializeComponent();
        }
        private async Task<DriveService> GetDriveService()      // función para conectarse a Drive con las credenciales
        {
            UserCredential credential;
            using (var stream = new FileStream("client_secrets.json", FileMode.Open, FileAccess.Read))
            {
                string credPath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "DriveTokenStore");
                credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.FromStream(stream).Secrets,
                    new[] { DriveService.Scope.DriveReadonly },
                    "user",
                    CancellationToken.None,
                    new FileDataStore(credPath, true));
            }

            return new DriveService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = "InterfazGraficaReto2",
            });
        }
        private async void ExportarInforme(object sender, RoutedEventArgs e)  // función para exportar informe
        {
            string urlDriveID = "1WRgYpw744mqvykyWKuolCgBIaMTgdAdK";
            // if para que salte un error si no se ha elegido nada del ComboBox antes de exportar
            if (CBextension.SelectedItem == null || CBInforme.SelectedItem == null)
            {
                MessageBox.Show("Selecciona un informe o tipo de extensión", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            string nombreInforme = ((ComboBoxItem)CBInforme.SelectedItem).Content.ToString();
            string extension = ((ComboBoxItem)CBextension.SelectedItem).Content.ToString().ToLower();
            string nombreInformeDrive = string.Empty;
            switch (nombreInforme)              // switch para igualar el nombre de los items del ComboBox con lo nombres de los archivos
            {
                case "Historial de partidas":
                    nombreInformeDrive = "HistorialPartidas";
                    break;
                case "Ranking de jugadores":
                    nombreInformeDrive = "Ranking";
                    break;
                case "Lista de jugadores":
                    nombreInformeDrive = "ListaJugadores";
                    break;
            }

            // guardado de archivos
            string nombreArchivoBuscado = $"{nombreInformeDrive}.{extension}";
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = $"{extension.ToUpper()} files|*.{extension}";
            saveFileDialog.FileName = nombreArchivoBuscado;

            if (saveFileDialog.ShowDialog() == true)
            {
                try
                {
                    // conexión al Drive y busqueda del archivo por el nombre seleccionado en el ComboBox
                    var service = await GetDriveService();

                    var listRequest = service.Files.List();
                    listRequest.Q = $"'{urlDriveID}' in parents and name = '{nombreArchivoBuscado}' and trashed = false";
                    listRequest.Fields = "files(id, name)";

                    var result = await listRequest.ExecuteAsync();
                    var archivoDrive = result.Files.FirstOrDefault();

                    if (archivoDrive == null)
                    {
                        MessageBox.Show("El archivo no se ha encontrado", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    // descarga del archivo seleccionado
                    var descargar = service.Files.Get(archivoDrive.Id);
                    using (var fileStream = new FileStream(saveFileDialog.FileName, FileMode.Create, FileAccess.Write))
                    {
                        await descargar.DownloadAsync(fileStream);
                    }
                    MessageBox.Show("Archivo descargado correctamente", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception error)    // control de excepciones
                {
                    MessageBox.Show("Error: " + error.Message);
                }
            }
        }
    }
}
