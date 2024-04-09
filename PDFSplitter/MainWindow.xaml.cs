using System.Text;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using PdfSharp;
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;

namespace PDFSplitter
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        string[] pdfs;
        string directory;
        public MainWindow()
        {
            InitializeComponent();
        }

        public Task<bool> GatherPdfs(int offset, CancellationToken ct)
        {
            Window1 window1 = Application.Current.Windows.OfType<Window1>().FirstOrDefault();
            if (pdfs != null)
            {
                foreach (string pdf in pdfs)
                {
                    //on cancel
                    ct.ThrowIfCancellationRequested();
                    try
                    {
                        int lastPageBreak = 0;
                        string name = System.IO.Path.GetFileNameWithoutExtension(pdf);
                        PdfDocument pdfInput = PdfReader.Open(pdf, PdfDocumentOpenMode.Import);
                        window1.SplitProgressbar.Maximum = pdfInput.PageCount;
                        for (int pageNumber = 0; pageNumber < pdfInput.PageCount; pageNumber += offset)
                        {
                            //on cancel
                            ct.ThrowIfCancellationRequested();
                            // Create new document
                            PdfDocument outputDocument = new();
                            outputDocument.Version = pdfInput.Version;
                            if (offset == 1)
                            {
                                outputDocument.Info.Title =
                                    String.Format("Seite {0} von {1}", pageNumber + 1, pdfInput.Info.Title);
                                outputDocument.Info.Creator = pdfInput.Info.Creator;

                                for (int i = 0; i < offset; i++)
                                {
                                    outputDocument.AddPage(pdfInput.Pages[pageNumber]);
                                }
                                string savePath = directory + "\\" + String.Format("{0} - Seite {1}.pdf", name, pageNumber + 1);
                                outputDocument.Save(savePath);
                                window1.SplitProgressbar.Value++;
                            }
                            else if (offset > 1 && offset % 1 == 0)
                            {
                                outputDocument.Info.Title =
                                    String.Format("Seite {0} von {1}", lastPageBreak
                                    .ToString() + "-" + (pageNumber + 1).ToString(), pdfInput.Info.Title);
                                outputDocument.Info.Creator = pdfInput.Info.Creator;
                                if (pageNumber + offset > pdfInput.PageCount)
                                {
                                    for (int i = 0; i < pdfInput.PageCount - pageNumber; i++)
                                    {
                                        outputDocument.AddPage(pdfInput.Pages[pageNumber + i]);
                                        window1.SplitProgressbar.Value++;
                                    }
                                }
                                else
                                {
                                    for (int i = 0; i < offset; i++)
                                    {
                                        outputDocument.AddPage(pdfInput.Pages[pageNumber + i]);
                                        window1.SplitProgressbar.Value++;
                                    }
                                }
                                if (pdfInput.PageCount - pageNumber > 1)
                                {
                                    string savePath = directory + "\\" + String.Format("{0} - Seite {1}.pdf", name, (pageNumber + 1) + "-" + (pageNumber + offset));
                                    outputDocument.Save(savePath);
                                }
                                else if (pdfInput.PageCount - pageNumber == 1)
                                {
                                    string savePath = directory + "\\" + String.Format("{0} - Seite {1}.pdf", name, pageNumber + 1);
                                    outputDocument.Save(savePath);
                                }
                            }
                            else
                            {
                                if (ct.IsCancellationRequested)
                                {
                                    window1.Close();
                                    return Task.FromResult(false);
                                }
                                //error on offset
                                Close();
                                ErrorWindow error1 = new();
                                error1.ErrorText.Text = "Üngültiger Wert für die Seiten-Vorgabe.";
                                error1.Show();
                                return Task.FromResult(false);
                            }
                        }
                        //successful operation
                        window1.Close();
                        return Task.FromResult(true);
                    }
                    catch
                    {
                        if (ct.IsCancellationRequested)
                        {
                            window1.Close();
                            return Task.FromResult(false);
                        }
                        //pdf error
                        this.Close();
                        ErrorWindow error1 = new();
                        error1.ErrorText.Text = "Keine gültige PDF-Datei.";
                        error1.Show();
                        return Task.FromResult(false);
                    }
                }
            }
            //no files somehow
            this.Close();
            ErrorWindow error = new();
            error.ErrorText.Text = "Es konnte keine Datei gefunden werden.";
            error.Show();
            return Task.FromResult(false);
        }

        private void Window_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                pdfs = (string[])e.Data.GetData(DataFormats.FileDrop);
                try
                {
                    directory = System.IO.Path.GetDirectoryName(pdfs[0]);
                }
                catch
                {
                    //directory not found
                    this.Close();
                    ErrorWindow error = new();
                    error.ErrorText.Text = "Verzeichnis konnte nicht gefunden werden. Womöglich ist der Zugriff auf dieses nicht erlaubt.";
                    error.Show();
                }
                //open window with prompt to select pdf split offset
                Window1 splitOffsetPrompt = new();
                splitOffsetPrompt.ShowDialog();
            }
        }

        public void ResetFiles()
        {
            this.pdfs = Array.Empty<string>();
        }
    }
}