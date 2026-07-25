using EveOPreview.Presenters;
using EveOPreview.View;

namespace EveOPreview.Services
{
	public interface IThumbnailManager
	{
		void AttachPresenter(IMainFormPresenter presenter);

		void Start();
		void Stop();

		void UpdateCycleGroupIndicator();
		void UpdateThumbnailsSize();
		void UpdateThumbnailFrames();

		void UpdateActionBindings();

		IThumbnailView GetClientByTitle(string title);
		IThumbnailView GetClientByPointer(System.IntPtr ptr);
		IThumbnailView GetActiveClient();
	}
}
