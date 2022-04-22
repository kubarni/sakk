using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace sakk
{
    class Program
    {
        static void Main(string[] args)
        {
            var js = Enumerable.Range(1, 20)
                .Select(i => new Jatekos()).ToList();
            var ms = Enumerable.Range(1, 3)
                .Select(i => new Mester()).ToList();

            var ts = js.Select(x => new Task(() =>
            {
                x.Folyamat(js);
            },TaskCreationOptions.LongRunning)).ToList();

            ts.AddRange(ms.Select(x => new Task(() =>
            {
                 x.Dolgozik(js);
            },TaskCreationOptions.LongRunning)).ToList());

            ts.Add(new Task(() =>
            {
                int ido = 0;
                while (js.Any(p => p.Allapota != Jatekos.Allapot.hazamegy))
                {
                    Console.Clear();
                    Console.WriteLine("Játékosok:");
                    foreach (var j in js)
                    {
                        Console.WriteLine(j);
                    }
                    Console.WriteLine("\nMesterek:");
                    foreach (var m in ms)
                    {
                        Console.WriteLine(m);
                    }
                    ido += 200;
                    Console.WriteLine("Indítás óta eltelt idő: "+ ido/1000.0 + " perc.");
                    Thread.Sleep(200);
                }

                Console.Clear();
                Console.WriteLine("VÉGE");
                Console.WriteLine("Összesen eltelt idő: "+ ido/1000.0 +" perc.");
            },TaskCreationOptions.LongRunning));

            ts.ForEach(t => t.Start());

            Console.ReadLine();
        }
    }

    class Mester
    {
        public static object ListaValaszto = new object();
        public enum Allapot
        {
            lep, raer
        }
        public Allapot Allapota { get; set; }
        public static int Nextid = 1;
        public int Id { get; set; }
        public Mester()
        {
            Allapota = Allapot.raer;
            Id = Nextid++;
        }

        public override string ToString()
        {
            return $"Id: {Id} Állapot: {Allapota}";
        }

        public void Dolgozik(List<Jatekos> jatekosok)
        {
            while (jatekosok.Any(p=>p.Allapota != Jatekos.Allapot.hazamegy))
            {
                Jatekos j;
                lock (ListaValaszto)
                {
                    j = jatekosok.Where(x => x.Allapota == Jatekos.Allapot.mesterre_var).FirstOrDefault();
                    if (j != null)
                    {
                        j.Allapota = Jatekos.Allapot.mester_lep;
                    }
                }
                if (j != null)
                {
                    Allapota = Allapot.lep;
                    Thread.Sleep(Util.rnd.Next(1000, 3001));
                    if (Util.rnd.Next(0, 100) < 95)
                    {
                        j.Allapota = Jatekos.Allapot.hazamegy;
                        lock (ListaValaszto)
                        {
                            if (jatekosok.Count(x=>(int)x.Allapota >= 1 && (int)x.Allapota <= 3) < 10)
                            {
                                Jatekos uj = jatekosok.Where(x => x.Allapota == Jatekos.Allapot.var).FirstOrDefault();
                                if (uj != null)
                                {
                                    uj.Allapota = Jatekos.Allapot.lep;
                                    Monitor.Pulse(ListaValaszto);
                                }
                            }
                        }
                    }
                    else
                    {
                        j.Allapota = Jatekos.Allapot.lep;
                        lock (j.lockObject)
                            Monitor.Pulse(j.lockObject);
                    }
                    Allapota = Allapot.raer;
                    Thread.Sleep(Util.rnd.Next(200, 501));
                }
            }
        }
    }

    class Jatekos
    {
        public object lockObject;
        public enum Allapot
        {
            var, lep, mesterre_var, mester_lep, hazamegy
        }
        public Allapot Allapota { get; set; }
        public static int Nextid = 1;
        public int Id { get; set; }

        public Jatekos()
        {
            Allapota = Allapot.var;
            Id = Nextid++;
            lockObject = new object();
        }

        public void Folyamat(List<Jatekos> jatekosok)
        {
            lock (Mester.ListaValaszto)
            {
                if (jatekosok.Count(x => (int)x.Allapota >= 1 && (int)x.Allapota <= 3) < 10)
                {
                    Jatekos uj = jatekosok.Where(x => x.Allapota == Jatekos.Allapot.var).FirstOrDefault();
                    if (uj != null)
                    {
                        uj.Allapota = Jatekos.Allapot.lep;
                    }
                }
                else
                {
                    Monitor.Wait(Mester.ListaValaszto);
                }
            }  
            while (Allapota != Allapot.hazamegy)
            {
                Thread.Sleep(Util.rnd.Next(1000, 10001));
                Allapota = Allapot.mesterre_var;
                lock (lockObject)
                    Monitor.Wait(lockObject);
            }
        }

        public override string ToString()
        {
            return $"Id: {Id} Állapot: {Allapota}";
        }

    }

    static public class Util
    {
        static public Random rnd = new Random();
    }
}
