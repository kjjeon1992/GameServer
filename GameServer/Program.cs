using System;

namespace GameServer
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Game Server Starting..");

            var server = new GameServer();
            await server.StartAsyncServer();

            Console.WriteLine("Game Server Started.");
            Console.WriteLine("Press Enter to Shut Down.");
            Console.ReadLine();

            await server.StopAsyncServer();
            Console.WriteLine("Game Server Stopped.");
        }
    }
}