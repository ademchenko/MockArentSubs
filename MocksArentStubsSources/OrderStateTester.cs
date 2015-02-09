using System.Collections.Generic;
using MocksArentStubs.V1;
using Xunit;

namespace MocksArentStubs
{
    public class MailServiceStub : IMailService
    {
        private readonly List<Message> messages = new List<Message>();

        public void Send(Message message)
        {
            messages.Add(message);
        }

        public int NumberOfMessagesSent { get { return messages.Count; } }
    }


    public class OrderStateTester
    {
        private readonly WarehouseImpl warehouse = new WarehouseImpl();

        public OrderStateTester()
        {
            //Set up
            warehouse.AddProduct(ProductType.Whiskey, 50m);
            warehouse.AddProduct(ProductType.Vodka, 25m);
        }

        [Fact]
        public void TestOrderIsFilledIfEnoughtInWarehouse()
        {
            //Set up
            var order = new Order(ProductType.Whiskey, 50m);

            //Exercize
            order.Fill(warehouse);

            //Verify
            Assert.True(order.IsFilled);
            Assert.Equal(0m, warehouse.GetInventory(ProductType.Whiskey));
        }

        [Fact]
        public void TestOrderIsNotFilledAndDoesNotRemoveFromWarehouseIfNotEnought()
        {
            //Set up
            var order = new Order(ProductType.Whiskey, 51m);

            //Exercize
            order.Fill(warehouse);

            //Verify
            Assert.False(order.IsFilled);
            Assert.Equal(50m, warehouse.GetInventory(ProductType.Whiskey));
        }

        [Fact]
        public void TestOrderSendsEmailsIfUnfilled()
        {
            var order = new V2.Order(ProductType.Whiskey, 51m);
            var mailServiceStub = new MailServiceStub();    
            order.Mailer = mailServiceStub;

            order.Fill(warehouse);

            Assert.Equal(1, mailServiceStub.NumberOfMessagesSent);
        }
    }
}
