using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Security.Cryptography.X509Certificates;
using System.Diagnostics;
using System.ComponentModel;

namespace КГ_Лабораторная_работа__1
{
    // каждый фильтр представляет отдельный класс, но имеет одинаковую функцию,
    // поэтому создадим абстрактный класс Filters
    abstract class Filters
    {
        // привести значение цвета к допустимому диапазону [0; 255]
        public int Clamp(int value, int min, int max)
        {
            if (value < min) // <= ?
                return min;
            else if (value > max) // >= ?
                return max;
            return value;
        }

        // ф-я, вычисляющая значение пикселя отфильтрованного изображения
        protected abstract Color calculateNewPixelColor(Bitmap sourceImage, int x, int y);

        public Bitmap processImage(Bitmap sourceImage, BackgroundWorker worker)
        {
            // создание пустого изображение того же размера, что и входное
            Bitmap resultImage = new Bitmap(sourceImage.Width, sourceImage.Height);

            // обход всех пикселей изображения
            for (int i = 0; i < sourceImage.Width; ++i)
            {
                worker.ReportProgress((int)((float)i / resultImage.Width * 100));
                if (worker.CancellationPending)
                    return null;

                for (int j = 0; j < sourceImage.Height; ++j)
                {
                    resultImage.SetPixel(i, j, calculateNewPixelColor(sourceImage, i, j));
                }
            }

            return resultImage;
        }
    }

    // наследник класса Filters
    class InvertFilter : Filters
    {
        // переопределение функции класса предка
        protected override Color calculateNewPixelColor(Bitmap sourceImage, int x, int y)
        {
            // вычисление цвета исходного пикселя
            Color sourceColor = sourceImage.GetPixel(x, y);
            // инварсия цвета пикселя
            Color resultColor = Color.FromArgb(255 - sourceColor.R, 255 - sourceColor.G, 255 - sourceColor.B);

            return resultColor;
        }
    }

}



