using System.Threading;
using System.Threading.Tasks;
using EveOPreview.Mediator.Messages;
using EveOPreview.Services;
using MediatR;

namespace EveOPreview.Mediator.Handlers.Configuration
{
	sealed class CycleBindingsUpdatedHandler : INotificationHandler<CycleBindingsUpdated>
	{
		private readonly IThumbnailManager _manager;

		public CycleBindingsUpdatedHandler(IThumbnailManager manager)
		{
			this._manager = manager;
		}

		public Task Handle(CycleBindingsUpdated notification, CancellationToken cancellationToken)
		{
			this._manager.UpdatePrimaryCycleBindings();

			return Task.CompletedTask;
		}
	}
}
