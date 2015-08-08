using System.Collections.Generic;


/// <summary>
/// Provides the items for a context menu that selects the type of seed to plant.
/// No context is needed here, so just pass a dummy "byte" value for the context.
/// </summary>
public class ContextMenuInitializer_SeedChoices : ContextMenuInitializer<byte>
{
	private class ContextMenuItem_Wood : IContextMenuItem<byte>
	{
		public string Text { get { return "Wood"; } }

		public bool IsItemAvailable(byte dummy)
		{
			return true;
		}

		public void OnSelected(byte dummy)
		{
			//TODO: Create the "plant seed" job.
		}
	}

	public override List<IContextMenuItem<byte>> GetAllItems()
	{
		return new List<IContextMenuItem<byte>>()
		{
			new ContextMenuItem_Wood(),
		};
	}
}