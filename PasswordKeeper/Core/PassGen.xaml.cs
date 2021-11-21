using PasswordKeeper.Core.Utility;
using PasswordKeeper.Core.Models;
using System;
using System.Collections.Generic;
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
        PasswordGenerator passwordGen;

        public PassGen()
        {
            InitializeComponent();
            lengthTxtBx.PreviewTextInput += new TextCompositionEventHandler(InputControl);

            passwordGen = new PasswordGenerator();
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
            // Получить желаемую длину пароля из TextBox.
            int passwordLength = 0;
            if (lengthTxtBx.Text != "")
            {
                passwordLength = int.Parse(lengthTxtBx.Text);
            }

            bool isNum = isNumBox.IsChecked.HasValue ? isNumBox.IsChecked.Value : false;
            bool isUpper = isUpperBox.IsChecked.HasValue ? isUpperBox.IsChecked.Value : false;
            bool isSymbol = isSymbBox.IsChecked.HasValue ? isSymbBox.IsChecked.Value : false;
            var passwordType = new PasswordTypeModel(isNum, isUpper, isSymbol);

            string password = passwordGen.Generate(passwordLength, passwordType);

            // Задать TextBox сгенерированный пароль.
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
