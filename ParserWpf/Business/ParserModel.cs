﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows.Forms;
using CefSharp;
using CefSharp.Wpf;
using ParserWpf.Wpf.Commands;

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
                _keyWords = Newtonsoft.Json.JsonConvert.DeserializeObject<List<string>>(file.ReadToEnd());
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

            ChangeScrollVisibilityCommand = new CustomClickCommand { ExecuteCommandAction = (obj) => { IsScrollVisible = !IsScrollVisible; } };
            SaveToFileCommand = new CustomClickCommand { ExecuteCommandAction = SaveResultsToFile };
        }

        #endregion

        #region Свойства

        public CustomClickCommand ChangeScrollVisibilityCommand { get; protected set; }

        public CustomClickCommand SaveToFileCommand { get; protected set; }

        private string _queryText = string.Empty;
        public string QueriesText
        {
            get => _queryText;
            set
            {
                _queryText = value;
                OnPropertyChanged();
            }
        }

        private bool _isScrollVisible;
        public bool IsScrollVisible
        {
            get => _isScrollVisible;
            set
            {
                _isScrollVisible = value;
                OnPropertyChanged();
            }
        }
        

        #endregion

        #region Поля

        private readonly ChromiumWebBrowser _browser;

        private readonly List<string> _keyWords;
        private WebState _webState = WebState.Init;

        private readonly int _publicationYearStart = 1990;
        private readonly int _publicationYearEnd = 2020;
        private int _publicationYearCurrent;
        private int _currentKeyWordIndex;

        private readonly string _searchJs;
        private readonly string _resultJs;
        private readonly string _resultBackJs;

        private readonly StringBuilder _resultsInFile = new StringBuilder();

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
            var setVarsJs = $"let keyWord = '{_keyWords[_currentKeyWordIndex]}';\r\n";
            setVarsJs += $"let publicationYear = '{_publicationYearCurrent}';\r\n";
            var js = _browser.GetMainFrame().EvaluateScriptAsync(setVarsJs + _searchJs);
            js.ContinueWith(t => 0);
        }

        private void ResultPageHandle()
        {
            var publicationYearCurrent = _publicationYearCurrent;
            var currentKeyWordIndex = _currentKeyWordIndex;

            var jsr = _browser.EvaluateScriptAsync(_resultJs);
            jsr.ContinueWith(t =>
            {
                var count = t.Result.Result.ToString();
                if (count == "")
                    count = "0";
                QueriesText += $"{_keyWords[currentKeyWordIndex]}\t{publicationYearCurrent}\t{count}\r\n";
                _resultsInFile.Append($"{_keyWords[_currentKeyWordIndex]};{_publicationYearCurrent};{count}\r\n");

                return 0;
            });


            var jsrb = _browser.EvaluateScriptAsync(_resultBackJs);
            jsrb.ContinueWith(t => 0);
        }

        private void SaveResultsToFile(object obj)
        {
            var date = DateTime.Now.ToString(@"dd/mm/yyyy HH-mm-ss");
            SaveFileDialog dialog = new SaveFileDialog
            {
                FileName = $@"ELibraryParseResults_{date}",
                DefaultExt = @".csv",
                Filter = @"Csv file (*.csv)|*.csv",
                InitialDirectory = Directory.GetCurrentDirectory()
            };
            if (dialog.ShowDialog() == DialogResult.OK)
                File.WriteAllText(dialog.FileName, _resultsInFile.ToString());
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