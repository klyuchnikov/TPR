using System;
using System.Collections;
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
using Microsoft.Win32;
using System.IO;

namespace Klyuchnikovds.TPR.Lab4
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        public static int[][] maps;

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            FileDialog fd = new OpenFileDialog() { InitialDirectory = Environment.CurrentDirectory };
            if (fd.ShowDialog() == true)
            {
                var fileName = fd.FileName;
                var arr = File.ReadAllLines(fileName);
                maps = new int[arr.Length][];
                for (int i = 0; i < arr.Length; i++)
                    maps[i] = arr[i].Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).Select(int.Parse).ToArray();
                GetMinPath(maps, new Point(0, (maps.Length - 1) / 2), new Point(maps[0].Length, 0));
            }

        }

        private Stack<Point> GetMinPath(int[][] map, Point start, Point end)
        {
            var countH = map[0].Length + 1;
            var countV = (map.Length + 1) / 2;
            var mmm = new MyStack[countH, countV];
            Point left, right;
            left = start.X > end.X ? end : start;
            right = start.X < end.X ? end : start;
            if (left.Y > right.Y)
            {
                var current = left;
                mmm[current.X, current.Y] = new MyStack();
                mmm[current.X, current.Y].Push(new Point(current.X, current.Y));
                for (int i = current.X + 1; i <= right.X; i++)
                {
                    var newStack = new MyStack(mmm[i - 1, current.Y].Reverse());
                    var ww = newStack.GetTotalWeight;
                    newStack.Push(new Point(i, current.Y));
                    ww = newStack.GetTotalWeight;
                    mmm[i, current.Y] = newStack;
                }
                for (int i = current.Y - 1; i >= right.Y; i--)
                {
                    var newStack = new MyStack(mmm[current.X, i + 1].Reverse());
                    var ww = newStack.GetTotalWeight;
                    newStack.Push(new Point(current.X, i));
                    ww = newStack.GetTotalWeight;
                    mmm[current.X, i] = newStack;
                }
                do
                {
                    current.Y = current.Y - 1 > -1 ? current.Y - 1 : current.Y;
                    current.X = current.X + 1 <= countH ? current.X + 1 : current.X;
                    for (int i = current.X; i <= right.X; i++)
                    {
                        var newStack1 = new MyStack(mmm[i - 1, current.Y].Reverse());
                        newStack1.Push(new Point(i, current.Y));
                        var newStack2 = new MyStack(mmm[i, current.Y + 1].Reverse());
                        newStack2.Push(new Point(i, current.Y));
                        var newStack = newStack1.GetTotalWeight > newStack2.GetTotalWeight ? newStack2 : newStack1;
                        var ww = newStack.GetTotalWeight;
                        mmm[i, current.Y] = newStack;
                    }
                    for (int i = current.Y; i >= right.Y; i--)
                    {
                        var newStack1 = new MyStack(mmm[current.X, i + 1].Reverse());
                        newStack1.Push(new Point(current.X, i));
                        var newStack2 = new MyStack(mmm[current.X - 1, i].Reverse());
                        newStack2.Push(new Point(current.X, i));
                        var newStack = newStack1.GetTotalWeight > newStack2.GetTotalWeight ? newStack2 : newStack1;
                        var ww = newStack.GetTotalWeight;
                        mmm[current.X, i] = newStack;
                    }
                } while (current.X < right.X);
                PaintMap(map, mmm, countH, countV, mmm[end.X, end.Y]);
            }
            else
            {

            }

            return null;
        }

        private void PaintMap(int[][] map, MyStack[,] mmm, int countH, int countV, MyStack st)
        {
            canvas.Children.Clear();
            var offset = 10;
            var wh = 10;
            var width = 50;
            var height = 30;
            canvas.Height = height * countV + offset * 2;
            canvas.Width = width * countH + offset * 2;
            for (int i = 0; i < countH; i++)
                for (int j = 0; j < countV; j++)
                {
                    var el = new Ellipse() { Width = wh, Height = wh, Fill = Brushes.LightSeaGreen };
                    Canvas.SetLeft(el, offset + i * width);
                    Canvas.SetTop(el, offset + j * height);
                    canvas.Children.Add(el);
                }
            for (int i = 0; i < map.Length; i++)
            {
                var aa = map[i];
                for (int j = 0; j < aa.Length; j++)
                {
                    var text = new TextBlock() { Text = aa[j].ToString() };
                    Canvas.SetLeft(text, offset + j * width + (i % 2 == 0 ? width / 2 : 0));
                    Canvas.SetTop(text, offset + i * (height / 2));
                    canvas.Children.Add(text);
                }
            }
            var stt = st.ToArray();
            for (int i = 1; i < stt.Length; i++)
            {
                var s = stt[i - 1];
                var e = stt[i];
                var line = new Line() { X1 = s.X * width, X2 = e.X * width, Y1 = s.Y * height, Y2 = e.Y * height, Stroke = Brushes.LightSeaGreen };
                Canvas.SetLeft(line, offset + wh / 2);
                Canvas.SetTop(line, offset + wh / 2);
                canvas.Children.Add(line);
            }
        }

        public struct Point
        {
            public int X;
            public int Y;
            public Point(int x, int y)
            {
                this.X = x;
                this.Y = y;
            }
            public override string ToString()
            {
                return string.Format("{0}, {1}", X, Y);
            }
        }

        public class MyStack : System.Collections.Generic.Stack<Klyuchnikovds.TPR.Lab4.MainWindow.Point>
        {
            public MyStack(IEnumerable<Point> collection)
                : base(collection)
            {
            }
            public MyStack()
                : base()
            {
            }
            public int GetTotalWeight
            {
                get
                {
                    if (Count < 1)
                        return 0;
                    var total = 0;
                    for (int i = 1; i < Count; i++)
                    {
                        Point s = this.Reverse().ToArray()[i - 1];
                        Point e = this.Reverse().ToArray()[i];
                        if (s.Y == e.Y)
                            total += MainWindow.maps[s.Y * 2][s.X];
                        else
                            total += MainWindow.maps[s.Y * 2 - 1][s.X];
                    }
                    return total;
                }
            }

            public override string ToString()
            {
                return string.Format(" {0}, {1}", this.Count, GetTotalWeight);
            }
        }
    }
}
