using UnityEngine;


public enum SelectionableObjectSpecification
{
    None,HandCard, AnimalCard, PropCard, Deck, Food
}

public class SelectionableObject : MonoBehaviour
{
    [SerializeField] private GameObject selectionObject;
    [SerializeField] private GameObject parent;
    [SerializeField] private SelectionableObjectSpecification specification = SelectionableObjectSpecification.None;
    private bool isActive = false;
    public int idOwner = -1;

    public void SetSpecification(SelectionableObjectSpecification specification) { this.specification = specification; }
    public SelectionableObjectSpecification GetSpecification() { return specification; }

    public void Selection()
    {
        if (!isActive)
        {
            isActive = true;
            
            
            if (specification == SelectionableObjectSpecification.HandCard)
            {
                parent.transform.localPosition -= new Vector3(0, 0, 0.02f);
                
            }
            selectionObject.SetActive(isActive);
        }

    }

    public void UnSelection()
    {
        if (isActive)
        {
            isActive = false;
            selectionObject.SetActive(isActive);
            if (specification == SelectionableObjectSpecification.HandCard)
            {
                parent.transform.localPosition += new Vector3(0, 0, 0.02f);
            }
        }
    }

    public GameObject GetLogicParent()
    {
        return parent;
    }

    public void SetLogicParent(GameObject parent)
    {
        this.parent = parent;
    }
}
