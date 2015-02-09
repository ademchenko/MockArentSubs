using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using FakeItEasy;
using FakeItEasy.Configuration;
using FakeItEasy.Core;
using FakeItEasy.Creation;
using MocksArentStubs.V1;
using Xunit;

namespace MocksArentStubs.FakeItEasy
{
    public interface IVerifiable<T> : IReturnValueArgumentValidationConfiguration<T>
    {
        IVerifiable<T> DoNotVerify();
    }

    public class Verifiable<T> : IVerifiable<T>
    {
        private readonly IReturnValueArgumentValidationConfiguration<T> _sourceItem;
        private readonly MockFactory _factory;

        public Verifiable(IReturnValueArgumentValidationConfiguration<T> sourceItem, MockFactory factory)
        {
            _sourceItem = sourceItem;
            _factory = factory;
        }

        public IAfterCallSpecifiedWithOutAndRefParametersConfiguration ReturnsLazily(Func<IFakeObjectCall, T> valueProducer)
        {
            return _sourceItem.ReturnsLazily(valueProducer);
        }

        public IVerifiable<T> DoNotVerify()
        {
            _factory.ExcludeFromVerification(this);
            return this;
        }

        public IAfterCallSpecifiedConfiguration Throws(Func<IFakeObjectCall, Exception> exceptionFactory)
        {
            return _sourceItem.Throws(exceptionFactory);
        }

        public IReturnValueConfiguration<T> Invokes(Action<IFakeObjectCall> action)
        {
            return _sourceItem.Invokes(action);
        }

        public void MustHaveHappened(Repeated repeatConstraint)
        {
           _sourceItem.MustHaveHappened(repeatConstraint);
        }

        public IAfterCallSpecifiedConfiguration CallsBaseMethod()
        {
            return _sourceItem.CallsBaseMethod();
        }

        public IReturnValueConfiguration<T> WhenArgumentsMatch(Func<ArgumentCollection, bool> argumentsPredicate)
        {
            return _sourceItem.WhenArgumentsMatch(argumentsPredicate);
        }
    }


    public sealed class MockFactory
    {
        private readonly bool _strictMocks;

        public MockFactory(bool strictMocks)
        {
            _strictMocks = strictMocks;
        }

        private List<IHideObjectMembers> _callsToVerify = new List<IHideObjectMembers>();

        public T CreateFake<T>(Action<IFakeOptionsBuilder<T>> options = null)
        {
            if (_strictMocks)
            {
                return A.Fake<T>(o => { o.Strict(); if (options != null) options(o); });
            }

            else
            {
                if (options != null)
                    return A.Fake<T>(options);

                return A.Fake<T>();
            }
        }


        public IVerifiable<T> CallTo<T>(Expression<Func<T>> callSpec)
        {
            var callMock = A.CallTo(callSpec);
            var verifiable = new Verifiable<T>(callMock, this);
            _callsToVerify.Add(verifiable);
            return verifiable;
        }

        public void ExcludeFromVerification<T>(Verifiable<T> sourceItem)
        {
            _callsToVerify.Remove(sourceItem);
        }

        public void VerifyAll()
        {
            foreach (IAssertConfiguration call in _callsToVerify)
            {
                call.MustHaveHappened(Repeated.AtLeast.Twice);
            }
        }
    }


    public class OrderInteractionTester
    {
        private readonly MockFactory _mockFactory = new MockFactory(true);

        [Fact]
        public void TestFillingRemovesInventoryIfEnought()
        {
            //Setup - creating data

            //Creating SUT
            Order order = new Order(ProductType.Whiskey, 50);
            //Creates proxy-object a.k.a mock that implements IWarehouse interface.
            IWarehouse warehouse = _mockFactory.CreateFake<IWarehouse>();


            var callHasInventory = _mockFactory.CallTo(() => warehouse.HasInventory(ProductType.Whiskey, 50m));
            
            callHasInventory.Returns(true).NumberOfTimes(3);

            var callRemoveInventory = A.CallTo(() => warehouse.RemoveInventory(ProductType.Whiskey, 50m));


            //Setup - adding expectations

            //Setup mock object's expectations at ordered mode - i.e. order of expected calls will be verified too.
            //using (warehouse.GetMockRepository().Ordered())
            {
                //Setup expect of call GetInventory with parameter ProductType.Whiskey 
                //warehouse.Expect(wh => wh.HasInventory(ProductType.Whiskey, 50m)).Return(true).Repeat.Once();
                //warehouse.Expect(wh => wh.RemoveInventory(ProductType.Whiskey, 50m)).Repeat.Once();
            }

            order.Fill(warehouse);


            //warehouse.VerifyAllExpectations();
            Assert.True(order.IsFilled);

            //callHasInventory.MustHaveHappened(Repeated.Exactly.Twice);
            //callRemoveInventory.MustHaveHappened(Repeated.Exactly.Once);

            //_mockFactory.VerifyAll();
        }

        [Fact]
        public void TestFillingDoesNotRemoveInventoryIfNotEnought()
        {
            IWarehouse warehouse = A.Fake<IWarehouse>(s => s.Strict());

            A.CallTo(() => warehouse.HasInventory(A<ProductType>._, A<decimal>._)).Returns(false).Once();

            Order order = new Order(ProductType.Whiskey, 50);
            order.Fill(warehouse);

            //warehouse.VerifyAllExpectations();

            Assert.False(order.IsFilled);


        }

        [Fact]
        public void TestOrderSendsEmailsIfUnfilled()
        {
            V2.Order order = new V2.Order(ProductType.Whiskey, 51m);

            IWarehouse warehouse = A.Fake<IWarehouse>(s => s.Strict());
            IMailService mailer = A.Fake<IMailService>(s => s.Strict());

            order.Mailer = mailer;

            A.CallTo(() => mailer.Send(A<Message>._)).MustHaveHappened(Repeated.Exactly.Once);
            A.CallTo(() => warehouse.HasInventory(A<ProductType>._, A<decimal>._)).Returns(false).Once();

            order.Fill(warehouse);

            //warehouse.VerifyAllExpectations();
            //mailer.VerifyAllExpectations();
        }
    }
}
