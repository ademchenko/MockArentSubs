using MocksArentStubs.V1;
using Rhino.Mocks;
using Xunit;

namespace MocksArentStubs
{
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