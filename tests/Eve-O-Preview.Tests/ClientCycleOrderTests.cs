using System;
using System.Collections.Generic;
using System.Linq;
using EveOPreview.Services;
using Xunit;

namespace EveOPreview.Tests
{
	public class ClientCycleOrderTests
	{
		private static (IntPtr Handle, string Title) Client(long handle, string title)
		{
			return (new IntPtr(handle), title);
		}

		[Fact]
		public void Sort_OrdersByTitleCaseInsensitive()
		{
			var clients = new[]
			{
				Client(10, "Charlie"),
				Client(20, "alice"),
				Client(30, "Bravo"),
			};

			var ordered = ClientCycleOrder.Sort(clients, c => c.Title, c => c.Handle);

			Assert.Equal(new[] { "alice", "Bravo", "Charlie" }, ordered.Select(c => c.Title));
		}

		[Fact]
		public void Sort_UsesHandleAsStableTiebreakerForEqualTitles()
		{
			// Multiple character-select windows all report the title "EVE".
			var clients = new[]
			{
				Client(300, "EVE"),
				Client(100, "EVE"),
				Client(200, "EVE"),
			};

			var ordered = ClientCycleOrder.Sort(clients, c => c.Title, c => c.Handle);

			Assert.Equal(new long[] { 100, 200, 300 }, ordered.Select(c => c.Handle.ToInt64()));
		}

		[Fact]
		public void Sort_IsStableAcrossRuns_RegardlessOfInputOrder()
		{
			var first = new[] { Client(30, "Cindy"), Client(10, "Adam"), Client(20, "Beth") };
			var second = new[] { Client(20, "Beth"), Client(30, "Cindy"), Client(10, "Adam") };

			var orderedFirst = ClientCycleOrder.Sort(first, c => c.Title, c => c.Handle).Select(c => c.Title);
			var orderedSecond = ClientCycleOrder.Sort(second, c => c.Title, c => c.Handle).Select(c => c.Title);

			Assert.Equal(orderedFirst, orderedSecond);
		}

		[Fact]
		public void Sort_HandlesNullTitle()
		{
			var clients = new[] { Client(10, null), Client(20, "Alice") };

			var ordered = ClientCycleOrder.Sort(clients, c => c.Title, c => c.Handle);

			Assert.Equal(new[] { null, "Alice" }, ordered.Select(c => c.Title));
		}

		[Theory]
		[InlineData(0, 3, true, 1)]
		[InlineData(2, 3, true, 0)]   // wraps forward
		[InlineData(0, 3, false, 2)]  // wraps backward
		[InlineData(1, 3, false, 0)]
		public void GetNextIndex_WrapsInBothDirections(int currentIndex, int count, bool forwards, int expected)
		{
			Assert.Equal(expected, ClientCycleOrder.GetNextIndex(currentIndex, count, forwards));
		}

		[Fact]
		public void GetNextIndex_TreatsMissingCurrentAsStart()
		{
			// Active client not in the list (currentIndex = -1): forward => first advance => index 1.
			Assert.Equal(1, ClientCycleOrder.GetNextIndex(-1, 3, true));
		}

		[Fact]
		public void GetNextIndex_ReturnsNegativeWhenEmpty()
		{
			Assert.Equal(-1, ClientCycleOrder.GetNextIndex(0, 0, true));
		}

		[Fact]
		public void Sort_ThrowsOnNullArguments()
		{
			var clients = new[] { Client(1, "A") };

			Assert.Throws<ArgumentNullException>(() => ClientCycleOrder.Sort<(IntPtr, string)>(null, c => "", c => IntPtr.Zero));
			Assert.Throws<ArgumentNullException>(() => ClientCycleOrder.Sort(clients, null, c => c.Handle));
			Assert.Throws<ArgumentNullException>(() => ClientCycleOrder.Sort(clients, c => c.Title, null));
		}
	}
}
