using Packages.StyngrSDK.Runtime.Scripts.Store.Handlers.Event.InternalDataStorage;
using Packages.StyngrSDK.Runtime.Scripts.Store.NFTs;
using Packages.StyngrSDK.Runtime.Scripts.Store.Strategies.Event.InternalDataStorage;
using Styngr.Exceptions;
using Styngr.Model.Tokens;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Packages.StyngrSDK.Runtime.Scripts.Store.Handlers.Event
{
    internal class NFTAssignEventHandler : IAssignEventHandler<NFT>
    {
        private readonly IStorage<NFT> storage;

        public NFTAssignEventHandler()
        {
            storage = new NFTInternalStorage();
        }

        public IEnumerator Bind()
        {
            throw new NotImplementedException();
        }

        public IEnumerator PopulateData(Action<List<NFT>> dataCallback, Action<ErrorInfo> onFail)
        {
            yield return storage.GetData(dataCallback, onFail);
        }

        public bool Search(List<Tile_MyNFT> data, Predicate<Tile_MyNFT> predicate)
        {
            data.ForEach(x => x.gameObject.SetActive(false));
            var temp = data.FindAll(predicate);
            if (temp.Count > 0)
            {
                temp.ForEach(x => x.gameObject.SetActive(true));
                return true;
            }

            return false;
        }
    }
}
