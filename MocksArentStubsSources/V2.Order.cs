namespace MocksArentStubs.V2
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
            if (!warehouse.HasInventory(product, amount))
            {
                Mailer.Send(new Message(text: string.Format("Not enought amount of {0}", product), to: "admin@example.com"));
                return;
            }

            warehouse.RemoveInventory(product, amount);
            IsFilled = true;
        }

        public bool IsFilled { get; private set; }

        public IMailService Mailer { get; set; }
    }
}