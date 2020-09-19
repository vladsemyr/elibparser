using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ParserWpf.Wpf
{
    /// <summary>
    /// Пока что класс не обобщен
    /// </summary>
    public class TextScaleBehavior
    {
        public static readonly DependencyProperty TextScaleBehaviorEnabledProperty = 
            DependencyProperty.RegisterAttached(
                "TextScaleBehaviorEnabled", 
                typeof(bool), 
                typeof(TextScaleBehavior), 
                new PropertyMetadata(default(bool), OnTextScaleBehaviorChanged));

        public static void SetTextScaleBehavior(TextBlock element, bool value) { element.SetValue(TextScaleBehaviorEnabledProperty, value); _text = element; }
        public static bool GetTextScaleBehavior(TextBlock element) { return (bool)element.GetValue(TextScaleBehaviorEnabledProperty); }

        private static Grid _grid;
        private static TextBlock _text;


        private static void OnTextScaleBehaviorChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            TextBlock text = (TextBlock)obj;
            if (_grid == null)
            {
                var parent = VisualTreeHelper.GetParent(text);
                while (!(parent is Grid) && parent != null)
                {
                    parent = VisualTreeHelper.GetParent(parent);
                }
                _grid = parent as Grid;
            }
            
            if (_grid != null)
                _grid.SizeChanged += TextOnSizeChanged;

            ChangeTextFontSize(_grid);
        }

        private static void TextOnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            ChangeTextFontSize(sender as Grid);
        }

        private static void ChangeTextFontSize(Grid grid)
        {
            if (grid == null)
                return;

            double height = grid.ActualHeight;
            double width = grid.ActualWidth;

            if (_text != null)
                _text.FontSize = Math.Min(height / 12, width / 20);
        }
    }
}