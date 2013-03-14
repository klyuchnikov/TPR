using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Text.RegularExpressions;

namespace Lab3
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            PaintCoordinates();
        }

        private bool isMove;
        private Point CurrentPoint;
        private List<Equation> Equations = new List<Equation>();
        private void canvas1_MouseDown(object sender, MouseButtonEventArgs e)
        {
            isMove = true;
            CurrentPoint = Mouse.GetPosition(grid1);
        }

        private void canvas1_MouseMove(object sender, MouseEventArgs e)
        {
            if (!isMove) return;
            var NewPoint = Mouse.GetPosition(grid1);
            var dxy = CurrentPoint - NewPoint;
            CurrentPoint = NewPoint;
            var dx = dxy.X / grid1.ActualWidth / 10;
            var dy = dxy.Y / grid1.ActualHeight / 10;
            var newP = canvas1.RenderTransformOrigin + new Vector(dx, dy);
            if (newP.X <= 0)
                newP.X = 0;
            if (newP.Y <= 0)
                newP.Y = 0;
            if (newP.X >= 1)
                newP.X = 1;
            if (newP.Y >= 1)
                newP.Y = 1;
            canvas1.RenderTransformOrigin = newP;
        }

        private void canvas1_MouseUp(object sender, MouseButtonEventArgs e)
        {
            isMove = false;
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            var str = conditionTB.Text.Replace("\r", "").Split('\n');
            foreach (var s in str)
            {
                var m = Regex.Match(s, @"(?<c1>-?\d+)(?<cond1>(<=|<))(?<exp>(?<a>-?\d*)x(?<b>(\-|\+)\d*)y)(?<cond2>(<=|<))(?<c2>-?\d+)");
                if (m.Value != "")
                {
                    PaintNormalLine(m.Groups["exp"].Value + m.Groups["cond2"].Value + m.Groups["c2"].Value);
                    PaintNormalLine(m.Groups["exp"].Value + m.Groups["cond1"].Value.Replace("<", ">") + m.Groups["c1"].Value);
                    conditionTB.Text = "";
                    continue;
                }
                m = Regex.Match(s, @"(?<exp>(?<a>-?\d*)x(?<b>(\-|\+)\d*)y)(?<cond>(>=|=|<=|<|>))(?<c>-?\d+)");
                if (m.Value != "")
                {
                    PaintNormalLine(m.Groups["exp"].Value + m.Groups["cond"].Value + m.Groups["c"].Value);
                    conditionTB.Text = "";
                    continue;
                }
                m = Regex.Match(s, @"(?<c1>-?\d+)(?<cond1>(<=|<))(?<exp>(?<a>-?\d*)(x|y))(?<cond2>(<=|<))(?<c2>-?\d+)");
                if (m.Value != "")
                {
                    PaintParallelLine(m.Groups["exp"].Value + m.Groups["cond2"].Value + m.Groups["c2"].Value);
                    PaintParallelLine(m.Groups["exp"].Value + m.Groups["cond1"].Value.Replace("<", ">") + m.Groups["c1"].Value);
                    conditionTB.Text = "";
                    continue;
                }
                m = Regex.Match(s, @"(?<exp>(?<a>-?\d*)(x|y))(?<cond>(>=|=|<=|<|>))(?<c>-?\d+)");
                if (m.Value != "")
                {
                    PaintParallelLine(m.Groups["exp"].Value + m.Groups["cond"].Value + m.Groups["c"].Value);
                    conditionTB.Text = "";
                    continue;
                }
                MessageBox.Show("Выражение '" + s + "' содержит ошибки.");
            }

        }
        private void PaintParallelLine(string str)
        { // дописать функцию разбора и рисовки x<5
            listBox1.Items.Add(str);
            var m = Regex.Match(str, @"(?<exp>(?<a>-?\d*)(x|y))(?<cond>(>=|=|<=|<|>))(?<c>-?\d+)");
            double a = 0, c = 0;
            var exp = m.Groups["exp"].Value;
            a = double.Parse(m.Groups["a"].Value == "" ? "1" : m.Groups["a"].Value == "-" ? "-1" : m.Groups["a"].Value);
            c = double.Parse(m.Groups["c"].Value);
            var cond = m.Groups["cond"].Value;
            var pX = new Point(0, c / a);
            var pY = new Point(c / a, 0);
            Equations.Add(exp.Contains("x")
                              ? new Equation() { a = a, b = 0, c = c }
                              : new Equation() { a = 0, b = a, c = c });
            if (cond.Contains("="))
            {
                Line line;
                if (exp.Contains("x"))
                    line = new Line
                    {
                        X1 = c / a * scale,
                        Y1 = maxX * scale,
                        X2 = c / a * scale,
                        Y2 = -maxX * scale,
                        StrokeThickness = 0.5,
                        Stroke = Brushes.MediumVioletRed
                    };
                else
                    line = new Line
                    {
                        X1 = (maxX) * scale,
                        Y1 = c / a * -scale,
                        X2 = -maxX * scale,
                        Y2 = c / a * -scale,
                        StrokeThickness = 0.5,
                        Stroke = Brushes.MediumVioletRed
                    };
                canvas1.Children.Add(line);
                Canvas.SetLeft(line, maxX * scale);
                Canvas.SetTop(line, maxY * scale);
            }
            if (cond.Contains(">") || cond.Contains("<"))
            {
                var polyline = new Polyline()
                {
                    StrokeThickness = 1,
                    Stroke = null,
                    Fill = Brushes.LightSeaGreen,
                    Opacity = 0.1
                };
                if (exp.Contains("x"))
                {
                    polyline.Points.Add(new Point(c / a * scale, maxX * scale));
                    polyline.Points.Add(new Point(c / a * scale, -maxX * scale));
                }
                else
                {
                    polyline.Points.Add(new Point(maxX * scale, c / a * -scale));
                    polyline.Points.Add(new Point(-maxX * scale, c / a * -scale));
                }
                if (a > 0)
                {
                    polyline.Points.Add(new Point((-maxX) * scale, (-maxY) * scale)); // верхняя левая
                    if (cond.Contains(">"))
                        polyline.Points.Add(new Point((maxX) * scale, (maxY) * -scale)); // верхняя правая
                    if (cond.Contains("<"))
                        polyline.Points.Add(new Point((-maxX) * scale, (-maxY) * -scale)); // нижняя левая
                    polyline.Points.Add(new Point((maxX) * scale, (-maxY) * -scale)); // нижняя правая
                }
                if (a < 0)
                {
                    polyline.Points.Add(new Point((-maxX) * scale, (-maxY) * -scale)); // нижняя левая
                    if (cond.Contains(">"))
                        polyline.Points.Add(new Point((-maxX) * scale, (-maxY) * scale)); // верхняя левая
                    if (cond.Contains("<"))
                        polyline.Points.Add(new Point((maxX) * scale, (-maxY) * -scale)); // нижняя правая
                    polyline.Points.Add(new Point((maxX) * scale, (maxY) * -scale)); // верхняя правая
                }
                canvas1.Children.Add(polyline);
                Canvas.SetLeft(polyline, maxX * scale);
                Canvas.SetTop(polyline, maxY * scale);
            }
        }

        private void PaintNormalLine(string str)
        {
            listBox1.Items.Add(str);
            var m = Regex.Match(str, @"(?<a>-?\d*)x(?<b>(\-|\+)\d*)y(?<cond>(>=|=|<=|<|>))(?<c>-?\d+)");
            var a = double.Parse(m.Groups["a"].Value == "" ? "1" : m.Groups["a"].Value == "-" ? "-1" : m.Groups["a"].Value);
            var b =
                double.Parse(m.Groups["b"].Value == ""
                                 ? "1"
                                 : m.Groups["b"].Value == "-" ? "-1" : m.Groups["b"].Value == "+" ? "1" : m.Groups["b"].Value);
            var c = double.Parse(m.Groups["c"].Value);
            var cond = m.Groups["cond"].Value;
            Equations.Add(new Equation() { a = a, b = b, c = c });
            if (cond.Contains("="))
            {
                var line = new Line()
                               {
                                   X1 = (maxX) * scale,
                                   Y1 = ((c - a * maxX) / b) * -scale,
                                   X2 = (-maxX) * scale,
                                   Y2 = ((c - a * -maxX) / b) * -scale,
                                   StrokeThickness = 0.5,
                                   Stroke = Brushes.MediumVioletRed
                               };
                canvas1.Children.Add(line);
                Canvas.SetLeft(line, maxX * scale);
                Canvas.SetTop(line, maxY * scale);
            }
            if (cond.Contains(">") || cond.Contains("<"))
            {
                var polyline = new Polyline()
                                   {
                                       StrokeThickness = 1,
                                       Stroke = null,
                                       Fill = Brushes.LightSeaGreen,
                                       Opacity = 0.1
                                   };
                polyline.Points.Add(new Point((maxX) * scale, ((c - a * maxX) / b) * -scale));
                polyline.Points.Add(new Point((-maxX) * scale, ((c - a * -maxX) / b) * -scale));
                if (a * b > 0)
                {
                    polyline.Points.Add(new Point((-maxX) * scale, (-maxY) * scale)); // верхняя левая
                    if (cond.Contains(">"))
                        polyline.Points.Add(new Point((maxX) * scale, (maxY) * -scale)); // верхняя правая
                    if (cond.Contains("<"))
                        polyline.Points.Add(new Point((-maxX) * scale, (-maxY) * -scale)); // нижняя левая
                    polyline.Points.Add(new Point((maxX) * scale, (-maxY) * -scale)); // нижняя правая
                }
                if (a * b < 0)
                {
                    polyline.Points.Add(new Point((-maxX) * scale, (-maxY) * -scale)); // нижняя левая
                    if ((cond.Contains(">") && a < 0 && b > 0) || (cond.Contains("<") && a > 0 && b < 0))
                        polyline.Points.Add(new Point((-maxX) * scale, (-maxY) * scale)); // верхняя левая
                    else
                        polyline.Points.Add(new Point((maxX) * scale, (-maxY) * -scale)); // нижняя правая
                    polyline.Points.Add(new Point((maxX) * scale, (maxY) * -scale)); // верхняя правая
                }
                canvas1.Children.Add(polyline);
                Canvas.SetLeft(polyline, maxX * scale);
                Canvas.SetTop(polyline, maxY * scale);
            }
        }

        double maxX = 10;
        double maxY = 10;
        int scale = 50;
        private void PaintCoordinates()
        {
            canvas1.Width = maxX * 20 * scale;
            canvas1.Height = maxY * 30 * scale;
            var StrokeThickness = 1 * (scale / maxX * 2 / 100);
            var lY = new Line() { X1 = 0, X2 = 0, Y1 = maxY * scale, Y2 = -maxY * scale, StrokeThickness = StrokeThickness, Stroke = Brushes.Black };
            var lX = new Line() { X1 = -maxX * scale, X2 = maxX * scale, Y1 = 0, Y2 = 0, StrokeThickness = StrokeThickness, Stroke = Brushes.Black };
            canvas1.Children.Add(lY);
            canvas1.Children.Add(lX);
            Canvas.SetLeft(lY, maxX * scale);
            Canvas.SetTop(lY, maxY * scale);
            Canvas.SetLeft(lX, maxX * scale);
            Canvas.SetTop(lX, maxY * scale);
            var ll = scale / maxY;
            var font = (scale / maxY);
            for (int i = (int)-maxX; i <= maxX; i++)
            {
                var lsX = new Line()
                              {
                                  X1 = i * scale,
                                  X2 = i * scale,
                                  Y1 = -ll,
                                  Y2 = ll,
                                  StrokeThickness = StrokeThickness / 2,
                                  Stroke = Brushes.Black
                              };
                var textX = new TextBlock() { Text = i.ToString(), FontSize = font };
                canvas1.Children.Add(textX);
                Canvas.SetLeft(textX, maxY * scale + i * scale + 1);
                Canvas.SetTop(textX, maxX * scale);
                canvas1.Children.Add(lsX);
                Canvas.SetLeft(lsX, maxY * scale);
                Canvas.SetTop(lsX, maxX * scale);

                var lsY = new Line()
                              {
                                  Y1 = i * scale,
                                  Y2 = i * scale,
                                  X1 = -ll,
                                  X2 = ll,
                                  StrokeThickness = StrokeThickness / 2,
                                  Stroke = Brushes.Black
                              };
                var textY = new TextBlock() { Text = i.ToString(), FontSize = font };
                canvas1.Children.Add(textY);
                Canvas.SetTop(textY, maxY * scale - i * scale);
                Canvas.SetLeft(textY, maxX * scale + 1);
                canvas1.Children.Add(lsY);
                Canvas.SetTop(lsY, maxY * scale);
                Canvas.SetLeft(lsY, maxX * scale);
            }
        }

        private void button2_Click(object sender, RoutedEventArgs e)
        {
            canvas1.Children.Clear();
            listBox1.Items.Clear();
            Equations.Clear();
            PaintCoordinates();
        }

        private void canvas1_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            var d = e.Delta / 1200.0;
            slider1.Value += d;
        }

        private void goal_Click(object sender, RoutedEventArgs e)
        {

            var m = Regex.Match(goal.Text, @"(?<exp>(?<a>-?\d*)x(?<b>(\-|\+)\d*)y)");
            if (m.Value == "")
            {
                MessageBox.Show("Выражение содержит ошибки.");
                goal.Text = "";
                return;
            }
            goal_st.Visibility = Visibility.Collapsed;
            goal_stClose.Visibility = Visibility.Visible;
            goal_text.Text = "f = " + goal.Text + " --> ";
            goal_text.Text += goal_max.IsChecked.Value ? "max" : "min";

            var a = double.Parse(m.Groups["a"].Value == "" ? "1" : m.Groups["a"].Value == "-" ? "-1" : m.Groups["a"].Value);
            var b =
                double.Parse(m.Groups["b"].Value == ""
                                 ? "1"
                                 : m.Groups["b"].Value == "-" ? "-1" : m.Groups["b"].Value == "+" ? "1" : m.Groups["b"].Value);
            var line = new Line()
            {
                X1 = 0,
                Y1 = 0,
                X2 = (3 * a) * scale,
                Y2 = (3 * b) * -scale,
                StrokeThickness = 0.5,
                Stroke = Brushes.OrangeRed,
                StrokeEndLineCap = PenLineCap.Round
            };
            canvas1.Children.Add(line);
            Canvas.SetLeft(line, maxX * scale);
            Canvas.SetTop(line, maxY * scale);
            var ellipse = new Ellipse() { Stroke = Brushes.OrangeRed, Fill = Brushes.OrangeRed, Width = scale / 10, Height = scale / 10 };
            canvas1.Children.Add(ellipse);
            Canvas.SetLeft(ellipse, maxX * scale + (3 * a) * scale - scale / 20);
            Canvas.SetTop(ellipse, maxY * scale + (3 * b) * -scale - scale / 20);

            var arr = listBox1.Items.OfType<string>().ToArray();
            foreach (var s in arr)
            {
                m = Regex.Match(s, @"(?<exp>(?<a>-?\d*)x(?<b>(\-|\+)\d*)y)(?<cond>(>=|=|<=|<|>))(?<c>-?\d+)");
                if (m.Value != "")
                {
                    continue;
                }
                m = Regex.Match(s, @"(?<exp>(?<a>-?\d*)(x|y))(?<cond>(>=|=|<=|<|>))(?<c>-?\d+)");
                if (m.Value != "")
                {
                    continue;
                }
            }



        }

        public struct Equation
        {
            public double a;
            public double b;
            public double c;

            public static Point Intersect(Equation eq1, Equation eq2)
            {
                var y = (eq1.a * eq2.c - eq2.a * eq1.c) / (eq2.b * eq1.a - eq1.b * eq2.a);
                var x = (eq1.c - eq1.b * y) / eq1.a;
                return new Point(x, y);
            }
        }

        private void goal_stClose_Click(object sender, RoutedEventArgs e)
        {
            goal_st.Visibility = Visibility.Visible;
            goal_stClose.Visibility = Visibility.Collapsed;
        }
    }

}
