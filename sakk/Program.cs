using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

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
                    if (Util.rnd.Next(0, 100) < 5)
                    {
                        j.Allapota = Jatekos.Allapot.hazamegy;
                        Monitor.Pulse(ListaValaszto);
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
                Monitor.Wait(Mester.ListaValaszto);
            Allapota = Allapot.lep;
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
