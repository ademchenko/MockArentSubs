using System;

namespace MocksArentStubs
{
    public sealed class WarehouseImpl : IWarehouse
    {
        private decimal amountOfWhiskey;
        private decimal amountOfVodka;

        #region Implementation of IWarehouse

        public void AddProduct(ProductType product, decimal amount)
        {
            switch (product)
            {
                case ProductType.Vodka:
                    amountOfVodka += amount;
                    break;
                case ProductType.Whiskey:
                    amountOfWhiskey += amount;
                    break;
                default:
                    throw new ArgumentOutOfRangeException("product");
            }
        }

        public bool HasInventory(ProductType product, decimal amount)
        {
            switch (product)
            {
                case ProductType.Vodka:
                    return amountOfVodka >= amount;
                case ProductType.Whiskey:
                    return amountOfWhiskey >= amount;
                default:
                    throw new ArgumentOutOfRangeException("product");
            }
        }

        public decimal GetInventory(ProductType product)
        {
            switch (product)
            {
                case ProductType.Vodka:
                    return amountOfVodka;
                case ProductType.Whiskey:
                    return amountOfWhiskey;
                default:
                    throw new ArgumentOutOfRangeException("product");
            }
        }

        public void RemoveInventory(ProductType product, decimal amount)
        {
            switch (product)
            {
                case ProductType.Vodka:
                    amountOfVodka -= amount;
                    return;
                case ProductType.Whiskey:
                    amountOfWhiskey -= amount;
                    return;
                default:
                    throw new ArgumentOutOfRangeException("product");
            }
        }

        #endregion
    }
}