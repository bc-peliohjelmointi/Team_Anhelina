// T‰m‰ luokka voidaan tallentaa Unity-editoriin [System.Serializable]-m‰‰ritteen avulla
[System.Serializable]
public class Mission
{
    public string missionName; // Teht‰v‰n nimi
    public bool completed;     // Onko teht‰v‰ suoritettu

    // Konstruktori ó luodaan uusi teht‰v‰ nimell‰, joka ei ole viel‰ suoritettu
    public Mission(string name)
    {
        missionName = name;
        completed = false; // Uusi teht‰v‰ ei ole viel‰ valmis
    }
}