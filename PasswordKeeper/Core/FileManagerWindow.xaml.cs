using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.ComponentModel;

namespace PasswordKeeper.Core
{
    /// <summary>
    /// Логика взаимодействия для FileManagerWindow.xaml
    /// </summary>
    public partial class FileManagerWindow : Window
    {
        // Хранит ссылку на последний выделенный элемент ListBox.
        private ListBoxItem LastSelectedItem { get; set; }
        // Название элемента списка на момент выделения.
        private string OriginalName { get; set; }

        // Конструкторы.
        public FileManagerWindow()
        {
            InitializeComponent();

            FillList();

            for (int i = 0; i < fileList.Items.Count; i++)
            {
                if ((fileList.Items[i] as TextBox).Text == FileManager.CurrentFilePair.Key)
                {
                    fileList.SelectedIndex = i;
                    (fileList.Items[i] as TextBox).IsEnabled = true;

                    break;
                }
            }
        }

        // ----- Методы -----

        // Добавляет новый элемент в список файлов.
        private void CreateListItem(string name, bool isNew)
        {
            // Создаем и задаем базовые настройки TextBox.
            TextBox textBox = new TextBox();
            textBox.Style = (Style)Resources["ListBoxItemTextBoxStyle"];
            textBox.Text = name;
            textBox.IsEnabled = isNew;
            // Добавляем его в ListBox.
            fileList.Items.Add(textBox);

            // Получаем созданный элемент списка и приводим к TextBox,
            // чтобы добавить обработчики для получения/потери фокуса и загрузки элемента.
            var createdElem = (TextBox)fileList.Items[fileList.Items.Count - 1];
            createdElem.Loaded += ListTextBoxLoaded;
            createdElem.GotFocus += ListTextBoxGotFocus;
            createdElem.LostFocus += ListTextBoxLostFocus;
            // Так же если это только что созданный, а не загруженый элемент, то добавляем
            // еще один обработчик к Loaded.
            if (isNew)
                createdElem.Loaded += NewListTextBoxLoaded;
        }

        // Заполняет список с файлами сохранений.
        private void FillList()
        {
            foreach (var pair in FileManager.FileNamesAndAliases)
            {
                CreateListItem(pair.Key, false);
            }
        }

        // ----- Обработчики событий -----

        // Нажатие кнопки "добавить".
        private void AddButtonClick(object sender, RoutedEventArgs e)
        {
            // Убираем фокус клавиатуры со всех элементов для того, чтобы предотвратить добавление
            // второго ключа "" в словарь, если предыдущий элемент только что создан и никак не назван.
            // При потере фокуса тем элементом, ему присвоится имя "Unnamed" с соответствующим номером.
            Keyboard.ClearFocus();
            // Создаем новый файл сохранения.
            FileManager.CreateFile(false);
            // Создаем новый элемент списка,  который отображает файлы сохранения.
            CreateListItem("", true);
        }

        // Окончание загрузки TextBox`а в элементе списка.
        private void ListTextBoxLoaded(object sender, RoutedEventArgs e)
        {
            // Получаем ListBoxItem этого TextBox`а и устанавливаем ему обработчик для Selected.
            var thisContainer = (ListBoxItem)fileList.ContainerFromElement((TextBox)sender);
            thisContainer.Selected += ListTextBoxSelected;
        }

        // Окончание загрузки TextBox`а в новом элементе списка.
        private void NewListTextBoxLoaded(object sender, RoutedEventArgs e)
        {
            // Устанавливаем фокус клавиатуры на TextBox.
            Keyboard.Focus((IInputElement)sender);

            // Снимаем выделение со всех элементов.
            fileList.SelectedIndex = -1;
            // Выделяем только что созданный элемент.
            fileList.SelectedIndex = fileList.Items.Count - 1;
        }

        // Получение фокуса TextBox`ом в элементе списка.
        private void ListTextBoxGotFocus(object sender, RoutedEventArgs e)
        {
            // Сохраняем название элемента до возможного изменения.
            OriginalName = ((TextBox)sender).Text;
        }

        // Потеря фокуса TextBox`ом в элементе списка.
        private void ListTextBoxLostFocus(object sender, RoutedEventArgs e)
        {
            // Приводим sender к TextBox для доступа к свойству Text.
            var txtBox = (TextBox)sender;

            // Если значение свойства Text без пробелов равно пустой строке,
            // то присвоить ему значение Unnamed.
            if (txtBox.Text.Trim(' ') == "")
            {
                txtBox.Text = FileManager.GetUnnamedWithIdx();
            }

            // Заменяем псевдоним файла в FileAlias на новый.
            if (txtBox.Text != OriginalName)
            {
                string fileName = FileManager.FileNamesAndAliases[OriginalName];
                FileManager.FileNamesAndAliases.Remove(OriginalName);
                FileManager.FileNamesAndAliases.Add(txtBox.Text, fileName);

                if (OriginalName.Contains("Unnamed"))
                {
                    int unnamedNum = Int32.Parse(OriginalName.Substring(OriginalName.IndexOf(" ")));
                    FileManager.UnnamedFilesIdx.Remove(unnamedNum);
                }
            }

            // ???
            if (fileList.SelectedItem != null)
                (fileList.SelectedItem as TextBox).IsEnabled = false;
        }

        // Выделение элемента списка.
        private void ListTextBoxSelected(object sender, RoutedEventArgs e)
        {
            // Если LastSelectedItem не равен null, то запрещаем редактирование текста,
            // устанавливая значение свойства IsEnabled, находящегося внутри TextBox`а, равным false;
            if(LastSelectedItem != null)
            {
                (LastSelectedItem.Content as TextBox).IsEnabled = false;
            }

            // Получаем TextBox из свойства Content.
            var listItem = (ListBoxItem)sender;
            var txtBox = (TextBox)listItem.Content;
            // Делаем возможным редактирование текста.
            txtBox.IsEnabled = true;

            // Устанавливаем выделенный элемент, как последний выделенный элемент.
            LastSelectedItem = listItem;
        }

        // Нажатие кнопки выбора файла сохранения.
        private void SelectButtonClick(object sender, RoutedEventArgs e)
        {
            // Если какой-либо элемент списка выделен.
            if(fileList.SelectedItem != null)
            {
                // По ключу из элемента списка получаем имя файла и передаем его в параметр события.
                string key = (fileList.SelectedItem as TextBox).Text;
                string value = FileManager.FileNamesAndAliases[key];
                FileSelected(this, new KeyValuePair<string, string>(key, value));

                // Закрываем это окно.
                this.Close();
            }
        }

        // Нажатие кнопки "удалить".
        private void DeleteButtonClick(object sender, RoutedEventArgs e)
        {
            // Если элемент выделен и если выделенный элемент не открыт сейчас, то выводим
            // MessageBox предупреждающий об удалении.
            if (fileList.SelectedItem != null)
            {
                // Заголовок предупреждающего окна.
                string caption = "Password keeper";

                // Если выделенный элемент не тот, который открыт в данный момент.
                if ((fileList.SelectedItem as TextBox).Text != FileManager.CurrentFilePair.Key)
                {
                    string warningMessage = "You are trying to delete this save file. This action cannot be undone.\n\nAre you sure?";
                    MessageBoxResult result = MessageBox.Show(warningMessage, caption, MessageBoxButton.YesNo, MessageBoxImage.Warning);

                    // Если пользователь подтвердил действие, то удаляем файл сохранений и
                    // и связаный с ним элемент списка.
                    if (result == MessageBoxResult.Yes)
                    {
                        var txtBox = (TextBox)fileList.SelectedItem;
                        FileManager.DeleteFile(txtBox.Text);
                        fileList.Items.Remove(fileList.SelectedItem);

                        // Устанавливаем значение последнего выделенного элемента равным null.
                        LastSelectedItem = null;
                    }
                }
                // При попытке удалить открытый файл, выводим MessageBox с ошибкой.
                else
                {
                    string warningMessage = "Cannot delete opened file!";
                    MessageBox.Show(warningMessage, caption, MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        // Закрытие окна.
        private void FileManagerWindowClosing(object sender, CancelEventArgs e)
        {
            FileManager.SaveGlobalData();
        }

        // ----- События -----

        // Выбор файла сохранения.
        public event EventHandler<KeyValuePair<string, string>> FileSelected;
    }
}
