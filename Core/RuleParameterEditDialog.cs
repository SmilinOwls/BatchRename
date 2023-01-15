using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace Core
{
    public class RuleParameterEditDialog : Window
    {

        public Grid grid = new Grid
        {
            RowDefinitions = {
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = GridLength.Auto },
            },
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
                new ColumnDefinition { Width = GridLength.Auto },
            },
            Margin = new Thickness(0, 0, 20, 30)
        };

       
        public Image image = new Image { Margin = new Thickness(20, 0, 0, 0), Source = new BitmapImage(new Uri(Path.GetFullPath("icons/edit.png"), UriKind.Absolute)) };

        public List<Label> label = new List<Label>();
        public List<TextBox> txtBox = new List<TextBox>();
        public StackPanel stackPanel = new StackPanel();
        public WrapPanel wrapPanel = new WrapPanel { HorizontalAlignment = HorizontalAlignment.Right, Margin = new Thickness(0, 15, 0, 0) };
        public Button addBtn = new Button { IsDefault = true, Content = "_Add", Name = "addBtn", MinWidth = 60, Margin = new Thickness(0, 0, 10, 0) };
        public Button cancelBtn = new Button { IsCancel = true, Content = "_Cancel", Name = "cancelBtn", MinWidth = 60 };
        public RuleFormat RuleFormat = new RuleFormat();
        public int Number = 0;

        public RuleParameterEditDialog(RuleFormat ruleFormat, string ruleName, List<string> content)
        {
            Number = content.Count;

            for (int i = 0; i < Number; i++)
            {
                label.Add(new Label { Content = content[i], FontSize = 14 });
                txtBox.Add(new TextBox { MinWidth = 250, TextWrapping = TextWrapping.Wrap });
            }

            this.Title = $"{string.Join(' ', ruleName.Split('.'))} Editing";

            Grid.SetColumn(image, 0);
            Grid.SetColumn(stackPanel, 1);
            Grid.SetRow(wrapPanel, 1);
            Grid.SetColumnSpan(wrapPanel, 2);

            cancelBtn.Click += CancelBtn_Clicked;

            wrapPanel.Children.Add(addBtn);
            wrapPanel.Children.Add(cancelBtn);

            for (int i = 0; i < Number; i++)
            {
                stackPanel.Children.Add(label[i]);
                stackPanel.Children.Add(txtBox[i]);
            }

            grid.Children.Add(image);
            grid.Children.Add(stackPanel);
            grid.Children.Add(wrapPanel);

            this.AddChild(grid);
            this.MaxWidth = 600;
            this.MaxHeight = 450;
        }


        public void CancelBtn_Clicked(object sender, RoutedEventArgs e)
        {

        }

    }

}
