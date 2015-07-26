/// <summary>
/// A single atomic movement, generally from one voxel to an adjacent one.
/// </summary>
public abstract class ElveState
{
	protected static Vector2i ToPosI(UnityEngine.Vector2 pos)
	{
		return new Vector2i((int)pos.x, (int)pos.y);
	}
	protected static Vector2i ToPosI(UnityEngine.Vector3 pos)
	{
		return new Vector2i((int)pos.x, (int)pos.y);
	}


	public ElveBehavior Owner { get; private set; }


	public ElveState(ElveBehavior owner) { Owner = owner; }


	public virtual void OnStateStarting(ElveState oldState) { }
	public virtual void OnStateEnding(ElveState nextState) { }

	public abstract void Update();

}