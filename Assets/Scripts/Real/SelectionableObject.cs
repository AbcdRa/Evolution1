using System;
using UnityEngine;


public enum SOSpecification
{
    None,HandCard, Spot, AnimalCard, PropCard, Deck, Food
}

public class SelectionableObject : MonoBehaviour
{
    [SerializeField] private GameObject selectionObject;
    [SerializeField] private GameObject _parent;
    [SerializeField] private SOSpecification _specification = SOSpecification.None;

    public SOSpecification specification { get => _specification; set => _specification = value; }
    public GameObject parent => _parent;
    private bool isActive = false;
    public int ownerId = -1;


    public void Selection()
    {
        if (!isActive)
        {
            isActive = true;
            
            
            if (specification == SOSpecification.HandCard)
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
            if (specification == SOSpecification.HandCard)
            {
                parent.transform.localPosition += new Vector3(0, 0, 0.02f);
            }
        }
    }

    public void SetSpecificationAndId(SOSpecification specification, int ownerId)
    {
        this.specification = specification;
        this.ownerId = ownerId;
    }
}
