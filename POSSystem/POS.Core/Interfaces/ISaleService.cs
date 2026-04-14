using System;
using System.Collections.Generic;
using System.Text;

using POS.Core.DTOs;

namespace POS.Core.Interfaces
{
    public interface ISaleService
    {
        Task<int> CreateSaleAsync(SaleRequestDto request);
    }
}

