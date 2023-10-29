using System;
using System.IO;

namespace TcpTool
{
    class Program
    {
        const string help = @"
*TcpTool*  Usage:
tcptool r <portnum> [logfile]
    receive incoming requests on portnum, optionally appending to logfile
tcptool d <portnum> <filename>
    receive incoming requests on portnum and download to file filename
tcptool p <incoming_portnum> <recipient_address> <recipient_portnum>
    pass-through messages coming in on port incoming_portnum to 
    server recipient_address on port recipient_portnum
tcptool u <recipient_address> <recipient_portnum> <filename> <number_of_repetitions>
    upload file filename to recipient address:recipient_portnum * number_of_repititions
";

        static bool OutputMessage(string msg, string log_file)
        {
            Console.WriteLine(msg);
            if (log_file != null)
            {
                File.AppendAllText(log_file, msg + Environment.NewLine);
            }
            return false;
        }


        static int Main(string[] args)
        {
            TcpReceiver server;
            TcpSender client;
            int port;
            if (args.Length < 2)
            {
                Console.WriteLine(help);
                return 0;
            }
            switch (args[0].ToLower().Substring(0, 1))
            {
                case "r":
                    server = new TcpReceiver();
                    port = int.Parse(args[1]);
                    Console.WriteLine("Listening for connections on port {0}", port);
                    server.startListening(port, (msg) => { return OutputMessage(msg, (args.Length > 2 ? args[2] : null)); });
                    break;

                case "d":
                    server = new TcpReceiver();
                    port = int.Parse(args[1]);
                    Console.WriteLine("Downloading from connections on port {0} to file {1}", port, args[2]);
                    using (var file = new StreamWriter(args[2], append: true)) {
                        server.startListening(port, (msg) => { file.Write(msg); return false; });
                    }
                    break;

                case "p":
                    server = new TcpReceiver();
                    int incoming_port = int.Parse(args[1]);
                    int recipient_port = int.Parse(args[3]);
                    Console.WriteLine("Listening for connections on port {0} and passing on to {1}:{2}", incoming_port, args[2], recipient_port);
                    TcpSender sender = new TcpSender();
                    sender.connect(args[2], recipient_port);
                    Func<string, bool> pass_thru = (msg) =>
                    {
                        sender.send(msg); return false;
                    };
                    server.startListening(incoming_port, pass_thru);
                    break;

                case "u":
                    port = int.Parse(args[2]);
                    int reps = args.Length > 4 ? int.Parse(args[4]) : 1;
                    Console.WriteLine("Uploading {0} to {1}:{2}", args[3], args[1], port);
                    using (client = new TcpSender())
                    {
                        client.connect(args[1], port);
                        string msg = File.ReadAllText(args[3]);
                        for (int rep = 0; rep < reps; ++rep)
                        {
                            client.send(msg);
                        }
                    }
                    break;

                default:
                    Console.WriteLine(help);
                    break;
            }
            return 0;
        }
    }
}
