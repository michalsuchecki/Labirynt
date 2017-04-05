using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Drawing.Imaging;

namespace Labirynt
{
    public partial class Form1 : Form
    {
        //======== Pola ========//
        // Pozycja Mety
        Point mMeta;
        bool mMetaExist;
        // Pozycja Gracza
        Player player;
        // Typ rysowania
        bool paint;             // Czy można rysować
        int mType;              // Typ rysowania 0 - ściana, 1 - gracz , 2 meta
        // Inne
        bool running;
        // Moduły
        Graphics mGraphics;     // Moduł rysowania w panelu
        Graphics mLabirynt;     // Moduł rysowania w pliku
        Bitmap mImgLabirynt;    // Plik PNG z namalowanym labiryntem
        Size mSize;             // Przechowuje rozmiar panelu - obramowanie (2px)
        // Pędzle
        SolidBrush mBrush;
        Pen mPen;
        // ========== Metody ==========//
        // Konstruktor
        public Form1()
        {
            InitializeComponent();
            // Ustawienie modułu do rysowania w panelu
            mGraphics = picture.CreateGraphics();
            // Struktura SIZE zawiera rozmiar palenu (2px naobramowanie)
            mSize  = new Size(picture.Width - 2, picture.Height - 2);
            player = new Player();
            mBrush = new SolidBrush(Color.White);   // Ustawiamy kolor
            mPen   = new Pen(mBrush);               // Ustawiamy pędzla          

            // Ustawienie Okna otwarcia pliku
            openFileDialog1.FileName = "Labirynt.png";
            openFileDialog1.Filter   = "Labirynt (*.png)|*.png";
            // Ustawienie Okna zapisu pliku
            saveFileDialog1.Filter   = "Labirynt (*.png)|*.png";
            saveFileDialog1.FilterIndex = 1;

            //Utworzenie pustej mapy bitowej i modułu do jej obsługi
            mImgLabirynt = new Bitmap(mSize.Width, mSize.Height);
            mLabirynt = Graphics.FromImage(mImgLabirynt);
            mBrush.Color = Color.Black;
            mPen.Color = Color.Black;
            mPen.Width = 1;                 // Ustawiamy grubość pędzla
            mLabirynt.Clear(Color.White);
            mLabirynt.DrawRectangle(mPen, 0, 0, 399, 399);
            // Ustawienie pozostałych zmiennych
            running = false;
            paint = false;                  // Zmienna informująca nas czy rysujemy
            timer1.Interval = 10;           // Ustawiamy szybkość timera
            Reset();
        }   
        // Metody
        public void Reset()
        {
            // Ustaw wszystko na 0 i skasuje mete i gracza
            player.mExist = false;
            player.Pos.X = 0;
            player.Pos.Y = 0;
            player.punkty.Clear();
            mMetaExist = false;
            mMeta.X = 0;
            mMeta.Y = 0;
            metaX.Text = Convert.ToString(mMeta.X);
            metaY.Text = Convert.ToString(mMeta.Y);
            playerX.Text = Convert.ToString(player.Pos.X);
            playerY.Text = Convert.ToString(player.Pos.Y);
            btnStart.Text = "START";
            picture.Refresh();
        }
        public void Start()
        {
            player.UstawKat((int)player.CalculateDegrees(mMeta, player.Pos));
            //========================================================
            player.index = 1;
            player.start = player.Pos;
            player.meta = mMeta;
            player.kat2 = player.SprawdzKat();
            Punkty();
            //========================================================
            running = true;
            timer1.Enabled = true;
            btnStart.Text = "STOP";
        }
        public void Stop()
        {
            running = false;
            timer1.Enabled = false;
            btnStart.Text = "START";
        }
        public void Punkty()
        {
            //====================
            Color b,c;
            Point pos = player.start;
            Point nextpos = new Point();
            int kat = (int)player.CalculateDegrees(player.meta, player.start);
            double posx = pos.X;
            double posy = pos.Y;
            //====================
            player.punkty.Clear();
            player.punkty.Add(player.start);
            while ((nextpos.X != player.meta.X) && (nextpos.Y != player.meta.Y))
            {
                posx = posx - (Math.Sin(player.DegreesToRadians(kat)));
                posy = posy - (Math.Cos(player.DegreesToRadians(kat)));
                nextpos.X = (int)Math.Ceiling(posx);
                nextpos.Y = (int)Math.Ceiling(posy);

                b = mImgLabirynt.GetPixel(nextpos.X, nextpos.Y);
                c = mImgLabirynt.GetPixel(pos.X, pos.Y);

                if ((c.ToArgb() == Color.Black.ToArgb()) && (b.ToArgb() == Color.White.ToArgb()))
                {
                    player.punkty.Add(nextpos);
                }

                pos.X = nextpos.X;
                pos.Y = nextpos.Y;
            }
            player.punkty.Add(player.meta);
        }
        public void Algorytm2()
        {
            //==================================================================
            Point p = player.Sprawdz(player.kat);
            Point k;
            Color kolor = mImgLabirynt.GetPixel(p.X, p.Y);
            // Algorytm wyszukiwania drogi TUTAJ
            if ((kolor.ToArgb() != Color.Black.ToArgb())) //1
            {
                player.Krok();
            }
            else
            {
                for (player.i = 1; ; player.i++)
                {
                    k = player.Sprawdz(player.kat - player.i);
                    kolor = mImgLabirynt.GetPixel(k.X, k.Y);
                    if ((kolor.ToArgb() != Color.Black.ToArgb()))
                    {
                        player.kat = player.kat - player.i;
                        player.kat = player.kat % 360;
                        player.sciana = true; // tu moze byc problem
                        break;
                    }
                }
                player.Krok();
            }
            if (((p.X >= (mMeta.X - 2)) && (p.X <= (mMeta.X + 2))) && ((p.Y >= (mMeta.Y - 2)) && (p.Y <= (mMeta.Y + 2))))
            {
                Stop();
            }
            else
            {
                if (player.sciana)
                {
                    int x = (int)player.CalculateDegrees(mMeta, player.Pos);
                    if ((x >= (player.kat2)) && (x <= (player.kat2)))
                    {
                        player.UstawKat(player.kat2);
                        player.sciana = false;
                    }
                    else
                    {
                        for (player.k = 1; ; player.k++)
                        {
                            k = player.Sprawdz(player.kat + player.k);
                            kolor = mImgLabirynt.GetPixel(k.X, k.Y);
                            if ((kolor.ToArgb() == Color.Black.ToArgb()))
                            {
                                player.kat = player.kat + player.k - 1;
                                player.kat = player.kat % 360;
                                break;
                            }
                        }
                    }
                }
            }
            // Koniec Algorytmu wyszukiwania drogi

            //==================================================================
        }
        public void Algorytm()
        {
            //==================================================================
            // Początek Algorytmu wyszukiwania drogi
            //==================================================================
            player.azymut = (int)player.CalculateDegrees(player.meta, player.Pos);
            Point p = player.Sprawdz(player.kat);
            Point k;
            Color kolor = mImgLabirynt.GetPixel(p.X, p.Y);
            if ((kolor.ToArgb() != Color.Black.ToArgb()))
            {
                player.Krok();
            }
            else
            {
                for (player.i = 1; ; player.i++)
                {
                    k = player.Sprawdz(player.kat - player.i);
                    kolor = mImgLabirynt.GetPixel(k.X, k.Y);
                    if ((kolor.ToArgb() != Color.Black.ToArgb()))
                    {
                        player.kat = player.kat - player.i;
                        player.kat = player.kat % 360;
                        player.sciana = true;
                        break;
                    }
                }
            }
            if (((p.X >= (mMeta.X - 2)) && (p.X <= (mMeta.X + 2))) && ((p.Y >= (mMeta.Y - 2)) && (p.Y <= (mMeta.Y + 2))))
            {
                Stop();
            }
            else
            {
                if (player.sciana)
                {
                    bool test = false;
                    for (int i = player.index; i < player.punkty.Count; i++)
                    {
                        if ((player.punkty[i].X <= (player.Pos.X + 1)) && (player.punkty[i].X >= (player.Pos.X - 1)) && (player.punkty[i].Y <= (player.Pos.Y + 1)) && (player.punkty[i].Y >= (player.Pos.Y - 1)))
                        {
                            // Nie potrzebne - służy do usuwania puntku z listy w którym się znajduje, inaczej zamiast
                            // i = index -> i = 0 w for;
                            player.index = i;
                            player.punkty.RemoveAt(player.index);
                            // =============
                            player.UstawKat(player.azymut);
                            player.sciana = false;
                            test = true;
                            break;
                        }
                        else
                            test = false;
                    }
                    if (!test)
                    {
                        for (player.k = 0; ; player.k++)
                        {
                            k = player.Sprawdz(player.kat + player.k);
                            kolor = mImgLabirynt.GetPixel(k.X, k.Y);
                            if ((kolor.ToArgb() == Color.Black.ToArgb()))
                            {
                                player.kat = player.kat + player.k - 1;
                                player.kat = player.kat % 360;
                                break;
                            }
                        }
                    }
                }
            }

            //==================================================================
            // Koniec Algorytmu wyszukiwania drogi
            //==================================================================
        }
        // ========== Zdarzenia ==========//
        // Obsługa paska prędkości
        private void trackBar1_ValueChanged(object sender, EventArgs e)
        {
            player.speed = (11-trackBar1.Value);
            label11.Text = Convert.ToString(trackBar1.Value);
        }
        // Obsługa timera
        private void timer1_Tick(object sender, EventArgs e)
        {
            // Wykonuje algorytm
            //
            //Algorytm2();
            Algorytm();
            // Odświeża statystyki
            label13.Text = Convert.ToString(player.kat);
            label15.Text = Convert.ToString(player.kat2);
            label17.Text = Convert.ToString(player.azymut);
            playerX.Text = Convert.ToString(player.Pos.X);
            playerY.Text = Convert.ToString(player.Pos.Y);
            // Odświeża obraz w panelu
            picture.Refresh();
        }
        // Obsługa przycisków
        private void btnClearClick(object sender, EventArgs e)
        {
            Stop();
            Reset();
            mGraphics.Clear(Color.White);
            mLabirynt.Clear(Color.White);
            mPen.Color = Color.Black;
            mLabirynt.DrawRectangle(mPen, 0, 0, 399, 399);
        }
        private void btnColorBlackClick(object sender, EventArgs e)
        {
            mPen.Color = Color.Black;
            mPen.Width = 4;
            mType = 0;
        }
        private void btnColorRedClick(object sender, EventArgs e)
        {
            mPen.Width = 1;
            mType = 1;
        }
        private void btnColorBlueClick(object sender, EventArgs e)
        {
            mPen.Width = 1;
            mType = 2;
        }
        private void btnLoadClick(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                mImgLabirynt.Dispose();
                mImgLabirynt = new Bitmap(openFileDialog1.FileName);
                picture.Refresh();
            }
            Reset();

        }
        private void btnSaveClick(object sender, EventArgs e)
        {
            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
                mImgLabirynt.Save(saveFileDialog1.FileName);
        }
        private void btnResetClick(object sender, EventArgs e)
        {
            Stop();
            Reset();
        }
        private void btnStartClick(object sender, EventArgs e)
        {
            if (!running)
            {
                if(player.mExist && mMetaExist)
                    Start();
            }
            else
                Stop();
        }
        // Obsługa PictureBox
        private void picture_Paint(object sender, PaintEventArgs e)
        {
            // Rysuje labirynt
            e.Graphics.DrawImage(mImgLabirynt, 0, 0);
            mPen.Width = 1;
            // Rysuje linie między graczem a metą
            if (player.mExist && mMetaExist)
            {
                mPen.Color = Color.Gray;
                e.Graphics.DrawLine(mPen, mMeta, player.Pos);
                mPen.Color = Color.LightGreen;
                e.Graphics.DrawLine(mPen, player.meta, player.start);
            }
            // Rysuje punkty z listy
            mPen.Color = Color.Red;
            if (player.punkty.Count > 0)
            {
                for (int i = 1; i < player.punkty.Count - 1; i++)
                    e.Graphics.DrawEllipse(mPen, player.punkty[i].X, player.punkty[i].Y, 2, 2);
            }
            // Rysuje mete jeśli ustawiono
            if (mMetaExist)
            {
                mBrush.Color = Color.Blue;
                e.Graphics.FillEllipse(mBrush, mMeta.X-2, mMeta.Y-2, 4, 4);
            }
            // Rysuje gracza jeśli ustawiono
            if (player.mExist)
            {
                mBrush.Color = Color.Red;
                e.Graphics.FillEllipse(mBrush, player.Pos.X-2, player.Pos.Y-2, 4, 4);
            }

        }
        private void picture_MouseDown(object sender, MouseEventArgs e)
        {
            paint = true;
        }
        private void picture_MouseUp(object sender, MouseEventArgs e)
        {
            if (paint && (mType == 2) && !mMetaExist) // Rysowanie mety / gracza (tylko raz)
            {
                mMetaExist = true;
                mMeta.X = e.X;
                mMeta.Y = e.Y;
                metaX.Text = Convert.ToString(mMeta.X);
                metaY.Text = Convert.ToString(mMeta.Y);
                mPen.Color = Color.Blue;
                mGraphics.DrawEllipse(mPen, mMeta.X, mMeta.Y, 2, 2);
            }

            if (paint && (mType == 1) && !player.mExist) // Rysowanie mety / gracza (tylko raz)
            {
                player.mExist = true;
                player.posx = player.Pos.X = e.X;
                player.posy = player.Pos.Y = e.Y;
                playerX.Text = Convert.ToString(player.Pos.X);
                playerY.Text = Convert.ToString(player.Pos.Y);
                mPen.Color = Color.Red;
                mGraphics.DrawEllipse(mPen, player.Pos.X, player.Pos.Y, 2, 2);
            }
            paint = false;
        }
        private void picture_MouseMove(object sender, MouseEventArgs e)
        {
            if (paint && (mType == 0)) // Rysowanie ścian
            {
                mBrush.Color = Color.Black;
                mLabirynt.FillEllipse(mBrush, e.X, e.Y, 3, 3);
                mGraphics.FillEllipse(mBrush, e.X, e.Y, 3, 3);
            }
        }
    }
}
