using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows.Forms;
using CefSharp;
using CefSharp.Wpf;
using ParserWpf.Business.JavaScript;
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

            var jsFiles = Enum.GetValues(typeof(JsFile));
            foreach (JsFile jsf in jsFiles)
            {
                using (StreamReader file = new StreamReader(JsFileToFilenameConvertor.Convert(jsf)))
                {
                    var script = file.ReadToEnd();
                    _jsFiles.Add(jsf, script);
                }
            }

            _browser = browser;
            _browser.LoadingStateChanged += Browser_LoadingStateChanged;

            ChangeScrollVisibilityCommand = new CustomClickCommand { ExecuteCommandAction = (obj) => { IsScrollVisible = !IsScrollVisible; } };
            SaveToFileCommand = new CustomClickCommand { ExecuteCommandAction = SaveResultsToFile };
            ChangeBrowserVisibilityCommand = new CustomClickCommand { ExecuteCommandAction = (obj) => { IsBrowserVisible = !IsBrowserVisible; } };
        }

        #endregion

        #region Свойства

        public CustomClickCommand ChangeScrollVisibilityCommand { get; }

        public CustomClickCommand SaveToFileCommand { get; }

        public CustomClickCommand ChangeBrowserVisibilityCommand { get; }

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

        private bool _isBrowserVisible;
        public bool IsBrowserVisible
        {
            get => _isBrowserVisible;
            set
            {
                _isBrowserVisible = value;
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

        private readonly Dictionary<JsFile, string> _jsFiles = new Dictionary<JsFile, string>();

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
                    {
                        var js = _browser.GetMainFrame().EvaluateScriptAsync(_jsFiles[JsFile.Robot]);
                        js.ContinueWith(t =>
                        {
                            if (!RobotCheck(t.Result))
                            {
                                SearchPageHandle();
                                _webState = WebState.ResultPage;
                            }
                            else
                            {
                                IsBrowserVisible = true;
                            }
                        });
                    }
                    break;

                case WebState.ResultPage:
                    //System.Threading.Thread.Sleep(1000);
                    {
                        var js = _browser.GetMainFrame().EvaluateScriptAsync(_jsFiles[JsFile.Robot]);
                        js.ContinueWith(t =>
                        {
                            if (!RobotCheck(t.Result))
                            {
                                ResultPageHandle();
                                if (_publicationYearCurrent != _publicationYearEnd)
                                    _webState = WebState.SearchPage;
                                else
                                {
                                    _currentKeyWordIndex++;
                                    _webState = _currentKeyWordIndex != _keyWords.Count ? WebState.Init : WebState.Done;
                                }
                                _publicationYearCurrent++;
                            }
                            else
                            {
                                IsBrowserVisible = true;
                            }
                        });
                    }
                    break;
            }
        }

        private bool RobotCheck(JavascriptResponse result)
        {
            return result.Result as bool? == true;
        }

        private void SearchPageHandle()
        {
            var setVarsJs = $"let keyWord = '{_keyWords[_currentKeyWordIndex]}';\r\n";
            setVarsJs += $"let publicationYear = '{_publicationYearCurrent}';\r\n";
            var js = _browser.GetMainFrame().EvaluateScriptAsync(setVarsJs + _jsFiles[JsFile.Search]);
            js.ContinueWith(t => 0);
        }

        private void ResultPageHandle()
        {
            var publicationYearCurrent = _publicationYearCurrent;
            var currentKeyWordIndex = _currentKeyWordIndex;

            var jsr = _browser.EvaluateScriptAsync(_jsFiles[JsFile.Result]);
            jsr.ContinueWith(t =>
            {
                var count = t.Result.Result.ToString();
                if (count == "")
                    count = "0";
                QueriesText += $"{_keyWords[currentKeyWordIndex]}\t{publicationYearCurrent}\t{count}\r\n";
                var resultString = $"{_keyWords[currentKeyWordIndex]};{publicationYearCurrent};{count}\r\n"
                                    .Replace("(", "")
                                    .Replace("|", "+")
                                    .Replace(")", "");
                _resultsInFile.Append(resultString);

                return 0;
            });


            var jsrb = _browser.EvaluateScriptAsync(_jsFiles[JsFile.ResultBack]);
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
                File.WriteAllText(dialog.FileName, _resultsInFile.ToString(), new UTF8Encoding(true) /* with BOM */);
        }

        #endregion

        #region Реализация INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}