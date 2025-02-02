using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Media.Imaging;
using GraphVizWrapper;
using GraphVizWrapper.Commands;
using GraphVizWrapper.Queries;
using SharpVectors.Converters;
using Svg;

namespace AntLab
{

    class Connection
    {
        public int id = 0;
        public AntEnvNode n1 = null;
        public AntEnvNode n2 = null;

        public double feromon, distance;
        private Connection connection;

        public Object Clone()
        {
            return this.MemberwiseClone();
        }

        public Connection(AntEnvNode n1, AntEnvNode n2, double feromon, int id = 0, double distance = 1)
        {
            this.id = id;
            this.n1 = n1;
            this.n2 = n2;
            this.feromon = feromon;
            this.distance = distance;
        }
    }

    class AntEnvNode
    {
        AntEnvironment env;

        public List<Ant> _ant = new List<Ant>();


        public AntEnvNode(AntEnvironment env)
        {
            this.env = env;
            _ant = new List<Ant>();
            
        }

        public void addNeighborNode(AntEnvNode neighbor)
        {
            if (!env.getNeighbors(this).Exists(e => e.Equals(neighbor)))
            {
                env.connects.Add(new Connection(this, neighbor, env.INIT_PHEROMONE, AntEnvironment.CONNECTID++, AntEnvironment.rand.Next(1, 100)));
            }

        }

    }

    class Ant
    {
        public List<Connection> history = new List<Connection>();
        public List<AntEnvNode> node_history = new List<AntEnvNode>();

        public int id;

        private AntEnvNode _startNode;
        public AntEnvNode startNode
        {
            get
            {
                return _startNode;
            }
        }

        public AntEnvNode currentNode;


        private double _history_distance;
        public double history_distance
        {
            get
            {
                return history.Sum(a => a.distance);
            }
        }


        public Ant(int id, AntEnvNode startNode)
        {
            this.id = id;
            this._startNode = startNode;
        }

    }
    public static class RandomExtensions
    {
        public static double NextDouble(
            this Random random,
            double minValue,
            double maxValue)
        {
            return random.NextDouble() * (maxValue - minValue) + minValue;
        }
    }

    internal class AntEnvironment
    {
        /// <summary>
        /// 
        /// </summary>
        public float ALPHA = 1.0f;
        public float BETA = 5.0f;
        public double INIT_PHEROMONE;
        public double RHO = 0.5;
        public double QVAL = 100;
        /// <summary>
        /// //////////////////
        /// </summary>



        int ANTGLOBALID = 0;
        public static int CONNECTID = 0;

        static public Random rand = new Random();
        public List<AntEnvNode> nodes = new List<AntEnvNode>();
        public List<Ant> antsList = new List<Ant>();


        public List<Connection> connects = new List<Connection>();


        public List<AntEnvNode> getNeighbors(AntEnvNode envNode)
        {
            List<AntEnvNode> result = new List<AntEnvNode>();
            List<Connection> f = connects.FindAll(c => c.n1 == envNode || c.n2 == envNode);
            foreach (var item in f)
            {
                if (item.n1 != envNode)
                {
                    result.Add(item.n1);
                }
                else
                {
                    result.Add(item.n2);
                }
            }

            return result;
        }

        public Connection findConnection(AntEnvNode envNode1, AntEnvNode envNode2)
        {
            return connects.Find(c => (c.n1 == envNode1 && c.n2 == envNode2) || (c.n1 == envNode2 && c.n2 == envNode1));
        }



        public AntEnvironment(int n)
        {
            INIT_PHEROMONE = 1.0f / n;
            CONNECTID = 0;

            for (int i = 0; i < n; i++)
            {
                nodes.Add(new AntEnvNode(this));
                Ant t = new Ant(ANTGLOBALID++, nodes.Last());
                nodes.Last()._ant.Add(t);
                antsList.Add(t);
            }


            foreach (var item in nodes)
            {
                foreach (var neighbor in nodes)
                {
                    if (!item.Equals(neighbor))
                    {
                        item.addNeighborNode(neighbor);
                    }
                }
            }


        }

        public void restart()
        {
            ANTGLOBALID = 0;

            foreach (var item in nodes)
            {
                item._ant.Clear();
            }

            for (int i = 0; i < antsList.Count; i++)
            {
                antsList[i] = null;
            }

            antsList.Clear();



            for (int i = 0; i < nodes.Count; i++)
            {
                Ant t = new Ant(ANTGLOBALID++, nodes[i]);
                nodes[i]._ant.Add(t);
                antsList.Add(t);
            }
        }

        public void stepEpoch()
        {
            //updateFeromon(nodes[3].neighbors[0], 2);
            foreach (var ant in antsList)
            {
                simStepAnt(ant);
            }
            Console.WriteLine();
        }

        void print(List<AntEnvNode> nodes)
        {
            foreach (var item in nodes)
            {
                //Console.Write("{0} ",item.ant.id);
            }
            Console.WriteLine();
        }

        void print(List<Connection> nodes)
        {
            foreach (var item in nodes)
            {
                //Console.Write("({0}<->{1}, f={2})\n", item.n1.ant.id, item.n2.ant.id, item.feromon);
            }
            Console.WriteLine();
        }

        private static BitmapImage LoadImage(byte[] imageData)
        {
            try
            {
                if (imageData == null || imageData.Length == 0) return null;
                var image = new BitmapImage();
                using (var mem = new MemoryStream(imageData))
                {
                    mem.Position = 0;
                    image.BeginInit();
                    image.CreateOptions = BitmapCreateOptions.PreservePixelFormat;
                    image.CacheOption = BitmapCacheOption.OnLoad;
                    image.UriSource = null;
                    image.StreamSource = mem;
                    image.EndInit();
                }
                image.Freeze();
                MessageBox.Show("nice");

                return image;
            }
            catch (Exception e)
            {

                MessageBox.Show("lol gg" + e.ToString());
                return null;
            }
            
        }

        private Connection findEdge(AntEnvNode n1, AntEnvNode n2)
        {
            foreach (var c in connects)
            {
                //Console.WriteLine((c.n1 == n1 && c.n2 == n2) || (c.n2 == n1 && c.n1 == n2));
                if ((c.n1 == n1 && c.n2 == n2) || (c.n2 == n1 && c.n1 == n2)) return c;
            }
            return null;
        }

        public Ant getBestAnt()
        {
            Ant result = null;
            try
            {



                double min = 9999;
                for (int i = 0; i < antsList.Count; i++)
                {
                    if (antsList[i].history_distance + findEdge(antsList[i].startNode, antsList[i].currentNode).distance < min)
                    {
                        result = antsList[i];
                    }

                }
                return result;

            }
            catch (Exception)
            {

                //Console.WriteLine("ERROR!");
            }
            return result;
        }

        public string draw()
        {
            //MessageBox.Show("lol1");
            var getStartProcessQuery = new GetStartProcessQuery();
            var getProcessStartInfoQuery = new GetProcessStartInfoQuery();
            var registerLayoutPluginCommand = new RegisterLayoutPluginCommand(getProcessStartInfoQuery, getStartProcessQuery);

            // GraphGeneration can be injected via the IGraphGeneration interface
            var wrapper = new GraphGeneration(getStartProcessQuery,
                                              getProcessStartInfoQuery,
                                              registerLayoutPluginCommand);
            wrapper.GraphvizPath = Environment.CurrentDirectory+"\\bin\\dot.exe";

            string dotfile = System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\bin\\dot.exe";
            string outfile = System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\tmp.tmp";
            string svgfile = System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\svg.tmp";

            String dot = "";

            Ant bestAnt = getBestAnt();
            foreach (var item in connects)
            {

                string bestEdgeFormat = bestAnt != null && (bestAnt.history.Exists(c => c == item) || (bestAnt.history.Count == nodes.Count - 1 && findEdge(bestAnt.startNode, bestAnt.currentNode) == item)) ? ", color = \"#3498db\", penwidth = 3.0" : ", color = \"#bdc3c7\"";
                dot += String.Format("{0} -- {1} [label = \"{2} [f={3}, d={4}]\" {5}];", item.n1.GetHashCode(), item.n2.GetHashCode(), item.id, Math.Round(item.feromon, 2), item.distance, bestEdgeFormat);

            }

            foreach (var item in nodes)
            {

                int hs1 = item.GetHashCode();

                String nodelabel = "";

                foreach (var t in item._ant)
                {
                    nodelabel += t.id + " ";
                }

                dot += String.Format("{0} [label=\"{1}\"];", item.GetHashCode(), nodelabel);

            }



            //dot += string.Format("{0}--{1}[color=blue,penwidth=3.0];", String.Join(" -- ", bestAnt.node_history.Select(v => v.GetHashCode().ToString())), bestAnt.startNode.GetHashCode().ToString());
            //dot += string.Format("{0}[color=blue,penwidth=3.0];", String.Join(" -- ", bestAnt.node_history.Select(v => v.GetHashCode())));




            dot = "graph{" + dot + "}";
            File.WriteAllText(outfile, dot);


            Process proc = new Process();
            proc.StartInfo.FileName = dotfile;
            proc.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            proc.StartInfo.Arguments = string.Format("-Tsvg \"{0}\" -o \"{1}\"", outfile, svgfile);
            proc.Start();

            proc.WaitForExit();
            //proc.CloseMainWindow(); proc.Close();



            //Console.WriteLine( dot);
            //byte[] output = wrapper.GenerateGraph(dot, Enums.GraphReturnType.Png);


            //using (Image image = Image.FromStream(new MemoryStream(output)))
            //{
            //image.Save("C:\\Users\\Alex\\Desktop\\output.png", ImageFormat.Png);  // Or Png
            //}
            string result = File.ReadAllText(svgfile);

            File.Delete(outfile);
            File.Delete(svgfile);

            return result;//LoadImage(output);

        }

        void print(List<List<AntEnvNode>> matx)
        {
            foreach (var line in matx)
            {
                foreach (var item in line)
                {
                    //Console.Write("{0} ", item.ant.id);
                }
                Console.WriteLine();
            }
            Console.WriteLine(); Console.WriteLine();
        }

        public void updateTrails()
        {

            //Процесс испарения старого ферамона
            foreach (var item in connects)
            {
                item.feromon *= 1 - RHO;
                if (item.feromon < 0.0005f)
                {
                    item.feromon = INIT_PHEROMONE;
                }
            }

            //Процесс распределения ферамона
            foreach (var item in antsList)
            {
                var d_between_start_end1 = connects.Find(a => (a.n1 == item.startNode && a.n2 == item.currentNode) || (a.n2 == item.startNode && a.n1 == item.currentNode));
                Console.WriteLine(item.startNode.GetHashCode() + " " + item.currentNode.GetHashCode());
                var d_between_start_end = d_between_start_end1.distance;



                foreach (var step in item.history)
                {
                    step.feromon = (1 - RHO) * step.feromon + QVAL / (item.history_distance + d_between_start_end);
                }
            }

        }

        /* public void OLDupdateTrails()
         {
             foreach (var item in connects)
             {
                 item.feromon *= 1 - RHO;
                 if (item.feromon < 0.0005f)
                 {
                     item.feromon = INIT_PHEROMONE;
                 }
             }

             foreach (var item in antsList)
             {
                 var d_between_start_end1 = connects.Find(a => (a.n1 == item.startNode && a.n2 == item.currentNode) || (a.n2 == item.startNode && a.n1 == item.currentNode));
                 Console.WriteLine(item.startNode.GetHashCode()+ " "+ item.currentNode.GetHashCode());
                 var d_between_start_end = d_between_start_end1.distance;



                 //var d_between_start_end = connects.Find(a => (a.n1 == item.startNode && a.n2 == item.currentNode) || (a.n2 == item.startNode && a.n1 == item.currentNode)).distance;  
                 foreach (var step in item.history)
                 {
                     step.feromon += QVAL / (item.history_distance + d_between_start_end);
                 }
             }

             foreach (var item in connects)
             {
                 item.feromon *= RHO;
             }
         }*/

        public void simStepAnt(Ant ant)
        {
            AntEnvNode antNode = null;
            foreach (var item in nodes)
            {
                foreach (var item2 in item._ant)
                {
                    if (item2.Equals(ant))
                    {
                        antNode = item;
                        break;
                    }
                }
            }



            List<double> P = new List<double>();
            List<Connection> Pconnections = new List<Connection>();
            foreach (var item in getNeighbors(antNode))
            {
                if (!ant.history.Exists(l => l.n1.Equals(item) || l.n2.Equals(item)))
                {

                    double sum = 0;
                    foreach (var item2 in getNeighbors(antNode))
                    {
                        var con = findConnection(item, item2);
                        if (!item.Equals(item2) && !ant.history.Exists(l => l.n1.Equals(item) || l.n2.Equals(item)))
                        {
                            sum += Math.Pow(con.feromon, ALPHA) * Math.Pow((1.0 / con.distance), BETA);
                        }
                    }

                    var c = findConnection(antNode, item);
                    double chichlitel = Math.Pow(c.feromon, ALPHA) * Math.Pow((1.0 / c.distance), BETA);

                    P.Add(chichlitel / sum);
                    Pconnections.Add(c);

                }
            }


            double r = rand.NextDouble(0, P.Sum());
            double tmps = 0;
            for (int i = 0; i < P.Count; i++)
            {
                tmps += P[i];
                if (r <= tmps)
                {
                    ant.history.Add(Pconnections[i]);

                    if (Pconnections[i].n1 == antNode)
                    {
                        Pconnections[i].n1._ant.Remove(ant);
                        Pconnections[i].n2._ant.Add(ant);
                        ant.node_history.Add(Pconnections[i].n2);
                        ant.currentNode = Pconnections[i].n2;
                    }
                    else
                    {
                        Pconnections[i].n2._ant.Remove(ant);
                        Pconnections[i].n1._ant.Add(ant);
                        ant.node_history.Add(Pconnections[i].n1);
                        ant.currentNode = Pconnections[i].n1;
                    }



                    break;
                }

            }

            Console.Write(ant.history.Count.ToString() + " ");


        }
    }
}