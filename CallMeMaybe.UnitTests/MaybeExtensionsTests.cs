using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using FluentAssertions;
using Moq;
using Xunit;

namespace CallMeMaybe.UnitTests
{
    public class MaybeExtensionsTests
    {
        [Fact]
        public void TestSelectManyIEnumerables()
        {
            var values = new int?[] {1, null, 3};
            var values2 = values.SelectMany(Maybe.From);
            values2.Should().BeEquivalentTo(1, 3);
        }

        [Fact]
        public void TestDictionaryMaybeValue()
        {
            var values = new[] {1, 2, 4};
            var dict = values.ToDictionary(v => v.ToString(CultureInfo.InvariantCulture));
            var values2 =
                (from k in new[] {"1", "2", "3"}
                    from v in dict.GetMaybe(k)
                    select new {k, v})
                    .ToList();
            values2.Count.Should().Be(2);
            values2[0].v.Should().Be(1);
            values2[0].k.Should().Be("1");
            values2[1].v.Should().Be(2);
            values2[1].k.Should().Be("2");
        }

        [Fact]
        public void TestNullableConversions()
        {
            UseNullable(Maybe.From(1).Nullable()).Should().Be("1");
            UseNullable(Maybe<int>.Not.Nullable()).Should().Be("");
            UseMaybe(((int?) 1).Maybe()).Should().Be("1");
            UseMaybe(((int?) null).Maybe()).Should().Be("");
        }

        [Fact]
        public void TestFirstMaybe()
        {
            Assert.Equal(Maybe.From(1), new[] {1}.FirstMaybe());
            Assert.Equal(Maybe.From(1), new[] {1}.AsQueryable().FirstMaybe());
            Assert.Equal(Maybe<int>.Not, new int?[] {null}.FirstMaybe());
            Assert.Equal(Maybe<int>.Not, new int?[] {null}.AsQueryable().FirstMaybe());
            Assert.Equal(Maybe<int>.Not, new int?[] {null, 2, 3}.FirstMaybe());
            Assert.Equal(Maybe<int>.Not, new int?[] {null, 2, 3}.AsQueryable().FirstMaybe());
            Assert.Equal(Maybe.From(1), new[] {1, 2, 3}.FirstMaybe());
            Assert.Equal(Maybe.From(1), new[] {1, 2, 3}.AsQueryable().FirstMaybe());
            Assert.Equal(Maybe<int>.Not, new int[0].FirstMaybe());
            Assert.Equal(Maybe<int>.Not, new int[0].AsQueryable().FirstMaybe());

            Assert.Equal(Maybe.From("1"), new[] {"1"}.FirstMaybe());
            Assert.Equal(Maybe.From("1"), new[] {"1"}.AsQueryable().FirstMaybe());
            Assert.Equal(Maybe<string>.Not, new string[] {null}.FirstMaybe());
            Assert.Equal(Maybe<string>.Not, new string[] {null}.AsQueryable().FirstMaybe());
            Assert.Equal(Maybe<string>.Not, new[] {null, "2", "3"}.FirstMaybe());
            Assert.Equal(Maybe<string>.Not, new[] {null, "2", "3"}.AsQueryable().FirstMaybe());
            Assert.Equal(Maybe.From("1"), new[] {"1", "2", "3"}.FirstMaybe());
            Assert.Equal(Maybe.From("1"), new[] {"1", "2", "3"}.AsQueryable().FirstMaybe());
            Assert.Equal(Maybe<string>.Not, new string[0].FirstMaybe());
            Assert.Equal(Maybe<string>.Not, new string[0].AsQueryable().FirstMaybe());
        }

        [Fact]
        public void QueryableFirstMaybeShouldNotInvokeIEnumerableInterface()
        {
            // Arrange
            var mock = new Mock<QueryableMockBase<int>>(new[]{1}.AsQueryable()){CallBase = true};

            // Act
            mock.Object.FirstMaybe();

            // Assert
            mock.Verify(q => q.GetEnumerator(), Times.Never);
            mock.Verify(q => q.Provider, Times.Once);
        }

        [Fact]
        public void TestSingleMaybe()
        {
            Assert.Equal(Maybe.From(1), new[] {1}.SingleMaybe());
            Assert.Equal(Maybe.From(1), new[] {1}.AsQueryable().SingleMaybe());
            Assert.Equal(Maybe<int>.Not, new int?[] { null }.SingleMaybe());
            Assert.Equal(Maybe<int>.Not, new int?[] { null }.AsQueryable().SingleMaybe());
            
            Assert.Throws<InvalidOperationException>(() => new int?[] { null, 2, 3 }.SingleMaybe());
            Assert.Throws<InvalidOperationException>(() => new int?[] { null, 2, 3 }.AsQueryable().SingleMaybe());
            Assert.Throws<InvalidOperationException>(() => new[] { 1, 2, 3 }.SingleMaybe());
            Assert.Throws<InvalidOperationException>(() => new[] {1, 2, 3}.AsQueryable().SingleMaybe());
            Assert.Equal(Maybe<int>.Not, new int[0].SingleMaybe());
            Assert.Equal(Maybe<int>.Not, new int[0].AsQueryable().SingleMaybe());

            Assert.Equal(Maybe.From("1"), new[] {"1"}.SingleMaybe());
            Assert.Equal(Maybe.From("1"), new[] {"1"}.AsQueryable().SingleMaybe());
            Assert.Equal(Maybe<string>.Not, new string[] {null}.SingleMaybe());
            Assert.Equal(Maybe<string>.Not, new string[] {null}.AsQueryable().SingleMaybe());
            Assert.Throws<InvalidOperationException>(() => new [] { null, "2", "3" }.SingleMaybe());
            Assert.Throws<InvalidOperationException>(() => new [] { null, "2", "3" }.AsQueryable().SingleMaybe());
            Assert.Throws<InvalidOperationException>(() => new[] { "1", "2", "3" }.SingleMaybe());
            Assert.Throws<InvalidOperationException>(() => new[] {"1", "2", "3"}.AsQueryable().SingleMaybe());
            Assert.Equal(Maybe<string>.Not, new string[0].SingleMaybe());
            Assert.Equal(Maybe<string>.Not, new string[0].AsQueryable().SingleMaybe());
        }

        private string UseNullable(int? x)
        {
            return x.ToString();
        }

        private string UseMaybe(Maybe<int> x)
        {
            return x.ToString();
        }

        /// <summary>
        /// This wrapper class acts like the given IQueryable, but with virtual methods that can be
        /// mocked.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public abstract class QueryableMockBase<T> : IQueryable<T>
        {
            private readonly IQueryable<T> _data;

            protected QueryableMockBase(IQueryable<T> data)
            {
                _data = data;
            }

            public virtual IEnumerator<T> GetEnumerator()
            {
                return _data.GetEnumerator();
            }

            public virtual Type ElementType => _data.ElementType;
            public virtual Expression Expression => _data.Expression;
            public virtual IQueryProvider Provider => _data.Provider;
            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }
    }
}