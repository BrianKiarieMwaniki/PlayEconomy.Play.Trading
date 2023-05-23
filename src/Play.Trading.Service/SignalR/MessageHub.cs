using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Play.Trading.Service.StateMachine;

namespace Play.Trading.Service.SignalR
{
    [Authorize]
    public class MessageHub : Hub
    {
        public async Task SendStatusAsync(PurchaseState status)
        {
            if(Clients!= null)
            {
                 await Clients.User(Context.UserIdentifier)
                    .SendAsync("ReceivePurchaseStatus", status);
            }
        }
    }
}