using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Play.Trading.Service
{
    public record PurchaseRequested(
        Guid UserId,
        Guid ItemId,
        int Quantity,
        Guid CorrelationId
    );
    

    public record GetPurchaseState(Guid CorrelationId);
}