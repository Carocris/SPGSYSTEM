using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Database.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services
{
    public class SaleDetailService : GenericService<SaleDetail>, ISaleDetailService
    {
        public SaleDetailService(IGenericRepository<SaleDetail> repository)
            : base(repository)
        {
        }
    }
}
