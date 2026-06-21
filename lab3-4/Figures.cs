using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

namespace Lab34
{
    // Символьне полотно для малювання фігур у консолі.
    class Canvas
    {
        private char[,] grid;
        private int w, h;

        public Canvas(int width, int height)
        {
            if (width <= 0 || height <= 0)
                throw new ArgumentException("Розміри полотна мають бути додатними.");
            w = width; h = height;
            grid = new char[h, w];
            Clear();
        }

        public void Clear()
        {
            for (int r = 0; r < h; r++)
                for (int c = 0; c < w; c++)
                    grid[r, c] = ' ';
        }

        private bool InBounds(int col, int row) { return col >= 0 && col < w && row >= 0 && row < h; }
        private int ColOf(double x) { return (int)Math.Round(x); }
        private int RowOf(double y) { return h - 1 - (int)Math.Round(y); }

        public void Plot(double x, double y, char ch)
        {
            int col = ColOf(x), row = RowOf(y);
            if (InBounds(col, row)) grid[row, col] = ch;
        }

        // Відрізок за алгоритмом Брезенхема.
        public void DrawLine(Point p1, Point p2)
        {
            int x0 = ColOf(p1.X), y0 = RowOf(p1.Y);
            int x1 = ColOf(p2.X), y1 = RowOf(p2.Y);
            int dx = Math.Abs(x1 - x0), dy = Math.Abs(y1 - y0);
            int sx = x0 < x1 ? 1 : -1, sy = y0 < y1 ? 1 : -1;
            int err = dx - dy;
            while (true)
            {
                if (InBounds(x0, y0)) grid[y0, x0] = '*';
                if (x0 == x1 && y0 == y1) break;
                int e2 = 2 * err;
                if (e2 > -dy) { err -= dy; x0 += sx; }
                if (e2 < dx) { err += dx; y0 += sy; }
            }
        }

        // Заливка трикутника символом ch (не перетирає вже намальовані ребра).
        public void FillTriangle(Point a, Point b, Point c, char ch)
        {
            int ax = ColOf(a.X), ay = RowOf(a.Y);
            int bx = ColOf(b.X), by = RowOf(b.Y);
            int cx = ColOf(c.X), cy = RowOf(c.Y);
            int minc = Math.Min(ax, Math.Min(bx, cx)), maxc = Math.Max(ax, Math.Max(bx, cx));
            int minr = Math.Min(ay, Math.Min(by, cy)), maxr = Math.Max(ay, Math.Max(by, cy));
            for (int row = minr; row <= maxr; row++)
                for (int col = minc; col <= maxc; col++)
                    if (InBounds(col, row) && PointInTriangle(col, row, ax, ay, bx, by, cx, cy))
                        if (grid[row, col] == ' ') grid[row, col] = ch;
        }

        private static int Sign(int px, int py, int ax, int ay, int bx, int by)
        {
            return (px - bx) * (ay - by) - (ax - bx) * (py - by);
        }
        private static bool PointInTriangle(int px, int py, int ax, int ay, int bx, int by, int cx, int cy)
        {
            int d1 = Sign(px, py, ax, ay, bx, by);
            int d2 = Sign(px, py, bx, by, cx, cy);
            int d3 = Sign(px, py, cx, cy, ax, ay);
            bool neg = (d1 < 0) || (d2 < 0) || (d3 < 0);
            bool pos = (d1 > 0) || (d2 > 0) || (d3 > 0);
            return !(neg && pos);
        }

        public void Render()
        {
            StringBuilder sb = new StringBuilder();
            for (int r = 0; r < h; r++)
            {
                for (int c = 0; c < w; c++) sb.Append(grid[r, c]);
                sb.Append('\n');
            }
            Console.Write(sb.ToString());
        }
    }

    // Базова абстрактна фігура.
    abstract class Figure
    {
        public abstract Point Anchor { get; }
        public abstract void Move(double dx, double dy);
        public abstract void Scale(double k);
        public abstract double Area();
        public abstract double Perimeter();
        public abstract void Draw(Canvas canvas);

        // Перемістити так, щоб опорна точка опинилась у (x, y).
        public virtual void MoveTo(double x, double y)
        {
            Point a = Anchor;
            Move(x - a.X, y - a.Y);
        }

        // Масштаб відносно довільного центра (для масштабування зображення).
        public virtual void ScaleAbout(Point center, double k)
        {
            Scale(k);
            Point a = Anchor;
            double nx = center.X + (a.X - center.X) * k;
            double ny = center.Y + (a.Y - center.Y) * k;
            MoveTo(nx, ny);
        }
    }

    // Точка. Також використовується як вершина інших фігур.
    class Point : Figure
    {
        private double x, y;

        public double X { get { return x; } set { x = value; } }
        public double Y { get { return y; } set { y = value; } }

        public Point(double x, double y) { this.x = x; this.y = y; }

        public override Point Anchor { get { return this; } }
        public override void Move(double dx, double dy) { x += dx; y += dy; }
        public override void Scale(double k) { } // точка не має розміру
        public override double Area() { return 0.0; }
        public override double Perimeter() { return 0.0; }
        public override void Draw(Canvas canvas) { canvas.Plot(x, y, '*'); }

        public void ScaleAround(Point center, double k)
        {
            x = center.X + (x - center.X) * k;
            y = center.Y + (y - center.Y) * k;
        }

        public double DistanceTo(Point p)
        {
            double dx = x - p.X, dy = y - p.Y;
            return Math.Sqrt(dx * dx + dy * dy);
        }

        public override string ToString()
        {
            return string.Format("Точка ({0:0.##}; {1:0.##})", x, y);
        }
    }

    // Трикутник за трьома вершинами. Базовий поліморфний клас для решти трикутників.
    class Triangle : Figure
    {
        protected Point a, b, c;

        protected virtual string Name { get { return "Трикутник"; } }

        public Triangle(Point a, Point b, Point c)
        {
            if (a == null || b == null || c == null)
                throw new ArgumentNullException("Вершина не може бути null.");
            if (Collinear(a, b, c))
                throw new ArgumentException("Точки лежать на одній прямій.");
            this.a = a; this.b = b; this.c = c;
        }

        protected Triangle(Point[] v) : this(v[0], v[1], v[2]) { }

        private static bool Collinear(Point p1, Point p2, Point p3)
        {
            double area2 = (p2.X - p1.X) * (p3.Y - p1.Y) - (p3.X - p1.X) * (p2.Y - p1.Y);
            return Math.Abs(area2) < 1e-9;
        }

        // Доступ до вершин за індексом 0..2.
        public Point this[int i]
        {
            get
            {
                if (i == 0) return a; if (i == 1) return b; if (i == 2) return c;
                throw new IndexOutOfRangeException("Індекс вершини 0..2.");
            }
            set
            {
                if (value == null) throw new ArgumentNullException("value");
                if (i == 0) a = value; else if (i == 1) b = value; else if (i == 2) c = value;
                else throw new IndexOutOfRangeException("Індекс вершини 0..2.");
            }
        }

        // Центр мас — опорна точка.
        public override Point Anchor
        {
            get { return new Point((a.X + b.X + c.X) / 3.0, (a.Y + b.Y + c.Y) / 3.0); }
        }

        public override void Move(double dx, double dy)
        {
            a.Move(dx, dy); b.Move(dx, dy); c.Move(dx, dy);
        }

        public override void Scale(double k)
        {
            if (k <= 0) throw new ArgumentException("Коефіцієнт масштабування має бути додатним.");
            Point ctr = Anchor;
            a.ScaleAround(ctr, k); b.ScaleAround(ctr, k); c.ScaleAround(ctr, k);
        }

        public override double Perimeter()
        {
            return a.DistanceTo(b) + b.DistanceTo(c) + c.DistanceTo(a);
        }

        public override double Area()
        {
            return Math.Abs((b.X - a.X) * (c.Y - a.Y) - (c.X - a.X) * (b.Y - a.Y)) / 2.0;
        }

        public override void Draw(Canvas canvas)
        {
            canvas.DrawLine(a, b); canvas.DrawLine(b, c); canvas.DrawLine(c, a);
        }

        public override string ToString()
        {
            return string.Format("{0}: A({1:0.##};{2:0.##}), B({3:0.##};{4:0.##}), C({5:0.##};{6:0.##}); P={7:0.##}, S={8:0.##}",
                Name, a.X, a.Y, b.X, b.Y, c.X, c.Y, Perimeter(), Area());
        }
    }

    // Трикутник із заливкою.
    class HatchedTriangle : Triangle
    {
        protected override string Name { get { return "Заштрихований трикутник"; } }

        public HatchedTriangle(Point a, Point b, Point c) : base(a, b, c) { }

        public override void Draw(Canvas canvas)
        {
            canvas.FillTriangle(a, b, c, '/');
            base.Draw(canvas);
        }
    }

    // Прямокутний трикутник за вершиною прямого кута і катетами.
    class RightTriangle : Triangle
    {
        protected override string Name { get { return "Прямокутний трикутник"; } }

        public RightTriangle(Point rightVertex, double legX, double legY)
            : base(BuildVertices(rightVertex, legX, legY)) { }

        private static Point[] BuildVertices(Point r, double legX, double legY)
        {
            if (r == null) throw new ArgumentNullException("rightVertex");
            if (legX <= 0 || legY <= 0) throw new ArgumentException("Катети мають бути додатними.");
            return new Point[] {
                new Point(r.X, r.Y),
                new Point(r.X + legX, r.Y),
                new Point(r.X, r.Y + legY)
            };
        }
    }

    // Правильний трикутник за центром і стороною.
    class EquilateralTriangle : Triangle
    {
        protected override string Name { get { return "Правильний трикутник"; } }

        public EquilateralTriangle(Point center, double side)
            : base(BuildVertices(center, side)) { }

        private static Point[] BuildVertices(Point center, double side)
        {
            if (center == null) throw new ArgumentNullException("center");
            if (side <= 0) throw new ArgumentException("Сторона має бути додатною.");
            double r = side / Math.Sqrt(3.0);
            Point[] v = new Point[3];
            for (int i = 0; i < 3; i++)
            {
                double ang = Math.PI / 2 + i * 2 * Math.PI / 3;
                v[i] = new Point(center.X + r * Math.Cos(ang), center.Y + r * Math.Sin(ang));
            }
            return v;
        }

        public double Side { get { return a.DistanceTo(b); } }
    }

    // Правильний тетраедр. Грані — правильні трикутники, тому наслідує грань-основу.
    class Tetrahedron : EquilateralTriangle
    {
        public Tetrahedron(Point baseCenter, double edge) : base(baseCenter, edge) { }

        public double Edge { get { return Side; } }

        // Для тетраедра площа — це площа поверхні (4 грані).
        public override double Area()
        {
            double e = Edge;
            return Math.Sqrt(3.0) * e * e;
        }

        // Сумарна довжина 6 ребер.
        public override double Perimeter() { return 6.0 * Edge; }

        public double Volume()
        {
            double e = Edge;
            return e * e * e / (6.0 * Math.Sqrt(2.0));
        }

        public override void Draw(Canvas canvas)
        {
            base.Draw(canvas);
            Point ctr = Anchor;
            Point apex = new Point(ctr.X, ctr.Y + Edge * 0.6);
            canvas.DrawLine(a, apex);
            canvas.DrawLine(b, apex);
            canvas.DrawLine(c, apex);
        }

        public override string ToString()
        {
            return string.Format("Тетраедр (правильний): ребро={0:0.##}, площа поверхні={1:0.##}, об'єм={2:0.##}",
                Edge, Area(), Volume());
        }
    }

    // Зображення: положення, розмір і колекція фігур.
    class Image
    {
        private List<Figure> figures = new List<Figure>();
        private double x, y, width, height;

        public Image(double x, double y, double width, double height)
        {
            if (width <= 0 || height <= 0)
                throw new ArgumentException("Розміри зображення мають бути додатними.");
            this.x = x; this.y = y; this.width = width; this.height = height;
        }

        public int Count { get { return figures.Count; } }
        public double X { get { return x; } }
        public double Y { get { return y; } }
        public double Width { get { return width; } }
        public double Height { get { return height; } }

        public Figure this[int i]
        {
            get { CheckIndex(i); return figures[i]; }
            set { CheckIndex(i); if (value == null) throw new ArgumentNullException("value"); figures[i] = value; }
        }
        private void CheckIndex(int i)
        {
            if (i < 0 || i >= figures.Count) throw new IndexOutOfRangeException("Невірний індекс фігури.");
        }

        public void Add(Figure f)
        {
            if (f == null) throw new ArgumentNullException("f");
            figures.Add(f);
        }

        public void RemoveAt(int i) { CheckIndex(i); figures.RemoveAt(i); }

        public void MoveAllFigures(double dx, double dy)
        {
            foreach (Figure f in figures) f.Move(dx, dy);
        }

        public void MoveImage(double dx, double dy)
        {
            x += dx; y += dy;
            foreach (Figure f in figures) f.Move(dx, dy);
        }

        // Масштаб зображення разом з фігурами, зі збереженням пропорцій.
        public void SetScale(double k)
        {
            if (k <= 0) throw new ArgumentException("Коефіцієнт масштабування має бути додатним.");
            Point origin = new Point(x, y);
            foreach (Figure f in figures) f.ScaleAbout(origin, k);
            width *= k; height *= k;
        }

        public double TotalArea()
        {
            double s = 0;
            foreach (Figure f in figures) s += f.Area();
            return s;
        }

        public void DrawAll(Canvas canvas)
        {
            foreach (Figure f in figures) f.Draw(canvas);
        }

        // Об'єднання двох зображень у нове.
        public static Image operator +(Image first, Image second)
        {
            if (first == null) return second;
            if (second == null) return first;
            double minX = Math.Min(first.x, second.x);
            double minY = Math.Min(first.y, second.y);
            double maxX = Math.Max(first.x + first.width, second.x + second.width);
            double maxY = Math.Max(first.y + first.height, second.y + second.height);
            Image r = new Image(minX, minY, maxX - minX, maxY - minY);
            foreach (Figure f in first.figures) r.figures.Add(f);
            foreach (Figure f in second.figures) r.figures.Add(f);
            return r;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("Зображення: положення ({0:0.##}; {1:0.##}), розмір {2:0.##} x {3:0.##}, фігур: {4}",
                x, y, width, height, figures.Count);
            for (int i = 0; i < figures.Count; i++)
            {
                sb.Append("\n  ");
                sb.Append(i + 1);
                sb.Append(") ");
                sb.Append(figures[i].ToString());
            }
            return sb.ToString();
        }

        // Збереження у текстовий файл: кожна фігура — окремий рядок з тегом типу.
        public void Save(string path)
        {
            CultureInfo ci = CultureInfo.InvariantCulture;
            List<string> lines = new List<string>();
            lines.Add(string.Format(ci, "IMAGE {0} {1} {2} {3}", x, y, width, height));
            foreach (Figure f in figures) lines.Add(Serialize(f, ci));
            File.WriteAllLines(path, lines);
        }

        public static Image Load(string path)
        {
            CultureInfo ci = CultureInfo.InvariantCulture;
            string[] lines = File.ReadAllLines(path);
            if (lines.Length == 0) throw new IOException("Порожній файл зображення.");
            string[] h = lines[0].Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (h[0] != "IMAGE") throw new IOException("Невірний формат файлу зображення.");
            Image img = new Image(D(h[1], ci), D(h[2], ci), D(h[3], ci), D(h[4], ci));
            for (int i = 1; i < lines.Length; i++)
            {
                if (lines[i].Trim().Length == 0) continue;
                img.Add(Deserialize(lines[i], ci));
            }
            return img;
        }

        private static double D(string s, CultureInfo ci) { return double.Parse(s, ci); }

        // Тип перевіряється від похідного до базового.
        private static string Serialize(Figure f, CultureInfo ci)
        {
            if (f is Tetrahedron)
            {
                Tetrahedron t = (Tetrahedron)f; Point c = t.Anchor;
                return string.Format(ci, "TETRAHEDRON {0} {1} {2}", c.X, c.Y, t.Edge);
            }
            if (f is EquilateralTriangle)
            {
                EquilateralTriangle e = (EquilateralTriangle)f; Point c = e.Anchor;
                return string.Format(ci, "EQUILATERAL {0} {1} {2}", c.X, c.Y, e.Side);
            }
            if (f is RightTriangle)
            {
                RightTriangle r = (RightTriangle)f;
                double legX = r[1].X - r[0].X, legY = r[2].Y - r[0].Y;
                return string.Format(ci, "RIGHT {0} {1} {2} {3}", r[0].X, r[0].Y, legX, legY);
            }
            if (f is HatchedTriangle)
            {
                HatchedTriangle t = (HatchedTriangle)f;
                return string.Format(ci, "HATCHED {0} {1} {2} {3} {4} {5}",
                    t[0].X, t[0].Y, t[1].X, t[1].Y, t[2].X, t[2].Y);
            }
            if (f is Triangle)
            {
                Triangle t = (Triangle)f;
                return string.Format(ci, "TRIANGLE {0} {1} {2} {3} {4} {5}",
                    t[0].X, t[0].Y, t[1].X, t[1].Y, t[2].X, t[2].Y);
            }
            if (f is Point)
            {
                Point p = (Point)f;
                return string.Format(ci, "POINT {0} {1}", p.X, p.Y);
            }
            throw new NotSupportedException("Невідомий тип фігури для збереження.");
        }

        private static Figure Deserialize(string line, CultureInfo ci)
        {
            string[] p = line.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            switch (p[0])
            {
                case "POINT": return new Point(D(p[1], ci), D(p[2], ci));
                case "TRIANGLE": return new Triangle(new Point(D(p[1], ci), D(p[2], ci)), new Point(D(p[3], ci), D(p[4], ci)), new Point(D(p[5], ci), D(p[6], ci)));
                case "HATCHED": return new HatchedTriangle(new Point(D(p[1], ci), D(p[2], ci)), new Point(D(p[3], ci), D(p[4], ci)), new Point(D(p[5], ci), D(p[6], ci)));
                case "RIGHT": return new RightTriangle(new Point(D(p[1], ci), D(p[2], ci)), D(p[3], ci), D(p[4], ci));
                case "EQUILATERAL": return new EquilateralTriangle(new Point(D(p[1], ci), D(p[2], ci)), D(p[3], ci));
                case "TETRAHEDRON": return new Tetrahedron(new Point(D(p[1], ci), D(p[2], ci)), D(p[3], ci));
                default: throw new IOException("Невідомий тег фігури: " + p[0]);
            }
        }
    }

    class Program
    {
        static void Main()
        {
            Console.WriteLine("Лабораторна робота 3, 4\n");

            Point p = new Point(3, 3);
            Triangle t = new Triangle(new Point(6, 2), new Point(22, 2), new Point(14, 12));
            HatchedTriangle ht = new HatchedTriangle(new Point(26, 2), new Point(40, 2), new Point(33, 12));
            RightTriangle rt = new RightTriangle(new Point(45, 2), 12, 9);
            EquilateralTriangle eq = new EquilateralTriangle(new Point(12, 22), 12);
            Tetrahedron tet = new Tetrahedron(new Point(45, 19), 12);

            Console.WriteLine("Фігури:");
            Figure[] all = { p, t, ht, rt, eq, tet };
            foreach (Figure f in all)
                Console.WriteLine("  " + f);
            Console.WriteLine("  Об'єм тетраедра: " + tet.Volume().ToString("0.##"));

            Console.WriteLine("\nПереміщення і масштабування:");
            Console.WriteLine("  до Move(5,3):    " + t);
            t.Move(5, 3);
            Console.WriteLine("  після Move(5,3): " + t);
            Console.WriteLine("  до Scale(1.5):    " + eq);
            eq.Scale(1.5);
            Console.WriteLine("  після Scale(1.5): " + eq);

            Console.WriteLine("\nВершини трикутника через індексатор:");
            Console.WriteLine("  rt[0]=" + rt[0] + ", rt[1]=" + rt[1] + ", rt[2]=" + rt[2]);

            Console.WriteLine("\nЗображення 1:");
            Image img1 = new Image(0, 0, 72, 30);
            img1.Add(p); img1.Add(t); img1.Add(ht); img1.Add(rt);
            Console.WriteLine(img1);

            Image img2 = new Image(0, 0, 72, 30);
            img2.Add(eq); img2.Add(tet);

            Console.WriteLine("\nОб'єднання зображень:");
            Image img3 = img1 + img2;
            Console.WriteLine("Фігур у об'єднаному: " + img3.Count);

            Console.WriteLine("\nМалювання в консолі:");
            Canvas canvas = new Canvas(72, 30);
            img3.DrawAll(canvas);
            canvas.Render();

            Console.WriteLine("Сумарна площа: " + img3.TotalArea().ToString("0.##"));

            Console.WriteLine("\nЗбереження і завантаження з файлу:");
            string path = "image.txt";
            img3.Save(path);
            Console.WriteLine("Вміст файлу:");
            foreach (string line in File.ReadAllLines(path)) Console.WriteLine("  " + line);

            Image loaded = Image.Load(path);
            Console.WriteLine("\nЗавантажене зображення:");
            Console.WriteLine(loaded);
            Console.WriteLine("\nКількість фігур збігається: " +
                (loaded.Count == img3.Count ? "так" : "ні"));
        }
    }
}
