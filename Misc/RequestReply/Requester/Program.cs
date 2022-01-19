using System;

namespace Requester
{
    class Program
    {
        static void Main(string[] args)
        {
            var rpcClient = new RpcClient();

            Console.WriteLine("[x] Requesting Fibonacci(30)");
            var response = rpcClient.Call("30");

            Console.WriteLine("[.] Response: {0}", response);
            rpcClient.Close();
        }
    }
}
