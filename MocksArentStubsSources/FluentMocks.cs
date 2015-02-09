using System;
using System.Collections.Generic;
using FakeItEasy;
using FakeItEasy.Configuration;
using Xunit;

namespace MocksArentStubs.FakeItEasy
{
    public class FluentMocks
    {
        [Fact]
        public void MockBehaviourStrict()
        {
            var outerFake = A.Fake<IOuter>();
            var newGuid = Guid.NewGuid();
            var call = A.CallTo(() => outerFake.Inner.Id).Returns(newGuid);

            Assert.Equal(newGuid, outerFake.Inner.Id);

            var inner = outerFake.Inner;

            ((IAssertConfiguration) call).MustHaveHappened(Repeated.Exactly.Once);
        }

        public interface IInner
        {
            Guid Id { get; set; }
            int Number { get; set; }
        }

        public interface IOuter
        {
            IInner Inner { get; set; }
            object Object { get; set; }
        }
    }
}
