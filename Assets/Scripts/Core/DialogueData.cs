using UnityEngine;

namespace Core
{
    [CreateAssetMenu(fileName = "NewDialogue", menuName = "Dialogue/Dialogue Data", order = 0)]
    public class DialogueData : ScriptableObject
    {
        [TextArea]
        public string[] lines;
    }
}