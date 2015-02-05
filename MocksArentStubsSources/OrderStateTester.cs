using System.Collections.Generic;
using Rhino.Mocks;
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


    public class OrderInteractionTester
    {
        [Fact]
        public void TestFillingRemovesInventoryIfEnought()
        {
            //Setup - creating data

            //Creating SUT
            Order order = new Order(ProductType.Whiskey, 50);
            //Creates proxy-object a.k.a mock that implements IWarehouse interface.
            IWarehouse warehouse = MockRepository.GenerateStrictMock<IWarehouse>();

            //Setup - adding expectations

            //Setup mock object's expectations at ordered mode - i.e. order of expected calls will be verified too.
            using (warehouse.GetMockRepository().Ordered())
            {
                //Setup expect of call GetInventory with parameter ProductType.Whiskey 
                warehouse.Expect(wh => wh.HasInventory(ProductType.Whiskey, 50m)).Return(true).Repeat.Once();
                warehouse.Expect(wh => wh.RemoveInventory(ProductType.Whiskey, 50m)).Repeat.Once();
            }

            order.Fill(warehouse);

            warehouse.VerifyAllExpectations();
            Assert.True(order.IsFilled);
        }

        [Fact]
        public void TestFillingDoesNotRemoveInventoryIfNotEnought()
        {
            IWarehouse warehouse = MockRepository.GenerateStrictMock<IWarehouse>();

            warehouse.Expect(wh => wh.HasInventory(Arg<ProductType>.Is.Anything, Arg<decimal>.Is.Anything)).Return(false).Repeat.Once();

            Order order = new Order(ProductType.Whiskey, 50);
            order.Fill(warehouse);

            warehouse.VerifyAllExpectations();
            Assert.False(order.IsFilled);
        }

        [Fact]
        public void TestOrderSendsEmailsIfUnfilled()
        {
            V2.Order order = new V2.Order(ProductType.Whiskey, 51m);

            IWarehouse warehouse = MockRepository.GenerateStrictMock<IWarehouse>();
            IMailService mailer = MockRepository.GenerateStrictMock<IMailService>();

            order.Mailer = mailer;

            mailer.Expect(m => m.Send(Arg<Message>.Is.Anything)).Repeat.Once();
            warehouse.Expect(wh => wh.HasInventory(Arg<ProductType>.Is.Anything, Arg<decimal>.Is.Anything)).Return(false).Repeat.Once();
            
            order.Fill(warehouse);

            warehouse.VerifyAllExpectations();
            mailer.VerifyAllExpectations();
        }
    }
}
