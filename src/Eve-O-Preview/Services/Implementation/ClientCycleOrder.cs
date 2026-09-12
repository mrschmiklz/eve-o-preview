using System;
using System.Collections.Generic;
using System.Linq;

namespace EveOPreview.Services
{
	// Pure helpers for computing a stable, predictable client cycle order.
	//
	// Clients are ordered by title (character name), case-insensitive, with the
	// window handle as a stable tiebreaker. Ordering by handle alone (the previous
	// behaviour) is effectively random and changes whenever a client is restarted,
	// so "next client" was unpredictable. Ordering by name keeps the rotation the
	// same from session to session; the handle tiebreaker keeps clients that share
	// a title (e.g. multiple character-select windows titled "EVE") in a
	// deterministic position instead of swapping places between refreshes.
	internal static class ClientCycleOrder
	{
		public static List<T> Sort<T>(IEnumerable<T> clients, Func<T, string> titleSelector, Func<T, IntPtr> handleSelector)
		{
			if (clients == null)
			{
				throw new ArgumentNullException(nameof(clients));
			}

			if (titleSelector == null)
			{
				throw new ArgumentNullException(nameof(titleSelector));
			}

			if (handleSelector == null)
			{
				throw new ArgumentNullException(nameof(handleSelector));
			}

			return clients
				.OrderBy(client => titleSelector(client) ?? string.Empty, StringComparer.OrdinalIgnoreCase)
				.ThenBy(client => handleSelector(client).ToInt64())
				.ToList();
		}

		// Returns the index of the next client to activate, wrapping around the ends.
		// A negative currentIndex (active client not present in the list) is treated
		// as the start of the list, preserving the historical cycle behaviour.
		public static int GetNextIndex(int currentIndex, int count, bool isForwards)
		{
			if (count <= 0)
			{
				return -1;
			}

			if (currentIndex < 0)
			{
				currentIndex = 0;
			}

			return isForwards
				? (currentIndex + 1) % count
				: (currentIndex - 1 + count) % count;
		}
	}
}
