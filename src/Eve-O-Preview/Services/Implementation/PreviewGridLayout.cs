using System;
using System.Collections.Generic;
using System.Drawing;

namespace EveOPreview.Services
{
	// Pure layout math for the "show all previews" overview grid.
	//
	// Splits a target screen area into equal cells for `count` previews, laid out
	// left-to-right, top-to-bottom, with a uniform margin around each cell so the
	// previews are evenly spaced. Kept free of WinForms/DWM dependencies so it can
	// be unit-tested (and verified for any monitor geometry) without a display.
	internal static class PreviewGridLayout
	{
		public static int ColumnCount(int count)
		{
			return count <= 0 ? 0 : (int)Math.Ceiling(Math.Sqrt(count));
		}

		public static int RowCount(int count)
		{
			int columns = ColumnCount(count);
			return columns == 0 ? 0 : (int)Math.Ceiling((double)count / columns);
		}

		// Returns one inner rectangle per preview (already inset by `margin`).
		public static List<Rectangle> ComputeCells(int count, Rectangle area, int margin)
		{
			List<Rectangle> cells = new List<Rectangle>(Math.Max(count, 0));

			if (count <= 0 || area.Width <= 0 || area.Height <= 0)
			{
				return cells;
			}

			if (margin < 0)
			{
				margin = 0;
			}

			int columns = ColumnCount(count);
			int rows = RowCount(count);
			int cellWidth = area.Width / columns;
			int cellHeight = area.Height / rows;

			for (int index = 0; index < count; index++)
			{
				int column = index % columns;
				int row = index / columns;

				int x = area.X + (column * cellWidth) + margin;
				int y = area.Y + (row * cellHeight) + margin;
				int width = Math.Max(cellWidth - (2 * margin), 1);
				int height = Math.Max(cellHeight - (2 * margin), 1);

				cells.Add(new Rectangle(x, y, width, height));
			}

			return cells;
		}
	}
}
