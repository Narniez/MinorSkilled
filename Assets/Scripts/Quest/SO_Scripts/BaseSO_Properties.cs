using UnityEngine;

public abstract class BaseSO_Properties : ScriptableObject
{
    public string questName;
    public string questDescription;
    public int id;

    public abstract bool isCompleted { get; }
    public abstract void MarkAsCompleted();
}
