using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using MessageBox = System.Windows.Forms.MessageBox;

namespace AntLab
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        bool draw = true;
        Random rand = new Random();

        double record = 9999;


        AntEnvironment environment;


        public MainWindow()
        {
            
            InitializeComponent();

            lastN = int.Parse(N_textBox.Text);
            environment = new AntEnvironment(lastN);

            if (draw) mysvgViewbox.SvgSource = environment.draw();
           
           

        }

        StreamWriter sw = null;

        //1 итерация всех муравьёв эпохи
        bool calcEpoch(bool auto = false)
        {
            environment.QVAL = double.Parse(Q_textBox.Text.Replace(".",","));
            environment.ALPHA = float.Parse(alpha_textBox.Text.Replace(".", ","));
            environment.BETA = float.Parse(Q_textBox.Text.Replace(".", ","));
            environment.RHO = double.Parse(ro_textBox.Text.Replace(".", ","));


            environment.stepEpoch();

            if (draw) mysvgViewbox.SvgSource = environment.draw();

            if (!auto) 
            {
                listBox.Items.Clear();
                foreach (var item in environment.antsList)
                {
                    ListBoxItem l = new ListBoxItem();
                    foreach (var item2 in item.history)
                    {
                        l.Content += item2.id.ToString() + "->";
                    }
                    listBox.Items.Add(l);
                }
            }




            float s1 = environment.antsList.Sum(a => a.history.Count);
            float s2 = environment.antsList.Count * (environment.nodes.Count - 1);

            if (s1 == s2)
            {
                button.IsEnabled = false;
                button1.IsEnabled = true;


                if (!auto)
                {
                    listBox.Items.Clear();
                    foreach (var a in environment.antsList)
                    {
                        ListBoxItem l = new ListBoxItem();

                        l.Content += string.Format("#{0} | ", a.id.ToString());
                        foreach (var item2 in a.history)
                        {
                            l.Content += item2.id.ToString() + "->";
                        }
                        l.Content += environment.connects.Find(c => (c.n1 == a.startNode && c.n2 == a.currentNode) || (c.n2 == a.startNode && c.n1 == a.currentNode)).id.ToString();

                        if (environment.getBestAnt() == a) l.Content += "(Лучший)";

                        listBox.Items.Add(l);
                    }
                }





                double cur_record = environment.getBestAnt().history.Sum(a => a.distance) + environment.connects.Find(c => (c.n1 == environment.getBestAnt().startNode && c.n2 == environment.getBestAnt().currentNode) || (c.n2 == environment.getBestAnt().startNode && c.n1 == environment.getBestAnt().currentNode)).distance;

                if (cur_record < record)
                {
                    //label.Content = String.Format("Рекорд: {0} шагов", cur_record);
                    record = cur_record;

                    if (true)/////////////////////AUTO
                    {
                        if (true) mysvgViewbox.SvgSource = environment.draw(); //draw



                        listBox.Items.Clear();
                        foreach (var a in environment.antsList)
                        {
                            ListBoxItem l = new ListBoxItem();

                            l.Content += string.Format("#{0} | ", a.id.ToString());
                            foreach (var item2 in a.history)
                            {
                                l.Content += item2.id.ToString() + "->";
                            }
                            l.Content += environment.connects.Find(c => (c.n1 == a.startNode && c.n2 == a.currentNode) || (c.n2 == a.startNode && c.n1 == a.currentNode)).id.ToString();

                            if (environment.getBestAnt() == a) l.Content += "(Лучший)" + (environment.getBestAnt().history_distance + environment.connects.Find(c => (c.n1 == a.startNode && c.n2 == a.currentNode) || (c.n2 == a.startNode && c.n1 == a.currentNode)).distance).ToString();




                            listBox.Items.Add(l);
                            if (environment.getBestAnt() == a)
                                listBox_Copy.Items.Add(new ListBoxItem() { Content = l.Content });
                        }




                    }

                }

                return false;
            }

            return true;

        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            draw = true;
            calcEpoch();
        }

        int lastN = 0;
        private void button1_Click(object sender, RoutedEventArgs e)
        {
            if (int.Parse(N_textBox.Text) != lastN)
            {
                environment = new AntEnvironment(int.Parse(N_textBox.Text));
                lastN = int.Parse(N_textBox.Text);
            }
            else
            {
                environment.updateTrails();
            }


            environment.restart();
            if (draw) mysvgViewbox.SvgSource = environment.draw();
            button.IsEnabled = true;
            button1.IsEnabled = false;
        }

        int autoN = 5;

        void autoCalc()
        {


            draw = false;

            for (int i = 0; i < autoN; i++)
            {

                while (calcEpoch(true))
                {

                }

                environment.updateTrails();

                if (i < autoN - 1)
                    environment.restart();

            }

            //draw = true;
            //image.Source = environment.draw();

            DialogResult result = MessageBox.Show("Симуляция завершена.", "Внимание!", MessageBoxButtons.OK);
        }

        private void button2_Click(object sender, RoutedEventArgs e)
        {

            autoCalc();




        }

        void nTextBoxChanged()
        {
            DialogResult result = MessageBox.Show("Вы уверенны, что хотите перестроить граф?", "Предупреждение!", MessageBoxButtons.YesNo);
            if (result == System.Windows.Forms.DialogResult.Yes)
            {

                environment = new AntEnvironment(int.Parse(N_textBox.Text));
                lastN = int.Parse(N_textBox.Text);



                environment.restart();
                if (draw) mysvgViewbox.SvgSource = environment.draw();
                button.IsEnabled = true;
                button1.IsEnabled = false;

                listBox.Items.Clear();
                listBox_Copy.Items.Clear();

                record = 9999;
                AntEnvironment.CONNECTID = 0;
                mysvgViewbox.SvgSource = environment.draw();
            }
        }

        private void N_textBox_LostFocus(object sender, RoutedEventArgs e)
        {
            //nTextBoxChanged();
            
        }


        private void label2_Initialized(object sender, EventArgs e)
        {
            autoN = Convert.ToInt32(slider.Value);
            label2.Content = String.Format("{0} эпох(а)", slider.Value);
        }

        private void slider_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            autoN = Convert.ToInt32(slider.Value);
            label2.Content = String.Format("{0} эпох(а)", Convert.ToInt32(slider.Value));

        }

        private void N_textBox_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Enter) nTextBoxChanged();
        }
    }
}
