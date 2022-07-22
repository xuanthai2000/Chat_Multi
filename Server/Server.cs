using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.SqlClient;
using System.Text.RegularExpressions;

namespace Server
{
    public partial class Server : Form
    {

        TcpListener tcpListener;
        //NetworkStream stream = null;
        //List<string> roomid = new List<string>();
        //List<TcpClient> clientList = new List<TcpClient>();
        //Dictionary<string, List<TcpClient>> Room = new Dictionary<string, List<TcpClient>>();
        Dictionary<string, TcpClient> useractive = new Dictionary<string, TcpClient>();
        List<string> userroom = new List<string>();
        List<string> idroom = new List<string>();
        string[] cp_userroom;
        string[] cp_idroom;
        SqlConnection con = new SqlConnection(@"Data Source=DESKTOP-31II010;Initial Catalog=QuanLyTaiKhoan;Integrated Security=True");

        public Server()
        {
            InitializeComponent();
        }

        //int flag = 0;

        private void Setup()
        {
            CheckForIllegalCrossThreadCalls = false;
            tcpListener = new TcpListener(IPAddress.Parse("127.0.0.1"), 8080);
            tcpListener.Start();
            Thread ServerThread = new Thread(Connect);
            ServerThread.IsBackground = true;
            ServerThread.Start();
        }

        private void Connect()
        {
            while(true)
            {
                TcpClient client;
                client = tcpListener.AcceptTcpClient();
                var t = new Thread(new ParameterizedThreadStart(Receive));
                t.IsBackground = true;
                t.Start(client);
            }
        }

        private void SendData(string message, object clientObj)
        {
            //message = Rot13(message);
            TcpClient client = clientObj as TcpClient;
            NetworkStream stream = client.GetStream();
            byte[] noti = Encoding.UTF8.GetBytes(message);
            stream.Write(noti, 0, noti.Length);
        }

        private string ReceiveData(NetworkStream str, TcpClient client)
        {
            byte[] buffer = new byte[client.ReceiveBufferSize];
            str.Read(buffer, 0, buffer.Length);
            string mess = Encoding.UTF8.GetString(buffer);
            //mess = Rot13(mess);
            return mess;
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
        private void Receive(object Client)
        {
            TcpClient client = (TcpClient)Client;
            NetworkStream stream = client.GetStream();
            SendData("Connected sucess", client);
            while (true)
            {
                try
                {
                    string message = ReceiveData(stream, client);
                    //message = Rot13(message);
                    string[] check = message.Split(' ');
                    if (message.StartsWith("Login"))
                    {
                        con.Open();
                        label1.Text = check[1];
                        label2.Text = check[2];
                        string query = "SELECT COUNT(*) FROM TaiKhoan WHERE username = '" + label1.Text.Trim() + "' AND password = '" + label2.Text.Trim() + "'";
                        SqlDataAdapter da = new SqlDataAdapter(query, con);
                        DataTable dt = new DataTable();
                        da.Fill(dt);
                        if (dt.Rows[0][0].ToString() == "1")
                        {
                            SendData("Login Successfully", client);
                        }
                        else
                        {
                            SendData("Login Fail", client);
                        }
                        con.Close();
                    }
                    else if (message.StartsWith("Register"))
                    {
                        con.Open();
                        label1.Text = check[1];
                        label2.Text = check[2];
                        string query = "SELECT COUNT(*) FROM TaiKhoan WHERE username = '" + label1.Text.Trim() + "'";
                        SqlDataAdapter da = new SqlDataAdapter(query, con);
                        DataTable dt = new DataTable();
                        da.Fill(dt);
                        if (dt.Rows[0][0].ToString() == "0")
                        {
                            string sql = "INSERT INTO TaiKhoan(username, password) VALUES (N'" + label1.Text.Trim() + "', N'" + label2.Text.Trim() + "')";
                            SqlCommand Sqlcmd = new SqlCommand(sql, con);
                            Sqlcmd.ExecuteNonQuery();
                            SendData("Register Successfully", client);
                        }
                        else
                        {
                            SendData("Register Fail", client);
                        }
                        con.Close();
                    }
                    else if (message.StartsWith("Create"))
                    {
                        Int32 roomid;
                        con.Open();
                        label1.Text = check[1];
                        string query = "SELECT COUNT(*) FROM Room WHERE RoomName = '" + label1.Text + "'";
                        SqlDataAdapter da = new SqlDataAdapter(query, con);
                        DataTable dt = new DataTable();
                        da.Fill(dt);
                        if (dt.Rows[0][0].ToString() == "0")
                        {
                            Random rd = new Random();
                            roomid = rd.Next(10000000, 99999999);
                            string sql = "INSERT INTO Room(RoomName, RoomID) VALUES (N'" + label1.Text + "', N'" + roomid.ToString() + "')";
                            SqlCommand Sqlcmd = new SqlCommand(sql, con);
                            Sqlcmd.ExecuteNonQuery();
                            SendData("Create Successfully" + " " + roomid.ToString(), client);
                        }
                        else
                        {
                            SendData("Create Fail", client);
                        }
                        con.Close();
                    }
                    else if (message.StartsWith("Join"))
                    {
                        con.Open();
                        label1.Text = check[1];//id
                        label2.Text = check[2];//user
                        string query = "SELECT COUNT(*) FROM Room WHERE RoomID = '" + label1.Text + "'";//kiểm tra bảng room có id chưa
                        SqlDataAdapter da = new SqlDataAdapter(query, con);
                        DataTable dt = new DataTable();
                        da.Fill(dt);
                        if (dt.Rows[0][0].ToString() == "1")
                        {
                            SendData("Join Successfully", client);
                            useractive.Add(label2.Text + label1.Text, client);
                            userroom.Add(label2.Text);
                            idroom.Add(label1.Text);
                            cp_userroom = userroom.ToArray();
                            cp_idroom = idroom.ToArray();
                            
                            foreach (var item in useractive)
                            {
                                if (item.Key.Contains(cp_idroom[idroom.Count - 1]))
                                {
                                    SendData("Welcome " + cp_userroom[idroom.Count - 1], item.Value);
                                }
                            }

                        }
                        else
                        {
                            SendData("Join Fail", client);
                        }
                        con.Close();
                    }
                    else if (message.StartsWith("Out"))
                    {
                        //con.Open();
                        //SendData("Out Success", client);
                        label1.Text = check[1];//id
                        label2.Text = check[2];//user
                        useractive.Remove(label2.Text + label1.Text);
                        //userroom.Add(label2.Text);
                        //idroom.Add(label1.Text);
                        //cp_userroom = userroom.ToArray();
                        //cp_idroom = idroom.ToArray();
                        foreach (var item in useractive)
                        {
                            if (item.Key.Contains(label1.Text))
                            {
                                SendData("Out " + label2.Text + " " + "left" , item.Value);
                                item.Value.Close();
                            }
                        }
                        //con.Close();
                    }


                    else if (message.StartsWith("Send"))
                    {
                        label1.Text = check[1];//id
                        label2.Text = check[2];//user
                        List<string> data = new List<string>();
                        for(int i =3; i < check.Length; i++)
                        {
                            data.Add(check[i]);
                            data.Add(" ");
                        }
                        string[] cp_data = data.ToArray();
                        string t = "";
                        for( int j = 0; j < cp_data.Length; j++)
                        {
                            t += cp_data[j];
                        }    
                        foreach (var item in useractive)
                        {
                            if (item.Key.Contains(label1.Text))
                            {
                                SendData("Send" + label2.Text + ": "+ t.Trim(), item.Value);
                                //item.Value.Close();
                            }
                        }
                    }
                }
                catch
                {
                    //clientList.Remove(client);
                    client.Close();
                    stream.Close();
                }
            }
        }
        private void btnListen_Click(object sender, EventArgs e)
        {
            btnListen.Enabled = false;
            Setup();
        }
    }
}
