using System.Collections.Generic;
using Vector2 = UnityEngine.Vector2;
using MyContext = ChooseTreePatternContext;


/// <summary>
/// Provides the items for a context menu that selects the material of seed to plant.
/// The context is the world position of the voxel to plant in.
/// </summary>
public class ContextMenuInitializer_TreePatternChoices : ContextMenuInitializer<MyContext>
{
	#region Menu items

	private class ContextMenuItem_Oak : IContextMenuItem<MyContext>
	{
		public string Text { get { return "Oak"; } }

		public bool IsItemAvailable(MyContext context)
		{
			return true;
		}

		public void OnSelected(MyContext context, Vector2 menuScreenPos)
		{
			JobManager.Instance.AddNewJob(new Job_PlantSeed(context.WorldPos, context.SeedType,
															new GrowPattern_Oak()));
		}
	}
	
	#endregion

	public override List<IContextMenuItem<MyContext>> GetAllItems()
	{
		return new List<IContextMenuItem<MyContext>>()
		{
			new ContextMenuItem_Oak(),
		};
	}
}