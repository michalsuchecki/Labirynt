using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace Labirynt
{
    class Player
    {
        //======  POLA  ======//
        public bool   mExist;       // Gracz istnieje ?
        // Pozycja
        public Point  Pos;          // Pozycja - zaokrąglona
        public double posx, posy;   // Pozycja - dokładna
        //Lista punktów pośrednich
        public List<Point> punkty;
        public int index;
        // Kąty
        public int kat;          // Kąt między poz.początkową a metą
        public int kat2;         // Aktualny kąt odchylenia
        public int azymut;
        // Inne
        public int i,k;          // stopnie o ile obrócić obrotu
        public int speed;        // szybkość poruszania punktu
        public bool sciana;      // Czy przy ścianie
        private Point kat1;
        private Point p;
        // Do rysowania lini od początku do końca - pomocne przy testach :)
        public  Point start, meta;
        //====== METODY ======//
        public Player()
        {
            punkty = new List<Point>();
            speed = 10;
            sciana = false;
            kat = 0;
            kat2 = 0;
            azymut = 0;
            index = 1; //?
        }
        public double RadiansToDegrees(double radians)
        {
            return 180 * radians / Math.PI;
        }
        public double DegreesToRadians(int degrees)
        {
            return (degrees * Math.PI) / 180;
        }
        public double CalculateDegrees(Point p1, Point p2) // p1 - meta , p2 - player
        {
            double kat2;
            kat1.X = -p1.Y + p2.Y;
            kat1.Y = -p1.X + p2.X;
            kat2 = Math.Atan2(kat1.Y, kat1.X);
            kat2 = RadiansToDegrees(kat2);
            kat2 = (kat2 > 0 ? kat2 : (360 + kat2));
            return kat2;
        }
        //====================//
        public void UstawKat(int _kat) // Ustawia kąt
        {
            kat = _kat;
        }
        public Point Sprawdz(int _kat) // Sprawdza następną pozycje
        {
            double _x = -Math.Sin(DegreesToRadians(_kat))/speed;
            double _y = -Math.Cos(DegreesToRadians(_kat))/speed;
            p.X = (int)Math.Ceiling(_x + posx);
            p.Y = (int)Math.Ceiling(_y + posy);
            return p;
        }
        public int SprawdzKat()
        {
            return (int)CalculateDegrees(meta,start);
        }
        public void Krok() 
        {
            posx = posx - (Math.Sin(DegreesToRadians(kat))/speed);
            posy = posy - (Math.Cos(DegreesToRadians(kat))/speed);
            Pos.X = (int)Math.Ceiling(posx);
            Pos.Y = (int)Math.Ceiling(posy);
        }
    }
}
