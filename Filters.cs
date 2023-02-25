using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Security.Cryptography.X509Certificates;
using System.Diagnostics;
using System.ComponentModel;
using System.IO;
using System.Windows.Forms;

namespace КГ_Лабораторная_работа__1
{
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

    class MatrixFilter : Filters
    {
        protected float[,] kernel = null;
        protected MatrixFilter() { }
        public MatrixFilter(float[,] kernel)
        {
            this.kernel = kernel;
        }

        // вычисление цвета пикселя на основании своих соседей
        protected override Color calculateNewPixelColor(Bitmap sourceImage, int x, int y)
        {
            int radiusX = kernel.GetLength(0) / 2;
            int radiusY = kernel.GetLength(1) / 2;

            float resultR = 0;
            float resultG = 0;
            float resultB = 0;
            for (int l = -radiusY; l <= radiusY; l++)
                for (int k = -radiusX; k <= radiusX; k++)
                {
                    int idX = Clamp(x + k, 0, sourceImage.Width - 1);
                    int idY = Clamp(y + l, 0, sourceImage.Height - 1);
                    Color neighborColor = sourceImage.GetPixel(idX, idY);
                    resultR += neighborColor.R * kernel[k + radiusX, l + radiusY];

                }

            return Color.FromArgb(Clamp((int)resultR, 0, 255), Clamp((int)resultG, 0, 255), Clamp((int)resultB, 0, 255));
        }
    }

    class SharpnessFilter : MatrixFilter
    {
        public SharpnessFilter()
        {
            int sizeX = 3;
            int sizeY = 3;
            kernel = new float[sizeX, sizeY];
            kernel[0, 0] = 0;
            kernel[0, 1] = -1;
            kernel[0, 2] = 0;
            kernel[1, 0] = -1;
            kernel[1, 1] = 5;
            kernel[1, 2] = -1;
            kernel[2, 0] = 0;
            kernel[2, 1] = -1;
            kernel[2, 2] = 0;

        }
    }

    class GaussianFilter : MatrixFilter
    {
        public void createGaussianKernel(int radius, float sigma)
        {
            int i, j;
            int size = 2 * radius + 1;
            kernel = new float[size, size];
            float norm = 0;

            for (i = -radius; i <= radius; i++)
                for (j = -radius; j <= radius; j++)
                {
                    kernel[i + radius, j + radius] = (float)(Math.Exp(-(i * i + j * j) / (sigma * sigma)));
                    norm += kernel[i + radius, j + radius];
                }

            for (i = 0; i < size; i++)
                for (j = 0; j < size; j++)
                    kernel[i, j] /= norm;
        }
        public GaussianFilter()
        {
            createGaussianKernel(3, 2);
        }
    }

    class BlurFilter : MatrixFilter
    {
        public BlurFilter()
        {
            int sizeX = 3;
            int sizeY = 3;
            kernel = new float[sizeX, sizeY];
            for (int i = 0; i < sizeX; i++)
                for (int j = 0; j < sizeY; j++)
                    kernel[i, j] = 1.0f / (float)(sizeX * sizeY);
        }
    }

    class GrayScaleFilter : Filters //ЧерноБелое
    {
        protected override Color calculateNewPixelColor(Bitmap sourceImage, int x, int y)
        {
            Color sourceColor = sourceImage.GetPixel(x, y);
            return Color.FromArgb((int)(0.36 * sourceColor.R + 0.53 * sourceColor.G + 0.11 * sourceColor.B), (int)(0.36 * sourceColor.R + 0.53 * sourceColor.G + 0.11 * sourceColor.B), (int)(0.36 * sourceColor.R + 0.53 * sourceColor.G + 0.11 * sourceColor.B));
        }
    }

    class Sepia : Filters //СЕПИЯ
    {
        protected override Color calculateNewPixelColor(Bitmap sourceImage, int x, int y)
        {
            Color sourceColor = sourceImage.GetPixel(x, y);
            int Intensity = (int)(0.36 * sourceColor.R + 0.53 * sourceColor.G + 0.11 * sourceColor.B);
            double k = 30;
            return Color.FromArgb(Clamp((int)(Intensity + 2 * k), 0, 255), Clamp((int)(Intensity + 0.5 * k), 0, 255), Clamp((int)(Intensity - 1 * k), 0, 255)); //k = ?
        }

    }

    class Brithness : Filters //Яркость
    {
        protected override Color calculateNewPixelColor(Bitmap sourceImage, int x, int y)
        {
            Color sourceColor = sourceImage.GetPixel(x, y);
            int k = 60;
            return Color.FromArgb(Clamp((int)(k + sourceColor.R), 0, 255), Clamp((int)(k + sourceColor.G), 0, 255), Clamp((int)(k + sourceColor.B), 0, 255));
        }

    }

    class LinearStreching : Filters // линейное растягивание
    {
        int localMax = 0;
        int localMin = 255;
        int count = 0;

        void FindMaxMin(Bitmap image)
        {
            if (count != 1)
            {
                Color PixelColor;
                for (int i = 0; i < image.Height; i++)
                {
                    for (int j = 0; j < image.Width; j++)
                    {
                        PixelColor = image.GetPixel(j, i);
                        if (PixelColor.R > localMax) localMax = PixelColor.R;
                        if (PixelColor.R < localMin) localMin = PixelColor.R;
                    }

                }
                count++;
            }

        }

        protected override Color calculateNewPixelColor(Bitmap sourceImage, int x, int y)
        {
            FindMaxMin(sourceImage);
            Color sourceColor = sourceImage.GetPixel(x, y);
            return Color.FromArgb(LinearFormula(sourceColor.R, localMax, localMin), LinearFormula(sourceColor.G, localMax, localMin), LinearFormula(sourceColor.B, localMax, localMin));
        }

        private int LinearFormula(int x, int Xmax, int Xmin) //Xmax and Xmin - максимальное значение "оттенка" из всего изображения.
        {
            return (x - Xmin) * (255 / (Xmax - Xmin));
        }
    }

    class MedianFilter : MatrixFilter
    {
        protected override Color calculateNewPixelColor(Bitmap sourceImage, int x, int y)
        {
            int count = 0;
            int size = 5;
            kernel = new float[size, size];
            int[] arrayR = new int[size * size];
            int[] arrayG = new int[size * size];
            int[] arrayB = new int[size * size];

            int radiusX = kernel.GetLength(0) / 2;
            int radiusY = kernel.GetLength(1) / 2;

            int resultR = 0;
            int resultG = 0;
            int resultB = 0;

            for (int l = -radiusY; l <= radiusY; l++)
                for (int k = -radiusX; k <= radiusX; k++)
                {
                    int idX = Clamp(x + k, 0, sourceImage.Width - 1);
                    int idY = Clamp(y + l, 0, sourceImage.Height - 1);
                    Color neighborColor = sourceImage.GetPixel(idX, idY);

                    arrayR[count] = neighborColor.R;
                    arrayG[count] = neighborColor.G;
                    arrayB[count] = neighborColor.B;
                    count++;
                }
            resultR = FindMedianValue(arrayR, count);
            resultG = FindMedianValue(arrayG, count);
            resultB = FindMedianValue(arrayB, count);

            return Color.FromArgb(resultR, resultG, resultB);
        }

        int FindMedianValue(int[] arr, int count)
        {
            Array.Sort(arr);
            return arr[(int)(count / 2)];
        }
    }

    class GreyWorld : Filters
    {
        double r_ = 0;
        double g_ = 0;
        double b_ = 0; //Это среднее всех значений одного цвета
        double Avg;
        int k = 0;
        int size;
        void FindAvarageValues(Bitmap image)
        {
            if (k != 1)
            {
                Color PixelColor;
                for (int i = 0; i < image.Height; i++)
                {
                    for (int j = 0; j < image.Width; j++)
                    {
                        PixelColor = image.GetPixel(j, i);
                        r_ += PixelColor.R;
                        g_ += PixelColor.G;
                        b_ += PixelColor.B;
                        ;
                    }

                }
                k++;
                size = image.Height * image.Width;
                r_ /= size;
                g_ /= size;
                b_ /= size;
                Avg = (r_ + g_ + b_) / 3;

            }

        }
        protected override Color calculateNewPixelColor(Bitmap sourceImage, int x, int y)
        {
            FindAvarageValues(sourceImage);
            Color sourceColor = sourceImage.GetPixel(x, y);
            int ReturnR = Clamp((int)(sourceColor.R * (Avg / r_)), 0, 255);
            int ReturnG = Clamp((int)(sourceColor.G * (Avg / g_)), 0, 255);
            int ReturnB = Clamp((int)(sourceColor.B * (Avg / b_)), 0, 255);
            return Color.FromArgb(ReturnR, ReturnG, ReturnB);
        }
    }

    class SobelFilter : MatrixFilter
    {
        private static double[,] xSobel
        {
            get
            {
                return new double[,]
                {
            { -1, 0, 1 },
            { -2, 0, 2 },
            { -1, 0, 1 }
                };
            }
        }
        private static double[,] ySobel
        {
            get
            {
                return new double[,]
                {
            {  1,  2,  1 },
            {  0,  0,  0 },
            { -1, -2, -1 }
                };
            }
        }

        protected override Color calculateNewPixelColor(Bitmap sourceImage, int x, int y)
        {

            int size = 3;
            kernel = new float[size, size]; //Не используется сетка по умолчанию, потому что алгоритм Собеля использует сразу две штуки
            int radiusX = kernel.GetLength(0) / 2;
            int radiusY = kernel.GetLength(1) / 2;

            double XresR = 0, XresG = 0, XresB = 0, YresR = 0, YresG = 0, YresB = 0;

            for (int l = -radiusY; l <= radiusY; l++)
                for (int k = -radiusX; k <= radiusX; k++)
                {
                    int idX = Clamp(x + k, 0, sourceImage.Width - 1);
                    int idY = Clamp(y + l, 0, sourceImage.Height - 1);
                    Color neighborColor = sourceImage.GetPixel(idX, idY);
                    XresR += neighborColor.R * xSobel[k + radiusX, l + radiusY];
                    XresG += neighborColor.G * xSobel[k + radiusX, l + radiusY];
                    XresB += neighborColor.B * xSobel[k + radiusX, l + radiusY];
                    YresR += neighborColor.R * ySobel[k + radiusX, l + radiusY];
                    YresG += neighborColor.G * ySobel[k + radiusX, l + radiusY];
                    YresB += neighborColor.B * ySobel[k + radiusX, l + radiusY];

                }

            int resultR, resultG, resultB;
            resultR = Clamp((int)Math.Sqrt(XresR * XresR + YresR * YresR), 0, 255);
            resultG = Clamp((int)Math.Sqrt(XresG * XresG + YresG * YresG), 0, 255);
            resultB = Clamp((int)Math.Sqrt(XresB * XresB + YresB * YresB), 0, 255);
            return Color.FromArgb(resultR, resultG, resultB);
        }


    }

    class ShalyaFilter : MatrixFilter
    {
        private static double[,] xShalya
        {
            get
            {
                return new double[,]
                {
            { 3, 0, -3 },
            { 10, 0, -10 },
            { 3, 0, -3 }
                };
            }
        }
        private static double[,] yShalya
        {
            get
            {
                return new double[,]
                {
            {  3,  10,  3 },
            {  0,  0,  0 },
            { -3, -10, -3 }
                };
            }
        }

        protected override Color calculateNewPixelColor(Bitmap sourceImage, int x, int y)
        {

            int size = 3;
            kernel = new float[size, size]; //Не используется сетка по умолчанию, потому что алгоритм Собеля использует сразу две штуки
            int radiusX = kernel.GetLength(0) / 2;
            int radiusY = kernel.GetLength(1) / 2;

            double XresR = 0, XresG = 0, XresB = 0, YresR = 0, YresG = 0, YresB = 0;

            for (int l = -radiusY; l <= radiusY; l++)
                for (int k = -radiusX; k <= radiusX; k++)
                {
                    int idX = Clamp(x + k, 0, sourceImage.Width - 1);
                    int idY = Clamp(y + l, 0, sourceImage.Height - 1);
                    Color neighborColor = sourceImage.GetPixel(idX, idY);
                    XresR += neighborColor.R * xShalya[k + radiusX, l + radiusY];
                    XresG += neighborColor.G * xShalya[k + radiusX, l + radiusY];
                    XresB += neighborColor.B * xShalya[k + radiusX, l + radiusY];
                    YresR += neighborColor.R * yShalya[k + radiusX, l + radiusY];
                    YresG += neighborColor.G * yShalya[k + radiusX, l + radiusY];
                    YresB += neighborColor.B * yShalya[k + radiusX, l + radiusY];

                }

            int resultR, resultG, resultB;
            resultR = Clamp((int)Math.Sqrt(XresR * XresR + YresR * YresR), 0, 255);
            resultG = Clamp((int)Math.Sqrt(XresG * XresG + YresG * YresG), 0, 255);
            resultB = Clamp((int)Math.Sqrt(XresB * XresB + YresB * YresB), 0, 255);
            return Color.FromArgb(resultR, resultG, resultB);
        }
    }

    class PryttFilter : MatrixFilter
    {
        private static double[,] xPrytt
        {
            get
            {
                return new double[,]
                {
            { -1, 0, 1 },
            { -1, 0, 1 },
            { -1, 0, 1 }
                };
            }
        }
        private static double[,] yPrytt
        {
            get
            {
                return new double[,]
                {
            {  -1,  -1,  -1 },
            {  0,  0,  0 },
            {  1,  1,  1 }
                };
            }
        }

        protected override Color calculateNewPixelColor(Bitmap sourceImage, int x, int y)
        {

            int size = 3;
            kernel = new float[size, size]; //Не используется сетка по умолчанию, потому что алгоритм Собеля использует сразу две штуки
            int radiusX = kernel.GetLength(0) / 2;
            int radiusY = kernel.GetLength(1) / 2;

            double XresR = 0, XresG = 0, XresB = 0, YresR = 0, YresG = 0, YresB = 0;

            for (int l = -radiusY; l <= radiusY; l++)
                for (int k = -radiusX; k <= radiusX; k++)
                {
                    int idX = Clamp(x + k, 0, sourceImage.Width - 1);
                    int idY = Clamp(y + l, 0, sourceImage.Height - 1);
                    Color neighborColor = sourceImage.GetPixel(idX, idY);
                    XresR += neighborColor.R * xPrytt[k + radiusX, l + radiusY];
                    XresG += neighborColor.G * xPrytt[k + radiusX, l + radiusY];
                    XresB += neighborColor.B * xPrytt[k + radiusX, l + radiusY];
                    YresR += neighborColor.R * yPrytt[k + radiusX, l + radiusY];
                    YresG += neighborColor.G * yPrytt[k + radiusX, l + radiusY];
                    YresB += neighborColor.B * yPrytt[k + radiusX, l + radiusY];

                }

            int resultR, resultG, resultB;
            resultR = Clamp((int)Math.Sqrt(XresR * XresR + YresR * YresR), 0, 255);
            resultG = Clamp((int)Math.Sqrt(XresG * XresG + YresG * YresG), 0, 255);
            resultB = Clamp((int)Math.Sqrt(XresB * XresB + YresB * YresB), 0, 255);
            return Color.FromArgb(resultR, resultG, resultB);
        }
    }

    class MaxFilter : MatrixFilter
    {

        protected override Color calculateNewPixelColor(Bitmap sourceImage, int x, int y)
        {
            int count = 0;
            int size = 5;
            kernel = new float[size, size];
            int[] arrayR = new int[size * size];
            int[] arrayG = new int[size * size];
            int[] arrayB = new int[size * size];

            int radiusX = kernel.GetLength(0) / 2;
            int radiusY = kernel.GetLength(1) / 2;

            int resultR = 0;
            int resultG = 0;
            int resultB = 0;

            for (int l = -radiusY; l <= radiusY; l++)
                for (int k = -radiusX; k <= radiusX; k++)
                {
                    int idX = Clamp(x + k, 0, sourceImage.Width - 1);
                    int idY = Clamp(y + l, 0, sourceImage.Height - 1);
                    Color neighborColor = sourceImage.GetPixel(idX, idY);

                    arrayR[count] = neighborColor.R;
                    arrayG[count] = neighborColor.G;
                    arrayB[count] = neighborColor.B;
                    count++;
                }
            resultR = FindMaxValue(arrayR, count);
            resultG = FindMaxValue(arrayG, count);
            resultB = FindMaxValue(arrayB, count);

            return Color.FromArgb(resultR, resultG, resultB);
        }

        int FindMaxValue(int[] arr, int count)
        {
            Array.Sort(arr);
            return arr[count - 1];
        }

    }
}



