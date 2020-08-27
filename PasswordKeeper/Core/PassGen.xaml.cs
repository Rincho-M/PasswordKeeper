using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace PasswordKeeper.Core
{
    /// <summary>
    /// Логика взаимодействия для PassGen.xaml
    /// </summary>
    public partial class PassGen : Window
    {
        // Различные наборы символов для генерации.
        string nums = "1234567890";
        string lowerL = "abcdefghijklmnopqrstuvwxyz";
        string upperL = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        string symb = "-_";

        // Переменные для хранения значения CheckBox.
        bool isNum = false;
        bool isBig = false;
        bool isSymb = false;

        // Конструктор.
        public PassGen()
        {
            InitializeComponent();
            lengthTxtBx.PreviewTextInput += new TextCompositionEventHandler(InputControl);
        }

        // Копирует пароль из TextBox при фокусе.
        private void passTxtBox_GotFocus(object sender, RoutedEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            textBox.SelectAll();
            textBox.Copy();
        }

        // Генерирует пароль.
        private void genButton_Click(object sender, RoutedEventArgs e)
        {
            // Объект для случайного выбора символов.
            Random rand = new Random();
            // Объявляем и инициализируем пустой строкой переменную для пароля.
            string password = "";

            // Получаем желаемую длину из соответствующего TextBox.
            int length;
            if (lengthTxtBx.Text != "")
            {
                length = int.Parse(lengthTxtBx.Text);
            }
            else
            {
                length = 0;
            }

            // Получаем значение CheckBox`ов.
            // Если значение равно null, то считаем это за false.
            isNum = isNumBox.IsChecked ?? false;
            isBig = isUpperBox.IsChecked ?? false;
            isSymb = isSymbBox.IsChecked ?? false;

            // Алгоритм случайной генерации пароля в соответствии с условиями.
            for (int i = 0; i < length; i++)
            {
                string tempstr = "";

                tempstr += lowerL[rand.Next(0, lowerL.Length)];
                if (isNum)
                    tempstr += nums[rand.Next(0, nums.Length)];
                if (isBig)
                    tempstr += upperL[rand.Next(0, upperL.Length)];
                if (isSymb)
                    tempstr += symb[rand.Next(0, symb.Length)];

                if (!isNum && !isSymb && !isBig)
                    password += tempstr;
                else
                    password += tempstr[rand.Next(0, tempstr.Length)];
            }

            // Задаем TextBox сгенерированный пароль.
            passTxtBx.Text = password;
        }

        // Не дает вводить какие-либо символы кроме цифр в поле с длиной пароля. 
        private void InputControl(object sender, TextCompositionEventArgs e)
        {
            if (!char.IsDigit(e.Text, 0))
                e.Handled = true;
        }
        
        // Обработчики для работы чекбоксов при нажатии на их текст.
        private void NumLabel_MouseUp(object sender, MouseButtonEventArgs e)
        {
            isNumBox.IsChecked = !isNumBox.IsChecked;
        }

        private void SymbLabel_MouseUp(object sender, MouseButtonEventArgs e)
        {
            isSymbBox.IsChecked = !isSymbBox.IsChecked;
        }

        private void UpperLabel_MouseUp(object sender, MouseButtonEventArgs e)
        {
            isUpperBox.IsChecked = !isUpperBox.IsChecked;
        }
    }
}
