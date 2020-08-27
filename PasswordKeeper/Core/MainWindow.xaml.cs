using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.ComponentModel;

namespace PasswordKeeper.Core
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // Переменная для хранения ссылки на команду сохранения.
        public static RoutedCommand saveHotkey = new RoutedCommand();

        // Переменная для хранения состояния функции удаления элементов.
        public static bool selectionActive = false;

        // Динамический массив для хранения ссылок на все созданые пользователем InfoBox`ы. 
        private List<InfoBox> infoBoxes = new List<InfoBox>();
        // Динамический массив для хранения индексов всех совпадений при поиске.
        private List<int> idxOfEnabled = new List<int>();

        // Переменная для разрешения или запрета на поиск. 
        // В данный момент используется только для определения есть плейсхолдер или нет.
        private bool searchAllowed = false;
        // Переменная для проверки на сортированость массива InfoBox.
        private bool isSorted;
        // Есть ли несохраненные данные.
        private bool isDataSaved = true;

        // Переменная для хранения ссылки на окно менеджера файлов.
        private FileManagerWindow fileManagerWin;

        // Конструктор.
        public MainWindow()
        {
            InitializeComponent();

            // Назначаем команде горячие клавиши.
            saveHotkey.InputGestures.Add(new KeyGesture(Key.S, ModifierKeys.Control));

            // Загружаем информацию о файлах сохранений, и если нет ни одного такого файла,
            // то создаем его.
            FileManager.LoadGlobalData();
            if (FileManager.NumOfFiles == 0)
            {
                FileManager.CreateFile(true);
                FileManager.SaveGlobalData();
            }

            // Загружаем сохраненную пользователем информацию и формируем по ней интерфейс.
            LoadInterface(FileManager.CurrentFilePair);

            // После загрузки данные всегда отсортированы.
            isSorted = true;
        }

        // ----- Методы -----

        // Сохранение данных аккаунта.
        private void SaveData()
        {
            // Сортируем информацию перед сохранением.
            infoBoxes.Sort();

            // Вызываем метод сохранения данных.
            FileManager.SaveSortedData(infoBoxes);

            // Данные сохранены.
            isDataSaved = true;
        }
        
        // Построение интерфейса по загруженным данным.
        private void LoadInterface(KeyValuePair<string, string> fileName)
        {
            // Загрузка данных.
            List<string[]> info = FileManager.LoadInfo(fileName);

            // Добавляем InfoBox`ы на панель и в массив InfoBox`ов.
            if (info.Count != 0)
            {
                int numOfBoxes = Int32.Parse(info.Last()[0]);

                for (int i = 0; i < numOfBoxes; i++)
                {
                    AddInfoBox(new InfoBox(Resources, Application.Current.Resources, info[i]));
                }
            }
        }

        // Очистка интерфейса и связаных данных.
        private void InterfaceReset()
        {
            infoPanel.Children.Clear();
            infoBoxes.Clear();
        }

        // Сортировка элементов интерфейса.
        private void RealtimeSort()
        {
            infoBoxes.Sort();

            for (int i = 0; i < infoBoxes.Count; i++)
            {
                if(infoPanel.Children[i] != infoBoxes[i])
                {
                    infoPanel.Children.Remove(infoBoxes[i]);
                    infoPanel.Children.Insert(i, infoBoxes[i]);
                }
            }

            isSorted = true;
        }

        // Включает/отключает режим выделения.
        private void SwitchSelectionState()
        {
            foreach (var box in infoBoxes)
            {
                box.mainStPanel.IsEnabled = !box.mainStPanel.IsEnabled;
            }

            if (selectionActive)
            {
                trashButton.Visibility = Visibility.Visible;
                listButton.Visibility = Visibility.Visible;
                denyButton.Visibility = Visibility.Collapsed;
                okButton.Visibility = Visibility.Collapsed;
            }
            else
            {
                trashButton.Visibility = Visibility.Collapsed;
                listButton.Visibility = Visibility.Collapsed;
                denyButton.Visibility = Visibility.Visible;
                okButton.Visibility = Visibility.Visible;
            }

            selectionActive = !selectionActive;
        }

        // Добавляет новый элемент на панель.
        private void AddInfoBox(InfoBox iBox)
        {
            //iBox.NameChange += InfoBox_NameChange;
            iBox.nameTxtBox.TextChanged += InfoBox_NameChanged;
            iBox.nameTxtBox.TextChanged += InfoBox_DataChanged;
            iBox.loginTxtBox.TextChanged += InfoBox_DataChanged;
            iBox.passTxtBox.TextChanged += InfoBox_DataChanged;

            infoPanel.Children.Add(iBox);
            infoBoxes.Add(iBox);
        }

        // Записывает ошибки в лог файл.
        public static void Logger(Exception exception)
        {
            using (FileStream fs = new FileStream(AppDomain.CurrentDomain.BaseDirectory + "\\log.txt", FileMode.Append))
            {
                using (StreamWriter sw = new StreamWriter(fs))
                {
                    sw.WriteLine(exception.Message);
                }
            }
        }

        public static void Logger(Exception exception, string data)
        {
            using (FileStream fs = new FileStream(AppDomain.CurrentDomain.BaseDirectory + "\\log.txt", FileMode.Append))
            {
                using (StreamWriter sw = new StreamWriter(fs))
                {
                    sw.WriteLine(exception.Message);
                    sw.WriteLine($"Дополнительная информация: {data}");
                }
            }
        }
        
        // ----- Обработчики событий ----- 

        // Обработчики событий строки поиска.
        private void SearchTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            TextBox txtBx = sender as TextBox;

            if(txtBx.Text == "Type here...")
                txtBx.Text = "";
            txtBx.Foreground = Brushes.Black;

            searchAllowed = true;

            if(!isSorted)
                RealtimeSort();
        }

        private void SearchTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            TextBox txtBx = sender as TextBox;

            searchAllowed = false;

            if (txtBx.Text == "")
            {
                txtBx.Text = "Type here...";
                txtBx.Foreground = Brushes.LightGray;
            }
        }

        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Проверка на разрешение на поиск.
            if (!searchAllowed)
                return;

            // Приводим object-инициатор события к типу TextBox.
            TextBox txtBox = sender as TextBox;

            // При пустом значении TextBox, показать все элементы.
            if (txtBox.Text == "")
            {
                foreach (InfoBox box in infoBoxes)
                {
                    box.Visibility = Visibility.Visible;
                }

                return;
            }

            // Проходимся по коллекции изменений в TextBox.
            foreach (TextChange txtCh in e.Changes)
            {
                // Если было удалено или добавлено более нуля символов. Не уверен нужна ли эта проверка.
                if(txtCh.AddedLength > 0 || txtCh.RemovedLength > 0)
                {
                    // Границы подмассива, который будет появляться при делении главного массива и
                    // появляющихся в результате подмассивов.
                    int left = 0;
                    int right = infoBoxes.Count;

                    // Индекс центрального элемента массива.
                    int idx = left + (right - left) / 2;

                    while (left != right)
                    {
                        // Сравниваем текст из центрального элемента массива с текстом в поисковой строке
                        int compareValue = infoBoxes[idx].CompareTo(txtBox.Text);

                        // Если центральный элемент начинается с искомой строки.
                        if (compareValue == 0)
                        {
                            // Добавляем его в массив найденых подходящих элементов.
                            idxOfEnabled.Add(idx);

                            // Смотрим есть ли элементы начинающиеся с такой же строки слева и справа от элемента.
                            // Так же добавляем их в массив найденых подходящих элементов.
                            int nearIdx = idx;
                            while (nearIdx > left && infoBoxes[nearIdx - 1].CompareTo(txtBox.Text) == 0)
                            {
                                nearIdx--;
                                idxOfEnabled.Add(nearIdx);
                            }

                            nearIdx = idx;
                            while (nearIdx < right - 1 && infoBoxes[nearIdx + 1].CompareTo(txtBox.Text) == 0)
                            {
                                nearIdx++;
                                idxOfEnabled.Add(nearIdx);
                            }

                            // Т.к. искомые элементы найдены, искусственно создаем условие выхода из цикла.
                            right = left;
                        }
                        else
                        {
                            // Вызывающий больше.
                            if (compareValue > 0)
                            {
                                right = idx;
                            }
                            // Вызывающий меньше, т.к. на равенство уже проверили.
                            else
                            {
                                left = idx + 1;
                            }
                        }

                        // Вычисляем индекс среднего элемента нового подмассива.
                        idx = left + (right - left) / 2;
                    }
                }
            }

            // Сначала скрываем все InfoBox`ы, а затем показываем нужные по найденым индексам.
            foreach (InfoBox box in infoBoxes)
            {
                box.Visibility = Visibility.Collapsed;
            }
            foreach (int idx in idxOfEnabled)
            {
                infoBoxes[idx].Visibility = Visibility.Visible;
            }

            // Очищаем массив индексов искомых элементов.
            idxOfEnabled.Clear();
        }

        // Нажатие кнопки "добавить".
        private void AddBoxButton_Click(object sender, RoutedEventArgs e)
        {
            // Добавляем InfoBox.
            AddInfoBox(new InfoBox(Resources, Application.Current.Resources));
            // Так как с добавлением нового InfoBox`а данные больше не отсортированы
            // и не сохранены, устанавливаем значение переменных isSorted и isDataSaved равным false.
            isSorted = false;
            isDataSaved = false;
        }

        // Нажатие кнопки "удаление".
        private void TrashButton_Click(object sender, RoutedEventArgs e)
        {
            SwitchSelectionState();
        }

        // Нажатие на кнопку открытия окна фалов сохранений.
        private void ListButton_Click(object sender, RoutedEventArgs e)
        {
            // Создаем окно, добавляем обработчик и устанавливаем начальное положение.
            fileManagerWin = new FileManagerWindow();
            fileManagerWin.FileSelected += FileManagerWinFileSelected;
            fileManagerWin.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            fileManagerWin.ShowDialog();
        }

        // Нажатие кнопки "отмена" в режиме удаления.
        private void DenyButton_Click(object sender, RoutedEventArgs e)
        {
            foreach (var box in infoBoxes)
            {
                if(box.IsSelected)
                {
                    box.SwitchSelection();
                }
            }

            SwitchSelectionState();
        }

        // Нажатие кнопки "ok" в режиме удаления.
        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            // Идем с конца, потому что если идти с начала, то при удалении элемента из коллекции,
            // ее индексы сдвигаются и нарушается порядок удаления.
            for (int i = infoBoxes.Count - 1; i >= 0; i--)
            {
                if (infoBoxes[i].IsSelected)
                {
                    infoPanel.Children.Remove(infoBoxes[i]);
                    infoBoxes.RemoveAt(i);
                }
            }

            SwitchSelectionState();
        }

        // Нажатие кнопки открытия окна генератора паролей.
        private void RandButton_Click(object sender, RoutedEventArgs e)
        {
            PassGen passGenWindow = new PassGen();
            passGenWindow.ShowDialog();
        }

        // Выбор файла сохранений в FileManagerWindow.
        private void FileManagerWinFileSelected(object sender, KeyValuePair<string, string> e)
        {
            // Если данные из предыдущего файла не сохранены, предложить сохранить.
            if(!isDataSaved)
            {
                string warningMessage = "Do you want to save the changes?";
                string caption = "File Manager";
                MessageBoxResult result = MessageBox.Show(warningMessage, caption, MessageBoxButton.YesNo, MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                    SaveData();
            }

            // Заполняем интерфейс данными из только что открытого файла.
            InterfaceReset();
            LoadInterface(e);

            // Устанавливаем значение переменной true, т.к. в только что загруженных данных нет
            // не сохраненных изменений.
            isDataSaved = true;
        }

        // Изменение названия InfoBox`a.
        private void InfoBox_NameChanged(object sender, TextChangedEventArgs e)
        {
            isSorted = false;
        }

        // Изменение любого поля InfoBox`a.
        private void InfoBox_DataChanged(object sender, TextChangedEventArgs e)
        {
            isDataSaved = false;
        }

        // Нажатие хоткея для сохранения данных.
        private void SaveHotkey_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            SaveData();
        }

        // Нажатие пункта "save" в верхнем меню.
        private void MenuItemSave_Click(object sender, RoutedEventArgs e)
        {
            SaveData();
        }

        // Нажатие пункта "quit" в верхнем меню.
        private void MenuItemQuit_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        
        // Нажатие пункта "about" в верхнем меню.
        private void MenuItemAboutClick(object sender, RoutedEventArgs e)
        {
            AboutWindow aboutW = new AboutWindow();
            aboutW.ShowDialog();
        }

        // Закрытие окна.
        private void MainWindowClosing(object sender, CancelEventArgs e)
        {
            // Если есть несохраненные данные, то выводим MessageBox, в котором спрашиваем
            // сохранить ли данные перед выходом. Так же можно отменить закрытие.
            if (!isDataSaved)
            {
                string warningMessage = "Do you want to save the changes?";
                string caption = "Password keeper";
                MessageBoxResult result = MessageBox.Show(warningMessage, caption, MessageBoxButton.YesNoCancel, MessageBoxImage.Warning);

                switch(result)
                {
                    case MessageBoxResult.Yes:
                        SaveData();
                        break;
                    case MessageBoxResult.Cancel:
                        e.Cancel = true;
                        break;
                }
            }
        }
    }
}
