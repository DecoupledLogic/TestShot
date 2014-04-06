using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace SunGard.PNE.Test.TestShot
{
	
	//DO NOT USE THIS. I was just playing around with sniffing net traffic to discover if email is vulnerable to exploit. I didn't event build or try this so I don't know what it does.
	public class Sniff
	{
		public string ServerIp { get; set; }
		public int Port { get; set; }
		public string Username { get; set; }
		public string Password { get; set; }
		public int MessageCount { get; set; }

		public void Get()
		{
			TcpClient tcpClient;
			NetworkStream networkStream;
			string message;

			using (tcpClient = new TcpClient(this.ServerIp, this.Port))
			{
				networkStream = tcpClient.GetStream();

				StreamWriter sw = new StreamWriter(networkStream);
				StreamReader sr = new StreamReader(networkStream);

				if (!CheckError(sr.ReadLine()))
				{
					sw.WriteLine(string.Format("USER {0}", this.Username));
					sw.Flush();
				}

				if (!CheckError(sr.ReadLine()))
				{
					sw.WriteLine(String.Format("PASS {0}",this.Password));
					sw.Flush();
				}

				if (!CheckError(sr.ReadLine()))
				{
					sw.WriteLine("STAT ");
					sw.Flush();
				}

				message = sr.ReadLine();

				string[] MsgCount = message.Split(new string[]{" "}, 1, StringSplitOptions.RemoveEmptyEntries);
				int messageCount = MsgCount.Count();

				if (this.MessageCount < messageCount)
				{
					this.MessageCount = messageCount;
				}

				sw.WriteLine("Quit ");
				sw.Flush();
				sw.Close();
				sr.Close();
				networkStream.Close();
				tcpClient.Close();
			}
		}

		public bool CheckError(string message)
		{
			return false;
		}
	}
}
