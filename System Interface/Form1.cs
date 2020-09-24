using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO.Ports;  // serial port handling
using MySql.Data.MySqlClient;

namespace Arduino_RFID_MySQL
{
    public partial class Form1 : Form
    {
        private SerialPort SerPort;     // serial port
        private string ReceivedData;    // received data from serial port
        private static MySqlConnection connection;
        private static MySqlCommand command = null;
        private System.Windows.Forms.Timer tmr;

        public Form1()
        {
            InitializeComponent();
            FetchAvailablePorts();

        }

        public void FetchAvailablePorts()
        {
            String[] ports = SerialPort.GetPortNames(); // get the available COM port

            ToolStripMenuItem[] items = new ToolStripMenuItem[ports.Length];

            for (int i = 0; i < items.Length; i++)
            {
                items[i] = new ToolStripMenuItem();
                items[i].Name = "dynamicItem" + ports[i].ToString(); //You have to set your string value here
                items[i].Tag = "specialDataHere";
                items[i].Text = ports[i].ToString();
                items[i].Click += new EventHandler(MenuItemClickHandler);
            }

            portToolStripMenuItem.DropDownItems.AddRange(items);
        }

        private void PortSelection(int i)
        {

        }

        private void MenuItemClickHandler(object sender, EventArgs e)
        {
            ToolStripMenuItem clickedItem = (ToolStripMenuItem)sender;
            portToolStripMenuItem.Text += " (" + clickedItem.ToString() + ")";
            //((ToolStripMenuItem)portToolStripMenuItem.DropDownItems[i]).Checked = true;
            //portToolStripMenuItem. += clickedItem;
            label5.Text = clickedItem.ToString();
            //i++;
            // Take some action based on the data in clickedItem
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            connect();

            Connect_Arduino();
        }

        public void connect()
        {
            string connetionString = null;
            //MessageBox.Show("Connecting database");
            connetionString = "server=paneldatabase.humbleservers.com;database=s900_likweitan;uid=u900_6AhR1HXWRP;pwd=mDXozLEkDXvOKhtg78Q5z8wC;";
            connection = new MySqlConnection(connetionString);
            try
            {
                connection.Open();
                //MessageBox.Show("Connection established successfully.");
                toolStripStatusLabel1.Text = "Connected on " + connection.DataSource;
                //this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error...");
                toolStripStatusLabel1.Text = "Failed to connect on " + connection.DataSource;
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                SerPort.Close();    // close the port
            }
            catch (Exception ex)
            {

            }
        }

        private void Connect_Arduino()
        {
            SerPort = new SerialPort();

            SerPort.BaudRate = 9600;
            SerPort.PortName = "COM6";
            SerPort.Parity = Parity.None;
            SerPort.DataBits = 8;
            SerPort.StopBits = StopBits.One;
           // SerPort.ReadBufferSize = 200000000;
            SerPort.DataReceived += Serport_DataReceived;

            try
            {
                SerPort.Open();
                MessageBox.Show("Connected to the system.");
            }
            catch (Exception ex)
            {
                if(SerPort.IsOpen)
                {

                }
                SerPort.Close();
                MessageBox.Show(ex.Message, "Error connecting to the system.");
            }
        }

        private void Serport_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            ReceivedData = SerPort.ReadLine();  // read the line from the serial port
            this.Invoke(new Action(ProcessingData));
        }

        private void ProcessingData()
        {
            string student_tag = ReceivedData.ToString() + Environment.NewLine;
            student_tag = student_tag.Trim();

                textBox3.Text = student_tag;

            command = connection.CreateCommand();
            command.CommandType = CommandType.Text;
            command.CommandText = "SELECT * FROM student where student_tag = '" + student_tag + "' limit 1";
            connection.Close();
            connection.Open();
            MySqlDataReader reader = command.ExecuteReader();
            if (reader.Read())
            {
                //statement = myReader.GetString(0);
                textBox1.Text = reader["student_name"].ToString();
                textBox2.Text = reader["student_id"].ToString();

                if(reader["student_block"].ToString() == "F")
                {
                    SerPort.WriteLine("1");
                    label5.Text = "You are authorized.";
                    
                }
                else
                {
                    label5.Text = "You are not authorized.";
                }
            }
            else
            {
                textBox1.Text = "";
                textBox2.Text = "";
                label5.Text = "You are not authorized.";
            }

            tmr = new System.Windows.Forms.Timer();
            tmr.Tick += delegate {
                textBox1.Text = "";
                textBox2.Text = "";
                textBox3.Text = "";
                label5.Text = "";
            };
            tmr.Interval = (int)TimeSpan.FromMinutes(0.1).TotalMilliseconds;
            tmr.Start();
            
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void aboutToolStripMenuItem1_Click(object sender, EventArgs e)
        {

        }

        private void aboutToolStripMenuItem2_Click(object sender, EventArgs e)
        {
            AboutBox1 aboutbox = new AboutBox1();
            aboutbox.Show();
        }

        private void checkForUpdatesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string message = "INTI Hostel Curfew System is up to date.";
            string title = "Check for Updates";
            MessageBox.Show(message, title);
        }
    }
}
