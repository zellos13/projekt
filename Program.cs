using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DMU.Math;
namespace ConsoleAppProject
{
    class Program
    {
        static void Main(string[] args)
        {
            FileStream ostrm;
            StreamWriter writer;
            TextWriter oldOut = Console.Out;
            try
            {
                ostrm = new FileStream("file.txt", FileMode.OpenOrCreate, FileAccess.Write);
                writer = new StreamWriter(ostrm);
            }
            catch (Exception e)
            {
                Console.WriteLine("Cannot open file.txt for writing");
                Console.WriteLine(e.Message);
                return;
            }
            Console.SetOut(writer);
            // Krok asynchronizacji (które są nw a które nie)
            int krokasyn = 0;
            // Przykładowa macierz 3x3
            double[,] exampleMatrix = new double[3, 3] {
                {0,-1,-3 },
                {-1, 0, 2 },
                {-3, 2, 0 } };
            // Wszystkie wektory biPolarne
            double[][] bipolarTab = new double[8][] {
                    new double[3] { -1, -1, -1 },
                    new double[3] { -1, -1, 1 },
                    new double[3] { -1, 1, -1 },
                    new double[3] { -1, 1, 1 },
                    new double[3] { 1, -1, -1 },
                    new double[3] { 1, 1, -1 },
                    new double[3] { 1, -1, 1 },
                    new double[3] { 1, 1, 1 } };


            //^^^^^^^^^^^^^^^^^^^^^^^^^^ synchronicznym ^^^^^^^^^^^^^^^^^^^^^^^^^^ 
            System.Console.WriteLine("Sieć Hopfielda bipolarna synchronicznym");
            // wczytanie macierzy W
            Matrix W = new Matrix(exampleMatrix);
            // wyświetlenie macierzy wag
            System.Console.WriteLine("Macierz W:");
            System.Console.WriteLine(W.ToString("F1","\t", "\r\n"));
            
            for (int i=0;i<bipolarTab.Length;i++)
            {
                // wczytanie wktora v0  ?? true
                System.Console.WriteLine("Badanie nr: " + (i + 1));
                System.Console.WriteLine("Badany wektor V:");
                //Badany wektor V0 Bipolarny, jednocześnie przechowuje ostatni wynik 
                Matrix V0 = new Matrix(bipolarTab[i], true);
                //Badany wektor pomocniczy do wyznaczania kolejnych wektorów bipolarnych(aktualnych w pentli)
                Matrix V_pom = V0.Clone();                
                // Maksymalna ilość kroków
                double MIK = Math.Pow(2, V0.Length);                
                System.Console.WriteLine(V0.ToString("F1", "\t", "\r\n"));
                System.Console.WriteLine("Badanie punktu w tybie synchronicznym");

                //Energia poprzedniego ruchu
                double e_pom = 0;
                for (int j = 0; j < MIK; j++)
                {
                    System.Console.WriteLine("Krok: " + (j + 1));
                    System.Console.WriteLine("Potencjał wejściowy U: ");
                    //Mnożene macierzy przez wektor vi wynik przed zmianą na funkcje bipolarną
                    Matrix U = Matrix.Multiply(W, V_pom);
                    System.Console.WriteLine(U.ToString("F1", "\t", "\r\n"));
                    System.Console.WriteLine("Potencjał wyjściowy V: ");
                    //Zmiana wejściowego na wyjściowy (bipolar)
                    V_pom = U.ToBiPolar();
                    System.Console.WriteLine(V_pom.ToString("F1", "\t", "\r\n"));
                    //Obliczenie enegrii E
                    double e = 0;

                    // wzór na energie
                    for (int wiersz = 0; wiersz < 3; wiersz++)
                    {

                        for (int kolumna = 0; kolumna < 3; kolumna++)
                        {
                            e += (exampleMatrix[wiersz, kolumna] * V0.GetElement(wiersz, 0) * V_pom.GetElement(kolumna, 0));
                        }
                    }
                    
                    // Ze wzoru - przed sumą
                    e *= (-1);
                    System.Console.WriteLine("Energia: " + e);

                    // Stablilizacja sieci
                    if (V_pom.Equals(V0))
                    {
                        System.Console.WriteLine("Sieć sie ustabilizowala");
                        System.Console.WriteLine("V2:");
                        System.Console.WriteLine(V0.ToString("F1", "\t", "\r\n"));
                        break;
                    }
                    else if (e == e_pom)
                    {
                        System.Console.WriteLine("Oscylacja dwupunktowa");
                        System.Console.WriteLine("Punkty Oscylacji: V1");
                        System.Console.WriteLine(V0.ToString("F1", "\t", "\r\n"));
                        System.Console.WriteLine("V2:");
                        System.Console.WriteLine(V_pom.ToString("F1", "\t", "\r\n"));
                        break;
                    }
                    e_pom = e;
                    V0 = V_pom;
                }
            }



            
            //^^^^^^^^^^^^^^^^^^^^^^^^^^ asynchronicznym ^^^^^^^^^^^^^^^^^^^^^^^^^^ 
            System.Console.WriteLine("Badanie punky w trybie Asynchronicznym");
            for (int i = 0; i < bipolarTab.Length; i++)
            {

                System.Console.WriteLine("Badanie punky w trybie Asynchronicznym");

                //Badany wektor V0 Bipolarny, jednocześnie przechowuje ostatni wynik 
                Matrix V0 = new Matrix(bipolarTab[i], true);
                //Badany wektor pomocniczy do wyznaczania kolejnych wektorów bipolarnych(aktualnych w pentli)
                Matrix V_pom = V0.Clone();
                // Maksymalna ilość kroków
                double MIK = Math.Pow(2, V0.Length);

                // wczytanie wktora v0  ?? true
                System.Console.WriteLine("Badanie nr: " + (i + 1));
                System.Console.WriteLine("Badany wektor V:");
                System.Console.WriteLine(V0.ToString("F1", "\t", "\r\n"));
                double e = 0;
                int iterator = 0;
                for (int j = 0; j < MIK; j++)
                {
                    // Do wyświetlenia potencjiału U
                    double potencialU = 0;
                    // zapisanie kroku synchronizacji przed zmianą
                    int krokasyn_pom = krokasyn;
                    //Mnożene macierzy przez wektor vi wynik po zmianą na funkcje bipolarną
                    //Własna funkcja
                    Matrix V = Matrix.MultiplyCustom(W, V_pom, ref krokasyn, ref potencialU);
                    System.Console.WriteLine("Krok: " + (j + 1));
                    System.Console.WriteLine("Potencjał wejściowy U: ");

                    // Jedynie wyświetlenie wejścia U
                    if(krokasyn_pom == 0)
                    {
                        System.Console.WriteLine(potencialU);
                        System.Console.WriteLine("nw");
                        System.Console.WriteLine("nw");
                    }else if (krokasyn_pom == 1)
                    {
                        System.Console.WriteLine("nw");
                        System.Console.WriteLine(potencialU);
                        System.Console.WriteLine("nw");
                    }
                    else if (krokasyn_pom == 2)
                    {
                        System.Console.WriteLine("nw");
                        System.Console.WriteLine("nw");
                        System.Console.WriteLine(potencialU);
                    }

                    System.Console.WriteLine("Potencjał wejściowy V: ");
                    System.Console.WriteLine(V.ToString("F1", "\t", "\r\n"));


                    // Obliczanie Energii ? całość do ogarnięcia jeszcze
                    e = 0;

                    for (int row = 0; row < 3; row++)
                    {
                        for (int cell = 0; cell < 3; cell++)
                        {


                            if (row == cell)
                                e += exampleMatrix[row, cell] * Math.Pow(V_pom.GetElement(row, 0), 2);
                            else
                                e += (exampleMatrix[row, cell] * V.GetElement(row, 0) * V.GetElement(cell, 0));

                        }
                    }

                    e = e * -0.5;

                    System.Console.WriteLine("Energia: " + e);

                    if (V_pom.Equals(V))
                    {
                        iterator++;
                    }
                    else
                    {
                        iterator = 0;
                    }

                    if (iterator >= 3)
                    {
                        System.Console.WriteLine("Siec sie ustabilizowala");
                        break;
                    }                    
                    V_pom = V;
                }                
            }
            System.Console.ReadKey();

        }
    }
}
