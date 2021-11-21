using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Shapes;
using System.Windows.Media;
using System.Windows.Input;

namespace PasswordKeeper.Core
{
    class InfoBox : Border, IComparable
    {
        // Объявление переменных.
        private string[] info { get; set; }
        public bool IsSelected { get; private set; }
        private string origName { get; set; }

        public StackPanel mainStPanel { get; private set; }
        private Grid mainGrid { get; set; }
        private Border selectionMarkBorder { get; set; }

        private DockPanel nameDockPanel;
        public TextBox nameTxtBox { get; private set; }

        private Border infoBorder;
        private StackPanel infoStPanel;
        private DockPanel infoDockPanel;

        private StackPanel loginPassStPanel;
        private StackPanel loginStPanel, passStPanel;
        private TextBlock loginTxtBlock, passTxtBlock;
        public TextBox loginTxtBox { get; private set; }
        public TextBox passTxtBox { get; private set; }

        private StackPanel buttonsMainStPanel, buttonsStPanel;
        private Line lineButton;
        private Button addInfoButton, removeInfoButton;
        private Polygon addInfoPolygon, removeInfoPolygon;

        private StackPanel otherInfoMainStPanel, otherInfoLineStPanel, otherInfoTextStPanel;
        private Canvas otherInfoCanvas;
        private Line otherInfoLine;
        private TextBox otherInfoTxtBox;
        private TextBlock otherInfoTxtBlock;

        private ResourceDictionary winRes;
        private ResourceDictionary appRes;

        // Конструкторы.
        public InfoBox(ResourceDictionary winResources, ResourceDictionary appResources)
        {
            CreateInfoBox(winResources, appResources);
        }

        public InfoBox(ResourceDictionary winResources, ResourceDictionary appResources, string[] infoArr)
        {
            CreateInfoBox(winResources, appResources);

            nameTxtBox.Text = infoArr[0];
            loginTxtBox.Text = infoArr[1];
            passTxtBox.Text = infoArr[2];

            if(infoArr[3] != "")
            {
                CreateOtherInfo();

                otherInfoTxtBox.Text = infoArr[3];
            }
        }

        // Методы.
        internal string[] GetInfo()
        {
            info[0] = nameTxtBox.Text;
            info[1] = loginTxtBox.Text;
            info[2] = passTxtBox.Text;
            if(otherInfoTxtBox != null)
                info[3] = otherInfoTxtBox.Text;

            return info;
        }

        private void CreateInfoBox(ResourceDictionary winResources, ResourceDictionary appResources)
        {
            // Создаем все объекты элементов панели.
            mainGrid = new Grid();
            mainStPanel = new StackPanel();
            selectionMarkBorder = new Border();

            nameDockPanel = new DockPanel();
            nameTxtBox = new TextBox();

            infoBorder = new Border();
            infoStPanel = new StackPanel();
            infoDockPanel = new DockPanel();

            loginPassStPanel = new StackPanel();
            loginStPanel = new StackPanel();
            passStPanel = new StackPanel();
            loginTxtBlock = new TextBlock();
            passTxtBlock = new TextBlock();
            loginTxtBox = new TextBox();
            passTxtBox = new TextBox();

            buttonsMainStPanel = new StackPanel();
            buttonsStPanel = new StackPanel();
            lineButton = new Line();
            addInfoButton = new Button();
            removeInfoButton = new Button();
            addInfoPolygon = new Polygon();
            removeInfoPolygon = new Polygon();
            
            info = new string[4];

            // Устанавливаем значение переменной ресурсов.
            winRes = winResources;
            appRes = appResources;

            // Устанавливаем наследования.
            this.Child = mainGrid;

            mainGrid.Children.Add(mainStPanel);
            mainGrid.Children.Add(selectionMarkBorder);

            mainStPanel.Children.Add(nameDockPanel);
            mainStPanel.Children.Add(infoBorder);

            nameDockPanel.Children.Add(nameTxtBox);

            infoBorder.Child = infoStPanel;
            infoStPanel.Children.Add(infoDockPanel);

            infoDockPanel.Children.Add(buttonsMainStPanel);
            infoDockPanel.Children.Add(loginPassStPanel);

            loginPassStPanel.Children.Add(loginStPanel);
            loginPassStPanel.Children.Add(passStPanel);

            loginStPanel.Children.Add(loginTxtBlock);
            loginStPanel.Children.Add(loginTxtBox);

            passStPanel.Children.Add(passTxtBlock);
            passStPanel.Children.Add(passTxtBox);

            buttonsMainStPanel.Children.Add(lineButton);
            buttonsMainStPanel.Children.Add(buttonsStPanel);

            buttonsStPanel.Children.Add(addInfoButton);
            buttonsStPanel.Children.Add(removeInfoButton);

            addInfoButton.Content = addInfoPolygon;
            removeInfoButton.Content = removeInfoPolygon;

            // Устанавливаем различные свойства элементов.

            // Свойства для главного элемента InfoBox.
            this.Padding = new Thickness(10d);
            this.CornerRadius = new CornerRadius(6d);
            this.MouseUp += Selection_MouseUp;
            this.IsSelected = false;

            // Свойства Canvas, который определяет цвет InfoBox при выделении.
            selectionMarkBorder.Background = new SolidColorBrush(Color.FromArgb(60, 16, 110, 232));
            selectionMarkBorder.CornerRadius = new CornerRadius(8d);
            selectionMarkBorder.Visibility = Visibility.Hidden;

            // Свойства DockPanel для TextBox, который содержит имя InfoBox.
            nameDockPanel.LastChildFill = false;

            // Свойства поля для имени панели.
            nameTxtBox.Template = (ControlTemplate)winResources["TxtBoxNameTpl"];
            nameTxtBox.Style = (Style)winResources["TextBoxName"];
            nameTxtBox.MaxWidth = 300d;
            nameTxtBox.MinWidth = 80d;
            DockPanel.SetDock(nameTxtBox, Dock.Right);
            //nameTxtBox.TextChanged += NameTextBox_TextChanged;
            nameTxtBox.LostFocus += TextBox_LostFocus;

            infoBorder.Style = (Style)winResources["BorderInfoFields"];

            // Свойства панели содержащей панели с логином и паролем.
            loginPassStPanel.Margin = new Thickness(5d, 0d, 5d, 0d);
            loginPassStPanel.Width = 280d;
            DockPanel.SetDock(loginPassStPanel, Dock.Right);

            // Свойства панели с элементами логина.
            loginStPanel.Margin = new Thickness(0d, 5d, 0d, 0d);
            loginStPanel.Orientation = Orientation.Horizontal;

            // Свойства текста рядом с полем логина.
            loginTxtBlock.Text = "Login:";
            loginTxtBlock.FontSize = 22d;
            loginTxtBlock.Style = (Style)winResources["LabelTextBlock"];

            // Свойства поля для логина.
            loginTxtBox.Template = (ControlTemplate)appResources["TxtBoxTpl"];
            loginTxtBox.Style = (Style)winResources["TextBoxInfoFields"];
            loginTxtBox.LostFocus += TextBox_LostFocus;
            loginTxtBox.GotFocus += InfoTextBox_GotFocus;

            // Свойства панели с элементами пароля.
            passStPanel.Margin = new Thickness(0d, 10d, 0d, 0d);
            passStPanel.Orientation = Orientation.Horizontal;

            // Свойства текста рядом с полем пароля.
            passTxtBlock.Text = "Password:";
            passTxtBlock.FontSize = 22d;
            passTxtBlock.Style = (Style)winResources["LabelTextBlock"];

            // Свойства поля для пароля.
            passTxtBox.Template = (ControlTemplate)appResources["TxtBoxTpl"];
            passTxtBox.Style = (Style)winResources["TextBoxInfoFields"];
            passTxtBox.MinWidth = 64d;
            passTxtBox.LostFocus += TextBox_LostFocus;
            passTxtBox.GotFocus += InfoTextBox_GotFocus;

            //Свойства панели с кнопками.
            buttonsMainStPanel.Orientation = Orientation.Horizontal;
            DockPanel.SetDock(buttonsMainStPanel, Dock.Right);

            lineButton.X1 = 0d;
            lineButton.Y1 = 0d;
            lineButton.X2 = 0d;
            lineButton.Y2 = 83d;
            lineButton.Stroke = Brushes.DimGray;
            lineButton.StrokeThickness = 4d;
            lineButton.Margin = new Thickness(8, 0, 8, 0);
            lineButton.SnapsToDevicePixels = true;

            buttonsStPanel.Orientation = Orientation.Vertical;

            // Свойства для кнопки +.
            addInfoButton.Template = (ControlTemplate)appResources["ButtonTmpl"];
            addInfoButton.Style = (Style)winResources["BoxButton"];
            addInfoButton.Height = 34d;
            addInfoButton.Width = 34d;
            addInfoButton.Margin = new Thickness(0, 10, 0, 0);
            addInfoButton.Click += AddInfoButton_Click;
            addInfoButton.SnapsToDevicePixels = true;

            Point[] plusPoints = new Point[]
            {
                new Point(11d, 0d), new Point(15d, 0d), new Point(15d, 11d),
                new Point(26d, 11d), new Point(26d, 15d), new Point(15d, 15d),
                new Point(15d,26d), new Point(11d, 26d), new Point(11d, 15d),
                new Point(0d,15d), new Point(0d,11d), new Point(11d,11d)
            };
            addInfoPolygon.Points = new PointCollection(plusPoints);
            addInfoPolygon.Fill = Brushes.DimGray;
            Canvas.SetTop(addInfoPolygon, -13d);
            Canvas.SetLeft(addInfoPolygon, -13d);

            // Свойства для кнопки -.
            removeInfoButton.Template = (ControlTemplate)appResources["ButtonTmpl"];
            removeInfoButton.Style = (Style)winResources["BoxButton"];
            removeInfoButton.MinHeight = 34d;
            removeInfoButton.MinWidth = 34d;
            removeInfoButton.Click += RemoveInfoButton_Click;
            removeInfoButton.SnapsToDevicePixels = true;

            Point[] minusPoints = new Point[]
            {
                new Point(0d,0d), new Point(26d,0d), new Point(26d, 4d),
                new Point(0d, 4d)
            };
            removeInfoPolygon.Points = new PointCollection(minusPoints);
            removeInfoPolygon.Fill = Brushes.DimGray;
            Canvas.SetTop(removeInfoPolygon, -2d);
            Canvas.SetLeft(removeInfoPolygon, -13d);
            removeInfoButton.IsEnabled = false;
        }

        private void CreateOtherInfo()
        {
            // Создаем все объекты элементов дополнительной панели.
            otherInfoMainStPanel = new StackPanel();
            otherInfoLineStPanel = new StackPanel();
            otherInfoTextStPanel = new StackPanel();
            otherInfoCanvas = new Canvas();
            otherInfoLine = new Line();
            otherInfoTxtBox = new TextBox();
            otherInfoTxtBlock = new TextBlock();

            // Устанавливаем наследования.
            otherInfoMainStPanel.Children.Add(otherInfoLineStPanel);
            otherInfoMainStPanel.Children.Add(otherInfoTextStPanel);

            otherInfoLineStPanel.Children.Add(otherInfoCanvas);

            otherInfoCanvas.Children.Add(otherInfoLine);

            otherInfoTextStPanel.Children.Add(otherInfoTxtBlock);
            otherInfoTextStPanel.Children.Add(otherInfoTxtBox);

            // Устанавливаем различные свойства элементов.
            otherInfoLineStPanel.Height = 29d;

            otherInfoCanvas.SnapsToDevicePixels = true;

            otherInfoLine.X1 = 0d;
            otherInfoLine.Y1 = 3d;
            otherInfoLine.X2 = 335d;
            otherInfoLine.Y2 = 3d;
            otherInfoLine.Stroke = Brushes.DimGray;
            otherInfoLine.StrokeThickness = 4d;
            otherInfoLine.StrokeDashArray = new DoubleCollection(new double[] { 1d });
            otherInfoLine.Margin = new Thickness(0d, 12d, 0d, 12d);
            otherInfoLine.SnapsToDevicePixels = true;

            otherInfoTextStPanel.Orientation = Orientation.Horizontal;
            otherInfoTextStPanel.Margin = new Thickness(5d, 0d, 5d, 0d);

            otherInfoTxtBlock.Text = "Info: ";
            otherInfoTxtBlock.FontSize = 22d;
            otherInfoTxtBlock.Style = (Style)winRes["LabelTextBlock"];

            // Свойства поля дополнительной информации.
            otherInfoTxtBox.Template = (ControlTemplate)appRes["TxtBoxTpl"];
            otherInfoTxtBox.Style = (Style)winRes["TextBoxInfoFields"];
            otherInfoTxtBox.LostFocus += TextBox_LostFocus;
            otherInfoTxtBox.GotFocus += InfoTextBox_GotFocus;

            // Присоединяем к остальной панели.
            infoStPanel.Children.Add(otherInfoMainStPanel);
            // Устанавливаем кнопки добавления/удаление доп инфы в нужное состояние.
            addInfoButton.IsEnabled = false;
            removeInfoButton.IsEnabled = true;
        }

        internal void SwitchSelection()
        {
            selectionMarkBorder.Visibility = IsSelected ? Visibility.Hidden : Visibility.Visible;
            IsSelected = !IsSelected;
        }

        // Реализация интерфейса IComparable.
        public int CompareTo(object obj)
        {
            string paramText = null;
            try
            {
                if (obj is InfoBox)
                {
                    paramText = (obj as InfoBox).nameTxtBox.Text;
                }
                else if (obj is string)
                {
                    paramText = (string)obj;
                }
                else
                {
                    throw new ArgumentException("Недопустимый аргумент для сравнения!");
                }
            }
            catch(ArgumentException exc)
            {
                MainWindow.Logger(exc);
            }

            paramText = paramText.ToLower();
            string thisText = nameTxtBox.Text.ToLower();

            return BaseCompareTo(thisText, paramText);
        }

        private int BaseCompareTo(string firstString, string secondString)
        {
            int shortNameLength = firstString.Length > secondString.Length ? secondString.Length : firstString.Length;

            for (int i = 0; i < shortNameLength; i++)
            {
                // Если значение символа вызывающего объекта больше, чем значение символа аргумента,
                // то значит, что символ вызывающего объекта располагается в алфавите дальше,
                // соответственно и в массиве он должен располагаться дальше. 
                if (firstString[i] > secondString[i])
                {
                    return 1;
                }
                else if (firstString[i] < secondString[i])
                {
                    return -1;
                }

                // Если же символы равны, то цикл должен перейти на следующий символ.
                // Если равны последние символы, то сами строки равны.
                if (i == shortNameLength - 1)
                {
                    break;
                }
                else
                {
                    continue;
                }
            }

            if (firstString == "" && secondString != "")
                return 1;
            if (secondString == "" && firstString != "")
                return -1;

            return 0;
        }

        // Обработчики событий.
        private void AddInfoButton_Click(object sender, RoutedEventArgs e)
        {
            CreateOtherInfo();
        }

        private void RemoveInfoButton_Click(object sender, RoutedEventArgs e)
        {
            infoStPanel.Children.RemoveAt(1);

            addInfoButton.IsEnabled = true;
            removeInfoButton.IsEnabled = false;
        }
        
        private void Selection_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if(MainWindow.selectionActive)
            {
                SwitchSelection();
            }
        }

        private void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            TextBox txtBox = sender as TextBox;
            txtBox.CaretIndex = 0;
        }

        private void InfoTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            textBox.SelectAll();
            textBox.Copy();
        }
    }
}
