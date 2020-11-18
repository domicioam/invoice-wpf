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

namespace NFe.WPF.View.Ferramentas
{
    /// <summary>
    /// Interaction logic for ImportarXML.xaml
    /// </summary>
    public partial class ImportarXMLWindow : Window
    {
        public ImportarXMLWindow()
        {
            InitializeComponent();
        }

        private void textBox_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Arquivos de certificado (*.zip)|*.zip";

            if (openFileDialog.ShowDialog() == true)
            {
                for (int i = 0; i < openFileDialog.FileNames.Length; i++)
                {
                    btnImportar.IsEnabled = true;
                    textBoxSelecioneZip.Text = openFileDialog.FileName;
                }
            }
        }
    }
}
