using Rentzy.DAL.Repository;
using Rentzy.DAL.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Rentzy.BLL.Services
{
    public class RentalRequestService
    {
        private readonly IRentalRequestRepository _repo;
        public RentalRequestService(IRentalRequestRepository repo)
        {
            _repo = repo;
        }

        public Task<List<DateTime>> GetBookedDatesAsync(int propertyId)
            => _repo.GetBookedDatesForPropertyAsync(propertyId);

        public Task AddRequestAsync(PropertyRentalRequest request)
            => _repo.AddRequestAsync(request);
    }
}
