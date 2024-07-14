using System.IO;
using System.Net.Sockets;

namespace SERVER1
{
    internal class Client
    {
        protected internal static int count = 1;
        protected internal int id;

        protected internal TcpClient client;

        protected internal NetworkStream stream;
        protected internal StreamWriter Writer { get; }
        protected internal StreamReader Reader { get; }

        protected internal bool close = false;

        public Client(TcpClient tcpClient)
        {
            id = count;
            client = tcpClient;
            stream = client.GetStream();
            Reader = new StreamReader(stream);
            Writer = new StreamWriter(stream);
            count++;
        }

        protected internal void Message(string message)
        {
            Writer.WriteLine(message);
            Writer.Flush();
        }

        protected internal void Close()
        {
            Writer.Close();
            Reader.Close();
            client.Close();
            close = true;
        }
    }
}