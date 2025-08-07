using System.Collections.Generic;
using UnityEngine;

namespace Infrastructure.Services.Path
{
    public static class Assets
    {
        public static async Awaitable<T> Create<T>(T asset) where T : Object => (await Object.InstantiateAsync(asset))[0];
        
        public static async Awaitable<IEnumerable<T>> Create<T>(T asset, int amount) where T : Object => await Object.InstantiateAsync(asset, amount);
    }
}