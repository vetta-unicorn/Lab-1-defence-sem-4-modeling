using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Laba_1_Modeling_Defence
{
    public partial class Form1 : Form
    {
        double sigma = 1;
        double M = 2;
        int n = 500;
        int k;

        double[] normalY;

        public Form1()
        {
            InitializeComponent();

            double kk = 1.0 + Math.Log(Convert.ToDouble(n), 2.0);
            k = Convert.ToInt32(kk);
        }

        public double[] Normal()
        {
            Random r = new Random();

            // записываем массив с нормальным распределением
            int q = 100;
            double[] x = new double[n]; // отвечает за равномерное распределение

            double[] z = new double[n]; // отвечает за нормальное со стандартными мат ожиданием и дисперсией
            double[] y = new double[n]; // итоговое норм распределение с заданными параметрами
            for (int i = 0; i < n; i++)
            {
                x[i] = r.NextDouble();
                for (int j = 0; j < q - 1; j++)
                {
                    x[i] += r.NextDouble(); // возвращает равномерное [0, 1]
                }
                z[i] = (x[i] - q / 2) / (Math.Sqrt(q / 12));
                y[i] = z[i] * sigma + M;
            }

            return y;
        }

        public double[] MakeScale(double[] y)
        {
            double MaxValue = y.Max();
            double MinValue = y.Min();

            double[] scale = new double[k];
            double h = (MaxValue - MinValue) / k;

            for (int i = 0; i < k; i++)
            {
                scale[i] = 0;
                for (int j = 0; j < n; j++)
                {

                    if ((y[j] >= (MinValue + i * h)) && (y[j] <= (MinValue + (i + 1) * h)))
                    {
                        scale[i] += 1;
                    }
                }
                scale[i] = (scale[i] / (double)n) / h;
            }

            return scale;
        }

        public double SimilarTriangles(NormalDistributionArr arr, double r1)
        {
            double y_curr = -10000;

            for (int i = 0; i < arr.number - 1; i++)
            {
                if (r1 >= arr.F_y[i] && r1 <= arr.F_y[i + 1])
                {
                    double delta_y = arr.y[i + 1] - arr.y[i];
                    double delta_F_y = arr.F_y[i + 1] - arr.F_y[i];

                    y_curr = (delta_y * (r1 - arr.F_y[i])/delta_F_y) + arr.y[i];

                    break;
                }
            }

            return y_curr;
        }

        // нормальное распределение ЦПТ
        private void button1_Click(object sender, EventArgs e)
        {
            chart1.Series[0].Points.Clear(); // очищение гистограммы

            normalY = Normal();

            double[] scale = MakeScale(normalY);
            double MinValue = normalY.Min();
            double MaxValue = normalY.Max();
            double h = (MaxValue - MinValue) / k;

            // построение гистограммы 1
            for (int i = 0; i < k; i++)
            {
                chart1.Series[0].Points.AddXY(MinValue + h * i, scale[i]);
            }

        }


        // нормальное распределение кусочно-линейная аппроксимация
        private void button2_Click(object sender, EventArgs e)
        {
            chart2.Series[0].Points.Clear();

            double[] approximation = new double[n];

            Random r = new Random();

            NormalDistributionArr arr = new NormalDistributionArr();
            arr.SetDistr(M, sigma);
            bool Flag = false;

            int i = 0;
            while (i < n)
            {
                double r1 = r.NextDouble();
                for (int j = 0; j < arr.number; j++)
                {
                    if (r1 == arr.F_y[j])
                    {
                        approximation[i] = arr.y[j];
                        Flag = true;
                        break;
                    }
                }

                if (!Flag)
                {
                    double num = SimilarTriangles(arr, r1);
                    if (num == -10000)
                    {
                        continue;
                    }
                    else
                    {
                        approximation[i] = num;
                    }
                }

                Flag = false;
                i++;
            }

            double[] scale = MakeScale(approximation);
            double MinValue = approximation.Min();
            double MaxValue = approximation.Max();
            double h = (MaxValue - MinValue) / k;

            // построение гистограммы 2
            for (int j = 0; j < k; j++)
            {
                chart2.Series[0].Points.AddXY(MinValue + h * j, scale[j]);
            }

        }

    }

    public class NormalDistributionArr
    {
        public double[] y { get; set; }
        public double[] F_y { get; set; }
        public int number {  get; set; }

        public Microsoft.Office.Interop.Excel.Application _ex;


        public NormalDistributionArr()
        {
            number = 20;
            _ex = new Microsoft.Office.Interop.Excel.Application();
            y = new double[number];
            F_y = new double[number];
        }

        public void SetDistr(double M, double sigma)
        {
            double MinY = M - 3 * sigma;
            double MaxY = M + 3 * sigma;
            double step = (MaxY - MinY) / number;

            for (int i = 0; i < number; i++)
            {
                y[i] = MinY + i * step;
                F_y[i] = _ex.WorksheetFunction.NormDist(y[i], M, sigma, false);
            }
        }
    }
}
