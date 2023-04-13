using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using PSPDFKit.Pdf;
using PSPDFKit.Document;
using PSPDFKit.UI;
using Windows.UI.Popups;
using PSPDFKit.Search;
using PSPDFKitFoundation.Search;
using PSPDFKitFoundation;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.ApplicationModel.Contacts;
using System.Reflection.Metadata;
using Document = PSPDFKit.Pdf.Document;

using Windows.ApplicationModel.Core;
using Windows.System;
using Windows.Storage;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace UWPSearchExample
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private Document _document;
        public MainPage()
        {
            this.InitializeComponent();
            PdfView.InitializationCompletedHandler += OnPdfViewInitializationCompleted;
        }

        private async void OnPdfViewInitializationCompleted(PdfView sender, Document doc)
        {
            // Copy pdf asset to temp folder as it is not updatable in packaged version
            var pdfAssetUri = new Uri("ms-appx:///Assets/sample.pdf");
            StorageFile pdf = await StorageFile.GetFileFromApplicationUriAsync(pdfAssetUri);
            var tempPdfFile= await pdf.CopyAsync(ApplicationData.Current.TemporaryFolder, pdf.Name, NameCollisionOption.ReplaceExisting);

            // Load this document.
            var documentSource = DocumentSource.CreateFromStorageFile(tempPdfFile);
            _document = await sender.Controller.ShowDocumentAsync(documentSource);
        }

        private async void Button_Search_Click(object sender, RoutedEventArgs e)
        {
            var searchComplete = new TaskCompletionSource<bool>();
            var textSearcher = new TextSearcher();
            var results = new List<PageResults>();
            var allResults = new List<Result>();

            textSearcher.SearchCompleteHandler += (snd, args) => { searchComplete.SetResult(true); };
            textSearcher.SearchResultHandler += (snd, args) => { results.Add(args); };

            var query = Query.FromText("");
            query.SearchTerm = "do"; // text to search
            query.CompareOptions = CompareOptions.CaseInsensitive; // replace with 0 for case sensitive search
            query.SearchType = SearchType.Text;
            query.ReturnEmptyResults = false;

            textSearcher.SearchDocumentAsync(_document, query);
            
            await searchComplete.Task;

            var count = results[0].Results.Count;

            allResults.AddRange(results[0].Results);

            CoreApplication.MainView.CoreWindow.DispatcherQueue.TryEnqueue(DispatcherQueuePriority.Normal,
                async () =>
                {
                    await PdfView.HighlightResultsAsync(allResults);
                });

            var messageDialog = new MessageDialog("Search result count ->" + count.ToString());
            await messageDialog.ShowAsync();

        }
    }
}
