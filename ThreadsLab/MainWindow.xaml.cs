using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO.Compression;
using System.Drawing;
using System.Windows.Forms;
using System.IO;
using System.Runtime.Intrinsics.X86;
using System.Net;
using System.Runtime.CompilerServices;

namespace lab5
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window 
    {
        [DllImport("Kernel32")]
        public static extern void AllocConsole();

        [DllImport("Kernel32")]
        public static extern void FreeConsole();
        public MainWindow()
        {
            InitializeComponent();
            AllocConsole();
        }
        // Exercise 1
        private double TakeN()
        {
            try
            {
                double k = (double)Int32.Parse(N.Text);
                return k;
            }
            catch (FormatException)
            {
                Console.WriteLine("Unable to parse N");
                Environment.Exit(1);
                return 0;
            }
        }

        private double Strong(double n)
        {
            double sum = 1;
            for (double i = 1; i <= n; i++) sum *= i;
            return sum;
        }
        private double TakeK()
        {
            try
            {
                double n = (double)Int32.Parse(K.Text);
                return n;
            }
            catch (FormatException)
            {
                Console.WriteLine("Unable to parse K");
                Environment.Exit(1);
                return 0;
            }
        }
        private void Ex1Tasks(object sender, EventArgs e)
        {
            double n = TakeN();
            double k = TakeK();
            Task<double> licznik = Task.Factory.StartNew<double>(
                (obj) =>
                {
                    return Strong(n);
                },
                100
                );
            Task<double> mianownik1 = Task.Factory.StartNew<double>(
                (obj) =>
                {
                    return Strong(k);
                },
                100
                );
            Task<double> mianownik2 = Task.Factory.StartNew<double>(
                (obj) =>
                {
                    return Strong(n - k);
                },
                100
                );
            ex1T.Text = (licznik.Result/(mianownik1.Result*mianownik2.Result)).ToString();
        }
        public static double D_licznik(double n, double k)
        {
            if (n <= 1) return 1;
            double sum = 1;
            for (double i = 1; i <= n; i++) sum *= i;
            return sum;
        }
        public static double D_mianownik(double n, double k)
        {
            if (n <= 1) return 1;
            double sum1 = 1;
            for (double i = 1; i <= n-k; i++) sum1 *= i;
            if (n <= 1) return 1;
            double sum2 = 1;
            for (double i = 1; i <= k; i++) sum2 *= i;
            return sum1*sum2;
        }
        private void Ex1Delegates(object sender, EventArgs e)
        {
            double n = TakeN();
            double k = TakeK();
            Func<double, double, double> l = D_licznik;
            Func<double, double, double> m = D_mianownik;
            Task<double> tl = Task.Run(() => l(n, k));
            Task<double> tm = Task.Run(() => m(n, k));
            ex1D.Text = ((double)tl.Result / (double)tm.Result).ToString();
        }

        private Task<double> Amianownik(double n, double k)
        {
            Task<double> mianownik = Task.Factory.StartNew<double>(
                (obj) =>
                {
                    return Strong(k) * Strong(n - k);
                },
                100
                );
            return mianownik;
        }

        private Task<double> Alicznik(double n, double k)
        {
            Task<double> licznik = Task.Factory.StartNew<double>(
                (obj) =>
                {
                    return Strong(n);
                },
                100
                );
            return licznik;
        }
        private async void Ex1Async(object sender, EventArgs e)
        {
            double n = TakeN();
            double k = TakeK();
            double licznik = await Alicznik(n, k);
            double mianownik = await Amianownik(n, k);
            ex1A.Text = ((double)licznik / (double)mianownik).ToString();

        }
        // Exercise 2
        private void Fibonacci(object sender, EventArgs e)
        {
            Fib_p.Value = 0;
            BackgroundWorker bw = new BackgroundWorker();
            bw.DoWork += ((object sender, DoWorkEventArgs args) =>
            {
                BackgroundWorker worker = sender as BackgroundWorker;
                double n = (double)args.Argument;
                double[] fib = new double[3];
                fib[0] = 1;
                fib[1] = 1;
                for (double i = 0; i < n - 2; i++)
                {
                    fib[2] = fib[0] + fib[1];
                    if (i % 2 == 0) fib[1] = fib[2];
                    else fib[0] = fib[2];
                    worker.ReportProgress((int)(i/n * 100));
                    Thread.Sleep(20);
                }
                args.Result = fib[2];
                
            });
            bw.ProgressChanged += ((object sender, ProgressChangedEventArgs args) =>
            {
                Fib_p.Value = args.ProgressPercentage;
            });
            bw.RunWorkerCompleted += ((object sender, RunWorkerCompletedEventArgs args) =>
            {
                Fib_result.Text = args.Result.ToString();
                Fib_p.Value = 100;
            });
            bw.WorkerReportsProgress = true;
            bw.RunWorkerAsync((double)Int32.Parse(Fib_i.Text));
        }

        // Exercise 3

        private void Comp(object sender, EventArgs e)
        {
            var dlg = new FolderBrowserDialog() { Description = "Select directory to open" };
            dlg.ShowDialog();

            string[] fileEntries = Directory.GetFiles(dlg.SelectedPath);
            
            Task[] tasks = new Task[fileEntries.Length];
            int i = 0;
            foreach (string fileEntry in fileEntries)
            {
                tasks[i] = Task.Run(() =>
                {
                    if (System.IO.Path.GetExtension(fileEntry) != ".gz")
                    {
                        string compressFileName = System.IO.Path.ChangeExtension(fileEntry, ".gz");
                        using FileStream originalFileStream = File.Open(fileEntry, FileMode.Open);
                        using FileStream compressedFileStream = File.Create(compressFileName);
                        using var compressor = new GZipStream(compressedFileStream, CompressionMode.Compress);
                        originalFileStream.CopyTo(compressor);
                    }
                });
                i++;
            }

            Task.WaitAll(tasks);
        }

        private void Decomp(object sender, EventArgs e)
        {
            var dlg = new FolderBrowserDialog() { Description = "Select directory to open" };
            dlg.ShowDialog();

            string[] fileEntries = Directory.GetFiles(dlg.SelectedPath);

            Task[] tasks = new Task[fileEntries.Length];
            int i = 0;
            foreach(string fileName in fileEntries)
            {
                tasks[i] = Task.Run(() =>
                {
                    if (System.IO.Path.GetExtension(fileName) == ".gz")
                    {
                        string decompressedFileName = System.IO.Path.ChangeExtension(fileName, ".txt");
                        using FileStream compressedFileStream = File.Open(fileName, FileMode.Open);
                        using FileStream outputFileStream = File.Create(decompressedFileName);
                        using var decompressor = new GZipStream(compressedFileStream, CompressionMode.Decompress);
                        decompressor.CopyTo(outputFileStream);
                    }
                });
                i++;
            }
            Task.WaitAll(tasks);
        }

        // Exercise 4
        string[] hostNames = { "www.microsoft.com", "www.apple.com",
            "www.google.com", "www.ibm.com", "cisco.netacad.net",
            "www.oracle.com", "www.nokia.com", "www.hp.com", "www.dell.com",
            "www.samsung.com", "www.toshiba.com", "www.siemens.com",
            "www.amazon.com", "www.sony.com", "www.canon.com", 
            "www.alcatel-lucent.com", "www.acer.com", "www.motorola.com" };
        private void Resolve(object sender, EventArgs e)
        {
            var addresses = hostNames
                .AsParallel()
                .Select(hostName => Dns.GetHostAddresses(hostName).FirstOrDefault())
                .ToList();
            int i = 0;
            foreach ( var address in addresses)
            {
                ResolveResult.Text += "\n" + hostNames[i] + " =>\n\t" + address.ToString();
                i++;
            }
        }
    }
}
