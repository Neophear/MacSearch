using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using MinimalisticTelnet;

namespace MacSearch
{
    class Program
    {
        public static string macAddress = "XXXX";
        public static string outgoingPort = string.Empty;
        public static string IPAddress = string.Empty;
        public static TelnetConnection tc = new TelnetConnection("10.10.99.1", 23);

        static void Main(string[] args)
        {
            //login with user "root",password "rootpassword", using a timeout of 100ms, and show server output
            string s = tc.Login("password",1000);
            Console.Write(s);

            // server output should end with "$" or ">", otherwise the connection failed
            string prompt = s.TrimEnd();
            prompt = s.Substring(prompt.Length -1,1);
            if (prompt != "$" && prompt != ">")
                throw new Exception("Connection failed");

            tc.WriteLine("en");
            tc.WriteLine("password");
            tc.WriteLine("show mac address-table address " + macAddress);
            prompt = "";

            // while connected
            while (tc.IsConnected && prompt.Trim() != "exit" )
            {
                string LastLine = "";
                // display server output
                LastLine = tc.Read();
                Console.Write(LastLine);
                ReadLine(LastLine);

                // send client input to server
                prompt = Console.ReadLine();
                tc.WriteLine(prompt);

                // display server output
                LastLine = tc.Read();
                Console.Write(LastLine);
                ReadLine(LastLine);
            }
            Console.WriteLine("***DISCONNECTED");
            Console.ReadLine();
        }

        public static void ReadLine(string line)
        {
            if (Regex.IsMatch(line, macAddress, RegexOptions.IgnoreCase))
            {
                outgoingPort = Regex.Match(line, "(fa|gi)(\\d/|\\d)+", RegexOptions.IgnoreCase).Value;
                SendCommand("sho cdp neigh gi1/0/3 deta | incl IP");
            }
            else if (Regex.IsMatch(line, "IP address: ", RegexOptions.IgnoreCase))
            {
                IPAddress = Regex.Match(line, "\\d+.\\d+.\\d+.\\d+").Value;
                Console.WriteLine("+++++" + IPAddress);
            }
        }

        public static void SendCommand(string Command)
        {
            tc.WriteLine(Command);
        }
    }
}