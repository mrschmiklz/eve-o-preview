namespace EveOPreview.View
{
	sealed class ThumbnailDescription : IThumbnailDescription
	{
		public ThumbnailDescription(string title, bool isPriority)
		{
			this.Title = title;
			this.IsPriority = isPriority;
		}

		public string Title { get; set; }
		public bool IsPriority { get; set; }
	}
}