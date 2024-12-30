using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Windows.Forms.VisualStyles;
using System.Threading;


namespace ClientSocket
{
    public partial class Form1 : Form
    {
        TcpClient clientSocket;
        NetworkStream stream = default(NetworkStream);
        string message = string.Empty;
        private int PORT = 5000;
        private string USER_NAME = string.Empty;
        private static string CONNECT_STATUS = "DISCONNECT";



        public Form1()
        {
            InitializeComponent();
            InitForm();
            ServerIP.Text = "서버의 IP를 입력";
            ServerIP.ForeColor = Color.Gray;
            ClientPort.Text = PORT.ToString();
            ClientID.Text = GetPID();
            ClientStatus.Text = "대기중...";
        }

        private string GetPID()
        {
            string pid = string.Empty;
            Process currentProcess = Process.GetCurrentProcess();
            pid = currentProcess.Id.ToString();

            return pid;
        }

        private void InitForm()
        {
            ServerIP.Text = "";
            ClientPort.Text = "";
            ClientStatus.Text = "";
            ClientID.Text = "";
            richTextBox1.Text = "";
            textBox1.Text = "";
        }

        private void button1_Click(object sender, EventArgs e)
        {
            clientSocket = new TcpClient(); // 클라이언트 전송 객체 생성
            int conn = 0;
            try
            {
                if (CONNECT_STATUS.Equals("DISCONNECT"))
                {
                    conn = Connect();
                    if(conn > 0)
                    {
                        USER_NAME = ClientID.Text.Trim();
                        ClientStatus.Text = "CONNECT";
                        CONNECT_STATUS = "CONNECT";
                    }
                }
                else if (CONNECT_STATUS.Equals("CONNECT"))
                {
                    return;
                }
                else
                {
                    return;
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show("서버가 실행중이 아닙니다.", "연결 실패!");
            }

            if(stream == null)
            {
                MessageBox.Show("서버 IP와 포트 재설정 바랍니다.");
                return;
            }
            else
            {
                message = "채팅 서버에 연결 되었습니다.";
                DisplayText(message);

                byte[] buffer = Encoding.Unicode.GetBytes(USER_NAME + "$");

                stream.Write(buffer, 0, buffer.Length);
                stream.Flush();

                Thread t_handler = new Thread(GetMessage);
                t_handler.IsBackground = true;
                t_handler.Start();
            }            
        }

        private void GetMessage()
        {
            while (true)
            {              
                if(clientSocket == null)
                {
                    break;
                }
                stream = clientSocket.GetStream();
                int BUFFERSIZE = clientSocket.ReceiveBufferSize;
                byte[] buffer = new byte[BUFFERSIZE];
                int bytes = stream.Read(buffer, 0, buffer.Length);
                string message = Encoding.Unicode.GetString(buffer, 0, bytes);

                DisplayText(message);            
            }
        }

        private void DisplayText(string message)
        {
            richTextBox1.Invoke((MethodInvoker)delegate
            {
                richTextBox1.AppendText(message + "\r\n");
            });
            richTextBox1.Invoke((MethodInvoker)delegate
            {
                richTextBox1.ScrollToCaret();
            });
        }

        private int Connect()
        {
            int conn = 0;
            try
            {
                if(clientSocket == null)
                {
                    Console.WriteLine("clientSocket이 null");
                }
                else
                {
                    clientSocket.Connect(ServerIP.Text.ToString(), int.Parse(ClientPort.Text));
                    stream = clientSocket.GetStream();
                    conn = 1;

                }         
            }catch(SocketException se)
            {
                Console.WriteLine(se.Message);
              
            }catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return conn;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (CONNECT_STATUS.Equals("DISCONNECT"))
            {
                MessageBox.Show("연결된 상태가 아닙니다.");
                return;
            }
            else if (CONNECT_STATUS.Equals("CONNECT"))
            {
                DisConnect();
                ClientStatus.Text = "대기중...";
                CONNECT_STATUS = "DISCONNECT";
                return;
            }
            else
            { }
        }

        private void DisConnect()
        {
            clientSocket = null;
            byte[] buffer = Encoding.Unicode.GetBytes("LeaveChat" + "$");
            stream.Write(buffer, 0, buffer.Length);
            stream.Flush();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            textBox1.Focus();
            byte[] buffer = Encoding.Unicode.GetBytes(textBox1.Text + "$");
            stream.Write(buffer, 0, buffer.Length);
            stream.Flush();

            textBox1.Text = "";
        }

        private void textbox1_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                button3_Click(this, e);
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            DisConnect();

            Application.ExitThread();
            Environment.Exit(0);
        }

        private void ServerIP_MouseDown(object sender, MouseEventArgs e)
        {
            ServerIP.ForeColor = Color.Black;
        }
    }
}
