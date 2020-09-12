using System.Collections.Generic;
using System.IO;

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
    public partial class MainWindow
    {
        private readonly List<List<string>> _keyWords;
        private WebState _webState = WebState.Init;

        private readonly int _publicationYearStart = 1990;
        private readonly int _publicationYearEnd = 2020;
        private int _publicationYearCurrent;
        private int _currentKeyWordIndex;

        private readonly string _searchJs;
        private readonly string _resultJs;
        private readonly string _resultBackJs;

        public MainWindow()
        {
            InitializeComponent();
            
            // загрузка запросов из json-файла
            using (StreamReader file = new StreamReader("key_words.json"))
            {
                _keyWords = Newtonsoft.Json.JsonConvert.DeserializeObject<List<List<string>>>(file.ReadToEnd());
            }

            using (StreamReader file = new StreamReader("search.js"))
            {
                _searchJs = file.ReadToEnd();
            }

            using (StreamReader file = new StreamReader("result.js"))
            {
                _resultJs = file.ReadToEnd();
            }

            using (StreamReader file = new StreamReader("result_back.js"))
            {
                _resultBackJs = file.ReadToEnd();
            }

            //browser.IsBrowserInitializedChanged += Browser_IsBrowserInitializedChanged;
            Browser.LoadingStateChanged += Browser_LoadingStateChanged;
            
        }

        private void Browser_LoadingStateChanged(object sender, LoadingStateChangedEventArgs e)
        {
            if (e.IsLoading)
                return;

            switch (_webState)
            {
                case WebState.Init:
                    _webState = WebState.SearchPage;
                    _publicationYearCurrent = _publicationYearStart;
                    Browser.Load("https://www.elibrary.ru/querybox.asp?scope=newquery");
                    break;

                case WebState.SearchPage:
                    SearchPageHandle(Browser);
                    _webState = WebState.ResultPage;
                    break;

                case WebState.ResultPage:
                    //System.Threading.Thread.Sleep(1000);
                    ResultPageHandle();
                    if (_publicationYearCurrent != _publicationYearEnd)
                        _webState = WebState.SearchPage;
                    else
                    {
                        _currentKeyWordIndex++;
                        if (_currentKeyWordIndex != _keyWords.Count)
                            _webState = WebState.Init;
                        else
                            _webState = WebState.Done;
                    }
                    _publicationYearCurrent++;
                    break;
            }
        }


        void SearchPageHandle(ChromiumWebBrowser browser)
        {
            var setVarsJs =$"let keyWord = '{_keyWords[_currentKeyWordIndex][0]}';\r\n";
            setVarsJs += $"let publicationYear = '{_publicationYearCurrent}';\r\n";
            var js = browser.GetMainFrame().EvaluateScriptAsync(setVarsJs + _searchJs);
            js.ContinueWith(t => 0);
        }

        void ResultPageHandle()
        {
            var jsr = Browser.EvaluateScriptAsync(_resultJs);
            jsr.ContinueWith(t =>
            {
                var count = t.Result.Result.ToString();
                if (count == "")
                    count = "0";
                Dispatcher.Invoke(() => { Queries.Text += $"{_keyWords[_currentKeyWordIndex][0]}\t{_publicationYearCurrent}\t{count}\r\n"; });
                
                return 0;
            });


            var jsrb = Browser.EvaluateScriptAsync(_resultBackJs);
            jsrb.ContinueWith(t => 0);
        }
    }
}
