﻿using System.Threading;
using System.Threading.Tasks;
using ShopMe.Application.Models;
using ShopMe.Application.Observable.Collections;

namespace ShopMe.Application
{
    public interface IShopMeEngine
    {
        Task<IObservableCollection<ShopList>> GetActualListsAsync(CancellationToken cancellationToken);
    }
}
