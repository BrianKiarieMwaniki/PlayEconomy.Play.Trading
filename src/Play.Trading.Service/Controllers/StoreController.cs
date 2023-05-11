using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Play.Common;
using Play.Trading.Service.Entities;

namespace Play.Trading.Service.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class StoreController : ControllerBase
    {
        private readonly IRepository<CatalogItem> _catalogRepository;
        private readonly IRepository<ApplicationUser> _usersRepository;
        private readonly IRepository<InventoryItem> _inventoryRepository;

        public StoreController(IRepository<CatalogItem> catalogRepository, IRepository<ApplicationUser> usersRepository, IRepository<InventoryItem> inventoryRepository)
        {
            _catalogRepository = catalogRepository;
            _usersRepository = usersRepository;
            _inventoryRepository = inventoryRepository;
        }

        [HttpGet]
        public async Task<ActionResult<StoreDto>> GetAsync()
        {
            string userId = User.FindFirstValue("sub");

            var catalogitems = await _catalogRepository.GetAllAsync();

            var inventoryitems = await _inventoryRepository.GetAllAsync(i => i.UserId == Guid.Parse(userId));

            var user = await _usersRepository.GetAsync(Guid.Parse(userId));

            var storeDto = new StoreDto(
                catalogitems.Select(catalogItem => new StoreItemDto(
                    catalogItem.Id,
                    catalogItem.Name,
                    catalogItem.Description,
                    catalogItem.Price,
                    inventoryitems.FirstOrDefault(inventoryItem => inventoryItem.CatalogItemId == catalogItem.Id)?.Quantity ?? 0
                )),
                user?.Gil ?? 0
            );

            return Ok(storeDto);
        }
    }
}