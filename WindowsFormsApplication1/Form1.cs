using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApplication1
{
    public partial class Form1 : Form
    {
        public string serverAddress;
        private int port;
        private Socket socket;
        private string fileName;
        public Form1()
        {
            InitializeComponent();
            ipTextBox.Text = "127.0.0.1";
            portBox.Text = "5656";
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            pathBox.ReadOnly = true;
        }

        private void label3_Click(object sender, EventArgs e)
        {

        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "All Files (*.*)|*.*";
            ofd.FilterIndex = 1;

            if(ofd.ShowDialog() == DialogResult.OK)
            {
                fileName = ofd.FileName;
                pathBox.Text = fileName;

            }
        }

        private void connectButton_Click(object sender, EventArgs e)
        {
            bool isOk = true;
            string message = "";

            if(connectButton.Text.Equals("CONNECT"))
            {
                if (socket != null)
                {
                    if (socket.Connected)
                    {
                        socket.Close();
                        socket = null;

                    }
                }
                else
                {
                    serverAddress = ipTextBox.Text;
                    if (String.IsNullOrEmpty(serverAddress))
                    {
                        isOk = false;
                        MessageBox.Show("Ip Addres!", "Bad Ip Addres!", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                    }
                    if (String.IsNullOrEmpty(portBox.Text))
                    {
                        isOk = false;
                        MessageBox.Show("Wrong Port!", "Bad PORT!", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                    }
                    else
                    {
                        port = int.Parse(portBox.Text.ToString());
                    }

                    if (isOk)
                    {
                        try
                        {
                            IPAddress[] ipAddress = Dns.GetHostAddresses(serverAddress);
                            IPEndPoint ipEnd = new IPEndPoint(ipAddress[0], port);
                            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.IP);
                            socket.Connect(ipEnd);


                        }
                        catch (Exception)
                        {
                            if (e.GetType().FullName.Equals("System.Net.Sockets.SocketException"))
                            {
                                socket = null;
                            }
                            message += " Connecting Error!";
                            statusLabel.Text = message;

                        }
                        if (socket != null)
                        {

                            if (socket.Connected)
                            {
                                message += " Connected to:" + serverAddress;
                                statusLabel.Text = message;
                                connectButton.Text = "Disconnect";

                            }
                        }
                    }
                }
            }else if(connectButton.Text.Equals("Disconnect"))
            {
                socket.Close();
                connectButton.Text = "CONNECT";
                statusLabel.Text = "Disconected!";
                
            }
           
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string message = "";
            try
            {
                if (socket.Connected)
                {
                    if (String.IsNullOrEmpty(pathBox.Text))
                    {
                        message += " File path is empty";
                    }
                    else
                    {
                        string fileName = pathBox.Text.Substring(pathBox.Text.LastIndexOf("\\") + 1);
                        string filePath = pathBox.Text.Substring(0, pathBox.Text.LastIndexOf("\\") + 1);

                        byte[] fileNameByte = Encoding.ASCII.GetBytes(fileName);

                        byte[] fileData = File.ReadAllBytes(filePath + fileName);
                        byte[] clientData = new byte[4 + fileNameByte.Length + fileData.Length];
                        byte[] fileNameLen = BitConverter.GetBytes(fileNameByte.Length);

                        fileNameLen.CopyTo(clientData, 0);
                        fileNameByte.CopyTo(clientData, 4);
                        fileData.CopyTo(clientData, 4 + fileNameByte.Length);

                        socket.Send(clientData);

                        message += " File has been sent ";
                    }
                }
                else
                {
                    message += " No connection with server";
                }
            }
            catch (Exception ex)
            {
                if (ex.GetType().FullName.Equals("System.Net.Sockets.SocketException"))
                {
                    socket.Close();
                    socket = null;
                    connectButton.Text = "Connect";
                 
                }
                message += ex.Message;
            }

    }
    }
}
