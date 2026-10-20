using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
namespace Workwise.Web.Hubs;
[Authorize] public class NotificationHub : Hub { }