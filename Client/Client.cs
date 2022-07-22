using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Client
{
    public partial class Client : Form
    {
        public NetworkStream ServerStream = null;
        public TcpClient tcpClient;
        public Client()
        {
            InitializeComponent();
        }

        private void Setup()
        {
            try
            {
                CheckForIllegalCrossThreadCalls = false;
                tcpClient = new TcpClient();
                tcpClient.Connect(IPAddress.Parse("127.0.0.1"), 8080);
                ServerStream = tcpClient.GetStream();
                Thread listen = new Thread(Listen);
                //Thread listenroom = new Thread(ListenRoom);
                //listenroom.IsBackground = true;
                listen.IsBackground = true;
                listen.Start();
                //listenroom.Start();
            }
            catch
            {
                MessageBox.Show("Can't connect to server");
                this.Close();
            }
        }

        private void SendData(string message)
        {
            try
            {
                //message = Rot13(message);
                //ServerStream = tcpClient.GetStream();
                byte[] OutStream = Encoding.UTF8.GetBytes(message);
                ServerStream.Write(OutStream, 0, OutStream.Length);

            }
            catch
            {
                MessageBox.Show("Error, please try again!");
            }
        }

        //public static string Rot13(string input)
        //{
        //    StringBuilder result = new StringBuilder();
        //    Regex regex = new Regex("[A-Za-z]");

        //    foreach (char c in input)
        //    {
        //        if (regex.IsMatch(c.ToString()))
        //        {
        //            int charCode = ((c & 223) - 52) % 26 + (c & 32) + 65;
        //            result.Append((char)charCode);
        //        }
        //        else
        //        {
        //            result.Append(c);
        //        }
        //    }

        //    return result.ToString();
        //}

       
        private void ListenRoom()
        {
            try
            {
                while (true)
                {
                    var bufferSize = tcpClient.ReceiveBufferSize;
                    byte[] instream = new byte[bufferSize];
                    ServerStream.Read(instream, 0, bufferSize);
                    var message = Encoding.UTF8.GetString(instream);
                    string[] check = message.Split(' ');
                    if (string.Compare(check[0], "Welcome") == 0)
                    {
                        //richTextBox1.Text = richTextBox1.Text + check[0] + " " + check[1];
                        richTextBox1.Text += message;
                        richTextBox1.Text += "\n";
                        ListViewItem it = new ListViewItem(check[1]);
                        listView1.Items.Add(it);
                    }
                   
                    else if (message.StartsWith("Out"))
                    {
                        richTextBox1.Text = richTextBox1.Text + check[1] + " " + check[2];
                        richTextBox1.Text += "\n";
                    }
                    else if (message.StartsWith("Join"))
                    {
                        if (string.Compare(check[1], "Successfully") == 0)
                        {
                            MessageBox.Show("Join Success");
                            tabControl1.SelectedIndex = 3;
                            label10.Text = txtIDRoom.Text;
                            label14.Text = txtUsername.Text;

                        }
                        else
                        {
                            MessageBox.Show("ID không tồn tại");
                        }
                    }
                    else if(message.StartsWith("Send"))
                    {
                        richTextBox1.Text = richTextBox1.Text + message.Substring(4);
                        richTextBox1.Text += "\n";
                    }    
                }
            }
            catch
            {

            }
        }
        //void AddMessage(string s)
        //{
        //    listView1.Items.Add(new ListViewItem() { Text = s });
        //}

        private void Listen()
        {
            try
            {
                var bufferSize = tcpClient.ReceiveBufferSize;
                byte[] instream = new byte[bufferSize];
                ServerStream.Read(instream, 0, bufferSize);
                var message = Encoding.UTF8.GetString(instream);
                //listView1.Items.Add(new ListViewItem() { Text = message });
                //message = Rot13(message);
                string[] check = message.Split(' ');
                if (message.StartsWith("Login"))
                {
                    if (string.Compare(check[1], "Successfully") == 0)
                    {
                        MessageBox.Show("Successfully", "Congrates", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        tabControl1.SelectedIndex = 2;
                    }
                    else
                    {
                        MessageBox.Show("Please check your username or password");
                    }
                }
                else if (message.StartsWith("Register"))
                {
                    if (string.Compare(check[1], "Successfully") == 0)
                    {
                        MessageBox.Show("Register success", "Congrates", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        tabControl1.SelectedIndex = 0;
                        txtUsername.Text = txtRegisUsername.Text;
                        txtPassword.Text = txtRegisPassword.Text;
                    }
                    else
                    {
                        MessageBox.Show("Username exists");
                    }
                }
                else if (message.StartsWith("Create"))
                {
                    if (string.Compare(check[1], "Successfully") == 0)
                    {
                        MessageBox.Show("ID: " + check[2], "Thanh cong", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        txtIDRoom.Text = check[2];
                    }
                    else
                    {
                        MessageBox.Show("RoomName existed");
                    }
                }
                else if (message.StartsWith("Join"))
                {
                    if (string.Compare(check[1], "Successfully") == 0)
                    {
                        MessageBox.Show("Join Success");
                        tabControl1.SelectedIndex = 3;
                        label10.Text = txtIDRoom.Text;

                    }
                    else
                    {
                        MessageBox.Show("ID không tồn tại");
                    }
                }

                //else if(message.StartsWith("Welcome"))
                //{
                //    //richTextBox1.Text = richTextBox1.Text + check[0] + " " + check[1];
                //    //richTextBox1.Text += "\n";
                //    ListViewItem it = new ListViewItem(message);
                //    listView1.Items.Add(it);

                //}    

            }
            catch
            {
                tcpClient.Close();
            }

        }

        private void Client_Load(object sender, EventArgs e)
        {
            Thread stThread = new Thread(Setup);
            stThread.IsBackground = true;
            stThread.Start();
        }

        private void btnLogin_Click_1(object sender, EventArgs e)
        {
            if (txtUsername.Text != string.Empty && txtPassword.Text != string.Empty)
            {
                SendData("Login" + " " + txtUsername.Text + " " + txtPassword.Text);
                Listen();
            }
        }

        private void linkLbRegister_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            tabControl1.SelectedIndex = 1;
        }

        private void btnRegister_Click(object sender, EventArgs e)
        {
            if (txtRegisUsername.Text == string.Empty || txtRegisPassword.Text == string.Empty || txtConfirm.Text == string.Empty)
            {
                MessageBox.Show("Please fill in");
            }
            else if (txtRegisPassword.Text != txtConfirm.Text)
            {
                MessageBox.Show("Please check your password or confirm");
            }
            else if (txtRegisUsername.Text != string.Empty && txtRegisPassword.Text != string.Empty && txtConfirm.Text != string.Empty)
            {
                SendData("Register" + " " + txtRegisUsername.Text + " " + txtRegisPassword.Text);
                Listen();
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if(txtRoomName.Text != string.Empty)
            {
                SendData("Create" + " " + txtRoomName.Text);
                Listen();
            }    
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox1.Checked == true)
            {
                txtPassword.UseSystemPasswordChar = false;
            }
            else
            {
                txtPassword.UseSystemPasswordChar = true;
            }    
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (txtIDRoom.Text != string.Empty)
            {
                SendData("Join" + " " + txtIDRoom.Text + " " + txtUsername.Text);
                Listen();
                //Listen();
                Thread listenroom = new Thread(ListenRoom);
                listenroom.IsBackground = true;
                listenroom.Start();
                //ListenRoom();
            }    
        }

        private void btnSend_Click(object sender, EventArgs e)
        {
            if(txtSend.Text != string.Empty)
            {
                SendData("Send" + " " + txtIDRoom.Text + " " + txtUsername.Text + " " + txtSend.Text);
                Thread listenroom = new Thread(ListenRoom);
                listenroom.IsBackground = true;
                listenroom.Start();
                txtSend.Clear();
            }    
        }

        private void btnOut_Click(object sender, EventArgs e)
        {
            SendData("Out " + txtIDRoom.Text + " " + txtUsername.Text);
            //Listen();
            Thread listenroom = new Thread(ListenRoom);
            listenroom.IsBackground = true;
            listenroom.Start();
            this.Close();
            
        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox2.Checked == true)
            {
                txtRegisPassword.UseSystemPasswordChar = false;
            }
            else
            {
                txtRegisPassword.UseSystemPasswordChar = true;
            }
        }

        private void checkBox3_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox3.Checked == true)
            {
                txtConfirm.UseSystemPasswordChar = false;
            }
            else
            {
                txtConfirm.UseSystemPasswordChar = true;
            }
        }
    }
}
