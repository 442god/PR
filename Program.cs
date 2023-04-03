using UDP;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

internal class Program
{
    private static void Main(string[] args)
    {
        string multicastIP = "239.5.6.7";
        int multicastPort = 5002;

        UdpChat chat = new UdpChat(multicastIP, multicastPort);
        chat.StartReceiveLoop();

        Console.Write("Enter your username: ");
        string username = Console.ReadLine();

        while (string.IsNullOrEmpty(username))
        {
            Console.WriteLine("Username cannot be empty. Please try again.");
            Console.Write("Enter your username: ");
            username = Console.ReadLine();
        }

        // Check username uniqueness
        while (!IsUsernameUnique(chat, username))
        {
            Console.WriteLine($"The username '{username}' is already taken. Please choose a different username.");
            Console.Write("Enter your username: ");
            username = Console.ReadLine();
        }

        Console.WriteLine($"Welcome, {username}!");

        while (true)
        {
            try
            {
                Console.Write($"{username}: ");
                string text = Console.ReadLine() ?? "";

                if (!string.IsNullOrEmpty(text))
                {
                    chat.SendToGeneral($"{username}: {text}");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message.ToString());
            }
        }

        bool IsUsernameUnique(UdpChat chat, string username)
        {
            List<string> existingUsernames = new List<string>();
            chat.SendToGeneral($"Checking the username:{username}");

            // Wait for responses for 2 seconds
            DateTime timeout = DateTime.Now.AddSeconds(2);
            while (DateTime.Now < timeout)
            {
                byte[] buffer = new byte[1024];
                EndPoint remoteSender = new IPEndPoint(IPAddress.Any, 0);
                try
                {
                    chat.ReceiveMessage(buffer, ref remoteSender, 100);
                }
                catch (Exception)
                {
                    continue;
                }

                string message = Encoding.ASCII.GetString(buffer).Trim();
                if (message.StartsWith("USERNAME_EXISTS:"))
                {
                    string existingUsername = message.Substring("USERNAME_EXISTS:".Length);
                    existingUsernames.Add(existingUsername);
                }
            }

            return !existingUsernames.Contains(username);
        }
    }
}
