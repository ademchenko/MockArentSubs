using MocksArentStubs;

namespace MocksArentStubs.V1
{
    public sealed class Order
    {
        private readonly ProductType product;
        private readonly decimal amount;

        public Order(ProductType product, decimal amount)
        {
            this.product = product;
            this.amount = amount;

            IsFilled = false;
        }

        public void Fill(IWarehouse warehouse)
        {
            warehouse.HasInventory(product, amount);

            if (!warehouse.HasInventory(product, amount))
                return;

            warehouse.RemoveInventory(product, amount);
            IsFilled = true;
        }

        public bool IsFilled { get; private set; }
    }
}