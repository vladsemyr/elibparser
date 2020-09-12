using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using CefSharp;
using CefSharp.Wpf;

namespace ParserWpf.Business
{
    public class ParserModel : INotifyPropertyChanged
    {
        #region Конструктор

        public ParserModel(ChromiumWebBrowser browser)
        {
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

            _browser = browser;
            _browser.LoadingStateChanged += Browser_LoadingStateChanged;
        }

        #endregion

        #region Свойства

        private string _queryText;
        public string QueriesText
        {
            get => _queryText;
            set
            {
                _queryText = value;
                OnPropertyChanged();
            }
        }

        #endregion

        #region Поля

        private readonly ChromiumWebBrowser _browser;

        private readonly List<List<string>> _keyWords;
        private WebState _webState = WebState.Init;

        private readonly int _publicationYearStart = 1990;
        private readonly int _publicationYearEnd = 2020;
        private int _publicationYearCurrent;
        private int _currentKeyWordIndex;

        private readonly string _searchJs;
        private readonly string _resultJs;
        private readonly string _resultBackJs;

        #endregion

        #region Вспомогательные методы

        private void Browser_LoadingStateChanged(object sender, LoadingStateChangedEventArgs e)
        {
            if (e.IsLoading)
                return;

            switch (_webState)
            {
                case WebState.Init:
                    _webState = WebState.SearchPage;
                    _publicationYearCurrent = _publicationYearStart;
                    _browser.Load("https://www.elibrary.ru/querybox.asp?scope=newquery");
                    break;

                case WebState.SearchPage:
                    SearchPageHandle();
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
                        _webState = _currentKeyWordIndex != _keyWords.Count ? WebState.Init : WebState.Done;
                    }
                    _publicationYearCurrent++;
                    break;
            }
        }

        private void SearchPageHandle()
        {
            var setVarsJs = $"let keyWord = '{_keyWords[_currentKeyWordIndex][0]}';\r\n";
            setVarsJs += $"let publicationYear = '{_publicationYearCurrent}';\r\n";
            var js = _browser.GetMainFrame().EvaluateScriptAsync(setVarsJs + _searchJs);
            js.ContinueWith(t => 0);
        }

        private void ResultPageHandle()
        {
            var jsr = _browser.EvaluateScriptAsync(_resultJs);
            jsr.ContinueWith(t =>
            {
                var count = t.Result.Result.ToString();
                if (count == "")
                    count = "0";
                QueriesText += $"{_keyWords[_currentKeyWordIndex][0]}\t{_publicationYearCurrent}\t{count}\r\n";

                return 0;
            });


            var jsrb = _browser.EvaluateScriptAsync(_resultBackJs);
            jsrb.ContinueWith(t => 0);
        }

        #endregion

        #region Реализация INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}