using UnityEngine;


/// <summary>
/// Contains an event to be triggered when this object's animation ends.
/// </summary>
public class ElveAnimFinishEvent : MonoBehaviour
{
	public delegate void AnimFinishCallback();

	/// <summary>
	/// This event is cleared out every time it's triggered.
	/// </summary>
	public event AnimFinishCallback OnAnimFinished;


	public void TriggerEvent()
	{
		if (OnAnimFinished != null)
		{
			//Clear out the event before raising it so that new callbacks can be added to it.
			AnimFinishCallback tempEvent = (OnAnimFinished + DummyFunc);
			OnAnimFinished = null;
			tempEvent();
		}
	}


	private static void DummyFunc() { }
}