using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace PasswordKeeper.Core
{
    class AlignableWarpPanel : Panel
    {
        protected override Size MeasureOverride(Size availableSize)
        {
            // Коллекция дочерних объектов панели.
            UIElementCollection children = InternalChildren;

            if (InternalChildren.Count != 0)
            {
                // Суммарная ширина элементов.
                // Нужна чтобы сравнивать с шириной панели и узнавать когда следующий элемент не влезет.
                double summaryWidth = 0d;
                // Высота самого высокого элемента в ряду.
                double maxHeight = 0d;
                // Нужная высота контейнера, высчитываемая на основе кол-ва рядов
                // и самого высокого элемента в них.
                double totalHeight = 0d;

                foreach (UIElement child in children)
                {
                    // ???
                    child.Measure(availableSize);
                    // Увеличиваем суммарную ширину элементов.
                    summaryWidth += child.DesiredSize.Width;

                    if (child.DesiredSize.Height > maxHeight)
                    {
                        maxHeight = child.DesiredSize.Height;
                    }
                    if (summaryWidth + child.DesiredSize.Width > availableSize.Width || children[children.Count - 1] == child)
                    {
                        // Увеличиваем высоту панели на величину самого высокого элемента в ряду.
                        totalHeight += maxHeight;
                        // Обнуляем общую ширину элементов и высоту самого высокого элемента в ряду.
                        summaryWidth = 0d;
                        maxHeight = 0d;
                    }
                }

                
                return new Size(availableSize.Width, totalHeight);
            }
            else
            {
                return new Size(availableSize.Width, 0);
            }
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            if (InternalChildren.Count != 0)
            {
                // Коллекция видимых дочерних элементов панели.
                List<UIElement> children = new List<UIElement>();
                // Из всех дочерних элементов выбираем только видимые.
                foreach (UIElement child in InternalChildren)
                {
                    if (child.Visibility == Visibility.Visible)
                        children.Add(child);
                }

                // Координаты размещения объекта внутри панели.
                double x = 0d, y = 0d;
                // Суммарная ширина элементов.
                // Нужна чтобы сравнивать с шириной панели и узнавать когда следующий элемент не влезет.
                double summaryWidth = 0d;
                // Высота самого высокого элемента в ряду.
                double maxHeight = 0d;
                // Сколько элементов в ряду.
                int lineLength = 0;
                // Номер текущего элемента из коллекции дочерних объектов панели.
                int currentElement = 0;

                foreach (UIElement child in children)
                {
                    // Инкрементируем кол-во элементов в ряду.
                    lineLength++;
                    // Увеличиваем суммарную ширину элементов.
                    summaryWidth += child.DesiredSize.Width;

                    // Если высота текущего элемента больше максимальной высоты в ряду,
                    // заменить значение максимальной высоты на высоту текущего элемента.
                    if (child.DesiredSize.Height > maxHeight)
                    {
                        maxHeight = child.DesiredSize.Height;
                    }

                    if (summaryWidth + child.DesiredSize.Width > finalSize.Width || children[children.Count - 1] == child)
                    {
                        // Сколько пустого места остается.
                        double emptySpace = finalSize.Width - summaryWidth;
                        // На основе пустого места считаем нужный размер промежутков между элементами.
                        double gap = emptySpace / (lineLength + 1);
                        // Х координата первого элемента сразу с учетом промежутка.
                        x = gap;

                        for (int i = 0; i < lineLength; i++)
                        {
                            // Располагаем элемент.
                            Rect rct = new Rect(x, y, children[currentElement].DesiredSize.Width, children[currentElement].DesiredSize.Height);
                            children[currentElement].Arrange(rct);

                            // Если элемент не один, то меняем Х с учетом размера промежутка и размера элемента.
                            if (summaryWidth != child.DesiredSize.Width)
                            {
                                x += gap;
                                x += children[currentElement].DesiredSize.Width;
                            }

                            // Инкрементируем счетчик номера элемента.
                            currentElement++;
                        }

                        // Добавляем к Y переменной максимальную высоту элемента в ряду.
                        y += maxHeight;

                        // Обнуляем значения переменных важных только для этого ряда.
                        summaryWidth = 0d;
                        lineLength = 0;
                        x = 0d;
                        maxHeight = 0d;
                    }
                }
            }

            return finalSize;
        }
    }
}
