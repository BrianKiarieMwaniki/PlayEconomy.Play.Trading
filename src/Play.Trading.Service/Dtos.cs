using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Play.Trading.Service
{
    public record SubmitPurchaseDto(
        [Required]Guid? ItemId,
        [Range(1, 100)]int Quantity
    );

}