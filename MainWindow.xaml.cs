using System.Windows;
using System.Windows.Navigation;

using System.Collections.Generic;
using System.IO;

using System.Threading.Tasks;

using CefSharp;
using CefSharp.Wpf;

namespace WpfApp6
{
    enum WebState
    {
        Init,
        SearchPage,
        ResultPage,
        Done
    };


    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        List<List<string>> keyWords = new List<List<string>> { };
        WebState webState = WebState.Init;

        int publicationYearStart = 1990;
        int publicationYearEnd = 2020;
        int publicationYearCurrent = 0;
        int currentKeyWordIndex = 0;

        string searchJS = "";
        string resultJs = "";
        string resultBackJs = "";

        public MainWindow()
        {
            InitializeComponent();
            
            // загрузка запросов из json-файла
            using (StreamReader file = new StreamReader("key_words.json"))
            {
                keyWords = Newtonsoft.Json.JsonConvert.DeserializeObject<List<List<string>>>(file.ReadToEnd());
            }

            using (StreamReader file = new StreamReader("search.js"))
            {
                searchJS = file.ReadToEnd();
            }

            using (StreamReader file = new StreamReader("result.js"))
            {
                resultJs = file.ReadToEnd();
            }

            using (StreamReader file = new StreamReader("result_back.js"))
            {
                resultBackJs = file.ReadToEnd();
            }

            //browser.IsBrowserInitializedChanged += Browser_IsBrowserInitializedChanged;
            browser.LoadingStateChanged += Browser_LoadingStateChanged;
            
        }

        private void Browser_LoadingStateChanged(object sender, LoadingStateChangedEventArgs e)
        {
            if (e.IsLoading == true)
                return;

            switch (webState)
            {
                case WebState.Init:
                    webState = WebState.SearchPage;
                    publicationYearCurrent = publicationYearStart;
                    browser.Load("https://www.elibrary.ru/querybox.asp?scope=newquery");
                    break;

                case WebState.SearchPage:
                    SearchPageHandle(browser);
                    webState = WebState.ResultPage;
                    break;

                case WebState.ResultPage:
                    //System.Threading.Thread.Sleep(1000);
                    ResultPageHandle();
                    if (publicationYearCurrent != publicationYearEnd)
                        webState = WebState.SearchPage;
                    else
                    {
                        currentKeyWordIndex++;
                        if (currentKeyWordIndex != keyWords.Count)
                            webState = WebState.Init;
                        else
                            webState = WebState.Done;
                    }
                    publicationYearCurrent++;
                    break;
            }
        }


        void SearchPageHandle(ChromiumWebBrowser browser)
        {
            var set_varsJS =$"let keyWord = '{keyWords[currentKeyWordIndex][0]}';\r\n";
            set_varsJS += $"let publicationYear = '{publicationYearCurrent}';\r\n";
            var js = browser.GetMainFrame().EvaluateScriptAsync(set_varsJS + searchJS);
            var c = js.ContinueWith(t =>
            {
                return 0;
            });
        }

        void ResultPageHandle()
        {
            var jsr = browser.EvaluateScriptAsync(resultJs);
            var cr = jsr.ContinueWith(t =>
            {
                var count = t.Result.Result.ToString();
                if (count == "")
                    count = "0";
                Dispatcher.Invoke(() => { queries.Text += $"{keyWords[currentKeyWordIndex][0]}\t{publicationYearCurrent}\t{count}\r\n"; });
                
                return 0;
            });

            

            var jsrb = browser.EvaluateScriptAsync(resultBackJs);
            var crb = jsrb.ContinueWith(t =>
            {
                return 0;
            });
        }
    }
}
