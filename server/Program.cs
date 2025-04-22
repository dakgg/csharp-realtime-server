using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Text;

// 초기 실행 시 DB 생성
SqliteHelper.Initialize();

var tcpListener = new TcpListener(IPAddress.Any, 7777);
tcpListener.Start();
Console.WriteLine("✅ TCP 서버 시작 (포트 7777)");

var udpClient = new UdpClient(8888);
Console.WriteLine("✅ UDP 서버 시작 (포트 8888)");

var userUdpMap = new ConcurrentDictionary<int, IPEndPoint>();


_ = Task.Run(async () =>
{
    while (true)
    {
        var result = await udpClient.ReceiveAsync();
        string msg = Encoding.UTF8.GetString(result.Buffer);
        Console.WriteLine($"[UDP] {result.RemoteEndPoint} → {msg}");

        var parts = msg.Split(',');
        if (parts.Length != 3 || !int.TryParse(parts[0], out var userId)) continue;

        string posX = parts[1];
        string posY = parts[2];
        userUdpMap[userId] = result.RemoteEndPoint;

        string broadcast = $"POS,{userId},{posX},{posY}";
        byte[] data = Encoding.UTF8.GetBytes(broadcast);

        foreach (var kvp in userUdpMap)
        {
            if (kvp.Key != userId)
            {
                await udpClient.SendAsync(data, data.Length, kvp.Value);
            }
        }
    }
});

while (true)
{
    var client = await tcpListener.AcceptTcpClientAsync();
    _ = Task.Run(async () =>
    {
        using var stream = client.GetStream();
        var buffer = new byte[1024];
        int read = await stream.ReadAsync(buffer);
        string message = Encoding.UTF8.GetString(buffer, 0, read);

        if (int.TryParse(message.Trim(), out int userId))
        {
            Console.WriteLine($"[TCP] 로그인: userId={userId}");
            byte[] response = Encoding.UTF8.GetBytes("LOGIN_OK");
            await stream.WriteAsync(response);
        }

        client.Close();
    });
}