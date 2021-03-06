﻿using System;
using System.Threading;
using System.Threading.Tasks;
using ShopMe.Application.Models;

namespace ShopMe.Client.Services
{
    public interface IShopListService
    { 
        Task<IObservable<ShopListDescription>> GetListsAsync(CancellationToken cancellationToken = default);
    }
}