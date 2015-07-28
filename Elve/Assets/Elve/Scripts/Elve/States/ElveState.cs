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

	/// <summary>
	/// The type of function to be called when this state succeeds.
	/// </summary>
	public delegate void StateSucceedCallback(ElveState me);
	/// <summary>
	/// Raised when this state exits after successful completion.
	/// Child classes can call "Success()" to raise this event at the proper time.
	/// </summary>
	public event StateSucceedCallback OnStateSucceeded;


	public ElveState(ElveBehavior owner) { Owner = owner; }


	public virtual void OnStateStarting(ElveState oldState) { }
	public virtual void OnStateEnding(ElveState nextState) { }

	public abstract void Update();

	/// <summary>
	/// Raises the "OnStateSucceeded" event and switches to the given state.
	/// </summary>
	protected void Success(ElveState nextState = null)
	{
		Owner.CurrentState = nextState;

		if (OnStateSucceeded != null)
		{
			OnStateSucceeded(this);
		}
	}
}