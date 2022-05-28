using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;
using System.Media;
using System.Threading.Tasks;

namespace FFTforGenetic
{
    public partial class Form1 : Form
    {
        Convolution convolution;
        SoundPlayer simpleSound = new SoundPlayer(@"Data\\echosmith-over_my_head.wav");
        public Form1()
        {
            InitializeComponent();
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            convolution = new Convolution();

            openFileDialog1.FileName = "";
            openFileDialog1.Title = "Выберите файл:";
            saveFileDialog1.Filter = "Файл Gnuplot (*.plt)|.plt|Текстовые файлы (*.txt)|.txt";
            saveFileDialog1.InitialDirectory = "C:\\Users\\okvay\\Desktop";
            saveFileDialog1.Title = "Выберите место сохранения:";
            saveFileDialog1.FileName = "C:\\Users\\okvay\\Desktop\\Plot.plt";

            label1.Text = "";
            label2.Text = "";
            label3.Text = "";
            label4.Text = "";
            
            openFileDialog1.RestoreDirectory = false;
            saveFileDialog1.RestoreDirectory = false;

            ToolTip t = new ToolTip();
            t.SetToolTip(button1, "Выбрать файл в формате .gb/.fna/.fasta,\nсодержащий первую символьную последовательность");
            t.SetToolTip(button2, "Выбрать файл в формате .gb/.fna/.fasta,\nсодержащий вторую символьную последовательность");
            t.SetToolTip(checkBox1, "Отметьте, если хотите получить \"плоскую\" свёртку,\nт.е. свёртку за вычетом случайных совпадений");
            t.SetToolTip(button11, "Выбрать несколько файлов и вычислить свёртку каждого с каждым.\nРезультаты будут сохранены в подпапке той же директории");
            t.SetToolTip(button4, "Создать файл в формате .plt/.txt, содержащий последовательность элементов свёртки");
            t.SetToolTip(button9, "Выбрать в качестве входных последовательностей\nпоследние использованые и вычислить их свёртку");
            t.SetToolTip(button19, "Остановить воспроизведение аудиофайла");
            t.SetToolTip(button20, "Поиск выбранных транспозонов в геномах и формирование сводной таблицы");

            numericUpDown1.Visible = true;

            numericUpDown3.Visible = false;
            numericUpDown4.Visible = false;
            label5.Visible = false;
            label6.Visible = false;
        }
        private void Button1_Click(object sender, EventArgs e)//считать первую последовательность
        {
            openFileDialog1.Filter = "Все файлы (*.*)|*.*|FASTA format (*.fasta)|*.fasta|GenBank (*.gb)|*.gb|Универсальный формат (*.uniform)|*.uniform";
            if (openFileDialog1.ShowDialog() == DialogResult.Cancel)
                return;
            // получаем выбранный файл
            string filename = openFileDialog1.FileName;
            openFileDialog1.InitialDirectory = filename;
            label1.Text = filename;
            convolution.Create1stSequence(filename);
            label3.Text = "N = " + convolution.N;

            using (StreamWriter sw = new StreamWriter(@"Data\\last_main_chain.txt", false, Encoding.Default))
            {
                sw.Write(filename);
            }
        }
        private void Button2_Click(object sender, EventArgs e)//считать вторую последовательность
        {
            openFileDialog1.Filter = "Все файлы (*.*)|*.*|FASTA format (*.fasta)|*.fasta|GenBank (*.gb)|*.gb|Универсальный формат (*.uniform)|*.uniform";
            if (openFileDialog1.ShowDialog() == DialogResult.Cancel) return;

            string filename = openFileDialog1.FileName;
            label2.Text = filename;
            convolution.Create2ndSequence(filename);
            label4.Text = "L = " + convolution.L;

            using (StreamWriter sw = new StreamWriter(@"Data\\last_template.txt", false, Encoding.Default))
            {
                sw.Write(filename);
            }
        }
        private void Button3_Click(object sender, EventArgs e)//вычислить свёртку
        {
            Stopwatch time = new Stopwatch();
            time.Start();
            convolution.ConvolutionComputation();
            time.Stop();
            textBox1.Text += "На выполнение БПФ затрачено " + (time.ElapsedMilliseconds / 1000.0) + " сек." + Environment.NewLine;
        }
        private void button4_Click(object sender, EventArgs e)//вывести текущую свёртку в файл
        {
            if (saveFileDialog1.ShowDialog() == DialogResult.Cancel) return;

            string filename = saveFileDialog1.FileName;
            convolution.ToFile(filename);
            
        }
        private void button5_Click(object sender, EventArgs e)//локализация бисекцией
        {
            textBox1.Text += "N = " + convolution.N + ", L = " + convolution.L + Environment.NewLine;
            Stopwatch time = new Stopwatch();
            time.Start();
            convolution.Bisection(textBox1);
            time.Stop();
            //textBox.Text += "Всего на локализацию затрачено " + (time.ElapsedMilliseconds / 1000.0) + " сек.\r\n";
        }
        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            convolution.ChangeFlattingParametr(checkBox1.Checked);
        }
        private void button9_Click(object sender, EventArgs e)//взять последний пример
        {
            textBox1.Text = "";
            string filename = "";
            using (StreamReader sr = new StreamReader(@"Data\\last_main_chain.txt", Encoding.Default))
            {
                filename = sr.ReadLine();
            }
            label1.Text = filename;
            convolution.Create1stSequence(filename);
            label3.Text = "N = " + convolution.N;

            using (StreamReader sr = new StreamReader(@"Data\\last_template.txt", Encoding.Default))
            {
                filename = sr.ReadLine();
            }
            label2.Text = filename;
            convolution.Create2ndSequence(filename);
            label4.Text = "L = " + convolution.L;

            Button3_Click(this, new EventArgs());
        }
        private void button11_Click(object sender, EventArgs e)//сравнение всех со всеми
        {
            openFileDialog1.Multiselect = true;
            openFileDialog1.Filter = "Универсальный формат (*.uniform)|*.uniform|FASTA format (*.fasta)|*.fasta|GenBank (*.gb)|*.gb|Все файлы (*.*)|*.*";
            if (openFileDialog1.ShowDialog() == DialogResult.Cancel)
            {
                openFileDialog1.Multiselect = false;
                return;
            }
            Stopwatch time = new Stopwatch(), time_one_convolution = new Stopwatch();
            time.Start();
            string path_to_directory = openFileDialog1.FileNames[0].Substring(0, openFileDialog1.FileNames[0].LastIndexOf('\\') + 1) + "Results\\";
            if (!Directory.Exists(path_to_directory)) Directory.CreateDirectory(path_to_directory);

            using (StreamWriter sw = new StreamWriter(path_to_directory + "stats.txt", false, Encoding.Default))
            {
                sw.WriteLine("name1\tlen1\tname2\tlen2\tlen convolution\ttime (sec)\tmax value\tindex");
                for (int i = 0; i < (checkBox5.Checked ? openFileDialog1.FileNames.Length : openFileDialog1.FileNames.Length - 1); i++)
                {
                    for (int j = checkBox5.Checked ? i : i + 1; j < openFileDialog1.FileNames.Length; j++)
                    {
                        convolution.Create1stSequence(openFileDialog1.FileNames[i]);
                        convolution.Create2ndSequence(openFileDialog1.FileNames[j]);
                        time_one_convolution.Restart();
                        convolution.ConvolutionComputation(checkBox6.Checked, convolution.alphabet_capacity);
                        time_one_convolution.Stop();
                        int largest_peak = convolution.FindLargestPeak();
                        string filename = openFileDialog1.FileNames[i].Substring(openFileDialog1.FileNames[i].LastIndexOf('\\') + 1) + " + "
                                        + openFileDialog1.FileNames[j].Substring(openFileDialog1.FileNames[j].LastIndexOf('\\') + 1) + ".plt";
                        convolution.ToFile(path_to_directory + filename, new string[] { time_one_convolution.ElapsedMilliseconds / 1000.0 + " сек. затрачено." });
                        sw.WriteLine(openFileDialog1.FileNames[i].Substring(openFileDialog1.FileNames[i].LastIndexOf('\\') + 1) + "\t" + convolution.N + "\t" +
                                     openFileDialog1.FileNames[j].Substring(openFileDialog1.FileNames[j].LastIndexOf('\\') + 1) + "\t" + convolution.L + "\t" +
                                     Math.Pow(2, Math.Ceiling(Math.Log(convolution.N + convolution.L, 2))) + "\t" + time_one_convolution.ElapsedMilliseconds / 1000.0 + "\t" +
                                     convolution.result[largest_peak].real + "\t" + largest_peak);
                    }
                }
            }
                
            time.Stop();
            textBox1.Text += "Всего затрачено " + (time.ElapsedMilliseconds / 1000.0) + " сек.\r\n";
            
            openFileDialog1.Multiselect = false;
            //simpleSound.Play();
        }
        private void button12_Click(object sender, EventArgs e)//сделать текущую свёртку "плоской"
        {
            convolution.DrawCurrentFlat();
        }
        private void button17_Click(object sender, EventArgs e)//поменять последовательности местами
        {
            if ((label1.Text == "") || (label2.Text == "")) return;
            string temp = label1.Text;
            label1.Text = label2.Text;
            label2.Text = temp;

            convolution.Create1stSequence(label1.Text);
            label3.Text = "N = " + convolution.N;
            convolution.Create2ndSequence(label2.Text);
            label4.Text = "L = " + convolution.L;
        }
        private void button19_Click(object sender, EventArgs e)//stop
        {
            simpleSound.Stop();
        }
        private void button20_Click(object sender, EventArgs e)//транспозоны
        {
            openFileDialog1.Multiselect = true;
            openFileDialog1.Title = "Выберите файлы, содержащие транспозоны";
            if (openFileDialog1.ShowDialog() == DialogResult.Cancel)
            {
                openFileDialog1.Multiselect = false;
                return;
            }
            string[] transpozony = openFileDialog1.FileNames;
            openFileDialog1.Multiselect = false;

            if (folderBrowserDialog1.ShowDialog() == DialogResult.Cancel) return;
            Stopwatch time = new Stopwatch();
            time.Start();
            string[] divisios = new string[] { folderBrowserDialog1.SelectedPath };
            /*
            string[] divisios = new string[] { "C:\\Users\\okvay\\Desktop\\Оля Мутовина результаты\\Мхи\\Геномы",
                                               //"C:\\Users\\okvay\\Desktop\\Оля Мутовина результаты\\Папоротниковые\\Геномы",
                                               "C:\\Users\\okvay\\Desktop\\Оля Мутовина результаты\\Голосеменные\\Геномы",
                                               //"C:\\Users\\okvay\\Desktop\\Оля Мутовина результаты\\Покрытосеменные\\Геномы",
                                               "C:\\Users\\okvay\\Desktop\\Геномы от Тани Шпагиной\\Геномы"
                                             };
            */

            /* Составляем базу транспозонов */
            Dictionary<string, Tuple<int, char[]>> transpozone_base = new Dictionary<string, Tuple<int, char[]>>();
            foreach (string file in transpozony) using (StreamReader sr = new StreamReader(file, Encoding.Default))
                {
                    string name = sr.ReadLine().Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries)[1];
                    int len = Convert.ToInt32(sr.ReadLine().Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries)[1]);
                    HashSet<char> transpozone_alphabet = new HashSet<char>(sr.ReadLine().Split(new char[] { ' ', '\t' })[1].AsEnumerable());
                    string s, seq = "";
                    while ((s = sr.ReadLine()) != null) seq += s;
                    transpozone_base.Add(name, new Tuple<int, char[]>(len, seq.ToCharArray()));
                }
            foreach (string divisio in divisios)
            {
                string[] files = Directory.GetFiles(divisio, "*.uniform");
                //if (files.Length == 0) Directory.GetFiles(divisio, "*.uniform");
                string picture_folder = divisio.Substring(0, divisio.LastIndexOf('\\') + 1) + "Pictures",
                    comments_file = divisio.Substring(0, divisio.LastIndexOf('\\') + 1) + "Подробный отчет.txt",
                    table_file = divisio.Substring(0, divisio.LastIndexOf('\\') + 1) + "Итоговая таблица.txt";
                if (!Directory.Exists(picture_folder)) Directory.CreateDirectory(picture_folder);
                if (File.Exists(comments_file)) File.Delete(comments_file);

                /* Формируем сводную таблицу */
                using (StreamWriter sw = new StreamWriter(table_file, false, Encoding.Default))
                {
                    using (StreamWriter sw2 = new StreamWriter(divisio.Substring(0, divisio.LastIndexOf('\\') + 1) + "Макс.знач.txt", false, Encoding.Default))
                    {

                        sw.Write("Вид\tTE\tДлина\tНаправление\tЧисло копий");
                        sw.WriteLine();

                        sw2.Write("Вид\tTE\tДлина\tНаправление\tМакс.знач.");
                        sw2.WriteLine();
                        /* Начинаем искать */
                        foreach (string file in files)
                        {
                            string genome_name = file.Substring(file.LastIndexOf("\\") + 1);
                            convolution.Create1stSequence(file);
                            double MuDR_d = 0, MuDR_c = 0, Copia_d = 0, Copia_c = 0, MuDR_len = 0, Copia_len = 0;

                            foreach (string transpozone_name in transpozone_base.Keys)
                            {
                                char direction = transpozone_name[transpozone_name.LastIndexOf('(') + 1];
                                convolution.Create2ndSequence(transpozone_base[transpozone_name].Item2, transpozone_base[transpozone_name].Item1);
                                convolution.ConvolutionComputation();
                                convolution.ToFile(picture_folder + "\\" + genome_name + " + " + transpozone_name + ".plt");
                                double[] res1 = convolution.DetectTranspozone(comments_file, genome_name, transpozone_name);
                                if (transpozone_name.StartsWith("MuDR"))
                                {
                                    if (direction == 'd') MuDR_d += res1[0];
                                    else MuDR_c += res1[0];
                                    MuDR_len += res1.Sum() - res1[0];
                                }
                                else //Copia
                                {
                                    if (direction == 'd') Copia_d += res1[0];
                                    else Copia_c += res1[0];
                                    Copia_len += res1.Sum() - res1[0];
                                }
                                sw2.WriteLine(genome_name + "\t" + transpozone_name + "\t" + transpozone_base[transpozone_name].Item1 + "\t" + direction + "\t" + convolution.result[convolution.FindLargestPeak()].real);
                            }
                            if (MuDR_c + MuDR_d > 0)
                            {
                                string dir;
                                if ((MuDR_d > 0) && (MuDR_c > 0)) dir = "cd";
                                else if (MuDR_d > 0) dir = "d";
                                else dir = "c";
                                sw.WriteLine(genome_name + "\tMuDR-64_OS\t" + Math.Round(MuDR_len / (MuDR_c + MuDR_d)) + "\t" + dir + "\t" + (MuDR_c + MuDR_d));
                            }
                            if (Copia_c + Copia_d > 0)
                            {
                                string dir;
                                if ((Copia_d > 0) && (Copia_c > 0)) dir = "cd";
                                else if (Copia_d > 0) dir = "d";
                                else dir = "c";
                                sw.WriteLine(genome_name + "\tCopia-18_BD-I\t" + Math.Round(Copia_len / (Copia_d + Copia_c)) + "\t" + dir + "\t" + (Copia_c + Copia_d));
                            }
                        }
                    }
                }
                textBox1.Text += "Закончили с " + divisio + "\r\n";
            }
            time.Stop();
            textBox1.Text += "Всего затрачено " + (time.ElapsedMilliseconds / 1000.0) + " сек.\r\n";
            //simpleSound.Play();
        }

        private void button6_Click(object sender, EventArgs e)
        {
            Stopwatch time = new Stopwatch();
            openFileDialog1.Title = "Выберите файлы, которые необходимо сравнить между собой";
            openFileDialog1.Multiselect = true;

            int number_of_iterations = (int)numericUpDown2.Value;
            long[] times_seq = new long[number_of_iterations], times_parallel = new long[number_of_iterations], times_parallel_strong = new long[number_of_iterations];

            if (openFileDialog1.ShowDialog() == DialogResult.Cancel) return;
            string[] files = openFileDialog1.FileNames;
            string name1st, name2nd, folder = files[0].Substring(0, files[0].LastIndexOf('\\') + 1);
            if (!Directory.Exists(folder + "Results\\")) Directory.CreateDirectory(folder + "Results\\");

            if (checkBox4.Checked) //сравниваем попарно
            {
                string[] all_files = new string[files.Length];
                Array.Copy(files, all_files, files.Length);
                files = new string[2];
                for (int files_couple = 0; files_couple < all_files.Length - 1; files_couple += 2)
                {
                    Array.Copy(all_files, files_couple, files, 0, 2);
                    using (StreamWriter sw0 = new StreamWriter(folder + "Results\\common stats [" + files[0].Substring(files[0].LastIndexOf('\\') + 1) + " + " + files[1].Substring(files[1].LastIndexOf('\\') + 1) + "].txt", false, Encoding.Default))
                    {
                        sw0.WriteLine("thread count\tT_seq\tdeviation\tT^par_seq\tdeviation\tT_par\tdeviation\tT^par_par\tdeviation\tspeed up");
                        for (int i = 0; i < (checkBox2.Checked ? files.Length : files.Length - 1); i++)
                        {
                            convolution.Create1stSequence(files[i]);
                            name1st = files[i].Substring(files[i].LastIndexOf('\\') + 1);

                            for (int j = checkBox2.Checked ? i : i + 1; j < files.Length; j++)
                            {
                                long[] time_seq_par = new long[number_of_iterations];
                                convolution.Create2ndSequence(files[j]);
                                name2nd = files[j].Substring(files[j].LastIndexOf('\\') + 1);

                                for (int k = 0; k < number_of_iterations; k++)
                                {
                                    time.Restart();
                                    time_seq_par[k] = convolution.ConvolutionComputation()[' '];
                                    time.Stop();
                                    times_seq[k] = time.ElapsedMilliseconds;
                                }
                                GC.Collect();
                                if (checkBox3.Checked) convolution.ToFile(folder + "Results\\" + name1st + " + " + name2nd + " (seq).plt");
                                double exp_v_seq = times_seq.Sum(), exp2_v_seq = 0;
                                foreach (long l in times_seq) exp2_v_seq += l * l;
                                exp_v_seq /= number_of_iterations;
                                exp2_v_seq /= number_of_iterations;
                                double deviation_seq = Math.Sqrt(exp2_v_seq - exp_v_seq * exp_v_seq);

                                for (int thread_count = radioButton2.Checked ? (int)numericUpDown1.Value : (int)numericUpDown3.Value;
                                    thread_count <= (radioButton2.Checked ? (int)numericUpDown1.Value : (int)numericUpDown4.Value);
                                    thread_count++)
                                {
                                    for (int k = 0; k < number_of_iterations; k++)
                                    {
                                        time.Restart();
                                        times_parallel_strong[k] = convolution.ConvolutionComputation(true, thread_count)[' '];
                                        time.Stop();
                                        times_parallel[k] = time.ElapsedMilliseconds;
                                    }
                                    long exp_v_par = times_parallel.Sum(), exp2_v_par = 0,
                                        exp_v_par_strong = times_parallel_strong.Sum(), exp2_v_par_strong = 0,
                                        time_seq_par_average = time_seq_par.Sum(), time_seq_par_average2 = 0;
                                    foreach (long l in times_parallel) exp2_v_par += l * l;
                                    foreach (long l in times_parallel_strong) exp2_v_par_strong += l * l;
                                    foreach (long l in time_seq_par) time_seq_par_average2 += l * l;
                                    exp_v_par /= number_of_iterations;
                                    exp_v_par_strong /= number_of_iterations;
                                    exp2_v_par /= number_of_iterations;
                                    exp2_v_par_strong /= number_of_iterations;
                                    time_seq_par_average /= number_of_iterations;
                                    time_seq_par_average2 /= number_of_iterations;

                                    double deviation_par = Math.Sqrt(exp2_v_par - exp_v_par * exp_v_par),
                                        deviation_par_strong = Math.Sqrt(exp2_v_par_strong - exp_v_par_strong * exp_v_par_strong),
                                        deviation_seq_par = Math.Sqrt(time_seq_par_average2 - time_seq_par_average * time_seq_par_average);
                                    if (checkBox3.Checked) convolution.ToFile(folder + "Results\\" + name1st + " + " + name2nd + " (par).plt");

                                    using (StreamWriter sw = new StreamWriter(folder + "Results\\Times " + thread_count + " threads.txt", (i == 0) && (j == (checkBox2.Checked ? i : i + 1)) ? false : true, Encoding.Default))
                                    {
                                        if ((i == 0) && (j == (checkBox2.Checked ? i : i + 1))) sw.WriteLine("1st seq\tlen\t2nd seq\tlen\ttime seq\tdeviation\ttime par (T_0)\tdeviation\ttime par (T_par)\tdeviation\tspeed up");

                                        sw.WriteLine(name1st + "\t" + convolution.N + "\t" + name2nd + "\t" + convolution.L + "\t"
                                            + exp_v_seq + "\t" + deviation_seq + "\t"
                                            + exp_v_par + "\t" + deviation_par + "\t"
                                            + exp_v_par_strong + "\t" + deviation_par_strong + "\t" + (exp_v_seq / exp_v_par));
                                    }
                                    if ((i == 0) && (j == 1))
                                        sw0.WriteLine(thread_count + "\t" + exp_v_seq + "\t" + deviation_seq + "\t"
                                                + time_seq_par_average + "\t" + deviation_seq_par + "\t"
                                                + exp_v_par + "\t" + deviation_par + "\t"
                                                + exp_v_par_strong + "\t" + deviation_par_strong + "\t" + (exp_v_seq / exp_v_par));
                                }
                                //for (int thread_count = (int)numericUpDown3.Value; thread_count <= (int)numericUpDown4.Value; thread_count++)
                                //{

                                //}
                            }
                        }
                    }

                }
            }
            else //все со всеми
            {
                using (StreamWriter sw0 = new StreamWriter(folder + "Results\\common stats [" + files[0].Substring(files[0].LastIndexOf('\\') + 1) + " + " + files[1].Substring(files[1].LastIndexOf('\\') + 1) + "].txt", false, Encoding.Default))
                {
                    sw0.WriteLine("thread count\tT_seq\tdeviation\tT^par_seq\tdeviation\tT_par\tdeviation\tT^par_par\tdeviation\tspeed up");
                    for (int i = 0; i < (checkBox2.Checked ? files.Length : files.Length - 1); i++)
                    {
                        convolution.Create1stSequence(files[i]);
                        name1st = files[i].Substring(files[i].LastIndexOf('\\') + 1);

                        for (int j = checkBox2.Checked ? i : i + 1; j < files.Length; j++)
                        {
                            long[] time_seq_par = new long[number_of_iterations];
                            convolution.Create2ndSequence(files[j]);
                            name2nd = files[j].Substring(files[j].LastIndexOf('\\') + 1);

                            for (int k = 0; k < number_of_iterations; k++)
                            {
                                time.Restart();
                                time_seq_par[k] = convolution.ConvolutionComputation()[' '];
                                time.Stop();
                                times_seq[k] = time.ElapsedMilliseconds;
                            }
                            GC.Collect();
                            if (checkBox3.Checked) convolution.ToFile(folder + "Results\\" + name1st + " + " + name2nd + " (seq).plt");
                            double exp_v_seq = times_seq.Sum(), exp2_v_seq = 0;
                            foreach (long l in times_seq) exp2_v_seq += l * l;
                            exp_v_seq /= number_of_iterations;
                            exp2_v_seq /= number_of_iterations;
                            double deviation_seq = Math.Sqrt(exp2_v_seq - exp_v_seq * exp_v_seq);

                            for (int thread_count = radioButton2.Checked ? (int)numericUpDown1.Value : (int)numericUpDown3.Value; 
                                thread_count <= (radioButton2.Checked ? (int)numericUpDown1.Value : (int)numericUpDown4.Value); 
                                thread_count++)
                            {
                                for (int k = 0; k < number_of_iterations; k++)
                                {
                                    time.Restart();
                                    times_parallel_strong[k] = convolution.ConvolutionComputation(true, thread_count)[' '];
                                    time.Stop();
                                    times_parallel[k] = time.ElapsedMilliseconds;
                                }
                                long exp_v_par = times_parallel.Sum(), exp2_v_par = 0,
                                    exp_v_par_strong = times_parallel_strong.Sum(), exp2_v_par_strong = 0,
                                    time_seq_par_average = time_seq_par.Sum(), time_seq_par_average2 = 0;
                                foreach (long l in times_parallel) exp2_v_par += l * l;
                                foreach (long l in times_parallel_strong) exp2_v_par_strong += l * l;
                                foreach (long l in time_seq_par) time_seq_par_average2 += l * l;
                                exp_v_par /= number_of_iterations;
                                exp_v_par_strong /= number_of_iterations;
                                exp2_v_par /= number_of_iterations;
                                exp2_v_par_strong /= number_of_iterations;
                                time_seq_par_average /= number_of_iterations;
                                time_seq_par_average2 /= number_of_iterations;

                                double deviation_par = Math.Sqrt(exp2_v_par - exp_v_par * exp_v_par),
                                    deviation_par_strong = Math.Sqrt(exp2_v_par_strong - exp_v_par_strong * exp_v_par_strong),
                                    deviation_seq_par = Math.Sqrt(time_seq_par_average2 - time_seq_par_average * time_seq_par_average);
                                if (checkBox3.Checked) convolution.ToFile(folder + "Results\\" + name1st + " + " + name2nd + " (par).plt");

                                using (StreamWriter sw = new StreamWriter(folder + "Results\\Times " + thread_count + " threads.txt", (i == 0) && (j == (checkBox2.Checked ? i : i + 1)) ? false : true, Encoding.Default))
                                {
                                    if ((i == 0) && (j == (checkBox2.Checked ? i : i + 1))) sw.WriteLine("1st seq\tlen\t2nd seq\tlen\ttime seq\tdeviation\ttime par (T_0)\tdeviation\ttime par (T_par)\tdeviation\tspeed up");

                                    sw.WriteLine(name1st + "\t" + convolution.N + "\t" + name2nd + "\t" + convolution.L + "\t"
                                        + exp_v_seq + "\t" + deviation_seq + "\t"
                                        + exp_v_par + "\t" + deviation_par + "\t"
                                        + exp_v_par_strong + "\t" + deviation_par_strong + "\t" + (exp_v_seq / exp_v_par));
                                }
                                if ((i == 0) && (j == 1))
                                    sw0.WriteLine(thread_count + "\t" + exp_v_seq + "\t" + deviation_seq + "\t"
                                            + time_seq_par_average + "\t" + deviation_seq_par + "\t"
                                            + exp_v_par + "\t" + deviation_par + "\t"
                                            + exp_v_par_strong + "\t" + deviation_par_strong + "\t" + (exp_v_seq / exp_v_par));
                            }
                            //for (int thread_count = (int)numericUpDown3.Value; thread_count <= (int)numericUpDown4.Value; thread_count++)
                            //{
                                
                            //}
                        }
                    }
                }

            }
            textBox1.Text += "Готово!" + Environment.NewLine;

        }

        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton2.Checked)
            {
                numericUpDown1.Visible = true;

                numericUpDown3.Visible = false;
                numericUpDown4.Visible = false;
                label5.Visible = false;
                label6.Visible = false;
            }
            else
            {
                numericUpDown1.Visible = false;

                numericUpDown3.Visible = true;
                numericUpDown4.Visible = true;
                label5.Visible = true;
                label6.Visible = true;
            }
            
        }

        private void numericUpDown3_ValueChanged(object sender, EventArgs e)
        {
            if (numericUpDown4.Value < numericUpDown3.Value) numericUpDown4.Value = numericUpDown3.Value;
        }

        private void numericUpDown4_ValueChanged(object sender, EventArgs e)
        {
            if (numericUpDown4.Value < numericUpDown3.Value) numericUpDown3.Value = numericUpDown4.Value;
        }

        private void button7_Click(object sender, EventArgs e)
        {
            Stopwatch time = new Stopwatch();
            time.Start();
            convolution.ConvolutionComputation(true, 2);
            time.Stop();
            textBox1.Text += "На выполнение БПФ затрачено " + (time.ElapsedMilliseconds / 1000.0) + " сек." + Environment.NewLine;
        }

        private void button8_Click(object sender, EventArgs e)
        {
            openFileDialog1.Multiselect = false;
            openFileDialog1.Title = "Выберите первую последовательность";
            if (openFileDialog1.ShowDialog() == DialogResult.Cancel) return;
            string seq1 = openFileDialog1.FileName;
            openFileDialog1.Multiselect = true;
            openFileDialog1.Title = "Выберите остальные последовательности";
            openFileDialog1.Filter = "FASTA format (*.fasta)|*.fasta|GenBank (*.gb)|*.gb|Универсальный формат (*.uniform)|*.uniform|Все файлы (*.*)|*.*";
            if (openFileDialog1.ShowDialog() == DialogResult.Cancel)
            {
                openFileDialog1.Multiselect = false;
                return;
            }
            Stopwatch time = new Stopwatch(), time_one_convolution = new Stopwatch();
            time.Start();
            string path_to_directory = seq1.Substring(0, seq1.LastIndexOf('\\') + 1) + "Results\\";
            if (!Directory.Exists(path_to_directory)) Directory.CreateDirectory(path_to_directory);

            bool is_append;
            if (File.Exists(path_to_directory + "stats.txt")) is_append = true;
            else is_append = false;
            using (StreamWriter sw = new StreamWriter(path_to_directory + "stats.txt", is_append, Encoding.Default))
            {
                if (!is_append) sw.WriteLine("name1\tlen1\tname2\tlen2\tlen convolution\ttime (sec)\tmax value\tindex");
                convolution.Create1stSequence(seq1);
                for (int i = 0; i < openFileDialog1.FileNames.Length; i++)
                {
                    convolution.Create2ndSequence(openFileDialog1.FileNames[i]);
                    time_one_convolution.Restart();
                    convolution.ConvolutionComputation(checkBox6.Checked, convolution.alphabet_capacity);
                    time_one_convolution.Stop();

                    int largest_peak = convolution.FindLargestPeak();
                    string filename = seq1.Substring(seq1.LastIndexOf('\\') + 1) + " + "
                                    + openFileDialog1.FileNames[i].Substring(openFileDialog1.FileNames[i].LastIndexOf('\\') + 1) + ".plt";
                    convolution.ToFile(path_to_directory + filename, new string[] { time_one_convolution.ElapsedMilliseconds / 1000.0 + " сек. затрачено." });
                    sw.WriteLine(seq1.Substring(seq1.LastIndexOf('\\') + 1) + "\t" + convolution.N + "\t" +
                                 openFileDialog1.FileNames[i].Substring(openFileDialog1.FileNames[i].LastIndexOf('\\') + 1) + "\t" + convolution.L + "\t" +
                                 Math.Pow(2, Math.Ceiling(Math.Log(convolution.N + convolution.L, 2))) + "\t" + time_one_convolution.ElapsedMilliseconds / 1000.0 + "\t" +
                                 convolution.result[largest_peak].real + "\t" + largest_peak);
                }
            }

            time.Stop();
            textBox1.Text += "Всего затрачено " + (time.ElapsedMilliseconds / 1000.0) + " сек.\r\n";

            openFileDialog1.Multiselect = false;
        }

        private void button10_Click(object sender, EventArgs e)
        {

        }

        private void button10_Click_1(object sender, EventArgs e)
        {
            openFileDialog1.Multiselect = true;
            openFileDialog1.Title = "Выберите первый блок последовательностей";
            if (openFileDialog1.ShowDialog() == DialogResult.Cancel) return;
            string[] seq1 = openFileDialog1.FileNames;
            openFileDialog1.Title = "Выберите второй блок последовательностей";
            openFileDialog1.Filter = "Все файлы (*.*)|*.*";
            if (openFileDialog1.ShowDialog() == DialogResult.Cancel)
            {
                openFileDialog1.Multiselect = false;
                return;
            }
            Stopwatch time = new Stopwatch(), time_one_convolution = new Stopwatch();
            time.Start();
            string path_to_directory = seq1[0].Substring(0, seq1[0].LastIndexOf('\\') + 1) + "Results\\";
            if (!Directory.Exists(path_to_directory)) Directory.CreateDirectory(path_to_directory);
            bool is_append;
            if (File.Exists(path_to_directory + "stats.txt")) is_append = true;
            else is_append = false;
            using (StreamWriter sw = new StreamWriter(path_to_directory + "stats.txt", is_append, Encoding.Default))
            {
                if (!is_append) sw.WriteLine("name1\tlen1\tname2\tlen2\tlen convolution\ttime (sec)\tmax value\tindex");
                for (int j = 0; j < seq1.Length; j++)
                {
                    convolution.Create1stSequence(seq1[j]);
                    for (int i = 0; i < openFileDialog1.FileNames.Length; i++)
                    {
                        convolution.Create2ndSequence(openFileDialog1.FileNames[i]);
                        time_one_convolution.Restart();
                        convolution.ConvolutionComputation();
                        time_one_convolution.Stop();

                        int largest_peak = convolution.FindLargestPeak();
                        string filename = seq1[j].Substring(seq1[j].LastIndexOf('\\') + 1) + " + "
                                        + openFileDialog1.FileNames[i].Substring(openFileDialog1.FileNames[i].LastIndexOf('\\') + 1) + ".plt";
                        convolution.ToFile(path_to_directory + filename, new string[] { time_one_convolution.ElapsedMilliseconds / 1000.0 + " сек. затрачено." });
                        sw.WriteLine(seq1[j].Substring(seq1[j].LastIndexOf('\\') + 1) + "\t" + convolution.N + "\t" +
                                     openFileDialog1.FileNames[i].Substring(openFileDialog1.FileNames[i].LastIndexOf('\\') + 1) + "\t" + convolution.L + "\t" +
                                     Math.Pow(2, Math.Ceiling(Math.Log(convolution.N + convolution.L, 2))) + "\t" + time_one_convolution.ElapsedMilliseconds / 1000.0 + "\t" +
                                     convolution.result[largest_peak].real + "\t" + largest_peak);
                    }
                }
            }

            time.Stop();
            textBox1.Text += "Всего затрачено " + (time.ElapsedMilliseconds / 1000.0) + " сек.\r\n";

            openFileDialog1.Multiselect = false;
        }
    }
    public struct Complex : ICloneable
    {
        public double real, imaginary;
        public Complex(double a, double b)
        {
            this.real = a;
            this.imaginary = b;
        }
        public static Complex operator +(Complex c1, Complex c2)
        {
            return new Complex(c1.real + c2.real, c1.imaginary + c2.imaginary);
        }
        public static Complex operator -(Complex c1, Complex c2)
        {
            return new Complex(c1.real - c2.real, c1.imaginary - c2.imaginary);
        }
        public static Complex operator *(Complex c1, Complex c2)
        {
            return new Complex(c1.real * c2.real - c1.imaginary * c2.imaginary, c1.real * c2.imaginary + c1.imaginary * c2.real);
        }
        public static Complex operator /(Complex c1, double c2)
        {
            return new Complex(c1.real / c2, c1.imaginary / c2);
        }
        public static Complex operator /(Complex c1, int c2)
        {
            return new Complex(c1.real / c2, c1.imaginary / c2);
        }
        public object Clone()
        {
            return new Complex { real = this.real, imaginary = this.imaginary };
        }
    }
    public struct Pair
    {
        public double ro;
        public int index_of_largest_peak, estimated_length, intersection_length, N1, N2, L1, L2;
        public int index1, N1_1, N2_1, L1_1, L2_1, intersection_length1,
                   index2, N1_2, N2_2, L1_2, L2_2, intersection_length2;
    }
    public class Convolution
    {
        private HashSet<char> alphabet, default_alphabet;
        public int alphabet_capacity
        {
            get => alphabet.Count;
        }
        public int N = 0, L = 0;
        public char[] main_chain, template;
        public Complex[] result;

        private int final_length;
        private int[] rev;
        private Complex[] byte_chain, template_byte_chain;

        private int MAX_INSERTION = 5;
        private int estimated_length = 0;
        private double ro = 0, expected_value = 0, standart_deviation = 0;
        private bool is_flat;
        private string path_to_main = "", path_to_template = "";
        public Convolution()
        {
            this.alphabet = new HashSet<char>();
        }
        public void ChangeFlattingParametr(bool f)//меняет значение флага is_flat
        {
            if (f) is_flat = true;
            else is_flat = false;
        }
        public void DrawCurrentFlat(string path = "")//вывести текущую свёртку в файл "плоской"
        {
            if (result == null)
            {
                MessageBox.Show("Еще не вычислена свёртка!", "Ошибка", MessageBoxButtons.OK);
                return;
            }
            ro = GetProbability();
            if (path == "") path = "C:\\Users\\okvay\\Desktop\\Current Flat.plt";
            double Ex = 0, Ex2 = 0, Dx;
            double[] arr = new double[N + L - 1];
            for (int i = 0; i < N + L - 1; i++)
            {
                if (i <= L - 1) arr[i] = result[i].real - ro * (i + 1) / L;
                else if (i <= N - 1) arr[i] = result[i].real - ro;
                else arr[i] = result[i].real - ro * (N + L - 1 - i) / L;
                Ex += arr[i];
                Ex2 += arr[i] * arr[i];
            }
            Ex /= (N + L - 1);
            Ex2 /= (N + L - 1);
            Dx = Ex2 - Ex * Ex;
            ToFile(path, arr, new string[] { "Ex = " + Ex, "Dx = " + Dx });
        }
        public void Create1stSequence(string path)//считать первую последовательность из файла
        {
            path_to_main = path;
            HashSet<char> current_alphabet = new HashSet<char>(0);
            Encoding encoding;
            //if (path.EndsWith(".uniform")) encoding = Encoding.Unicode;
            //else encoding = Encoding.Default;
            encoding = Encoding.Default;
            using (StreamReader sr = new StreamReader(path, encoding))
            {
                if (path.EndsWith(".uniform"))
                {
                    sr.ReadLine();
                    N = Convert.ToInt32(sr.ReadLine().Split(new char[] { ' ', '\t' })[1]);
                    current_alphabet = new HashSet<char>(sr.ReadLine().Split(new char[] { ' ', '\t' })[1].AsEnumerable());
                    main_chain = new char[N];
                    int i = 0;
                    do
                    {
                        string st = sr.ReadLine();
                        foreach (char c in st) if (current_alphabet.Contains(c)) main_chain[i++] = c;
                            else if (char.IsLetter(c)) main_chain[i++] = ' ';
                    } while (i < N);
                }
                else if (path.EndsWith(".txt"))
                {
                    current_alphabet = default_alphabet;
                    string s;
                    while (!(s = sr.ReadLine()).StartsWith("SQ")) ;
                    s = s.Substring(s.IndexOfAny(new char[] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' }));
                    int k;
                    if ((k = s.IndexOfAny(new char[] { ' ', '\n', '\0', ',', ';', ':', '\r' })) >= 0) s = s.Substring(0, k);
                    N = Convert.ToInt32(s);
                    main_chain = new char[N];
                    int i = 0;
                    do
                    {
                        char c = Convert.ToChar(sr.Read());
                        if (current_alphabet.Contains(c)) main_chain[i++] = c;
                        else if (Char.IsLetter(c)) main_chain[i++] = ' ';
                    } while (i < N);
                }
                else if (path.EndsWith(".gb"))
                {
                    current_alphabet = default_alphabet;
                    string s = sr.ReadLine();
                    s = s.Substring(0, s.IndexOf("bp") - 1);
                    s = s.Substring(s.LastIndexOf(' ') + 1);
                    N = Convert.ToInt32(s);
                    main_chain = new char[N];
                    while (!sr.ReadLine().Contains("ORIGIN")) ;
                    int i = 0;
                    do
                    {
                        if ((s = sr.ReadLine().ToLower()) != null) foreach (char c in s) if (current_alphabet.Contains(c)) main_chain[i++] = c;
                                else if (char.IsLetter(c)) main_chain[i++] = ' ';
                    } while (i < N);
                }
                else if (path.EndsWith(".fasta"))
                {
                    current_alphabet = default_alphabet;
                    string s = sr.ReadLine();
                    int current_count = 0;
                    main_chain = new char[10000];
                    while ((s = sr.ReadLine()) != null)
                    {
                        if (current_count + s.Length > main_chain.Length) Array.Resize(ref main_chain, main_chain.Length + 10000);
                        Array.Copy(s.ToLower().ToCharArray(), 0, main_chain, current_count, s.Length);
                        current_count += s.Length;
                    }
                    Array.Resize(ref main_chain, current_count);
                    N = current_count;
                }
                else
                {
                    MessageBox.Show("Неверное расширение файла!");
                }
                alphabet.UnionWith(current_alphabet);
            }
        }
        public void Create2ndSequence(string path)//считать вторую последовательность из файла
        {
            path_to_template = path;
            HashSet<char> current_alphabet = new HashSet<char>(0);
            Encoding encoding;
            //if (path.EndsWith(".uniform")) encoding = Encoding.Unicode;
            //else encoding = Encoding.Default;
            encoding = Encoding.Default;
            using (StreamReader sr = new StreamReader(path, encoding))
            {
                if (path.EndsWith(".uniform"))
                {
                    sr.ReadLine();
                    L = Convert.ToInt32(sr.ReadLine().Split(new char[] { ' ', '\t' })[1]);
                    current_alphabet = new HashSet<char>(sr.ReadLine().Split(new char[] { ' ', '\t' })[1].AsEnumerable());
                    template = new char[L];
                    int i = 0;
                    do
                    {
                        string st = sr.ReadLine();
                        foreach (char c in st) if (current_alphabet.Contains(c)) template[i++] = c;
                            else template[i++] = ' ';
                    } while (i < L);
                }
                else if (path.EndsWith(".txt"))
                {
                    current_alphabet = default_alphabet;
                    string s;
                    while (!(s = sr.ReadLine()).StartsWith("SQ")) ;
                    s = s.Substring(s.IndexOfAny(new char[] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' }));
                    int k;
                    if ((k = s.IndexOfAny(new char[] { ' ', '\n', '\0', ',', ';', ':', '\r' })) >= 0) s = s.Substring(0, k);
                    L = Convert.ToInt32(s);
                    template = new char[L];
                    int i = 0;
                    do
                    {
                        char c = Convert.ToChar(sr.Read());
                        if (current_alphabet.Contains(c)) template[i++] = c;
                        else if (Char.IsLetter(c)) template[i++] = ' ';
                    } while (i < L);
                }
                else if (path.EndsWith(".gb"))
                {
                    current_alphabet = default_alphabet;
                    string s = sr.ReadLine();
                    s = s.Substring(0, s.IndexOf("bp") - 1);
                    s = s.Substring(s.LastIndexOf(' ') + 1);
                    L = Convert.ToInt32(s);
                    template = new char[L];
                    while (!sr.ReadLine().Contains("ORIGIN")) ;
                    int i = 0;
                    do
                    {
                        if ((s = sr.ReadLine().ToLower()) != null) foreach (char c in s) if (current_alphabet.Contains(c)) template[i++] = c;
                                else if (char.IsLetter(c)) template[i++] = ' ';
                    } while (i < L);
                }
                else if (path.EndsWith(".fasta"))
                {
                    current_alphabet = default_alphabet;
                    string s = sr.ReadLine();
                    int current_count = 0;
                    template = new char[10000];
                    while ((s = sr.ReadLine()) != null)
                    {
                        if (current_count + s.Length > template.Length) Array.Resize(ref template, template.Length + 10000);
                        Array.Copy(s.ToLower().ToCharArray(), 0, template, current_count, s.Length);
                        current_count += s.Length;
                    }
                    Array.Resize(ref template, current_count);
                    L = current_count;
                }
                else
                {
                    MessageBox.Show("Неверное расширение файла!");
                }
                alphabet.UnionWith(current_alphabet);
            }
        }
        public void Create2ndSequence(char[] arr, int l)//считать вторую последовательность из arr
        {
            template = new char[L = l];
            Array.Copy(arr, template, L);

        }
        public Dictionary<char, long> ConvolutionComputation(bool is_parallel = false, int thread_count = 1) //вычисление полноценной свёртки
        {
            if ((main_chain == null) || (template == null))
            {
                MessageBox.Show("Еще не выбраны файлы!");
                return null;
            }
            Stopwatch t_par = new Stopwatch();
            t_par.Reset();
            Dictionary<char, long> common_times = new Dictionary<char, long>(0);

            final_length = Convert.ToInt32(Math.Pow(2, Math.Ceiling(Math.Log(N + L, 2))));
            rev = new int[final_length];
            int log_n = 0;
            while ((1 << log_n) < final_length) ++log_n;
            for (int i = 0; i < final_length; ++i)
            {
                rev[i] = 0;
                for (int j = 0; j < log_n; ++j)
                    if ((i & (1 << j)) != 0)
                        rev[i] |= 1 << (log_n - 1 - j);
            }

            if (!is_parallel) //вычисляем последовательно
            {
                byte_chain = new Complex[final_length];
                template_byte_chain = new Complex[final_length];

                result = new Complex[final_length];
                Complex[] temp_result = new Complex[final_length];
                for (int i = 0; i < final_length; i++) result[i] = new Complex(0, 0);
                
                foreach (char c in alphabet)
                { 
                    for (int i = 0; i < N; i++) if (main_chain[i] == c) byte_chain[i] = new Complex(1, 0);
                        else byte_chain[i] = new Complex(0, 0);
                    for (int i = N; i < final_length; i++) byte_chain[i] = new Complex(0, 0);
                    for (int i = 0; i < L; i++) if (template[i] == c) template_byte_chain[L - 1 - i] = new Complex(1, 0);
                        else template_byte_chain[L - 1 - i] = new Complex(0, 0);
                    for (int i = L; i < final_length; i++) template_byte_chain[i] = new Complex(0, 0);

                    t_par.Start();
                    FFT(byte_chain, false);
                    FFT(template_byte_chain, false);
                    for (int i = 0; i < final_length; i++) temp_result[i] = byte_chain[i] * template_byte_chain[i];
                    //for (int i = 0; i < final_length; i++) result[i] += byte_chain[i] * template_byte_chain[i];
                    t_par.Stop();

                    for (int i = 0; i < final_length; i++) result[i] += temp_result[i];
                }
                FFT(result, true);
            }
            else //вычисляем параллельно
            {
                Dictionary<char, Complex[]> all_main_byte_chains = new Dictionary<char, Complex[]>(0), all_template_byte_chains = new Dictionary<char, Complex[]>(0);
                Dictionary<char, Complex[]> res_separately = new Dictionary<char, Complex[]>(0);
                result = new Complex[final_length];
                for (int i = 0; i < final_length; i++) result[i] = new Complex(0, 0);

                foreach (char c in alphabet)
                {
                    byte_chain = new Complex[final_length];
                    template_byte_chain = new Complex[final_length];
                    for (int i = 0; i < N; i++) if (main_chain[i] == c) byte_chain[i] = new Complex(1, 0);
                        else byte_chain[i] = new Complex(0, 0);
                    for (int i = N; i < final_length; i++) byte_chain[i] = new Complex(0, 0);
                    for (int i = 0; i < L; i++) if (template[i] == c) template_byte_chain[L - 1 - i] = new Complex(1, 0);
                        else template_byte_chain[L - 1 - i] = new Complex(0, 0);
                    for (int i = L; i < final_length; i++) template_byte_chain[i] = new Complex(0, 0);
                    all_main_byte_chains.Add(c, byte_chain);
                    all_template_byte_chains.Add(c, template_byte_chain);
                    res_separately.Add(c, new Complex[final_length]);
                }
                
                if (thread_count > 0)
                {
                    t_par.Restart();
                    Parallel.ForEach(alphabet, new ParallelOptions { MaxDegreeOfParallelism = thread_count }, c => FFT_ForOneSymbol(all_main_byte_chains[c], all_template_byte_chains[c], res_separately[c]));
                    t_par.Stop();
                } 
                else
                {
                    t_par.Restart();
                    Parallel.ForEach(alphabet, c => FFT_ForOneSymbol(all_main_byte_chains[c], all_template_byte_chains[c], res_separately[c]));
                    t_par.Stop();
                }
                
                foreach (char c in alphabet) for (int i = 0; i < final_length; i++) result[i] += res_separately[c][i];
                FFT(result, true);
            }
            common_times.Add(' ', t_par.ElapsedMilliseconds);
            /* Нормируем и считаем мат.ожидание и дисперсию */
            int l = Math.Min(N, L);
            standart_deviation = 0;
            expected_value = 0;
            double expected_square_of_value = 0;
            int count = N + L - 1;
            for (int i = 0; i < count; i++)
            {
                if (result[i].real >= 0) result[i] /= l;
                else result[i] /= -l;
                expected_value += result[i].real;
                expected_square_of_value += result[i].real * result[i].real;
            }
            expected_value /= count;
            expected_square_of_value /= count;
            standart_deviation = Math.Sqrt(expected_square_of_value - expected_value * expected_value);

            if (is_flat)
            {
                ro = GetProbability();
                for (int i = 0; i <= L - 1; i++) result[i].real -= ro * (i + 1) / L;
                for (int i = L; i <= N - 1; i++) result[i].real -= ro;
                for (int i = N; i < N + L - 1; i++) result[i].real -= ro * (N + L - 1 - i) / L;
            }

            return common_times;
        }
        private Complex[] FFT_ForOneSymbol(Complex[] first_byte_chain, Complex[] second_byte_chain, Complex[] res)
        {
            FFT(first_byte_chain, false);
            FFT(second_byte_chain, false);
            for (int i = 0; i < res.Length; i++) res[i] = first_byte_chain[i] * second_byte_chain[i];
            return res;
        }
        private void FFT(Complex[] a, bool invert) //БПФ одного массива
        {
            int length = a.Length;
            int lg_n = 0;
            while ((1 << lg_n) < length) ++lg_n;

            for (int i = 0; i < length; ++i)
            {
                if (i < rev[i])
                {
                    Complex temp = (Complex)a[i].Clone();
                    a[i] = (Complex)a[rev[i]].Clone();
                    a[rev[i]] = temp;
                }
            }

            for (int len = 2; len <= length; len <<= 1)
            {
                double ang = 2 * Math.PI / len * (invert ? -1 : 1);

                int len2 = len >> 1;
                Complex wlen = new Complex(Math.Cos(ang), Math.Sin(ang));
                Complex[] wlen_degrees = new Complex[len / 2];
                wlen_degrees[0] = new Complex(1, 0);

                for (int i = 1; i < len2; i++) wlen_degrees[i] = wlen_degrees[i - 1] * wlen;

                for (int i = 0; i < length; i += len)
                {
                    for (int j = 0; j < len2; ++j)
                    {
                        Complex u = a[i + j];
                        Complex v = a[i + j + len2] * wlen_degrees[j];
                        a[i + j] = u + v;
                        a[i + j + len2] = u - v;
                    }
                }
            }
            if (invert) for (int i = 0; i < length; ++i)
                {
                    a[i].real /= length;
                    a[i].imaginary /= length;
                }
        }
        public void ToFile(string path, string[] comments = null, bool append = false)
        {
            if ((main_chain == null) || (template == null))
            {
                MessageBox.Show("Еще не выбраны файлы!");
                return;
            }
            else if (result == null)
            {
                MessageBox.Show("Еще не вычислена свёртка!");
                return;
            }

            using (StreamWriter sw = new StreamWriter(path, append, Encoding.Default))
            {
                if (comments != null) for (int i = 0; i < comments.Length; i++) sw.WriteLine("# " + comments[i]);
                string name = path.Substring(path.LastIndexOf('\\') + 1, path.Length - path.LastIndexOf('\\') - 5);
                if (path.EndsWith(".plt"))
                {
                    sw.WriteLine("# 1st seq: " + path_to_main + "\n# N = " + N + "\n# 2nd seq " + path_to_template + "\n# L = " + L);
                    sw.WriteLine(string.Format("set xtics offset 0,-2 font \"Arial, 40pt\"\nset ytics font \"Arial, 40pt\"\n" +
                        "set rmargin 9\nset lmargin 15\nset bmargin 4\nset tmargin 4\n" + (N + L - 1 > 1e5 ? "set format x \"%.1e\"\n" : "") + "set format y \"%.2f\"\n" +
                        "set grid xtics ytics\nset yrange[0:1]\nset xrange[0:{0}]\n" +
                        "# set terminal png size 1920, 1080; set output '{1}.png';\nplot '-' with lines lw 4 lt rgb 'navy' notitle", N + L - 2, name));
                    //sw.WriteLine("set rmargin 6\nset tics font \"Arial, 14pt\"" + (N + L - 1 > 1e5 ? "\nset format x \"%.1e\"" : "") + "\nset format y \"%.2f\"\nset grid xtics ytics\nplot '-' with lines lw 2 lt rgb 'navy' notitle");
                }
                int l = Math.Min(N, L);
                for (int i = 0; i < N + L - 1; i++)
                {
                    string s = string.Format("{0,8}", i) + "  ";
                    s += string.Format("{0:0.000000000000000}", result[i].real).Replace(',', '.');

                    sw.WriteLine(s);
                }
                if (append) sw.WriteLine(">");
                else
                {
                    sw.WriteLine("# Ex = " + expected_value);
                    sw.WriteLine("# sigma = " + standart_deviation);
                }
            }
        }
        public void ToFile(string path, int[] array, string[] comments = null)
        {
            double[] arr = new double[array.Length];
            Array.Copy(array, arr, arr.Length);
            ToFile(path, arr, comments);
        }
        public void ToFile(string path, double[] array, string[] comments = null, bool append = false)
        {
            int length = array.Length;
            using (StreamWriter sw = new StreamWriter(path, append, Encoding.Default))
            {
                if (path.EndsWith(".plt"))
                {
                    sw.WriteLine("# length = " + length);

                    sw.WriteLine("set grid xtics ytics\nplot '-' with lines lw 2 lt rgb 'navy' notitle");
                }
                for (int i = 0; i < length; i++)
                {
                    string s = string.Format("{0,8}", i) + "  ";
                    //for (double j = 0; j < result[i].real; j += 0.01) s += '-';
                    s += string.Format("{0:0.000000000000000}", array[i]).Replace(',', '.');

                    sw.WriteLine(s);
                }
                if (comments != null) for (int i = 0; i < comments.Length; i++) sw.WriteLine("# " + comments[i]);
            }
        }
        public void ToFile(string path, double[] x_array, double[] y_array, string[] comments = null)
        {
            if (x_array.Length != y_array.Length) return;
            int length = x_array.Length;
            using (StreamWriter sw = new StreamWriter(path, false, Encoding.Default))
            {
                if (path.EndsWith(".plt"))
                {
                    sw.WriteLine("# length = " + length);

                    sw.WriteLine("set grid xtics ytics\nplot '-' with lines lw 2 lt rgb 'navy' notitle");
                }
                for (int i = 0; i < length; i++)
                {
                    string s = "";
                    s += string.Format("{0:0.000000000000000}", x_array[i]).Replace(',', '.') + " ";
                    s += string.Format("{0:0.000000000000000}", y_array[i]).Replace(',', '.');
                    sw.WriteLine(s);
                }
                if (comments != null) for (int i = 0; i < comments.Length; i++) sw.WriteLine("# " + comments[i]);
            }
        }
        public void Bisection(TextBox textBox)//локализация бисекцией
        {
            if ((main_chain == null) || (template == null))
            {
                MessageBox.Show("Еще не выбраны файлы!");
                return;
            }
            else if (result == null)
            {
                MessageBox.Show("Еще не вычислена свёртка!");
                return;
            }
            
            int index_of_largest_peak, second_largest_peak;
            ToFile("C:\\Users\\okvay\\Documents\\Scen1.plt");
            DrawCurrentFlat("C:\\Users\\okvay\\Documents\\Scen1 flat.plt");
            ro = GetProbability();
            index_of_largest_peak = FindLargestPeak();
            second_largest_peak = FindNearbyPeak(index_of_largest_peak, MAX_INSERTION);

            if (second_largest_peak == -1) LocalizationSinglePeak(index_of_largest_peak, textBox);
            else LocalizationTwoPeaks(index_of_largest_peak, second_largest_peak, textBox);
        }
        private void LocalizationSinglePeak(int index, TextBox textBox)
        {
            int N1, N2, L1, L2, intersection_length, N_begin, N_end, L_begin, L_end, iteration = 0;
            bool end = false;

            /*Вычисляем границы участка пересечения*/
            if (index <= L - 1)
            {
                intersection_length = index + 1;
                N1 = 0;
                N2 = index;
                L1 = L - 1 - index;
                L2 = L - 1;
            }
            else if (index <= N - 1)
            {
                intersection_length = L;
                N1 = index - L + 1;
                N2 = index;
                L1 = 0;
                L2 = L - 1;
            }
            else
            {
                intersection_length = N + L - 1 - index;
                N1 = index + 1 - L;
                N2 = N - 1;
                L1 = 0;
                L2 = N + L - 2 - index;
            }

            N_begin = N1;
            N_end = N2;
            L_begin = L1;
            L_end = L2;

            estimated_length = (int)Math.Round((result[index].real * Math.Min(N, L) - ro * intersection_length) / (1 - ro));
            textBox.Text += "Ожидаемая длина S = " + estimated_length + " символов." + Environment.NewLine;

            do
            {
                if (estimated_length >= Math.Min(N, L) * 0.99)
                {
                    MessageBox.Show("Найдено почти полное совпадение!");
                    return;
                }
                if (intersection_length < 2 * estimated_length)
                {
                    textBox.Text += "Длина оставшейся части шаблона не превышает двух ожидаемых!" + Environment.NewLine;
                    break;
                }
                textBox.Text += Environment.NewLine + "          " + ++iteration + "-Я ИТЕРАЦИЯ:" + Environment.NewLine + Environment.NewLine + "Длина пересечения равна " + intersection_length + Environment.NewLine;

                L = intersection_length / 2;
                N = intersection_length;
                textBox.Text += "    1-я половина:" + Environment.NewLine + "N = " + N + ", L = " + L + Environment.NewLine;
                char[] old_main_chain = main_chain;
                char[] old_template = template;

                /*********** Работаем с первой половиной шаблона ***********/
                var first_part = new Pair();
                main_chain = new char[N];
                template = new char[L];

                Array.Copy(old_main_chain, N1, main_chain, 0, N);
                Array.Copy(old_template, L1, template, 0, L);
                //textBox.Text += "В template скопировали с " + L1 + " по " + (L1 + L - 1) + " элементы." + Environment.NewLine;
                //ToFile("C:\\Users\\okvay\\Documents\\main1.txt", main_chain);
                //ToFile("C:\\Users\\okvay\\Documents\\template1.txt", template);

                ConvolutionComputation();
                ToFile("C:\\Users\\okvay\\Documents\\Scen" + iteration + ".1.plt");
                DrawCurrentFlat("C:\\Users\\okvay\\Documents\\Scen" + iteration + ".1 flat.plt");
                first_part.ro = GetProbability();
                first_part.index_of_largest_peak = FindLargestPeak();

                /*Вычисляем границы участка пересечения*/
                if (first_part.index_of_largest_peak <= L - 1)
                {
                    first_part.intersection_length = first_part.index_of_largest_peak + 1;
                    first_part.N1 = 0;
                    first_part.N2 = first_part.index_of_largest_peak;
                    first_part.L1 = L - 1 - first_part.index_of_largest_peak;
                    first_part.L2 = L - 1;
                }
                else if (first_part.index_of_largest_peak <= N - 1)
                {
                    first_part.intersection_length = L;
                    first_part.N1 = first_part.index_of_largest_peak - L + 1;
                    first_part.N2 = first_part.index_of_largest_peak;
                    first_part.L1 = 0;
                    first_part.L2 = L - 1;
                }
                else
                {
                    first_part.intersection_length = N + L - 1 - first_part.index_of_largest_peak;
                    first_part.N1 = first_part.index_of_largest_peak + 1 - L;
                    first_part.N2 = N - 1;
                    first_part.L1 = 0;
                    first_part.L2 = N + L - 2 - first_part.index_of_largest_peak;
                }
                textBox.Text += "Индекс максимального пика " + first_part.index_of_largest_peak + ", длина пересечения " + first_part.intersection_length + Environment.NewLine;
                first_part.estimated_length = (int)Math.Round((result[first_part.index_of_largest_peak].real * L - first_part.ro * first_part.intersection_length) / (1 - first_part.ro));
                textBox.Text += "Ожидаемая длина S1 = " + first_part.estimated_length + " символов." + Environment.NewLine;

                /*********** Работаем со второй половиной шаблона **********/
                var second_part = new Pair();
                L = intersection_length - L;
                template = new char[L];

                textBox.Text += "    2-я половина:" + Environment.NewLine + "N = " + N + ", L = " + L + Environment.NewLine;
                Array.Copy(old_main_chain, N1, main_chain, 0, N);
                Array.Copy(old_template, L1 + intersection_length / 2, template, 0, L);
                //ToFile("C:\\Users\\okvay\\Documents\\main2.txt", main_chain);
                //ToFile("C:\\Users\\okvay\\Documents\\template2.txt", template);
                //textBox.Text += "В template скопировали с " + (L1 + intersection_length / 2) + " по " + (L1 + intersection_length / 2 + L - 1) + " элементы." + Environment.NewLine;

                ConvolutionComputation();
                ToFile("C:\\Users\\okvay\\Documents\\Scen" + iteration + ".2.plt");
                DrawCurrentFlat("C:\\Users\\okvay\\Documents\\Scen" + iteration + ".2 flat.plt");
                second_part.ro = GetProbability();
                second_part.index_of_largest_peak = FindLargestPeak();
                /*Вычисляем границы участка пересечения*/
                if (second_part.index_of_largest_peak <= L - 1)
                {
                    second_part.intersection_length = second_part.index_of_largest_peak + 1;
                    second_part.N1 = 0;
                    second_part.N2 = second_part.index_of_largest_peak;
                    second_part.L1 = L - 1 - second_part.index_of_largest_peak;
                    second_part.L2 = L - 1;
                }
                else if (second_part.index_of_largest_peak <= N - 1)
                {
                    second_part.intersection_length = L;
                    second_part.N1 = second_part.index_of_largest_peak - L + 1;
                    second_part.N2 = second_part.index_of_largest_peak;
                    second_part.L1 = 0;
                    second_part.L2 = L - 1;
                }
                else
                {
                    second_part.intersection_length = N + L - 1 - second_part.index_of_largest_peak;
                    second_part.N1 = second_part.index_of_largest_peak + 1 - L;
                    second_part.N2 = N - 1;
                    second_part.L1 = 0;
                    second_part.L2 = N + L - 2 - second_part.index_of_largest_peak;
                }
                textBox.Text += "Индекс максимального пика " + second_part.index_of_largest_peak + ", длина пересечения " + second_part.intersection_length + Environment.NewLine;
                second_part.estimated_length = (int)Math.Round((result[second_part.index_of_largest_peak].real * L - second_part.ro * second_part.intersection_length) / (1 - second_part.ro));
                textBox.Text += "Ожидаемая длина S2 = " + second_part.estimated_length + " символов." + Environment.NewLine;


                if ((second_part.estimated_length > estimated_length * 0.95) && (second_part.estimated_length > intersection_length * 0.95))
                {
                    textBox.Text += "Во второй половине найдено почти полное совпадение!" + Environment.NewLine;
                    end = true;
                }
                else if ((first_part.estimated_length > estimated_length * 0.95) && (first_part.estimated_length > intersection_length * 0.95))
                {
                    textBox.Text += "В первой половине найдено почти полное совпадение!" + Environment.NewLine;
                    end = true;
                }
                /* Если совпадение в первой половине ближе к исходному, чем совпадение во второй половине, но при этом отличается от исходного не более, чем на 10% */
                else if ((Math.Abs(first_part.estimated_length - estimated_length) < 0.1 * estimated_length) && (Math.Abs(first_part.estimated_length - estimated_length) < Math.Abs(second_part.estimated_length - estimated_length)))
                {
                    L = intersection_length;
                    N = intersection_length;
                    main_chain = new char[N];
                    template = new char[L];

                    Array.Copy(old_main_chain, N1, main_chain, 0, N);
                    Array.Copy(old_template, L1, template, 0, L);
                    ConvolutionComputation();

                    N1 = first_part.N1;
                    N2 = first_part.N2;
                    L1 = first_part.L1;
                    L2 = first_part.L2;
                    N_begin += N1;
                    N_end -= (N - N2 - 1);
                    L_begin += L1;
                    L_end = L_begin + L - 1;
                    estimated_length = first_part.estimated_length;
                    intersection_length = first_part.intersection_length;
                    index = first_part.index_of_largest_peak;
                    textBox.Text += "На следующей итерации работаем с первой частью." + Environment.NewLine;
                    textBox.Text += "Итоговая S = " + estimated_length + Environment.NewLine;
                }
                /* Если совпадение во второй половине ближе к исходному, чем совпадение в первой половине, но при этом отличается от исходного не более, чем на 10% */
                else if ((Math.Abs(second_part.estimated_length - estimated_length) < 0.1 * estimated_length) && (Math.Abs(second_part.estimated_length - estimated_length) < Math.Abs(first_part.estimated_length - estimated_length)))
                {
                    N1 = second_part.N1;
                    N2 = second_part.N2;
                    L1 = second_part.L1;
                    L2 = second_part.L2;
                    N_begin += N1;
                    N_end -= (N - N2 - 1);
                    L_begin += L1 + intersection_length - L;
                    L_end = L_begin + L - 1;
                    estimated_length = second_part.estimated_length;
                    intersection_length = second_part.intersection_length;
                    index = second_part.index_of_largest_peak;
                    textBox.Text += "На следующей итерации работаем со второй частью." + Environment.NewLine;
                    textBox.Text += "Итоговая S = " + estimated_length + Environment.NewLine;
                }
                /* Если разрезали искомое совпадение */
                else
                {
                    /* Если длина шаблона больше, чем 2*S, отступаем от середины в обе стороны на S */
                    if (L1 + intersection_length / 2 > estimated_length)
                    {
                        textBox.Text += "     Отступаем от центра на S в обе стороны:\r\n";

                        L = 2 * estimated_length;
                        template = new char[L];
                        textBox.Text += "N = " + N + ", L = " + L + Environment.NewLine;

                        Array.Copy(old_main_chain, N1, main_chain, 0, N);
                        Array.Copy(old_template, L1 + intersection_length / 2 - estimated_length, template, 0, L);

                        L_begin = (L_begin + L_end) / 2 - estimated_length + 1;
                        L_end = L_begin + L - 1;
                    }
                    else
                    {
                        L = N;
                        template = new char[L];
                        Array.Copy(old_template, L1, template, 0, L);
                        textBox.Text += "Рассматриваемая длина не превышает двух ожидаемых!" + Environment.NewLine;
                    }
                    end = true;
                }
            } while (!end);
            textBox.Text += Environment.NewLine + "                 Начинаем уточнять..." + Environment.NewLine + Environment.NewLine;

            /* Определяем местоположение оставшейся части в исходном шаблоне и в основной цепи */
            char[] old_main = main_chain;
            char[] founded_match = template;

            Create2ndSequence(path_to_template);
            main_chain = template;
            N = template.Length;
            template = founded_match;
            L = founded_match.Length;
            ConvolutionComputation();
            index = FindLargestPeak();
            L_end = index;
            L_begin = index - founded_match.Length + 1;

            //template = old_main;
            //L = template.Length;
            Create1stSequence(path_to_main);
            ConvolutionComputation();
            index = FindLargestPeak();
            N_end = index;
            N_begin = index - founded_match.Length + 1;

            old_main = new char[N_end - N_begin + 1];
            Array.Copy(main_chain, N_begin, old_main, 0, old_main.Length);
            main_chain = old_main;
            N = old_main.Length;
            template = founded_match;
            L = template.Length;

            /*
            Tuple<int, int> cut_off_count = ClarificationSinglePeak(L_begin, L_end, N_begin, N_end, textBox);
            L_begin += cut_off_count.Item1;
            L_end -= cut_off_count.Item2;
            N_begin += cut_off_count.Item1;
            N_end -= cut_off_count.Item2;
            */
            Tuple<int, int> res = TestClarificationSinglePeak();
            L_begin += res.Item1;
            L_end = L_begin + res.Item2;
            N_begin += res.Item1;
            N_end = N_begin + res.Item2;
            
            textBox.Text += "В первой последовательности совпадение расположено между " + N_begin + " и " + N_end + Environment.NewLine;
            textBox.Text += "Во второй последовательности совпадение расположено между " + L_begin + " и " + L_end + Environment.NewLine;
            textBox.Text += "Длина совпадения " + (L_end - L_begin + 1) + " символов." + Environment.NewLine;
        }
        private Tuple<int, int> TestClarificationSinglePeak()//уточнение местоположения найденного точного совпадения
        {
            bool[] is_equal = new bool[N];
            for (int i = 0; i < N; i++) if (main_chain[i] == template[i]) is_equal[i] = true;
                else is_equal[i] = false;
            int start = 0, max_length = 0;
            for (int i = 0; i < N - 1; i++)
            {
                if (is_equal[i])
                {
                    int len = 1;
                    for (int j = i + 1; j < N; j++)
                    {
                        if (is_equal[j]) len++;
                        else break;
                    }
                    if (len > max_length)
                    {
                        start = i;
                        max_length = len;
                    }
                }
            }
            return new Tuple<int, int>(start, max_length);
        }
        private Tuple<int, int> ClarificationSinglePeak(int L_begin, int L_end, int N_begin, int N_end, TextBox textBox)//уточнение местоположения найденного точного совпадения
        {
            int first_cut_off_length = template.Length / 100, second_cut_off_length = first_cut_off_length / 10; //размер "отрезаемого" за раз куска
            bool end = false;
            int left_cut_off_count = 0, right_cut_off_count = 0, attempt_number = 0;
            //начинаем отрезать слева до тех пор, пока не дойдем до совпадения
            do
            {
                ++attempt_number;
                char[] old_template = template;
                L = old_template.Length - first_cut_off_length;
                template = new char[L];
                Array.Copy(old_template, first_cut_off_length, template, 0, L);
                textBox.Text += "Отрезали слева " + first_cut_off_length + " символов...";
                ConvolutionComputation();
                int index_of_largest_peak = FindLargestPeak(), intersection_length = 0;
                if (index_of_largest_peak <= L - 1) intersection_length = index_of_largest_peak + 1;
                else if (index_of_largest_peak <= N - 1) intersection_length = L;
                else intersection_length = N + L - 1 - index_of_largest_peak;
                ro = GetProbability();
                int new_estimated_length = (int)Math.Round((result[index_of_largest_peak].real * L - ro * intersection_length) / (1 - ro));

                double n_minus = (new_estimated_length - estimated_length + first_cut_off_length) / (1 - ro);
                double n_plus = first_cut_off_length - n_minus;
                //textBox.Text += "n_plus = " + n_plus + Environment.NewLine;
                /* Если на первом же шаге неудачно отрезали, то возвращаем несколько символов в шаблон и в основную цепь */
                if ((attempt_number == 1) && (n_plus > -first_cut_off_length / 10))
                {
                    L_begin -= 5 * first_cut_off_length;
                    N_begin -= 5 * first_cut_off_length;
                    left_cut_off_count -= 5 * first_cut_off_length;

                    old_template = new char[old_template.Length + 5 * first_cut_off_length];
                    Create2ndSequence(path_to_template);
                    Array.Copy(template, L_begin, old_template, 0, old_template.Length);
                    template = old_template;
                    L = template.Length;

                    char[] old_main_chain = new char[main_chain.Length + 5 * first_cut_off_length];
                    Create1stSequence(path_to_main);
                    Array.Copy(main_chain, N_begin, old_main_chain, 0, old_main_chain.Length);
                    main_chain = old_main_chain;
                    N = main_chain.Length;

                    ConvolutionComputation();
                    index_of_largest_peak = FindLargestPeak();
                    if (index_of_largest_peak <= L - 1) intersection_length = index_of_largest_peak + 1;
                    else if (index_of_largest_peak <= N - 1) intersection_length = L;
                    else intersection_length = N + L - 1 - index_of_largest_peak;
                    ro = GetProbability();
                    estimated_length = (int)Math.Round((result[index_of_largest_peak].real * L - ro * intersection_length) / (1 - ro));
                    //attempt_number = 0;
                }
                else if (n_plus > -first_cut_off_length / 10)
                {
                    template = old_template;
                    L = template.Length;
                    textBox.Text += "Неудачно... Уменьшаем длину." + Environment.NewLine;
                    do
                    {
                        old_template = template;
                        L = old_template.Length - second_cut_off_length;
                        template = new char[L];
                        Array.Copy(old_template, second_cut_off_length, template, 0, L);
                        textBox.Text += "Отрезали слева " + second_cut_off_length + " символов...";
                        ConvolutionComputation();
                        index_of_largest_peak = FindLargestPeak();
                        if (index_of_largest_peak <= L - 1) intersection_length = index_of_largest_peak + 1;
                        else if (index_of_largest_peak <= N - 1) intersection_length = L;
                        else intersection_length = N + L - 1 - index_of_largest_peak;
                        ro = GetProbability();
                        new_estimated_length = (int)Math.Round((result[index_of_largest_peak].real * L - ro * intersection_length) / (1 - ro));

                        n_minus = (new_estimated_length - estimated_length + second_cut_off_length) / (1 - ro);
                        n_plus = second_cut_off_length - n_minus;
                        //textBox.Text += "n_plus = " + n_plus + Environment.NewLine;
                        //textBox.Text += "Разница = " + (new_estimated_length - estimated_length) + Environment.NewLine;
                        //textBox.Text += "ro * cut = " + (ro * second_cut_off_length) + Environment.NewLine;
                        //if (Math.Abs(new_estimated_length - estimated_length) > second_cut_off_length * ro)
                        if (n_plus > second_cut_off_length * (1 - ro))
                        {
                            template = old_template;
                            L = template.Length;
                            textBox.Text += "Неудачно..." + Environment.NewLine;
                            end = true;
                        }
                        else
                        {
                            estimated_length = new_estimated_length;
                            L_begin += second_cut_off_length;
                            left_cut_off_count += second_cut_off_length;
                            textBox.Text += "Удачно!" + Environment.NewLine;
                        }
                    } while (!end);
                }
                else
                {
                    estimated_length = new_estimated_length;
                    L_begin += first_cut_off_length;
                    left_cut_off_count += first_cut_off_length;
                    textBox.Text += "Удачно!" + Environment.NewLine;
                }
            } while (!end);
            end = false;
            attempt_number = 0;
            //начинаем отрезать справа до тех пор, пока не дойдем до совпадения
            do
            {
                ++attempt_number;
                char[] old_template = template;
                L = old_template.Length - first_cut_off_length;
                template = new char[L];
                Array.Copy(old_template, 0, template, 0, L);
                textBox.Text += "Отрезали справа " + first_cut_off_length + " символов...";
                ConvolutionComputation();
                int index_of_largest_peak = FindLargestPeak(), intersection_length;
                if (index_of_largest_peak <= L - 1) intersection_length = index_of_largest_peak + 1;
                else if (index_of_largest_peak <= N - 1) intersection_length = L;
                else intersection_length = N + L - 1 - index_of_largest_peak;
                ro = GetProbability();
                int new_estimated_length = (int)Math.Round((result[index_of_largest_peak].real * L - ro * intersection_length) / (1 - ro));

                double n_minus = (new_estimated_length - estimated_length + first_cut_off_length) / (1 - ro);
                double n_plus = first_cut_off_length - n_minus;
                //textBox.Text += "n_plus = " + n_plus + Environment.NewLine;

                /* Если на первом же шаге неудачно отрезали, то возвращаем несколько символов */
                if ((attempt_number == 1) && (Math.Abs(new_estimated_length - estimated_length) > 0.01 * estimated_length))
                {
                    L_end += 5 * first_cut_off_length;
                    N_end += 5 * first_cut_off_length;
                    right_cut_off_count -= 5 * first_cut_off_length;
                    old_template = new char[old_template.Length + 5 * first_cut_off_length];
                    Create2ndSequence(path_to_template);
                    Array.Copy(template, L_begin, old_template, 0, old_template.Length);
                    template = old_template;
                    L = template.Length;

                    char[] old_main_chain = new char[main_chain.Length + 5 * first_cut_off_length];
                    Create1stSequence(path_to_main);
                    Array.Copy(main_chain, N_begin, old_main_chain, 0, old_main_chain.Length);
                    main_chain = old_main_chain;
                    N = main_chain.Length;

                    ConvolutionComputation();
                    index_of_largest_peak = FindLargestPeak();
                    if (index_of_largest_peak <= L - 1) intersection_length = index_of_largest_peak + 1;
                    else if (index_of_largest_peak <= N - 1) intersection_length = L;
                    else intersection_length = N + L - 1 - index_of_largest_peak;
                    ro = GetProbability();
                    estimated_length = (int)Math.Round((result[index_of_largest_peak].real * L - ro * intersection_length) / (1 - ro));
                    //attempt_number = 0;
                }
                else if (n_plus > 0)
                {
                    template = old_template;
                    L = template.Length;
                    textBox.Text += "Неудачно... Уменьшаем длину." + Environment.NewLine;
                    do
                    {
                        old_template = template;
                        L = old_template.Length - second_cut_off_length;
                        template = new char[L];
                        Array.Copy(old_template, 0, template, 0, L);
                        textBox.Text += "Отрезали справа " + second_cut_off_length + " символов...";
                        ConvolutionComputation();
                        index_of_largest_peak = FindLargestPeak();
                        if (index_of_largest_peak <= L - 1) intersection_length = index_of_largest_peak + 1;
                        else if (index_of_largest_peak <= N - 1) intersection_length = L;
                        else intersection_length = N + L - 1 - index_of_largest_peak;
                        ro = GetProbability();
                        new_estimated_length = (int)Math.Round((result[index_of_largest_peak].real * L - ro * intersection_length) / (1 - ro));

                        n_minus = (new_estimated_length - estimated_length + second_cut_off_length) / (1 - ro);
                        n_plus = second_cut_off_length - n_minus;
                        //textBox.Text += "n_plus = " + n_plus + Environment.NewLine;
                        //textBox.Text += "Разница = " + (new_estimated_length - estimated_length) + Environment.NewLine;
                        //textBox.Text += "ro * cut = " + (ro * second_cut_off_length) + Environment.NewLine;

                        //if (Math.Abs(new_estimated_length - estimated_length) > second_cut_off_length * ro)
                        if (n_plus > second_cut_off_length * (1 - ro))
                        {
                            template = old_template;
                            L = template.Length;
                            end = true;
                            textBox.Text += "Неудачно..." + Environment.NewLine;
                        }
                        else
                        {
                            estimated_length = new_estimated_length;
                            L_end -= second_cut_off_length;
                            right_cut_off_count += second_cut_off_length;
                            textBox.Text += "Удачно!" + Environment.NewLine;
                        }
                    } while (!end);
                }
                else
                {
                    estimated_length = new_estimated_length;
                    L_end -= first_cut_off_length;
                    right_cut_off_count += first_cut_off_length;
                    textBox.Text += "Удачно!" + Environment.NewLine;
                }
            } while (!end);

            textBox.Text += "Итоговая длина шаблона " + template.Length + " символов." + Environment.NewLine;
            textBox.Text += "Ожидаемая длина " + estimated_length + " символов." + Environment.NewLine;
            return new Tuple<int, int>(left_cut_off_count, right_cut_off_count);
        }
        private void LocalizationTwoPeaks(int index1, int index2, TextBox textBox)
        {
            int N1, N2, L1, L2, intersection_length;
            int N1_1, N2_1, L1_1, L2_1, intersection_length1, length1;
            int N1_2, N2_2, L1_2, L2_2, intersection_length2, length2;
            int iteration = 0, displacement = index2 - index1;
            int N_begin, N_end, L_begin, L_end;
            bool end = false;

            /*Вычисляем границы участков пересечения*/
            if (index1 <= L - 1)
            {
                intersection_length1 = index1 + 1;
                N1_1 = 0;
                N2_1 = index1;
                L1_1 = L - 1 - index1;
                L2_1 = L - 1;
            }
            else if (index1 <= N - 1)
            {
                intersection_length1 = L;
                N1_1 = index1 - L + 1;
                N2_1 = index1;
                L1_1 = 0;
                L2_1 = L - 1;
            }
            else
            {
                intersection_length1 = N + L - 1 - index1;
                N1_1 = index1 + 1 - L;
                N2_1 = N - 1;
                L1_1 = 0;
                L2_1 = N + L - 2 - index1;
            }
            if (index2 <= L - 1)
            {
                intersection_length2 = index2 + 1;
                N1_2 = 0;
                N2_2 = index2;
                L1_2 = L - 1 - index2;
                L2_2 = L - 1;
            }
            else if (index2 <= N - 1)
            {
                intersection_length2 = L;
                N1_2 = index2 - L + 1;
                N2_2 = index2;
                L1_2 = 0;
                L2_2 = L - 1;
            }
            else
            {
                intersection_length2 = N + L - 1 - index2;
                N1_2 = index2 + 1 - L;
                N2_2 = N - 1;
                L1_2 = 0;
                L2_2 = N + L - 2 - index2;
            }

            N1 = Math.Min(N1_1, N1_2);
            N2 = Math.Max(N2_1, N2_2);
            L1 = Math.Min(L1_1, L1_2);
            L2 = Math.Max(L2_1, L2_2);

            N_begin = N1;
            N_end = N2;
            L_begin = L1;
            L_end = L2;

            length1 = (int)Math.Round((result[index1].real * Math.Min(N, L) - ro * intersection_length1) / (1 - ro));
            length2 = (int)Math.Round((result[index2].real * Math.Min(N, L) - ro * intersection_length2) / (1 - ro));
            estimated_length = length1 + length2;
            textBox.Text += "Ожидаемая длина S = " + estimated_length + " символов." + Environment.NewLine;
            textBox.Text += "Предполагается присутствие вставки/выпадения длиной " + Math.Abs(displacement) + " символов." + Environment.NewLine;

            do
            {
                if (estimated_length >= Math.Min(N, L) * 0.99)
                {
                    MessageBox.Show("Найдено почти полное совпадение!");
                    return;
                }

                intersection_length = N2 - N1 + 1;
                if (intersection_length < 2 * estimated_length)
                {
                    break;
                }
                /*
                if (intersection_length > template.Length)
                {
                    int diff = intersection_length - template.Length;
                    intersection_length = template.Length;
                    N2 -= diff;
                    L2 -= diff;
                }
                */

                textBox.Text += Environment.NewLine + "          " + ++iteration + "-Я ИТЕРАЦИЯ:" + Environment.NewLine + Environment.NewLine + "Длина пересечения равна " + intersection_length + Environment.NewLine;
                textBox.Text += "Предполагается присутствие вставки/выпадения длиной " + Math.Abs(displacement) + " символов." + Environment.NewLine;

                L = (intersection_length + 1) / 2;
                N = intersection_length;
                textBox.Text += "    1-я половина:" + Environment.NewLine + "N = " + N + ", L = " + L + Environment.NewLine;
                char[] old_main_chain = main_chain;
                char[] old_template = template;

                /*********** Работаем с первой половиной шаблона ***********/
                var first_part = new Pair();
                main_chain = new char[N];
                template = new char[L];

                Array.Copy(old_main_chain, N1, main_chain, 0, N);
                Array.Copy(old_template, L1, template, 0, L);
                textBox.Text += "В template скопировали с " + L1 + " по " + (L1 + L - 1) + " элементы." + Environment.NewLine;

                ConvolutionComputation();
                ToFile("C:\\Users\\okvay\\Documents\\Scen" + iteration + ".1.plt");
                first_part.ro = GetProbability();
                first_part.index1 = FindLargestPeak();
                first_part.index2 = FindNearbyPeak(first_part.index1, MAX_INSERTION);

                /* Вычисляем границы участков пересечения */
                if (first_part.index1 <= L - 1)
                {
                    first_part.intersection_length1 = first_part.index1 + 1;
                    first_part.N1_1 = 0;
                    first_part.N2_1 = first_part.index1;
                    first_part.L1_1 = L - 1 - first_part.index1;
                    first_part.L2_1 = L - 1;
                }
                else if (first_part.index1 <= N - 1)
                {
                    first_part.intersection_length1 = L;
                    first_part.N1_1 = first_part.index1 - L + 1;
                    first_part.N2_1 = first_part.index1;
                    first_part.L1_1 = 0;
                    first_part.L2_1 = L - 1;
                }
                else
                {
                    first_part.intersection_length1 = N + L - 1 - first_part.index1;
                    first_part.N1_1 = first_part.index1 + 1 - L;
                    first_part.N2_1 = N - 1;
                    first_part.L1_1 = 0;
                    first_part.L2_1 = N + L - 2 - first_part.index1;
                }
                if (first_part.index2 <= L - 1)
                {
                    first_part.intersection_length2 = first_part.index2 + 1;
                    first_part.N1_2 = 0;
                    first_part.N2_2 = first_part.index2;
                    first_part.L1_2 = L - 1 - first_part.index2;
                    first_part.L2_2 = L - 1;
                }
                else if (first_part.index2 <= N - 1)
                {
                    first_part.intersection_length2 = L;
                    first_part.N1_2 = first_part.index2 - L + 1;
                    first_part.N2_2 = first_part.index2;
                    first_part.L1_2 = 0;
                    first_part.L2_2 = L - 1;
                }
                else
                {
                    first_part.intersection_length2 = N + L - 1 - first_part.index2;
                    first_part.N1_2 = first_part.index2 + 1 - L;
                    first_part.N2_2 = N - 1;
                    first_part.L1_2 = 0;
                    first_part.L2_2 = N + L - 2 - first_part.index2;
                }

                textBox.Text += "Индекс максимального пика " + first_part.index1 + ", индекс второго по высоте " + first_part.index2 + Environment.NewLine;
                first_part.estimated_length = (int)Math.Round((result[first_part.index1].real * L - first_part.ro * first_part.intersection_length1) / (1 - first_part.ro));
                if (first_part.index2 != -1) first_part.estimated_length += (int)Math.Round((result[first_part.index2].real * L - first_part.ro * first_part.intersection_length2) / (1 - first_part.ro));
                textBox.Text += "Ожидаемая длина S1 = " + first_part.estimated_length + " символов." + Environment.NewLine;
                if (first_part.index2 != -1) textBox.Text += "Присутствует вставка/выпадение длиной " + Math.Abs(first_part.index2 - first_part.index1) + " символов." + Environment.NewLine;

                /*********** Работаем со второй половиной шаблона **********/
                var second_part = new Pair();
                L = intersection_length - L;
                template = new char[L];

                textBox.Text += "    2-я половина:" + Environment.NewLine + "N = " + N + ", L = " + L + Environment.NewLine;
                Array.Copy(old_main_chain, N1, main_chain, 0, N);
                Array.Copy(old_template, L1 + intersection_length / 2, template, 0, L);
                //Array.Copy(old_template, old_template.Length - L, template, 0, L);
                textBox.Text += "В template скопировали с " + (L1 + intersection_length / 2) + " по " + (L1 + intersection_length / 2 + L - 1) + " элементы." + Environment.NewLine;

                ConvolutionComputation();
                ToFile("C:\\Users\\okvay\\Documents\\Scen" + iteration + ".2.plt");
                second_part.ro = GetProbability();
                second_part.index1 = FindLargestPeak();
                second_part.index2 = FindNearbyPeak(second_part.index1, MAX_INSERTION);
                /*Вычисляем границы участков пересечения*/
                if (second_part.index1 <= L - 1)
                {
                    second_part.intersection_length1 = second_part.index1 + 1;
                    second_part.N1_1 = 0;
                    second_part.N2_1 = second_part.index1;
                    second_part.L1_1 = L - 1 - second_part.index1;
                    second_part.L2_1 = L - 1;
                }
                else if (second_part.index1 <= N - 1)
                {
                    second_part.intersection_length1 = L;
                    second_part.N1_1 = second_part.index1 - L + 1;
                    second_part.N2_1 = second_part.index1;
                    second_part.L1_1 = 0;
                    second_part.L2_1 = L - 1;
                }
                else
                {
                    second_part.intersection_length1 = N + L - 1 - second_part.index1;
                    second_part.N1_1 = second_part.index1 + 1 - L;
                    second_part.N2_1 = N - 1;
                    second_part.L1_1 = 0;
                    second_part.L2_1 = N + L - 2 - second_part.index1;
                }
                if (second_part.index2 <= L - 1)
                {
                    second_part.intersection_length2 = second_part.index2 + 1;
                    second_part.N1_2 = 0;
                    second_part.N2_2 = second_part.index2;
                    second_part.L1_2 = L - 1 - second_part.index2;
                    second_part.L2_2 = L - 1;
                }
                else if (second_part.index2 <= N - 1)
                {
                    second_part.intersection_length2 = L;
                    second_part.N1_2 = second_part.index2 - L + 1;
                    second_part.N2_2 = second_part.index2;
                    second_part.L1_2 = 0;
                    second_part.L2_2 = L - 1;
                }
                else
                {
                    second_part.intersection_length2 = N + L - 1 - second_part.index2;
                    second_part.N1_2 = second_part.index2 + 1 - L;
                    second_part.N2_2 = N - 1;
                    second_part.L1_2 = 0;
                    second_part.L2_2 = N + L - 2 - second_part.index2;
                }

                textBox.Text += "Индекс максимального пика " + second_part.index1 + ", индекс второго по высоте " + second_part.index2 + Environment.NewLine;
                second_part.estimated_length = (int)Math.Round((result[second_part.index1].real * L - second_part.ro * second_part.intersection_length1) / (1 - second_part.ro));
                if (second_part.index2 != -1) second_part.estimated_length += (int)Math.Round((result[second_part.index2].real * L - second_part.ro * second_part.intersection_length2) / (1 - second_part.ro));
                textBox.Text += "Ожидаемая длина S2 = " + second_part.estimated_length + " символов." + Environment.NewLine;
                if (second_part.index2 != -1) textBox.Text += "Присутствует вставка/выпадение длиной " + Math.Abs(second_part.index2 - second_part.index1) + " символов." + Environment.NewLine;


                if ((second_part.estimated_length > estimated_length * 0.95) && (second_part.estimated_length > intersection_length * 0.95))
                {
                    textBox.Text += "Во второй половине найдено почти полное совпадение!" + Environment.NewLine;
                    end = true;
                }
                else if ((first_part.estimated_length > estimated_length * 0.95) && (first_part.estimated_length > intersection_length * 0.95))
                {
                    textBox.Text += "В первой половине найдено почти полное совпадение!" + Environment.NewLine;
                    end = true;
                }
                else if (second_part.estimated_length > estimated_length * 0.9)
                {
                    N1_1 = second_part.N1_1;
                    N2_1 = second_part.N2_1;
                    L1_1 = second_part.L1_1;
                    L2_1 = second_part.L2_1;
                    N1_2 = second_part.N1_2;
                    N2_2 = second_part.N2_2;
                    L1_2 = second_part.L1_2;
                    L2_2 = second_part.L2_2;

                    N1 = Math.Min(N1_1, N1_2);
                    N2 = Math.Max(N2_1, N2_2);
                    L1 = Math.Min(L1_1, L1_2);
                    L2 = Math.Max(L2_1, L2_2);

                    N_begin += N1;
                    N_end -= (N - N2);
                    L_begin += L1;
                    L_end -= (L - L2);

                    estimated_length = second_part.estimated_length;
                    intersection_length = second_part.intersection_length;
                    index1 = second_part.index1;
                    index2 = second_part.index2;
                    textBox.Text += "На следующей итерации работаем со второй частью." + Environment.NewLine;
                    textBox.Text += "Итоговая S = " + estimated_length + Environment.NewLine;
                }
                else if (first_part.estimated_length > estimated_length * 0.9)
                {
                    L = intersection_length;
                    N = intersection_length;
                    main_chain = new char[N];
                    template = new char[L];

                    Array.Copy(old_main_chain, N1, main_chain, 0, N);
                    Array.Copy(old_template, L1, template, 0, L);
                    ConvolutionComputation();

                    N1_1 = first_part.N1_1;
                    N2_1 = first_part.N2_1;
                    L1_1 = first_part.L1_1;
                    L2_1 = first_part.L2_1;
                    N1_2 = first_part.N1_2;
                    N2_2 = first_part.N2_2;
                    L1_2 = first_part.L1_2;
                    L2_2 = first_part.L2_2;

                    N1 = Math.Min(N1_1, N1_2);
                    N2 = Math.Max(N2_1, N2_2);
                    L1 = Math.Min(L1_1, L1_2);
                    L2 = Math.Max(L2_1, L2_2);

                    N_begin += N1;
                    N_end -= (N - N2);
                    L_begin += L1;
                    L_end -= (L - L2);

                    estimated_length = first_part.estimated_length;
                    intersection_length = first_part.intersection_length;
                    index1 = first_part.index1;
                    index2 = first_part.index2;
                    textBox.Text += "На следующей итерации работаем с первой частью." + Environment.NewLine;
                    textBox.Text += "Итоговая S = " + estimated_length + Environment.NewLine;
                }
                else /* if (first_part.estimated_length + second_part.estimated_length < or > estimated_length) */
                {
                    if (L1 + intersection_length / 2 > estimated_length)
                    {
                        textBox.Text += "     Попали в центр!\r\n";
                        textBox.Text += "     Отступаем от центра на S в обе стороны:\r\n";

                        L = 2 * estimated_length;
                        template = new char[L];
                        textBox.Text += "N = " + N + ", L = " + L + Environment.NewLine;

                        Array.Copy(old_main_chain, N1, main_chain, 0, N);
                        Array.Copy(old_template, L1 + intersection_length / 2 - estimated_length, template, 0, L);

                        textBox.Text += "В первой последовательности совпадение расположено между " + N_begin + " и " + N_end + Environment.NewLine;
                        textBox.Text += "Во второй последовательности совпадение расположено между " + L_begin + " и " + L_end + Environment.NewLine;
                        end = true;
                    }
                    else
                    {
                        textBox.Text += "Рассматриваемая длина не превышает двух ожидаемых!" + Environment.NewLine;
                        textBox.Text += "В первой последовательности совпадение расположено между " + N_begin + " и " + N_end + Environment.NewLine;
                        textBox.Text += "Во второй последовательности совпадение расположено между " + L_begin + " и " + L_end + Environment.NewLine;
                        end = true;
                    }
                }
                /*
                {
                    textBox.Text += "Что-то пошло не так..." + Environment.NewLine;
                    break;
                }
                */
            } while (!end);

            textBox.Text += Environment.NewLine + "                 Начинаем уточнять..." + Environment.NewLine + Environment.NewLine;

            /*
            var res = ClarificationTwoPeaks(textBox);
            L_begin += res.Item1;
            L_end -= res.Item2;
            */

            char[] founded_match = template;
            Create1stSequence(path_to_main);
            ConvolutionComputation();
            index1 = FindLargestPeak();
            index2 = FindNearbyPeak(index1, MAX_INSERTION);
            N_end = Math.Max(index1, index2);
            N_begin = N_end - founded_match.Length + 1 - Math.Abs(index1 - index2);

            Create2ndSequence(path_to_template);
            main_chain = template;
            N = template.Length;
            template = founded_match;
            L = founded_match.Length;
            ConvolutionComputation();
            index1 = FindLargestPeak();
            index2 = FindNearbyPeak(index1, MAX_INSERTION);
            L_end = Math.Max(index1, index2);
            if (index2 != -1) L_begin = L_end - founded_match.Length + 1 - Math.Abs(index1 - index2);
            else L_begin = L_end - founded_match.Length + 1;

            template = new char[L_end - L_begin + 1];
            Array.Copy(main_chain, L_begin, template, 0, L_end - L_begin + 1);
            L = template.Length;
            founded_match = new char[N_end - N_begin + 1];
            Create1stSequence(path_to_main);
            Array.Copy(main_chain, N_begin, founded_match, 0, N_end - N_begin + 1);
            main_chain = founded_match;
            N = main_chain.Length;

            int[] res = TestClarificationTwoPeaks(Math.Abs(displacement));
            int N1_begin = N_begin + res[0] + 2, N1_end = N_begin + res[0] + res[1] + 1,
                N2_begin = N_begin + res[2] + 2, N2_end = N_begin + res[2] + res[3] + 1,
                L1_begin = L_begin + res[0] + 2, L1_end = L_begin + res[0] + res[1] + 1,
                L2_begin = L_begin + res[2] + 2, L2_end = N_begin + res[2] + res[3] + 1;
            if (N2_begin < N1_begin) textBox.Text += "Во второй последовательности вставка относительно первой!" + Environment.NewLine;
            else textBox.Text += "Во второй последовательности выпадение относительно первой!" + Environment.NewLine;
            /*
            L_begin += res.Item1;
            L_end = L_begin + res.Item2;
            N_begin += res.Item1;
            N_end = N_begin + res.Item2;
            */

            textBox.Text += "В первой последовательности совпадение расположено с " + N1_begin + " по " + N1_end + Environment.NewLine;
            textBox.Text += "Во второй последовательности совпадение расположено с " + L1_begin + " по " + L1_end + Environment.NewLine;
            textBox.Text += "Длина совпадения " + res[1] + " символов." + Environment.NewLine;

            textBox.Text += "В первой последовательности совпадение расположено с " + N2_begin + " по " + N2_end + Environment.NewLine;
            textBox.Text += "Во второй последовательности совпадение расположено с " + L2_begin + " по " + L2_end + Environment.NewLine;
            textBox.Text += "Длина совпадения " + res[3] + " символов." + Environment.NewLine;
            /*
            textBox.Text += "В первой последовательности совпадение расположено между " + (N_begin + res[4]) + " и " + (N_begin + res[4] + res[5]) + Environment.NewLine;
            textBox.Text += "Во второй последовательности совпадение расположено между " + (L_begin + res[4]) + " и " + (N_begin + res[4] + res[5]) + Environment.NewLine;
            textBox.Text += "Длина совпадения " + res[5] + " символов." + Environment.NewLine;
            */

            textBox.Text += "Предполагается присутствие вставки/выпадения длиной " + Math.Abs(displacement) + " символов." + Environment.NewLine;
        }
        private int[] TestClarificationTwoPeaks(int displacement)//уточнение местоположения найденного совпадения со вставкой
        {
            int N = Math.Min(this.N, this.L);
            bool[] is_equal_middle = new bool[N];
            for (int i = 0; i < N; i++) if (main_chain[i] == template[i]) is_equal_middle[i] = true;
                else is_equal_middle[i] = false;
            int start = 0, max_length = 0;
            for (int i = 0; i < N - 1; i++)
            {
                if (is_equal_middle[i])
                {
                    int len = 1;
                    for (int j = i + 1; j < N; j++)
                    {
                        if (is_equal_middle[j]) len++;
                        else break;
                    }
                    if (len > max_length)
                    {
                        start = i;
                        max_length = len;
                    }
                }
            }
            int[] result = new int[] { start, max_length, 0, 0 };
            is_equal_middle = new bool[N - displacement];
            for (int i = 0; i < N - displacement; i++) if (main_chain[i + displacement] == template[i]) is_equal_middle[i] = true;
                else is_equal_middle[i] = false;
            start = 0;
            max_length = 0;
            for (int i = 0; i < N - 1 - displacement; i++)
            {
                if (is_equal_middle[i])
                {
                    int len = 1;
                    for (int j = i + 1; j < N - displacement; j++)
                    {
                        if (is_equal_middle[j]) len++;
                        else break;
                    }
                    if (len > max_length)
                    {
                        start = i;
                        max_length = len;
                    }
                }
            }
            result[2] = start;
            result[3] = max_length;
            return result;
        }
        private Tuple<int, int> ClarificationTwoPeaks(TextBox textBox)//уточнение местоположения найденного совпадения со вставкой
        {
            int first_cut_off_length = template.Length / 100, second_cut_off_length = first_cut_off_length / 10; //размер "отрезаемого" за раз куска
            bool end = false;
            int left_cut_off_count = 0, right_cut_off_count = 0;
            //начинаем отрезать слева до тех пор, пока не дойдем до совпадения
            do
            {
                char[] old_template = template;
                L = old_template.Length - first_cut_off_length;
                template = new char[L];
                Array.Copy(old_template, first_cut_off_length, template, 0, L);
                textBox.Text += "Отрезали слева " + first_cut_off_length + " символов...";
                ConvolutionComputation();
                int index_of_largest_peak = FindLargestPeak(),
                    second_largest_peak = FindNearbyPeak(index_of_largest_peak, MAX_INSERTION),
                    intersection_length1, intersection_length2;
                if (index_of_largest_peak <= L - 1) intersection_length1 = index_of_largest_peak + 1;
                else if (index_of_largest_peak <= N - 1) intersection_length1 = L;
                else intersection_length1 = N + L - 1 - index_of_largest_peak;
                if (second_largest_peak <= L - 1) intersection_length2 = second_largest_peak + 1;
                else if (second_largest_peak <= N - 1) intersection_length2 = L;
                else intersection_length2 = N + L - 1 - second_largest_peak;

                ro = GetProbability();
                int new_estimated_length1 = (int)Math.Round((result[index_of_largest_peak].real * L - ro * intersection_length1) / (1 - ro)),
                    new_estimated_length2 = (int)Math.Round((result[second_largest_peak].real * L - ro * intersection_length2) / (1 - ro)),
                    new_estimated_length = new_estimated_length1 + new_estimated_length2;
                if (new_estimated_length < estimated_length * 0.985)
                {
                    template = old_template;
                    L = template.Length;
                    textBox.Text += "Неудачно... Уменьшаем длину." + Environment.NewLine;
                    do
                    {
                        old_template = template;
                        L = old_template.Length - second_cut_off_length;
                        template = new char[L];
                        Array.Copy(old_template, second_cut_off_length, template, 0, L);
                        textBox.Text += "Отрезали слева " + second_cut_off_length + " символов...";
                        ConvolutionComputation();
                        index_of_largest_peak = FindLargestPeak();
                        second_largest_peak = FindNearbyPeak(index_of_largest_peak, MAX_INSERTION);
                        if (index_of_largest_peak <= L - 1) intersection_length1 = index_of_largest_peak + 1;
                        else if (index_of_largest_peak <= N - 1) intersection_length1 = L;
                        else intersection_length1 = N + L - 1 - index_of_largest_peak;
                        if (second_largest_peak <= L - 1) intersection_length2 = second_largest_peak + 1;
                        else if (second_largest_peak <= N - 1) intersection_length2 = L;
                        else intersection_length2 = N + L - 1 - second_largest_peak;

                        ro = GetProbability();
                        new_estimated_length1 = (int)Math.Round((result[index_of_largest_peak].real * L - ro * intersection_length1) / (1 - ro));
                        new_estimated_length2 = (int)Math.Round((result[second_largest_peak].real * L - ro * intersection_length2) / (1 - ro));
                        new_estimated_length = new_estimated_length1 + new_estimated_length2;
                        if (new_estimated_length < estimated_length)
                        {
                            template = old_template;
                            L = template.Length;
                            end = true;
                            textBox.Text += "Неудачно..." + Environment.NewLine;
                        }
                        else
                        {
                            estimated_length = new_estimated_length;
                            left_cut_off_count += second_cut_off_length;
                            textBox.Text += "Удачно!" + Environment.NewLine;
                        }
                    } while (!end);
                }
                else
                {
                    estimated_length = new_estimated_length;
                    left_cut_off_count += first_cut_off_length;
                    textBox.Text += "Удачно!" + Environment.NewLine;
                }
            } while (!end);
            end = false;
            //начинаем отрезать справа до тех пор, пока не дойдем до совпадения
            do
            {
                char[] old_template = template;
                L = old_template.Length - first_cut_off_length;
                template = new char[L];
                Array.Copy(old_template, 0, template, 0, L);
                textBox.Text += "Отрезали справа " + first_cut_off_length + " символов...";
                ConvolutionComputation();
                int index_of_largest_peak = FindLargestPeak(),
                     second_largest_peak = FindNearbyPeak(index_of_largest_peak, MAX_INSERTION),
                     intersection_length1, intersection_length2;
                if (index_of_largest_peak <= L - 1) intersection_length1 = index_of_largest_peak + 1;
                else if (index_of_largest_peak <= N - 1) intersection_length1 = L;
                else intersection_length1 = N + L - 1 - index_of_largest_peak;
                if (second_largest_peak <= L - 1) intersection_length2 = second_largest_peak + 1;
                else if (second_largest_peak <= N - 1) intersection_length2 = L;
                else intersection_length2 = N + L - 1 - second_largest_peak;
                ro = GetProbability();
                int new_estimated_length1 = (int)Math.Round((result[index_of_largest_peak].real * L - ro * intersection_length1) / (1 - ro)),
                   new_estimated_length2 = (int)Math.Round((result[second_largest_peak].real * L - ro * intersection_length2) / (1 - ro)),
                   new_estimated_length = new_estimated_length1 + new_estimated_length2;
                if (new_estimated_length < estimated_length * 0.985)
                {
                    template = old_template;
                    L = template.Length;
                    textBox.Text += "Неудачно... Уменьшаем длину." + Environment.NewLine;
                    do
                    {
                        old_template = template;
                        L = old_template.Length - second_cut_off_length;
                        template = new char[L];
                        Array.Copy(old_template, 0, template, 0, L);
                        textBox.Text += "Отрезали справа " + second_cut_off_length + " символов...";
                        ConvolutionComputation();
                        index_of_largest_peak = FindLargestPeak();
                        second_largest_peak = FindNearbyPeak(index_of_largest_peak, MAX_INSERTION);
                        if (index_of_largest_peak <= L - 1) intersection_length1 = index_of_largest_peak + 1;
                        else if (index_of_largest_peak <= N - 1) intersection_length1 = L;
                        else intersection_length1 = N + L - 1 - index_of_largest_peak;
                        if (second_largest_peak <= L - 1) intersection_length2 = second_largest_peak + 1;
                        else if (second_largest_peak <= N - 1) intersection_length2 = L;
                        else intersection_length2 = N + L - 1 - second_largest_peak;

                        ro = GetProbability();
                        new_estimated_length1 = (int)Math.Round((result[index_of_largest_peak].real * L - ro * intersection_length1) / (1 - ro));
                        new_estimated_length2 = (int)Math.Round((result[second_largest_peak].real * L - ro * intersection_length2) / (1 - ro));
                        new_estimated_length = new_estimated_length1 + new_estimated_length2;
                        if (new_estimated_length < estimated_length)
                        {
                            template = old_template;
                            L = template.Length;
                            end = true;
                            textBox.Text += "Неудачно..." + Environment.NewLine;
                        }
                        else
                        {
                            estimated_length = new_estimated_length;
                            right_cut_off_count += second_cut_off_length;
                            textBox.Text += "Удачно!" + Environment.NewLine;
                        }
                    } while (!end);
                }
                else
                {
                    estimated_length = new_estimated_length;
                    right_cut_off_count += first_cut_off_length;
                    textBox.Text += "Удачно!" + Environment.NewLine;
                }
            } while (!end);

            textBox.Text += "Итоговая длина шаблона " + template.Length + " символов." + Environment.NewLine;
            textBox.Text += "Ожидаемая длина " + estimated_length + " символов." + Environment.NewLine;
            return new Tuple<int, int>(left_cut_off_count, right_cut_off_count);
        }
        private double GetProbability()//вычисление ro
        {
            Dictionary<char, double> p_main = new Dictionary<char, double>(0), p_temp = new Dictionary<char, double>(0);
            double res = 0;
            foreach (char c in alphabet)
            {
                p_main.Add(c, 0);
                p_temp.Add(c, 0);
            }
            p_main.Add(' ', 0);
            p_temp.Add(' ', 0);
            for (int i = 0; i < N; i++) p_main[main_chain[i]]++;
            for (int i = 0; i < L; i++) p_temp[template[i]]++;
            foreach (char c in alphabet) res += p_main[c] / N * p_temp[c] / L;
            return res;
            
            /*
            double pa_main = 0, pc_main = 0, pg_main = 0, pt_main = 0;
            double pa_temp = 0, pc_temp = 0, pg_temp = 0, pt_temp = 0;
            double k = 0;
            for (int i = 0; i < N; i++)
                switch (main_chain[i])
                {
                    case 'A':
                        goto case 'a';
                    case 'a':
                        pa_main++;
                        break;
                    case 'C':
                        goto case 'c';
                    case 'c':
                        pc_main++;
                        break;
                    case 'G':
                        goto case 'g';
                    case 'g':
                        pg_main++;
                        break;
                    case 'T':
                        goto case 't';
                    case 't':
                        pt_main++;
                        break;
                    default:
                        //MessageBox.Show("Посторонний символ " + main_chain[i]);
                        k++;
                        break;
                }
            for (int i = 0; i < L; i++)
                switch (template[i])
                {
                    case 'A':
                        goto case 'a';
                    case 'a':
                        pa_temp++;
                        break;
                    case 'C':
                        goto case 'c';
                    case 'c':
                        pc_temp++;
                        break;
                    case 'G':
                        goto case 'g';
                    case 'g':
                        pg_temp++;
                        break;
                    case 'T':
                        goto case 't';
                    case 't':
                        pt_temp++;
                        break;
                    default:
                        //MessageBox.Show("Посторонний символ " + template[i]);
                        k++;
                        break;
                }
            //k += pa + pc + pg + pt;
            pa_main /= N;
            pc_main /= N;
            pg_main /= N;
            pt_main /= N;

            pa_temp /= L;
            pc_temp /= L;
            pg_temp /= L;
            pt_temp /= L;
            //k = pa + pc + pg + pt;
            return (pa_main * pa_temp + pc_main * pc_temp + pg_main * pg_temp + pt_main * pt_temp);
            */
        }
        public int FindLargestPeak()//поиск наивысшего пика в свёртке
        {
            double max = 0;
            int ind = 0;
            ro = GetProbability();
            for (int i = 0; i < N + L - 1; i++)
            {
                //double res = result[i].real;
                //if (i <= L - 1) res -= (i + 1) * ro / L;
                //else if (i <= N - 1) res -= L * ro / L;
                //else res -= ro * (N + L - 1 - i) / L;
                if (result[i].real > max)
                {
                    max = result[i].real;
                    ind = i;
                }
            }
            return ind;
        }
        private int FindNearbyPeak(int center, int radius)//поиск пика рядом с основным в заданном радиусе
        {
            double max = 3 * standart_deviation;
            int ind = -1;
            ro = GetProbability();
            for (int i = center - radius; i < center; i++)
            {
                double res = result[i].real;
                if (i <= L - 1) res -= (i + 1) * ro / L;
                else if (i <= N - 1) res -= L * ro / L;
                else res -= ro * (N + L - 1 - i) / L;
                if (res > max)
                {
                    max = res;
                    ind = i;
                }
            }
            for (int i = center + 1; i <= center + radius; i++)
            {
                double res = result[i].real;
                if (i <= L - 1) res -= (i + 1) * ro / L;
                else if (i <= N - 1) res -= L * ro / L;
                else res -= ro * (N + L - 1 - i) / L;
                if (res > max)
                {
                    max = res;
                    ind = i;
                }
            }
            return ind;
        }
        private int[] FindNearbyPeaks(int center, int radius)//поиск нескольких пиков рядом с основным в заданном радиусе
        {
            int[] res = new int[0];
            ro = GetProbability();
            for (int i = center - radius; i <= center + radius; i++)
            {
                if ((i != center) && (result[i].real > ro + 2 * standart_deviation))
                {
                    Array.Resize(ref res, res.Length + 1);
                    //double max = Math.Min(result[i].real - result[i - 1].real, result[i].real - result[i + 1].real);
                    res[res.Length - 1] = i;
                }
            }
            for (int i = 0; i < res.Length; i++)
                for (int j = 0; j < res.Length - 1; j++)
                {
                    if (result[res[j]].real < result[res[j + 1]].real)
                    {
                        int temp = res[j];
                        res[j] = res[j + 1];
                        res[j + 1] = temp;
                    }
                }
            return res;
        }
        public double[] DetectTranspozone(string file_for_comments, string genome_name, string transpozone_name)
        {
            double[] res = new double[] { 0 };
            int start, end, left_mismatch = 0, right_mismatch = 0;
            ro = GetProbability();

            int largest_peak = FindLargestPeak();
            if (result[largest_peak].real < 0.4)
            {
                using (StreamWriter sw = new StreamWriter(file_for_comments, true, Encoding.Default))
                {
                    sw.WriteLine(genome_name + "\t" + transpozone_name + "\t-");
                }
                return res;
            }
            if (result[largest_peak].real >= 0.8)
            {
                start = largest_peak - L + 2;
                end = largest_peak + 1;
                while (main_chain[start - 1 + left_mismatch] != template[left_mismatch]) left_mismatch++;
                while (main_chain[end - 1 - right_mismatch] != template[L - 1 - right_mismatch]) right_mismatch++;
                Array.Resize(ref res, res.Length + 1);
                res[res.Length - 1] = end - start + 1 - left_mismatch - right_mismatch;
                res[0]++;
                using (StreamWriter sw = new StreamWriter(file_for_comments, true, Encoding.Default))
                {
                    sw.WriteLine(genome_name + "\t" + transpozone_name + "\tПик на "
                        + largest_peak + "[== " + Math.Round(result[largest_peak].real, 5) + "], в геноме с " + start + " до " + end
                        + "\tДлина " + (end - start + 1 - left_mismatch - right_mismatch) + " символов\tСовпавших символов " + Math.Round(result[largest_peak].real * L, 1) + " (из " + L + ")");
                    //выводим в файл "выравнивание"
                    char[] genome_seq_to_file = new char[L];
                    int begin = largest_peak - L + 1;
                    Array.Copy(main_chain, begin, genome_seq_to_file, 0, L);
                    sw.WriteLine("genome\t" + new string(genome_seq_to_file) + "\n\t" + new string(template) + "\n");
                }
            }
            else
            {
                int[] nearby_peaks = FindNearbyPeaks(largest_peak, 15);
                Tuple<int, double>[] peaks_and_values;
                int second_peak, third_peak, fourth_peak, fifth_peak;
                switch (nearby_peaks.Length)
                {
                    case 0: //рядом не нашлось пиков
                        start = largest_peak - L + 2;
                        end = largest_peak + 1;
                        while (main_chain[start - 1 + left_mismatch] != template[left_mismatch]) left_mismatch++;
                        while (main_chain[end - 1 - right_mismatch] != template[L - 1 - right_mismatch]) right_mismatch++;
                        Array.Resize(ref res, res.Length + 1);
                        res[res.Length - 1] = end - start + 1 - left_mismatch - right_mismatch;
                        res[0]++;
                        using (StreamWriter sw = new StreamWriter(file_for_comments, true, Encoding.Default))
                        {
                            sw.WriteLine(genome_name + "\t" + transpozone_name + "\tПик на "
                                + largest_peak + "[== " + Math.Round(result[largest_peak].real, 5) + "], в геноме с " + start + " до " + end
                                + "\tДлина " + (end - start + 1 - left_mismatch - right_mismatch) + " символов\tСовпавших символов " + Math.Round(result[largest_peak].real * L, 1) + "\tОжидаемая длина совпадения " + Math.Round((result[largest_peak].real - ro) * L / (1 - ro), 1));
                            //выводим в файл "выравнивание"
                            char[] genome_seq_to_file = new char[L];
                            int begin = largest_peak - L + 1;
                            Array.Copy(main_chain, begin, genome_seq_to_file, 0, L);
                            sw.WriteLine("genome\t" + new string(genome_seq_to_file) + "\n\t" + new string(template) + "\n");
                        }
                        break;
                    case 1: //рядом нашелся только один пик
                        second_peak = nearby_peaks[0];
                        if ((result[largest_peak].real + result[second_peak].real - 2 * ro) / (1 - ro) >= 0.7)
                        {
                            //Array.Resize(ref res, res.Length + 1);
                            //res[res.Length - 1] = (result[largest_peak].real + result[second_peak].real - 2 * ro) * L / (1 - ro);
                            //res[0]++;
                            start = Math.Min(largest_peak, second_peak) - L + 2;
                            end = Math.Max(largest_peak, second_peak) + 1;
                            while (main_chain[start - 1 + left_mismatch] != template[left_mismatch]) left_mismatch++;
                            while (main_chain[end - 1 - right_mismatch] != template[L - 1 - right_mismatch]) right_mismatch++;
                            Array.Resize(ref res, res.Length + 1);
                            res[res.Length - 1] = end - start + 1 - left_mismatch - right_mismatch;
                            res[0]++;
                            using (StreamWriter sw = new StreamWriter(file_for_comments, true, Encoding.Default))
                            {
                                sw.WriteLine(genome_name + "\t" + transpozone_name + "\tДва пика: "
                                    + largest_peak + "[== " + Math.Round(result[largest_peak].real, 5) + "] и "
                                    + second_peak + "[== " + Math.Round(result[second_peak].real, 5) + "], в геноме с " + start + " до " + end
                                    + "\tДлина " + (end - start + 1 - left_mismatch - right_mismatch) + " символов\tОжидаемая длина совпадения " + Math.Round((result[largest_peak].real + result[second_peak].real - 2 * ro) * L / (1 - ro)));
                                //выводим в файл "выравнивание"
                                char[] genome_seq_to_file = new char[L];
                                int begin = largest_peak - L + 1;
                                Array.Copy(main_chain, begin, genome_seq_to_file, 0, L);
                                sw.WriteLine("genome\t" + new string(genome_seq_to_file) + "\n\t" + new string(template) + "\n");
                            }
                        }
                        else
                        {
                            start = Math.Min(largest_peak, second_peak) - L + 2;
                            end = Math.Max(largest_peak, second_peak) + 1;
                            while (main_chain[start - 1 + left_mismatch] != template[left_mismatch]) left_mismatch++;
                            while (main_chain[end - 1 - right_mismatch] != template[L - 1 - right_mismatch]) right_mismatch++;
                            Array.Resize(ref res, res.Length + 1);
                            res[res.Length - 1] = end - start + 1 - left_mismatch - right_mismatch;
                            res[0]++;
                            using (StreamWriter sw = new StreamWriter(file_for_comments, true, Encoding.Default))
                            {
                                sw.WriteLine(genome_name + "\t" + transpozone_name + "\tДва пика: "
                                    + largest_peak + "[== " + Math.Round(result[largest_peak].real, 5) + "] и "
                                    + second_peak + "[== " + Math.Round(result[second_peak].real, 5) + "], в геноме с " + start + " до " + end
                                    + "\tДлина " + (end - start + 1 - left_mismatch - right_mismatch) + " символов\tВОЗМОЖНО совпадение длины " + Math.Round((result[largest_peak].real + result[second_peak].real - 2 * ro) * L / (1 - ro)));
                                //выводим в файл "выравнивание"
                                char[] genome_seq_to_file = new char[L];
                                int begin = largest_peak - L + 1;
                                Array.Copy(main_chain, begin, genome_seq_to_file, 0, L);
                                sw.WriteLine("genome\t" + new string(genome_seq_to_file) + "\n\t" + new string(template) + "\n");
                            }
                        }
                        break;
                    case 2: //рядом нашлось два пика
                        Array.Sort(nearby_peaks);
                        double[] values_of_nearby_peaks = new double[nearby_peaks.Length];
                        for (int i = 0; i < nearby_peaks.Length; i++) values_of_nearby_peaks[i] = result[nearby_peaks[i]].real;
                        second_peak = nearby_peaks[Array.IndexOf(values_of_nearby_peaks, values_of_nearby_peaks.Max())];
                        third_peak = nearby_peaks[Array.IndexOf(values_of_nearby_peaks, values_of_nearby_peaks.Min())];
                        if ((result[largest_peak].real + result[second_peak].real - 2 * ro) / (1 - ro) >= 0.7)
                        {
                            //Array.Resize(ref res, res.Length + 1);
                            //res[res.Length - 1] = (result[largest_peak].real + result[second_peak].real - 2 * ro) * L / (1 - ro);
                            //res[0]++;
                            start = Math.Min(largest_peak, second_peak) - L + 2;
                            end = Math.Max(largest_peak, second_peak) + 1;
                            while (main_chain[start - 1 + left_mismatch] != template[left_mismatch]) left_mismatch++;
                            while (main_chain[end - 1 - right_mismatch] != template[L - 1 - right_mismatch]) right_mismatch++;
                            Array.Resize(ref res, res.Length + 1);
                            res[res.Length - 1] = end - start + 1 - left_mismatch - right_mismatch;
                            res[0]++;
                            using (StreamWriter sw = new StreamWriter(file_for_comments, true, Encoding.Default))
                            {
                                sw.WriteLine(genome_name + "\t" + transpozone_name + "\tТри пика: "
                                    + largest_peak + "[== " + Math.Round(result[largest_peak].real, 5) + "] и "
                                    + second_peak + "[== " + Math.Round(result[second_peak].real, 5) + "], в геноме с " + start + " до " + end
                                    + "\tДлина " + (end - start + 1 - left_mismatch - right_mismatch) + " символов\tОжидаемая длина совпадения " + Math.Round((result[largest_peak].real + result[second_peak].real - 2 * ro) * L / (1 - ro)));
                                //выводим в файл "выравнивание"
                                char[] genome_seq_to_file = new char[L];
                                int begin = largest_peak - L + 1;
                                Array.Copy(main_chain, begin, genome_seq_to_file, 0, L);
                                sw.WriteLine("genome\t" + new string(genome_seq_to_file) + "\n\t" + new string(template) + "\n");
                            }
                        }
                        else if ((result[largest_peak].real + result[second_peak].real + result[third_peak].real - 3 * ro) / (1 - ro) >= 0.7)
                        {
                            //Array.Resize(ref res, res.Length + 1);
                            //res[res.Length - 1] = (result[largest_peak].real + result[second_peak].real + result[third_peak].real - 3 * ro) * L / (1 - ro);
                            //res[0]++;
                            start = Math.Min(largest_peak, nearby_peaks.Min()) - L + 2;
                            end = Math.Max(largest_peak, nearby_peaks.Max()) + 1;
                            while (main_chain[start - 1 + left_mismatch] != template[left_mismatch]) left_mismatch++;
                            while (main_chain[end - 1 - right_mismatch] != template[L - 1 - right_mismatch]) right_mismatch++;
                            Array.Resize(ref res, res.Length + 1);
                            res[res.Length - 1] = end - start + 1 - left_mismatch - right_mismatch;
                            res[0]++;
                            using (StreamWriter sw = new StreamWriter(file_for_comments, true, Encoding.Default))
                            {
                                sw.WriteLine(genome_name + "\t" + transpozone_name + "\tТри пика: "
                                    + largest_peak + "[== " + Math.Round(result[largest_peak].real, 5) + "] и "
                                    + second_peak + "[== " + Math.Round(result[second_peak].real, 5) + "] и "
                                    + third_peak + "[== " + Math.Round(result[third_peak].real, 5) + "], в геноме с " + start + " до " + end
                                    + "\tДлина " + (end - start + 1 - left_mismatch - right_mismatch) + " символов\tОжидаемая длина совпадения " + Math.Round((result[largest_peak].real + result[second_peak].real + result[third_peak].real - 3 * ro) * L / (1 - ro)));
                                //выводим в файл "выравнивание"
                                char[] genome_seq_to_file = new char[L];
                                int begin = largest_peak - L + 1;
                                Array.Copy(main_chain, begin, genome_seq_to_file, 0, L);
                                sw.WriteLine("genome\t" + new string(genome_seq_to_file) + "\n\t" + new string(template) + "\n");
                            }
                        }
                        else
                        {
                            start = Math.Min(largest_peak, nearby_peaks.Min()) - L + 2;
                            end = Math.Max(largest_peak, nearby_peaks.Max()) + 1;
                            while (main_chain[start - 1 + left_mismatch] != template[left_mismatch]) left_mismatch++;
                            while (main_chain[end - 1 - right_mismatch] != template[L - 1 - right_mismatch]) right_mismatch++;
                            Array.Resize(ref res, res.Length + 1);
                            res[res.Length - 1] = end - start + 1 - left_mismatch - right_mismatch;
                            res[0]++;
                            using (StreamWriter sw = new StreamWriter(file_for_comments, true, Encoding.Default))
                            {
                                sw.WriteLine(genome_name + "\t" + transpozone_name + "\tТри пика: "
                                    + largest_peak + "[== " + Math.Round(result[largest_peak].real, 5) + "] и "
                                    + second_peak + "[== " + Math.Round(result[second_peak].real, 5) + "] и "
                                    + third_peak + "[== " + Math.Round(result[third_peak].real, 5) + "], в геноме с " + start + " до " + end
                                    + "\tДлина " + (end - start + 1 - left_mismatch - right_mismatch) + " символов\tВОЗМОЖНО совпадение длины " + Math.Round((result[largest_peak].real + result[second_peak].real + result[third_peak].real - 3 * ro) * L / (1 - ro)));
                                //выводим в файл "выравнивание"
                                char[] genome_seq_to_file = new char[L];
                                int begin = largest_peak - L + 1;
                                Array.Copy(main_chain, begin, genome_seq_to_file, 0, L);
                                sw.WriteLine("genome\t" + new string(genome_seq_to_file) + "\n\t" + new string(template) + "\n");
                            }
                        }
                        break;
                    case 3:
                        peaks_and_values = new Tuple<int, double>[nearby_peaks.Length + 1];
                        peaks_and_values[0] = new Tuple<int, double>(largest_peak, result[largest_peak].real);
                        for (int i = 0; i < nearby_peaks.Length; i++) peaks_and_values[i + 1] = new Tuple<int, double>(nearby_peaks[i], result[nearby_peaks[i]].real);
                        peaks_and_values.OrderByDescending(x => x.Item2); //сортируем массив по убыванию значений свёртки
                        second_peak = peaks_and_values[0].Item1;
                        third_peak = peaks_and_values[1].Item1;
                        fourth_peak = peaks_and_values[2].Item1;
                        if ((result[largest_peak].real + result[second_peak].real - 2 * ro) / (1 - ro) >= 0.7)
                        {
                            //Array.Resize(ref res, res.Length + 1);
                            //res[res.Length - 1] = (result[largest_peak].real + result[second_peak].real - 2 * ro) * L / (1 - ro);
                            //res[0]++;
                            start = Math.Min(largest_peak, second_peak) - L + 2;
                            end = Math.Max(largest_peak, second_peak) + 1;
                            while (main_chain[start - 1 + left_mismatch] != template[left_mismatch]) left_mismatch++;
                            while (main_chain[end - 1 - right_mismatch] != template[L - 1 - right_mismatch]) right_mismatch++;
                            Array.Resize(ref res, res.Length + 1);
                            res[res.Length - 1] = end - start + 1 - left_mismatch - right_mismatch;
                            res[0]++;
                            using (StreamWriter sw = new StreamWriter(file_for_comments, true, Encoding.Default))
                            {
                                sw.WriteLine(genome_name + "\t" + transpozone_name + "\tЧетыре пика: "
                                    + largest_peak + "[== " + Math.Round(result[largest_peak].real, 5) + "] и "
                                    + second_peak + "[== " + Math.Round(result[second_peak].real, 5) + "], в геноме с " + start + " до " + end
                                    + "\tДлина " + (end - start + 1 - left_mismatch - right_mismatch) + " символов\tОжидаемая длина совпадения " + Math.Round((result[largest_peak].real + result[second_peak].real - 2 * ro) * L / (1 - ro)));
                                //выводим в файл "выравнивание"
                                char[] genome_seq_to_file = new char[L];
                                int begin = largest_peak - L + 1;
                                Array.Copy(main_chain, begin, genome_seq_to_file, 0, L);
                                sw.WriteLine("genome\t" + new string(genome_seq_to_file) + "\n\t" + new string(template) + "\n");
                            }
                        }
                        else if ((result[largest_peak].real + result[second_peak].real + result[third_peak].real - 3 * ro) / (1 - ro) >= 0.7)
                        {
                            //Array.Resize(ref res, res.Length + 1);
                            //res[res.Length - 1] = (result[largest_peak].real + result[second_peak].real + result[third_peak].real - 3 * ro) * L / (1 - ro);
                            //res[0]++;
                            start = Math.Min(largest_peak, nearby_peaks.Min()) - L + 2;
                            end = Math.Max(largest_peak, nearby_peaks.Max()) + 1;
                            while (main_chain[start - 1 + left_mismatch] != template[left_mismatch]) left_mismatch++;
                            while (main_chain[end - 1 - right_mismatch] != template[L - 1 - right_mismatch]) right_mismatch++;
                            Array.Resize(ref res, res.Length + 1);
                            res[res.Length - 1] = end - start + 1 - left_mismatch - right_mismatch;
                            res[0]++;
                            using (StreamWriter sw = new StreamWriter(file_for_comments, true, Encoding.Default))
                            {
                                sw.WriteLine(genome_name + "\t" + transpozone_name + "\tЧетыре пика: "
                                    + largest_peak + "[== " + Math.Round(result[largest_peak].real, 5) + "] и "
                                    + second_peak + "[== " + Math.Round(result[second_peak].real, 5) + "] и "
                                    + third_peak + "[== " + Math.Round(result[third_peak].real, 5) + "], в геноме с " + start + " до " + end
                                    + "\tДлина " + (end - start + 1 - left_mismatch - right_mismatch) + " символов\tОжидаемая длина совпадения " + Math.Round((result[largest_peak].real + result[second_peak].real + result[third_peak].real - 3 * ro) * L / (1 - ro)));
                                //выводим в файл "выравнивание"
                                char[] genome_seq_to_file = new char[L];
                                int begin = largest_peak - L + 1;
                                Array.Copy(main_chain, begin, genome_seq_to_file, 0, L);
                                sw.WriteLine("genome\t" + new string(genome_seq_to_file) + "\n\t" + new string(template) + "\n");
                            }
                        }
                        else if ((result[largest_peak].real + result[second_peak].real
                            + result[third_peak].real + result[fourth_peak].real - 4 * ro) / (1 - ro) >= 0.7)
                        {
                            //Array.Resize(ref res, res.Length + 1);
                            //res[res.Length - 1] = (result[largest_peak].real + result[second_peak].real + result[third_peak].real + result[fourth_peak].real - 4 * ro) * L / (1 - ro);
                            //res[0]++;
                            start = Math.Min(largest_peak, nearby_peaks.Min()) - L + 2;
                            end = Math.Max(largest_peak, nearby_peaks.Max()) + 1;
                            while (main_chain[start - 1 + left_mismatch] != template[left_mismatch]) left_mismatch++;
                            while (main_chain[end - 1 - right_mismatch] != template[L - 1 - right_mismatch]) right_mismatch++;
                            Array.Resize(ref res, res.Length + 1);
                            res[res.Length - 1] = end - start + 1 - left_mismatch - right_mismatch;
                            res[0]++;
                            using (StreamWriter sw = new StreamWriter(file_for_comments, true, Encoding.Default))
                            {
                                sw.WriteLine(genome_name + "\t" + transpozone_name + "\tЧетыре пика: "
                                    + largest_peak + "[== " + Math.Round(result[largest_peak].real, 5) + "] и "
                                    + second_peak + "[== " + Math.Round(result[second_peak].real, 5) + "] и "
                                    + third_peak + "[== " + Math.Round(result[third_peak].real, 5) + "] и "
                                    + fourth_peak + "[== " + Math.Round(result[fourth_peak].real, 5) + "] в геноме с " + start + " до " + end
                                    + "\tДлина " + (end - start + 1 - left_mismatch - right_mismatch) + " символов\tОжидаемая длина совпадения " + Math.Round((result[largest_peak].real + result[second_peak].real + result[third_peak].real + result[fourth_peak].real - 4 * ro) * L / (1 - ro)));
                                //выводим в файл "выравнивание"
                                char[] genome_seq_to_file = new char[L];
                                int begin = largest_peak - L + 1;
                                Array.Copy(main_chain, begin, genome_seq_to_file, 0, L);
                                sw.WriteLine("genome\t" + new string(genome_seq_to_file) + "\n\t" + new string(template) + "\n");
                            }
                        }
                        else
                        {
                            start = Math.Min(largest_peak, nearby_peaks.Min()) - L + 2;
                            end = Math.Max(largest_peak, nearby_peaks.Max()) + 1;
                            while (main_chain[start - 1 + left_mismatch] != template[left_mismatch]) left_mismatch++;
                            while (main_chain[end - 1 - right_mismatch] != template[L - 1 - right_mismatch]) right_mismatch++;
                            Array.Resize(ref res, res.Length + 1);
                            res[res.Length - 1] = end - start + 1 - left_mismatch - right_mismatch;
                            res[0]++;
                            using (StreamWriter sw = new StreamWriter(file_for_comments, true, Encoding.Default))
                            {
                                sw.WriteLine(genome_name + "\t" + transpozone_name + "\tЧетыре пика: "
                                    + largest_peak + "[== " + Math.Round(result[largest_peak].real, 5) + "] и "
                                    + second_peak + "[== " + Math.Round(result[second_peak].real, 5) + "] и "
                                    + third_peak + "[== " + Math.Round(result[third_peak].real, 5) + "] и "
                                    + fourth_peak + "[== " + Math.Round(result[fourth_peak].real, 5) + "] в геноме с " + start + " до " + end
                                    + "\tДлина " + (end - start + 1 - left_mismatch - right_mismatch) + " символов\tВОЗМОЖНО совпадение длины " + Math.Round((result[largest_peak].real + result[second_peak].real + result[third_peak].real + result[fourth_peak].real - 4 * ro) * L / (1 - ro)));
                                //выводим в файл "выравнивание"
                                char[] genome_seq_to_file = new char[L];
                                int begin = largest_peak - L + 1;
                                Array.Copy(main_chain, begin, genome_seq_to_file, 0, L);
                                sw.WriteLine("genome\t" + new string(genome_seq_to_file) + "\n\t" + new string(template) + "\n");
                            }
                        }
                        break;
                    case 4:
                        peaks_and_values = new Tuple<int, double>[nearby_peaks.Length + 1];
                        peaks_and_values[0] = new Tuple<int, double>(largest_peak, result[largest_peak].real);
                        for (int i = 0; i < nearby_peaks.Length; i++) peaks_and_values[i + 1] = new Tuple<int, double>(nearby_peaks[i], result[nearby_peaks[i]].real);
                        peaks_and_values.OrderByDescending(x => x.Item2); //сортируем массив по убыванию значений свёртки
                        second_peak = peaks_and_values[0].Item1;
                        third_peak = peaks_and_values[1].Item1;
                        fourth_peak = peaks_and_values[2].Item1;
                        fifth_peak = peaks_and_values[3].Item1;

                        if ((result[largest_peak].real + result[second_peak].real - 2 * ro) / (1 - ro) >= 0.7)
                        {
                            //Array.Resize(ref res, res.Length + 1);
                            //res[res.Length - 1] = (result[largest_peak].real + result[second_peak].real - 2 * ro) * L / (1 - ro);
                            //res[0]++;
                            start = Math.Min(largest_peak, second_peak) - L + 2;
                            end = Math.Max(largest_peak, second_peak) + 1;
                            while (main_chain[start - 1 + left_mismatch] != template[left_mismatch]) left_mismatch++;
                            while (main_chain[end - 1 - right_mismatch] != template[L - 1 - right_mismatch]) right_mismatch++;
                            Array.Resize(ref res, res.Length + 1);
                            res[res.Length - 1] = end - start + 1 - left_mismatch - right_mismatch;
                            res[0]++;
                            using (StreamWriter sw = new StreamWriter(file_for_comments, true, Encoding.Default))
                            {
                                sw.WriteLine(genome_name + "\t" + transpozone_name + "\tПять пиков: "
                                    + largest_peak + "[== " + Math.Round(result[largest_peak].real, 5) + "] и "
                                    + second_peak + "[== " + Math.Round(result[second_peak].real, 5) + "], в геноме с " + start + " до " + end
                                    + "\tДлина " + (end - start + 1 - left_mismatch - right_mismatch) + " символов\tОжидаемая длина совпадения " + Math.Round((result[largest_peak].real + result[second_peak].real - 2 * ro) * L / (1 - ro)));
                                //выводим в файл "выравнивание"
                                char[] genome_seq_to_file = new char[L];
                                int begin = largest_peak - L + 1;
                                Array.Copy(main_chain, begin, genome_seq_to_file, 0, L);
                                sw.WriteLine("genome\t" + new string(genome_seq_to_file) + "\n\t" + new string(template) + "\n");
                            }
                        }
                        else if ((result[largest_peak].real + result[second_peak].real + result[third_peak].real - 3 * ro) / (1 - ro) >= 0.7)
                        {
                            //Array.Resize(ref res, res.Length + 1);
                            //res[res.Length - 1] = (result[largest_peak].real + result[second_peak].real + result[third_peak].real - 3 * ro) * L / (1 - ro);
                            //res[0]++;
                            start = Math.Min(largest_peak, nearby_peaks.Min()) - L + 2;
                            end = Math.Max(largest_peak, nearby_peaks.Max()) + 1;
                            while (main_chain[start - 1 + left_mismatch] != template[left_mismatch]) left_mismatch++;
                            while (main_chain[end - 1 - right_mismatch] != template[L - 1 - right_mismatch]) right_mismatch++;
                            Array.Resize(ref res, res.Length + 1);
                            res[res.Length - 1] = end - start + 1 - left_mismatch - right_mismatch;
                            res[0]++;
                            using (StreamWriter sw = new StreamWriter(file_for_comments, true, Encoding.Default))
                            {
                                sw.WriteLine(genome_name + "\t" + transpozone_name + "\tПять пиков: "
                                    + largest_peak + "[== " + Math.Round(result[largest_peak].real, 5) + "] и "
                                    + second_peak + "[== " + Math.Round(result[second_peak].real, 5) + "] и "
                                    + third_peak + "[== " + Math.Round(result[third_peak].real, 5) + "], в геноме с " + start + " до " + end
                                    + "\tДлина " + (end - start + 1 - left_mismatch - right_mismatch) + " символов\tОжидаемая длина совпадения " + Math.Round((result[largest_peak].real + result[second_peak].real + result[third_peak].real - 3 * ro) * L / (1 - ro)));
                                //выводим в файл "выравнивание"
                                char[] genome_seq_to_file = new char[L];
                                int begin = largest_peak - L + 1;
                                Array.Copy(main_chain, begin, genome_seq_to_file, 0, L);
                                sw.WriteLine("genome\t" + new string(genome_seq_to_file) + "\n\t" + new string(template) + "\n");
                            }
                        }
                        else if ((result[largest_peak].real + result[second_peak].real
                            + result[third_peak].real + result[fourth_peak].real - 4 * ro) / (1 - ro) >= 0.7)
                        {
                            //Array.Resize(ref res, res.Length + 1);
                            //res[res.Length - 1] = (result[largest_peak].real + result[second_peak].real + result[third_peak].real + result[fourth_peak].real - 4 * ro) * L / (1 - ro);
                            //res[0]++;
                            start = Math.Min(largest_peak, nearby_peaks.Min()) - L + 2;
                            end = Math.Max(largest_peak, nearby_peaks.Max()) + 1;
                            while (main_chain[start - 1 + left_mismatch] != template[left_mismatch]) left_mismatch++;
                            while (main_chain[end - 1 - right_mismatch] != template[L - 1 - right_mismatch]) right_mismatch++;
                            Array.Resize(ref res, res.Length + 1);
                            res[res.Length - 1] = end - start + 1 - left_mismatch - right_mismatch;
                            res[0]++;
                            using (StreamWriter sw = new StreamWriter(file_for_comments, true, Encoding.Default))
                            {
                                sw.WriteLine(genome_name + "\t" + transpozone_name + "\tПять пиков: "
                                    + largest_peak + "[== " + Math.Round(result[largest_peak].real, 5) + "] и "
                                    + second_peak + "[== " + Math.Round(result[second_peak].real, 5) + "] и "
                                    + third_peak + "[== " + Math.Round(result[third_peak].real, 5) + "] и "
                                    + fourth_peak + "[== " + Math.Round(result[fourth_peak].real, 5) + "] в геноме с " + start + " до " + end
                                    + "\tДлина " + (end - start + 1 - left_mismatch - right_mismatch) + " символов\tОжидаемая длина совпадения " + Math.Round((result[largest_peak].real + result[second_peak].real + result[third_peak].real + result[fourth_peak].real - 4 * ro) * L / (1 - ro)));
                                //выводим в файл "выравнивание"
                                char[] genome_seq_to_file = new char[L];
                                int begin = largest_peak - L + 1;
                                Array.Copy(main_chain, begin, genome_seq_to_file, 0, L);
                                sw.WriteLine("genome\t" + new string(genome_seq_to_file) + "\n\t" + new string(template) + "\n");
                            }
                        }
                        else if ((result[largest_peak].real + result[second_peak].real
                            + result[third_peak].real + result[fourth_peak].real + result[fifth_peak].real - 5 * ro) / (1 - ro) >= 0.7)
                        {
                            //Array.Resize(ref res, res.Length + 1);
                            //res[res.Length - 1] = (result[largest_peak].real + result[second_peak].real + result[third_peak].real + result[fourth_peak].real - 4 * ro) * L / (1 - ro);
                            //res[0]++;
                            start = Math.Min(largest_peak, nearby_peaks.Min()) - L + 2;
                            end = Math.Max(largest_peak, nearby_peaks.Max()) + 1;
                            while (main_chain[start - 1 + left_mismatch] != template[left_mismatch]) left_mismatch++;
                            while (main_chain[end - 1 - right_mismatch] != template[L - 1 - right_mismatch]) right_mismatch++;
                            Array.Resize(ref res, res.Length + 1);
                            res[res.Length - 1] = end - start + 1 - left_mismatch - right_mismatch;
                            res[0]++;
                            using (StreamWriter sw = new StreamWriter(file_for_comments, true, Encoding.Default))
                            {
                                sw.WriteLine(genome_name + "\t" + transpozone_name + "\tПять пиков: "
                                    + largest_peak + "[== " + Math.Round(result[largest_peak].real, 5) + "] и "
                                    + second_peak + "[== " + Math.Round(result[second_peak].real, 5) + "] и "
                                    + third_peak + "[== " + Math.Round(result[third_peak].real, 5) + "] и "
                                    + fourth_peak + "[== " + Math.Round(result[fourth_peak].real, 5) + "] и "
                                    + fifth_peak + "[== " + Math.Round(result[fifth_peak].real, 5) + "] в геноме с " + start + " до " + end
                                    + "\tДлина " + (end - start + 1 - left_mismatch - right_mismatch) + " символов\tОжидаемая длина совпадения " + Math.Round((result[largest_peak].real + result[second_peak].real + result[third_peak].real
                                    + result[fourth_peak].real + result[fifth_peak].real - 5 * ro) * L / (1 - ro)));
                                //выводим в файл "выравнивание"
                                char[] genome_seq_to_file = new char[L];
                                int begin = largest_peak - L + 1;
                                Array.Copy(main_chain, begin, genome_seq_to_file, 0, L);
                                sw.WriteLine("genome\t" + new string(genome_seq_to_file) + "\n\t" + new string(template) + "\n");
                            }
                        }
                        else
                        {
                            start = Math.Min(largest_peak, nearby_peaks.Min()) - L + 2;
                            end = Math.Max(largest_peak, nearby_peaks.Max()) + 1;
                            while (main_chain[start - 1 + left_mismatch] != template[left_mismatch]) left_mismatch++;
                            while (main_chain[end - 1 - right_mismatch] != template[L - 1 - right_mismatch]) right_mismatch++;
                            Array.Resize(ref res, res.Length + 1);
                            res[res.Length - 1] = end - start + 1 - left_mismatch - right_mismatch;
                            res[0]++;
                            using (StreamWriter sw = new StreamWriter(file_for_comments, true, Encoding.Default))
                            {
                                sw.WriteLine(genome_name + "\t" + transpozone_name + "\tПять пиков: "
                                    + largest_peak + "[== " + Math.Round(result[largest_peak].real, 5) + "] и "
                                    + second_peak + "[== " + Math.Round(result[second_peak].real, 5) + "] и "
                                    + third_peak + "[== " + Math.Round(result[third_peak].real, 5) + "] и "
                                    + fourth_peak + "[== " + Math.Round(result[fourth_peak].real, 5) + "] и "
                                    + fifth_peak + "[== " + Math.Round(result[fifth_peak].real, 5) + "] в геноме с " + start + " до " + end
                                    + "\tДлина " + (end - start + 1 - left_mismatch - right_mismatch) + " символов\tВОЗМОЖНО совпадение длины " + Math.Round((result[largest_peak].real + result[second_peak].real + result[third_peak].real
                                    + result[fourth_peak].real + result[fifth_peak].real - 5 * ro) * L / (1 - ro)));
                                //выводим в файл "выравнивание"
                                char[] genome_seq_to_file = new char[L];
                                int begin = largest_peak - L + 1;
                                Array.Copy(main_chain, begin, genome_seq_to_file, 0, L);
                                sw.WriteLine("genome\t" + new string(genome_seq_to_file) + "\n\t" + new string(template) + "\n");
                            }
                        }
                        break;
                    default:
                        Array.Sort(nearby_peaks);
                        using (StreamWriter sw = new StreamWriter(file_for_comments, true, Encoding.Default))
                        {
                            sw.WriteLine(genome_name + "\t" + transpozone_name + "\tБольше четырех подозрительных пиков, пока не обработано.");
                        }
                        break;
                }
            }
            return res;
        }
    }
}