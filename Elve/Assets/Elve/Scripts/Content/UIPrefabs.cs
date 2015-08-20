using UnityEngine;


public class UIPrefabs : Singleton<UIPrefabs>
{
	[System.Diagnostics.Conditional("UNITY_ASSERTIONS")]
	private static void AssertExists(GameObject prefabObj, string varName)
	{
		UnityEngine.Assertions.Assert.IsNotNull(prefabObj, "'" + varName + "' is null!");
	}


	public static Transform Create_JobPlantWoodSeed(Vector2i jobPos)
	{
		GameObject go = (GameObject)Instantiate(Instance.JobPlantWoodSeed);
		Transform tr = go.transform;
		tr.position = new Vector3(jobPos.x + 0.5f, jobPos.y + 0.5f, tr.position.z);
		return tr;
	}


	public GameObject JobPlantWoodSeed;


	protected override void Awake()
	{
		base.Awake();

		AssertExists(JobPlantWoodSeed, "Job Plant Wood Seed");
	}
}