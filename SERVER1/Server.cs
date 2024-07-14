using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace SERVER1
{
    internal class Server
    {
        protected internal TcpListener server;
        protected internal List<Client> users = new List<Client>();

        bool START_CHAT = false;

        public Server(TcpListener tcpListener)
        {
            server = tcpListener;
        }

        // Запуск сервера
        protected internal void Start()
        {
            try
            {
                server.Start();
                Console.WriteLine("Сервер запущен. Ожидается подключение клиентов");
                Task.Run(WaitingStartChat);
                while (true)
                {
                    if(users.Count < 2)
                    {
                        TcpClient player = server.AcceptTcpClient();
                        Client client = new Client(player);
                        users.Add(client);
                        Console.WriteLine($"К серверу подключен пользователь {client.id}");
                        Task.Run(() => СlientListening(client));
                    } 
                }
            }
            catch
            {
                Console.WriteLine("Сервер завершил свою работу");
                Disconnect();
            }
        }

        // Завершение работы сервера
        protected internal void Disconnect()
        {
            users.ForEach(client =>
            {
                if (!client.close)
                {
                    client.Message("Сервер завершил свою работу");
                    client.Close();
                }
            });
            users.Clear();
            server.Stop();
        }

        // Ожидание участников и запуск чата
        protected internal void WaitingStartChat()
        {
            while (true)
            {
                while (users.Count < 2 || START_CHAT)
                {
                    Thread.Sleep(5000);
                }
                users.ForEach(user =>
                {
                    user.Message("Вы подключены к чату. Происходит обмен ключами.");
                }
                );
                START_CHAT = true;
            }
        }

        // Обработка запросов пользователя
        protected internal void СlientListening(Client client)
        {
            client.Message("Вы подключены к серверу.");
            bool process = true;
            string request;
            while (process)
            {
                try
                {
                    request = client.Reader.ReadLine();
                    switch (request.ToLower())
                    {
                        case "\\exit":
                            {
                                bool exit = false;
                                while (!exit)
                                {
                                    client.Message("Вы точно хотите отключиться от сервера? (yes/no)");
                                    request = client.Reader.ReadLine();
                                    if (request.ToLower().Equals("yes"))
                                    {
                                        Console.WriteLine($"Пользователь {client.id} покинул сервер");
                                        users.ForEach(clients =>
                                        {
                                            if (clients.id != client.id)
                                            {
                                                clients.Message($"Собеседник покинул чат. Вы будете отключены от чата.");
                                            }
                                            client.Close();
                                            Disconnect();
                                            process = false;
                                            exit = true;
                                        });
                                    }
                                    else if (!request.ToLower().Equals("no"))
                                    {
                                        client.Message("Некорректный ответ, повторите попытку");
                                    }
                                    else
                                    {
                                        exit = true;
                                    }
                                }
                                break;
                            }
                        default:
                            {
                                if (Regex.IsMatch(request, @"\\chat\\.*", RegexOptions.IgnoreCase))
                                {
                                    Console.WriteLine($"Пользователь {client.id} отправил сообщение: " + 
                                        Regex.Replace(request, @"\\chat\\", "", RegexOptions.IgnoreCase)
                                    );
                                    request = Regex.Replace(request, @"\\chat\\", "\\message\\Сообщение собеседника: ", RegexOptions.IgnoreCase);
                                    SendMessage(client, request);
                                }
                                else if (Regex.IsMatch(request, @"\\key\\.*", RegexOptions.IgnoreCase))
                                {
                                    Console.WriteLine($"Пользователь {client.id} отправил свой открытый ключ: " +
                                        Regex.Replace(request, @"\\key\\", "(", RegexOptions.IgnoreCase) + ")"
                                    );
                                    SendMessage(client, request);
                                }
                                else
                                {
                                    client.Message("Неизвестный запрос.");
                                }
                                break;
                            }
                    }
                }
                catch
                {
                    Console.WriteLine($"Пользователь {client.id} покинул сервер");
                    users.ForEach(clients =>
                    {
                        if (clients.id != client.id)
                        {
                            clients.Message($"Собеседник покинул чат. Вы будете отключены от чата.");
                        }
                    });
                    client.Close();
                    process = false;
                    Disconnect();
                }
            }
        }

        // Отправка сообщения пользователю
        protected internal void SendMessage(Client speaker, string message)
        {
            users.ForEach(clients =>
            {
                if (clients.id != speaker.id)
                {
                    clients.Message(message);
                }
            }
            );
        }
    }
}