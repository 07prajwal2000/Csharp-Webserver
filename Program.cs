using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;

var server = new TcpListener(IPAddress.Parse("127.0.0.1"), 5599);
var filesPath = Directory.GetFiles("./static", "*.*", SearchOption.AllDirectories);
var files = filesPath.ToDictionary(f => Path.GetRelativePath("./static/", f).Replace(@"\", "/"));
Console.CancelKeyPress += (s, e) => 
{
    Console.WriteLine("Server stopped");
    server.Stop();
};
server.Start();
Console.WriteLine("Server started on port: 5599");
while (true)
{
    using var client = server.AcceptSocket();
    Console.WriteLine("client connected: " + client.RemoteEndPoint);
    Span<byte> buff = new byte[1024];
    var totalRecieve = client.Receive(buff);
    var recieved = buff[..totalRecieve];
    var req = Encoding.UTF8.GetString(recieved);
    HandleRequest(req, client);
    Console.WriteLine("closed");
    client.Close();
}

void HandleRequest(string buff, Socket client)
{
    var pathIdx = buff.IndexOf("Get /", StringComparison.CurrentCultureIgnoreCase);
    var httpIdx = buff.IndexOf(" ", 5, StringComparison.CurrentCultureIgnoreCase);
    var path = buff[(pathIdx+5)..httpIdx];
    Console.WriteLine("Filename: " + path);
    if (!files.TryGetValue(path, out var file)) 
    {
        var msg = GetMessageBytes("404 NotFound", Array.Empty<byte>(), "text/html");
        client.Send(msg);
        return;
    }
    var buf = File.ReadAllBytes(file);
    var extension = Path.GetExtension(file);
    var mime = MimeTypeMap.GetMimeType(extension);
    System.Console.WriteLine("Mime: " + mime);
    var msg1 = GetMessageBytes("200 OK", buf, mime);
    client.Send(msg1);
}

string GetMessage()
{
    var json = JsonSerializer.Serialize(new {Name = "Prajwal"});
    return @"HTTP/1.1 200 OK
Server: prajwal-server
Connection: Close
Content-Type: text/html

<head><title>this is title</title></head><body><h2>Prajwal aradhya</h2></body>";
}

byte[] GetMessageBytes(string statuscode, byte[] content, string mimetype)
{
    using var ms = new MemoryStream(100);
    using var writer = new BinaryWriter(ms, Encoding.ASCII);
    writer.Write($"HTTP/1.1 {statuscode}\n");
    writer.Write($"server:prajwal-server\nConnection: Close");
    writer.Write($"Content-Type: {mimetype}\n\n");
    writer.Write(content);
    return ms.ToArray();
}
