using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using EveOPreview.Services;
using Xunit;

namespace EveOPreview.Tests
{
	public class PreviewGridLayoutTests
	{
		// A second monitor (2560x1440) positioned to the right of a 1920x1080 primary.
		private static readonly Rectangle SecondMonitor = new Rectangle(1920, 0, 2560, 1440);

		[Theory]
		[InlineData(1, 1, 1)]
		[InlineData(2, 2, 1)]
		[InlineData(3, 2, 2)]
		[InlineData(4, 2, 2)]
		[InlineData(5, 3, 2)]
		[InlineData(6, 3, 2)]
		[InlineData(9, 3, 3)]
		public void GridDimensions_AreBalanced(int count, int expectedColumns, int expectedRows)
		{
			Assert.Equal(expectedColumns, PreviewGridLayout.ColumnCount(count));
			Assert.Equal(expectedRows, PreviewGridLayout.RowCount(count));
		}

		[Theory]
		[InlineData(1)]
		[InlineData(2)]
		[InlineData(3)]
		[InlineData(4)]
		[InlineData(6)]
		[InlineData(9)]
		public void ComputeCells_ReturnsOneCellPerClient(int count)
		{
			Assert.Equal(count, PreviewGridLayout.ComputeCells(count, SecondMonitor, 8).Count);
		}

		[Theory]
		[InlineData(2)]
		[InlineData(4)]
		[InlineData(6)]
		public void ComputeCells_AllCellsStayWithinTargetMonitor(int count)
		{
			List<Rectangle> cells = PreviewGridLayout.ComputeCells(count, SecondMonitor, 8);

			Assert.All(cells, cell =>
			{
				Assert.True(cell.X >= SecondMonitor.X);
				Assert.True(cell.Y >= SecondMonitor.Y);
				Assert.True(cell.Right <= SecondMonitor.Right);
				Assert.True(cell.Bottom <= SecondMonitor.Bottom);
				Assert.True(cell.Width > 0 && cell.Height > 0);
			});
		}

		[Fact]
		public void ComputeCells_CellsAreEquallySizedAndNonOverlapping()
		{
			List<Rectangle> cells = PreviewGridLayout.ComputeCells(4, SecondMonitor, 8);

			// Equal sizes.
			Assert.Single(cells.Select(c => c.Size).Distinct());

			// No two previews overlap.
			for (int i = 0; i < cells.Count; i++)
			{
				for (int j = i + 1; j < cells.Count; j++)
				{
					Assert.False(cells[i].IntersectsWith(cells[j]));
				}
			}
		}

		[Fact]
		public void ComputeCells_OffsetsByMonitorOrigin()
		{
			// A single preview should sit inside the second monitor, not at (0,0).
			Rectangle cell = PreviewGridLayout.ComputeCells(1, SecondMonitor, 8).Single();
			Assert.True(cell.X >= SecondMonitor.X);
		}

		[Fact]
		public void ComputeCells_ReturnsEmptyForNoClientsOrEmptyArea()
		{
			Assert.Empty(PreviewGridLayout.ComputeCells(0, SecondMonitor, 8));
			Assert.Empty(PreviewGridLayout.ComputeCells(4, Rectangle.Empty, 8));
		}
	}
}
