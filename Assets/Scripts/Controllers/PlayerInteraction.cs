
using UnityEngine;


public class PlayerInteraction : MonoBehaviour
{
    [SerializeField] private int playerId;

    public void Pass()
    {
        GameMananger.instance.Pass(playerId);
    }

    public void HandleSelection(SelectionableObject selection)
    {
        Debug.Log(selection.ToString());
    }
}

