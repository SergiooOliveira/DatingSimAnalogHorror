using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "New Node", menuName = "Dialog /Node")]
public class DialogNode : ScriptableObject
{
    [TextArea(3, 10)]
    public string dialogText;
    //public List<Choices> choices;
}