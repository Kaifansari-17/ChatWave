//////using Microsoft.AspNetCore.SignalR;

//////namespace ChatApp
//////{
//////    public class ChatHub : Hub
//////    {
//////        public async Task SendMessage(string user,string message)
//////        {
//////            await Clients.All.SendAsync("RecieveMessage",user, message);
//////        }
//////    }
//////}


//using Microsoft.AspNetCore.SignalR;
//using System.Collections.Concurrent;

//namespace ChatApp
//{
//    public class ChatHub : Hub
//    {
//        // A thread-safe collection to store user connection mappings
//        private static ConcurrentDictionary<string, string> _connections = new ConcurrentDictionary<string, string>();

//        // connectionId -> username
//        private static ConcurrentDictionary<string, string> ConnectionUsers
//            = new ConcurrentDictionary<string, string>();

//        // Called when a user connects
//        public override Task OnConnectedAsync()
//        {
//            var userName = Context.GetHttpContext()?.Request.Query["user"]; // Assuming you send the username as a query parameter
//            if (!string.IsNullOrEmpty(userName))
//            {
//                  _connections[userName] = Context.ConnectionId;
//            }

//            return base.OnConnectedAsync();
//        }

//        // Called when a user disconnects
//        public override Task OnDisconnectedAsync(Exception exception)
//        {
//            var userName = _connections.FirstOrDefault(x => x.Value == Context.ConnectionId).Key;
//            if (!string.IsNullOrEmpty(userName))
//            {
//                _connections.TryRemove(userName, out _);
//            }

//            return base.OnDisconnectedAsync(exception);
//        }

//        // Method to send a message to a specific user
//        public async Task SendMessageToUser(string recipientUser, string message)
//        {
//            var sender = "";
//            string currenUserId = Context.ConnectionId;
//            if (!string.IsNullOrEmpty(currenUserId))
//            {
//                //string currentusername = Context.User.ToString();
//                sender = ConnectionUsers[Context.ConnectionId];

//            }
//            Console.WriteLine(ConnectionUsers[Context.ConnectionId]);
//            Console.WriteLine(_connections[recipientUser]);
//            if (_connections.TryGetValue(sender, out var connectionId))
//            {

//                // Send message to specific user
//                await Clients.Client(connectionId).SendAsync("ReceiveMessage", sender, message);
//            }
//        }
//    }
//}

///////////////////////////////////////////////////////////

//using Microsoft.AspNetCore.SignalR;
//using System.Collections.Concurrent;

//namespace ChatApp
//{
//    public class ChatHub : Hub
//    {
//        // username -> connectionId
//        private static ConcurrentDictionary<string, string> UserConnections =
//            new ConcurrentDictionary<string, string>();

//        // connectionId -> username
//        private static ConcurrentDictionary<string, string> ConnectionUsers =
//            new ConcurrentDictionary<string, string>();

//        public override Task OnConnectedAsync()
//        {
//            var username = Context.GetHttpContext()?
//                .Request.Query["user"].ToString();

//            if (!string.IsNullOrEmpty(username))
//            {
//                UserConnections[username] = Context.ConnectionId;
//                ConnectionUsers[Context.ConnectionId] = username;
//            }

//            return base.OnConnectedAsync();
//        }

//        public override Task OnDisconnectedAsync(Exception exception)
//        {
//            if (ConnectionUsers.TryRemove(Context.ConnectionId, out var username))
//            {
//                UserConnections.TryRemove(username, out _);
//            }
//            return base.OnDisconnectedAsync(exception);
//        }

//        public async Task SendMessageToUser(string recipientUser, string message)
//        {
//            if (!ConnectionUsers.TryGetValue(Context.ConnectionId, out var sender))
//                return;

//            if (UserConnections.TryGetValue(recipientUser, out var receiverConnId))
//            {
//                await Clients.Client(receiverConnId)
//                    .SendAsync("ReceiveMessage", sender, message);
//            }
//        }
//    }
//}

using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;

namespace ChatApp
{
    public class ChatHub : Hub
    {
        static ConcurrentDictionary<string, string> Users = new();
        static ConcurrentDictionary<string, string> Connections = new();

        public override async Task OnConnectedAsync()
        {
            var user = Context.GetHttpContext().Request.Query["user"].ToString();
            Users[user] = Context.ConnectionId;
            Connections[Context.ConnectionId] = user;

            await Clients.All.SendAsync("UserOnline", user);
        }

        public override async Task OnDisconnectedAsync(Exception ex)
        {
            if (Connections.TryRemove(Context.ConnectionId, out var user))
            {
                Users.TryRemove(user, out _);
                await Clients.All.SendAsync("UserOffline", user);
            }
        }

        public async Task SendMessageToUser(string toUser, string message)
        {
            var fromUser = Connections[Context.ConnectionId];
            if (Users.TryGetValue(toUser, out var conn))
            {
                await Clients.Client(conn)
                    .SendAsync("ReceiveMessage", fromUser, message);
            }
        }

        public async Task Typing(string toUser)
        {
            if (Users.TryGetValue(toUser, out var conn))
                await Clients.Client(conn)
                    .SendAsync("ShowTyping", Connections[Context.ConnectionId]);
        }

        public async Task StopTyping(string toUser)
        {
            if (Users.TryGetValue(toUser, out var conn))
                await Clients.Client(conn).SendAsync("HideTyping");
        }

        public async Task MessageSeen(string toUser)
        {
            if (Users.TryGetValue(toUser, out var conn))
                await Clients.Client(conn).SendAsync("MessageSeenAck");
        }
    }
}


