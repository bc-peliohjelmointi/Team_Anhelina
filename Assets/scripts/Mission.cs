[System.Serializable]
public class Mission
{
    public string missionName;
    public bool completed;

    public Mission(string name)
    {
        missionName = name;
        completed = false;
    }
}