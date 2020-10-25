using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WindowsFormsApp1.Entities;

namespace WindowsFormsApp1
{
    public partial class Form1 : Form
    {
        Random rng = new Random(1234);

        List<Person> Population = new List<Person>();
        List<BirthProbability> BirthProbabilities = new List<BirthProbability>();
        List<DeathProbability> DeathProbabilities = new List<DeathProbability>();

        List<Person> male = new List<Person>();
        List<Person> female = new List<Person>();


        public Form1()
        {
            InitializeComponent();

            BirthProbabilities = GetBirthProbabilities(@"C:\Temp\születés.csv");
            DeathProbabilities = GetDeathProbabilities(@"C:\Temp\halál.csv");
        }

        private void Simulation()
        {
            Population = GetPopulation(textBox1.Text);

            for (int year = 2005; year <= numericUpDown1.Value; year++)
            {
                for (int j = 0; j < Population.Count; j++)
                {
                    SimStep(year, Population[j]);
                }

                int nbrOfMales = (from x in Population
                                  where x.Gender == Gender.Male && x.IsAlive
                                  select x).Count();
                int nbrOfFemales = (from x in Population
                                    where x.Gender == Gender.Female && x.IsAlive
                                    select x).Count();
                Console.WriteLine(
                    string.Format("Év: {0}  Fiúk: {1}  Lányok: {2}", year, nbrOfMales, nbrOfFemales));
            }
        }

        public List<Person> GetPopulation(string csvpath)
        {
            List<Person> population = new List<Person>();

            using (StreamReader sr = new StreamReader(csvpath, Encoding.Default))
            {
                while (!sr.EndOfStream)
                {
                    var line = sr.ReadLine().Split(';');
                    population.Add(new Person()
                    {
                        BirthYear = int.Parse(line[0]),
                        Gender = (Gender)Enum.Parse(typeof(Gender), line[1]),
                        NbrOfChildren = int.Parse(line[2])
                    });
                }
            }

            return population;
        }

        public List<BirthProbability> GetBirthProbabilities(string csvpath)
        {
            List<BirthProbability> birth = new List<BirthProbability>();

            using (StreamReader sr = new StreamReader(csvpath, Encoding.Default))
            {
                while (!sr.EndOfStream)
                {
                    var line = sr.ReadLine().Split(';');
                    birth.Add(new BirthProbability()
                    {
                        Age = int.Parse(line[0]),
                        NbrOfChildren = int.Parse(line[1]),
                        BirthP = double.Parse(line[2])
                    });
                }
            }

            return birth;
        }

        public List<DeathProbability> GetDeathProbabilities(string csvpath)
        {
            List<DeathProbability> death = new List<DeathProbability>();

            using (StreamReader sr = new StreamReader(csvpath, Encoding.Default))
            {
                while (!sr.EndOfStream)
                {
                    var line = sr.ReadLine().Split(';');
                    death.Add(new DeathProbability()
                    {
                        Gender = (Gender)Enum.Parse(typeof(Gender), line[0]),
                        Age = int.Parse(line[1]),
                        DeathP = double.Parse(line[2])
                    });
                }
            }

            return death;
        }

        private void SimStep(int year, Person person)
        {
            if (person.IsAlive && person.Gender == Gender.Female)
                female.Add(person);

            if (person.IsAlive && person.Gender == Gender.Male)
                male.Add(person);

            if (!person.IsAlive) return;

            byte age = (byte)(year - person.BirthYear);

            double pDeath = (from x in DeathProbabilities
                             where x.Gender == person.Gender && x.Age == age
                             select x.DeathP).FirstOrDefault();

            double a = rng.NextDouble();
            if (a <= pDeath)
            { 
                person.IsAlive = false; }

            if (person.IsAlive && person.Gender == Gender.Female)
            {
                double pBirth = (from x in BirthProbabilities
                                 where x.Age == age
                                 select x.BirthP).FirstOrDefault();

                if (rng.NextDouble() <= pBirth)
                {
                    Person newborn = new Person();
                    newborn.BirthYear = year;
                    newborn.NbrOfChildren = 0;
                    newborn.Gender = (Gender)(rng.Next(1, 3));
                    Population.Add(newborn);

                }
            }

            
        }

        private void StartButton_Click(object sender, EventArgs e)
        {
            male.Clear();
            female.Clear();
            richTextBox1.Clear();


            Simulation();

            DisplayResults();

        }

        private void DisplayResults()
        {
            for (int i = 0; i < (numericUpDown1.Value - 2005); i++)
            {

                richTextBox1.Text += "Szimulációs év: " + (2005 + i) + "\n" + "\t" + "Fiúk: " + male.Count() + "\n" + "\t" + "Lányok: " + female.Count() + "\n";

            }
        }

        private void BrowseButton_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                textBox1.Text = ofd.FileName;
            }
        }
    }
}
