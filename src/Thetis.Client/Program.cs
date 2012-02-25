using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.IO;
using System.Net.Sockets;
using System.Net;

namespace ThetisClient
{
    class Program
    {

        static public void SendMessage(String server, String channel, String message)
        {
            Console.WriteLine("Sending {0} {1} {2}", server, channel, message);

            StringBuilder sb = new StringBuilder();

            XmlWriter writer = XmlTextWriter.Create(sb);

            writer.WriteStartDocument();
            writer.WriteStartElement("Message");
            writer.WriteStartElement("Server");
            writer.WriteString(server);
            writer.WriteEndElement();
            writer.WriteStartElement("Channel");
            writer.WriteString(channel);
            writer.WriteEndElement();
            writer.WriteStartElement("Text");
            writer.WriteString(message);
            writer.WriteEndElement();
            writer.WriteEndElement();
            writer.Flush();


            String xmldoc = sb.ToString();

            Console.WriteLine(xmldoc);

            TcpClient client = new TcpClient();
            client.Connect(IPAddress.Parse("127.0.0.1"), 6116);
            StreamWriter stream = new StreamWriter(client.GetStream());
            stream.Write(xmldoc);
            stream.Flush();
            client.Close();
                

        }

        static void Main(string[] args)
        {
            if (args.Length < 3)
            {
                Console.WriteLine("Not enough arguments");
                return;
            }
            String server = args[0];
            String channel = args[1];
            StringBuilder sb = new StringBuilder();
            for (int i = 2; i < args.Length; i++)
            {
                sb.Append(args[i]);
                sb.Append(" ");
            }

            SendMessage(server, channel, sb.ToString());

        }
    }
}
