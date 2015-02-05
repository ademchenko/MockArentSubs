namespace MocksArentStubs
{
    public interface IWarehouse
    {
        void AddProduct(ProductType product, decimal amount);
        bool HasInventory(ProductType product, decimal amount);
        decimal GetInventory(ProductType product);
        void RemoveInventory(ProductType product, decimal amount);
    }
}