using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;
using System.Threading;
using System.Net;
using System.IO;
using System.Xml;

namespace Thetis
{

    public class ListenerEvent : EventArgs
    {
        public String Channel;
        public String Server;
        public String Message;

        public ListenerEvent(String rawMessage)
        {
            XmlDocument doc = new XmlDocument();
            try
            {
                doc.LoadXml(rawMessage);
                foreach (XmlNode xn in doc.ChildNodes)
                {
                    if (xn.Name == "Message")
                    {
                        foreach (XmlNode child in xn.ChildNodes)
                        {
                            if (child.Name == "Channel") Channel = child.InnerText;
                            else if (child.Name == "Server") Server = child.InnerText;
                            else if (child.Name == "Text") Message = child.InnerText;
                        }
                    }
                }
            }
            catch
            {
                return;
            }
        }

    }

    public class Listener
    {

        TcpListener listener;
        Thread listenerThread;
        bool running;

        public event EventHandler<ListenerEvent> MessageReceived;

        public Listener()
        {
            
        }

        public void Listen()
        {
            listener = new TcpListener(IPAddress.Parse("127.0.0.1"), 6116);
            listenerThread = new Thread(new ThreadStart(ListenThread));
            listenerThread.Start();
        }

        public void Stop()
        {
            running = false;
			listener.Stop();
			
        }

        public void ListenThread()
        {
            running = true;
            listener.Start();
            while (running)
            {
				TcpClient client;
				try
				{
                	client = listener.AcceptTcpClient();
				}
				catch 
				{
					return;
				}
                StreamReader stream = new StreamReader(client.GetStream());
                StringBuilder sb = new StringBuilder();
                
                char[] message = new char[4096];
                int bytesRead;

                while (true)
                {
                  bytesRead = 0;
                    
                    try
                    {
                      //blocks until a client sends a message
                        
                      bytesRead = stream.Read(message, 0, 4096);
                    }
                    catch
                    {
                      //a socket error has occured
                      break;
                    }

                    if (bytesRead == 0)
                    {
                      //the client has disconnected from the server
                      break;
                    }

                    
                }

                  client.Close();
                  if (MessageReceived != null) MessageReceived(this, new ListenerEvent(new String(message))); // TODO is this thread safe?

            }
            


        }



    }
}
