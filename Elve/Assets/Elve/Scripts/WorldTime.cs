using UnityEngine;


/// <summary>
/// World time. Can be sped up/slowed down by the player on command.
/// </summary>
public class WorldTime : Singleton<WorldTime>
{
	public static float TimeScale { get { return Instance.timeScale; } set { Instance.timeScale = value; } }
	public static bool IsPaused { get { return Instance.timeScale == 0.0f; } }

	public static float DeltaTime { get { return Instance.deltaTime; } set { Instance.deltaTime = value; } }
	public static float TotalTime { get { return Instance.totalTime; } set { Instance.totalTime = value; } }

	public static float FixedDeltaTime { get { return Instance.timeScale * Time.fixedDeltaTime; } }


	private float deltaTime = 0.000001f,
				  totalTime = 0.0f,
				  timeScale = 1.0f;


	protected override void Awake()
	{
		base.Awake();

		deltaTime = Time.deltaTime;
		totalTime = 0.0f;
		timeScale = 1.0f;
	}
	void Update()
	{
		deltaTime = Time.deltaTime * timeScale;
		totalTime += deltaTime;
	}
}