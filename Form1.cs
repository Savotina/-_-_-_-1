using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace КГ_Лабораторная_работа__1
{
    public partial class Form1 : Form
    {
        Bitmap image;
        public Form1()
        {

            InitializeComponent();
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }

        private void открытьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // создаём диалог для открытия файла
            OpenFileDialog dialog = new OpenFileDialog();

            // фильтр для удобства открытия только изображений (других файлов не видно)
            dialog.Filter = "Image files | *.png; *.jpg; *.bmp | All files (*.*) | *.*";

            // условие для проверки "выбрал ли пользователь файл?"
            if (dialog.ShowDialog() == DialogResult.OK) 
            {
                // в случ. выполнения инициализируем переменную image выбранным изображением
                image = new Bitmap(dialog.FileName);

                // после загрузки картинки в программу нужно визуализировать её на форме. Для этого:
                // 1) image присвоим свойству pictureBox.Image
                pictureBox1.Image = image;
                // 2) обновим pictureBox
                pictureBox1.Refresh();
            }
        }

        private void инверсияToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // изменение ф-и вызова инверсии, чтобы фильтр запускался в отдельном потоке
            Filters filter = new InvertFilter();
            backgroundWorker1.RunWorkerAsync(filter);

            // инициализируем объект класса значением по умолчанию
            //InvertFilter filter = new InvertFilter();
            // создаём экземпляр класса для изменнёного фильтром изображени
            // присваиваем ему результат функции
            //Bitmap resultImage = filter.processImage(image, backgroundWorker1);
            //pictureBox1.Image = resultImage;
            //pictureBox1.Refresh();
        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            Bitmap newImage = ((Filters)e.Argument).processImage(image, backgroundWorker1);
            if (backgroundWorker1.CancellationPending != true)
                image = newImage;
        }

        private void backgroundWorker1_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            progressBar1.Value = e.ProgressPercentage;
        }

        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (!e.Cancelled)
            {
                pictureBox1.Image = image;
                pictureBox1.Refresh();
            }
            progressBar1.Value = 0;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            // остановка выполнения фильтра
            backgroundWorker1.CancelAsync();
        }

        private void размытиеToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Filters filter = new BlurFilter();
            backgroundWorker1.RunWorkerAsync(filter);
        }

        private void чернобелоеToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Filters filter = new GrayScaleFilter();
            backgroundWorker1.RunWorkerAsync(filter);
        }

        private void точечныеToolStripMenuItem_Click(object sender, EventArgs e)
        {
            
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void серпияToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Filters filter = new Sepia();
            backgroundWorker1.RunWorkerAsync(filter);
        }

        private void яркостьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Filters filter = new Brithness();
            backgroundWorker1.RunWorkerAsync(filter);
        }

        private void резкостьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Filters filter = new SharpnessFilter();
            backgroundWorker1.RunWorkerAsync(filter);
        }

        private void размытиеГауссаToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Filters filter = new GaussianFilter();
            backgroundWorker1.RunWorkerAsync(filter);
        }

        private void progressBar1_Click(object sender, EventArgs e)
        {

        }
    }

    

}