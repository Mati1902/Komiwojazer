using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Komiwojazer
{
    class Sciezki //klasa ze ścieżkami do plików txt dla ułatwienia dostępu w metodach
    {
        public static string docelowasciezka;
        public static string sciezkawczytywanie;
    }

    class Program
    {
        static void Main(string[] args)
        {
            //ścieżka do pliku txt
            string execPath = Assembly.GetEntryAssembly().Location;
            int iloscznakowwsciezce = execPath.Length;
            string nowasciezka = execPath.Remove(iloscznakowwsciezce - 15);
            Sciezki.docelowasciezka = nowasciezka + @"wyniki.txt";
            Sciezki.sciezkawczytywanie = nowasciezka + @"wsp.txt";                        
            //plik zapisywany do lokalizacji w której uruchamiany był plik exe. domyślnie jest to Komiwojazer\Komiwojazer\bin\Debug
            double[,] tablica_wsp = { };
            try
            {
                tablica_wsp = odczytanieXY(); //wczytanie z pliku wsp.txt współrzędnych puntów
            }
            catch
            {
                Console.WriteLine("W folderze " + nowasciezka + " nie ma pliku wsp.txt");
                Console.ReadKey();
                Environment.Exit(0);
            }
            File.WriteAllText(Sciezki.docelowasciezka, String.Empty);
            int ilosc_miast = File.ReadAllLines(Sciezki.sciezkawczytywanie).Length;
            double[,] tablica_odl = new double[ilosc_miast,ilosc_miast];

            
            for (int i = 0; i < ilosc_miast; i++) //pętla tworząca macierz 'tablica_odl' zawierająca odległości pomiędzy poszczególnymi punktami
            {
                for (int j = 0; j< ilosc_miast; j++)
                {
                    if (i == j)
                    {
                        tablica_odl[i, j] = 0;
                    }
                    else
                    {
                        tablica_odl[i, j] = Math.Sqrt((Math.Pow((tablica_wsp[j, 0] - tablica_wsp[i, 0]), 2)) + (Math.Pow((tablica_wsp[j, 1] - tablica_wsp[i, 1]), 2))); //odległość jest obliczana ze wzoru Sqrt((x2-x1)^2+(y2-y1)^2)
                    }
                }
            }

            Console.WriteLine("Tablica odległości: (wymiary: " + ilosc_miast + "x" + ilosc_miast + ")"); 
            File.AppendAllText(Sciezki.docelowasciezka, "Tablica odległości: (wymiary: " + ilosc_miast + "x" + ilosc_miast + ")" + "\r\n"); 
            for (int i = 0; i < ilosc_miast; i++)
            {
                for (int j = 0; j < ilosc_miast; j++)
                {
                    Console.Write(tablica_odl[i, j] + " "); //wydrukowanie tablicy w konsoli
                    File.AppendAllText(Sciezki.docelowasciezka, tablica_odl[i, j] + " "); //zapisanie tablicy odległości do pliku
                }               
                File.AppendAllText(Sciezki.docelowasciezka, "\r\n");
                Console.WriteLine();
            }

            List<Int64> sciezki = mozliwe_drogi(ilosc_miast);

            Console.WriteLine("----------");
            Console.WriteLine("Możliwe ścieżki: ");

            foreach (Int64 p in sciezki)
            {
                Console.WriteLine(p + Convert.ToString(ilosc_miast - 1));
                File.AppendAllText(Sciezki.docelowasciezka,p + Convert.ToString(ilosc_miast-1)+ "\r\n");
            }

            char[,] kolejne_miasta_char = konwersja_na_char(sciezki, ilosc_miast);

            double[] odleglosci = new double[silnia(ilosc_miast-1)];
            double[] odcinki = new double[ilosc_miast];
            int poczatek_odcinka;
            int koniec_odcinka;
            double suma = 0;
            for(int i = 0; i < silnia(ilosc_miast - 1); i++)
            {
                for(int j = 0; j < ilosc_miast; j++)
                {
                    poczatek_odcinka = Convert.ToInt32(Convert.ToString(kolejne_miasta_char[i, j]));
                    koniec_odcinka = Convert.ToInt32(Convert.ToString(kolejne_miasta_char[i, j + 1]));
                    odcinki[j] = tablica_odl[poczatek_odcinka, koniec_odcinka];                    
                }
                suma = odcinki.Sum();
                odleglosci[i] = suma;
            }

            Console.WriteLine("----------");
            Console.WriteLine("Długość ścieżek: ");
            File.AppendAllText(Sciezki.docelowasciezka,"Długości ścieżek: " + "\r\n");
            foreach (double x in odleglosci)
            {
                Console.WriteLine(x);
                File.AppendAllText(Sciezki.docelowasciezka, x + "\r\n");
            }

            Console.WriteLine("----------");
            double najkrotsza_dlugosc = odleglosci.Min();
            int indeks = odleglosci.ToList().IndexOf(najkrotsza_dlugosc);
            File.AppendAllText(Sciezki.docelowasciezka, "Najkrótsza droga ma: " + "\r\n");
            Console.WriteLine("Najkrótsza droga ma: ");
            File.AppendAllText(Sciezki.docelowasciezka, najkrotsza_dlugosc + "\r\n");
            Console.WriteLine(najkrotsza_dlugosc);
            File.AppendAllText(Sciezki.docelowasciezka, "Kolejność odwiedzania punktów: " + "\r\n");
            Console.WriteLine("Kolejność odwiedzania punktów: ");
            File.AppendAllText(Sciezki.docelowasciezka, sciezki[indeks] + Convert.ToString(ilosc_miast - 1) + "\r\n");
            Console.WriteLine(sciezki[indeks] + Convert.ToString(ilosc_miast - 1));
            Console.WriteLine("Plik z wynikiem został zapisany w: " + Sciezki.docelowasciezka);
            Console.ReadKey();
        }

        public static char[,] konwersja_na_char(List<Int64> lista, int ilosc_miast) //metoda rozbijająca poszczególne ścieżki na tablicę zawierającą pojedyncze cyfry jako oznaczenie punktów
        {
            string linijka_str = "";
            char[] linijka_char = new char[ilosc_miast];
            char[,] kolejne_miasta_char = new char[silnia(ilosc_miast-1),ilosc_miast+1];
            for(int j = 0; j < silnia(ilosc_miast - 1); j++)
            {
                linijka_str = Convert.ToString(lista[j]);
                for (int i = 0; i < ilosc_miast; i++)
                {
                    kolejne_miasta_char[j,i] = linijka_str[i];
                }
                kolejne_miasta_char[j, ilosc_miast] = kolejne_miasta_char[j, 0];
            }

            return kolejne_miasta_char;
        }

        public static double[,] odczytanieXY() //wczytanie z pliku wsp.txt współrzędnych punktów i zapisanie ich do tablicy
        {
            string[] odczyt = File.ReadAllLines(Sciezki.sciezkawczytywanie);
            string[] linijki_str;
            string[,] tablica = new string[File.ReadAllLines(Sciezki.sciezkawczytywanie).Length,2];
            for (int i = 0; i < odczyt.Length; i++)
            {
                linijki_str = odczyt[i].Split(' ');
                tablica[i,0] = linijki_str[0];
                tablica[i,1] = linijki_str[1];
            }

            double[,] tablica_double = new double[File.ReadAllLines(Sciezki.sciezkawczytywanie).Length, 2]; ;
            for (int i = 0; i < (tablica.Length/2); i++)
            {
                tablica_double[i, 0] = konwertuj(tablica[i, 0]);
                tablica_double[i, 1] = konwertuj(tablica[i, 1]);
            }

            double[] wyniki = { 0 };
            return tablica_double;
        }

        public static int silnia(int n) //metoda zwracająca wynik silni z danej liczby
        {
            if(n<2)
            {
                return 1;
            }
            else
            {
                return n * silnia(n - 1);
            }
        }

        public static double konwertuj(string liczba) //metoda pozwalająca z pliku txt wczytywać zmienne typu double (jako separator występuje kropka '.')
        {

            string[] całk_przec;
            try
            {
                całk_przec = liczba.Split('.');
            }
            catch
            {
                całk_przec = liczba.Split(',');
            }
            double przed_przec = Convert.ToDouble(całk_przec[0]);
            int il_msc_po_przec;
            try
            {
                il_msc_po_przec = całk_przec[1].Length;
            }
            catch
            {
                il_msc_po_przec = 0;
            }

            double dzielnik = Math.Pow(10, il_msc_po_przec);
            double po_przec_zaduza;

            try
            {
                po_przec_zaduza = Convert.ToDouble(całk_przec[1]);
            }
            catch
            {
                po_przec_zaduza = 0;
            }

            double po_przec = po_przec_zaduza / dzielnik;
            double nowa_liczba = przed_przec + po_przec;

            return nowa_liczba;
        }

        public static List<Int64> mozliwe_drogi(int n) //metoda odsiewająca ścieżki które są prawidłowe (np. 43330 zostanie odrzucona, a 41230 nie)
        {
            //metoda sprawdza w pętli po kolei generowane malejąco liczby zaczynając od największej jaką można utworzyć z cyfr które odpowiadają poszczególnym punktom
            //np największą liczbą może być 543210
            //w liczbie takiej występują wszystkie cyfry (a więc odwiedzany jest każdy punkt) oraz każda występuje tylko raz
            //kolejna liczba o 1 mniejsza, a więc 543209 jest nieprawidłowa ze względu na występującą w niej cyfrę 9 która nie ma swojego odpowiednika wśród punktów współrzędnych
            //liczba taka zostaje odrzucona itd
            //pętla trwa aż do otrzymania liczby najmniejszej możliwej do uzyskania składającej się z odpowiednich cyfr
            //np dla 6 miast nawiększa liczba to 543210, a najmniejsza to 501234.
            n = n - 1;
            int liczba_porz = n;
            string n_str = Convert.ToString(n);
            for (int i = 0; i < liczba_porz; i++)
            {
                int kolejna_cyfra_int = n - 1;
                string kolejna_cyfra_str = Convert.ToString(kolejna_cyfra_int);
                n_str = n_str + kolejna_cyfra_str;
                n--;
            }

            Int64 max_liczba = Convert.ToInt64(n_str);
            string max_liczba_str = Convert.ToString(max_liczba);
            char[] tablica_cyfr = new char[max_liczba_str.Length];
            for (int i = 0; i < max_liczba_str.Length; i++)
            {
                tablica_cyfr[i] = max_liczba_str[i];
            }
            
            //tworzenie najmniejszej możliwej liczby do przerwania pętli
            string min_liczba_str = Convert.ToString(tablica_cyfr[0]);

            for (int i = 0; i < max_liczba_str.Length - 1; i++)
            {
                min_liczba_str = min_liczba_str + Convert.ToString(tablica_cyfr[(max_liczba_str.Length - 1) - i]);
            }

            Int64 min_liczba = Convert.ToInt64(min_liczba_str);
            Int64 warunek_min = min_liczba;
            Int64 warunek_max = max_liczba;
            Int64 liczba = max_liczba;
            string liczba_str;
            char[] liczba_char_tabl = new char[max_liczba_str.Length];
            Boolean[] tablica_prawdy = new Boolean[max_liczba_str.Length];

            for (int i = 0; i < max_liczba_str.Length; i++)
            {
                tablica_prawdy[i] = false;
            }

            
            int prawda = 0;
            int falsz = 0;
            int warunek = 0;
            List<Int64> lista = new List<Int64>();

            while (warunek_min <= warunek_max)
            {
                //zamiana liczby na stringa a potem na tablice charów
                liczba_str = Convert.ToString(liczba);
                for (int i = 0; i < max_liczba_str.Length; i++)
                {
                    liczba_char_tabl[i] = liczba_str[i];
                }

                for (int i = 0; i < max_liczba_str.Length; i++)
                {
                    char char_porownaniowy = tablica_cyfr[i];

                    for (int j = 0; j < max_liczba_str.Length; j++)
                    {
                        if (liczba_char_tabl[j] == char_porownaniowy)
                        {
                            prawda++;
                        }
                        else
                        {
                            falsz++;
                        }

                        if (prawda > 1)
                        {
                            break;
                        }
                    }

                    if (prawda == 1)
                    {
                        tablica_prawdy[i] = true;
                    }

                    prawda = 0;
                    falsz = 0;
                }

                for (int i = 0; i < tablica_prawdy.Length; i++)
                {
                    if (tablica_prawdy[i] == true)
                    {
                        warunek++;
                        
                    }
                }

                if (warunek == tablica_prawdy.Length)
                {
                    lista.Add(liczba);
                }

                warunek = 0;
                liczba--;
                warunek_min++;
                
                for (int i = 0; i < tablica_prawdy.Length; i++)
                {
                    tablica_prawdy[i] = false;
                }
            }
            return lista;
        }
    }
}
