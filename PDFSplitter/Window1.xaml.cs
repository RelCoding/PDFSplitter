using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace PDFSplitter
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class Window1 : Window
    {
        public Window1()
        {
            InitializeComponent();
        }

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            startPdfSplit(Convert.ToInt32(this.NumberTextBox.Text));
        }

        private async void startPdfSplit(int offset)
        {
            MainWindow mainWindow = Application.Current.Windows.OfType<MainWindow>().FirstOrDefault();
            if (mainWindow != null)
            {
                //hide prompt
                this.StartButton.Visibility = Visibility.Hidden;
                this.NumberTextBox.Visibility = Visibility.Hidden;
                this.PromptTextBlock.Visibility = Visibility.Hidden;
                //show progress UI
                this.SplitProgressbar.Visibility = Visibility.Visible;
                this.CancelButton.Visibility = Visibility.Visible;
                this.ProgressLabel.Visibility = Visibility.Visible;

                CancellationToken ct = new CancellationToken();
                mainWindow.GatherPdfs(offset, ct);
            }
        }

        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new("[^1-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        private void SplitOffsetPrompt_Closed(object sender, EventArgs e)
        {
            MainWindow mainWindow = Application.Current.Windows.OfType<MainWindow>().FirstOrDefault();
            if (mainWindow != null)
            {
                mainWindow.ResetFiles();
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
