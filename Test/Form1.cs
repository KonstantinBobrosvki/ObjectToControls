using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ObjectToControls;

namespace TypeToForm
{
    public partial class Form1 : Form
    {
        Object b = new HardBass();
        public Form1()
        {
            InitializeComponent();

            var result = WinFormsHelper.ObjectToControls(b, new Size(100, 40), 20);
            result.Location = new Point(300, 100);
            result.Size = new Size(300, 600);
            this.Controls.Add(result);
            
           // Timer t = new Timer();
            //t.Tick += (s, a) => ((HardBass)b).Name = Guid.NewGuid().ToString();
           // t.Interval = 1500;
            //t.Start();
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            ((HardBass)b).Name = textBox1.Text;

        }

        private void button1_Click(object sender, EventArgs e)
        {
            MessageBox.Show(b.ToString());
        }
    }
    public class HardBass
    {
        public int Bass { get; set; }
        public string Name { get; set; }
        public int Volume { get; private set; }
        public bool On { get; set; }
        public double Herz { get; set; }
        public List<string> SomeList { get; set; }
        public HardBass()
        {
            Bass = new Random().Next(10, 100);
            Volume = new Random().Next(0, 100);
            Name = Guid.NewGuid().ToString();
            Herz = new Random().NextDouble();
            On = new Random().Next(2) == 0;
            SomeList = new List<string>();
            var ttt = 30;
            for (int i = 0; i < ttt; i++)
            {
                SomeList.Add(Guid.NewGuid().ToString());
            }

            CoolName = new MyClass();

        }

        public MyClass CoolName { get; set; } 
        public override string ToString()
        {
            return $"Bass:{Bass} \n\r Volume:{Volume} \n\r Name:{Name} \n\r On:{On} \n\r Herz:{Herz} \n\r";
        }
    }

    public class MyClass
    {
        public string myField = string.Empty;

        public MyClass()
        {
            MyAutoImplementedProperty = new Random().Next();
            MyProperty = Guid.NewGuid().ToString();
            SomeSHitField = 54;
        }

        public void MyMethod(int parameter1, string parameter2)
        {
            Console.WriteLine("First Parameter {0}, second parameter {1}",
                                                        parameter1, parameter2);
        }

        public int MyAutoImplementedProperty { get; set; }

        private string myPropertyVar;

        public string MyProperty
        {
            get { return myPropertyVar; }
            set { myPropertyVar = value; }
        }

        private int SomeSHitField;
        private int SomeSHitProper { get; set; }

    }
}
