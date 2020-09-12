using ParserWpf.Business;

namespace ParserWpf.Wpf.UI
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        #region Конструктор

        public MainWindow()
        {
            InitializeComponent();

            var model = new ParserModel(Browser);
            DataContext = model;
        }

        #endregion
    }
}
