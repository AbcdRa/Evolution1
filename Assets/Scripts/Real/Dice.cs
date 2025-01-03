using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;

public class Dice : MonoBehaviour
{
    [SerializeField] private Transform[] sides;
    [SerializeField] private Rigidbody _rigidbody;
    public bool isFlying = false;
    public UnityEvent OnStopFlying;

    // Update is called once per frame

    private void Start()
    {
        this._rigidbody = GetComponent<Rigidbody>();
    }

    public void Roll()
    {

        Vector3 randomVectorUp = new Vector3(Random.value, Random.value, Random.value) + Vector3.one * -0.5f;
        this._rigidbody.AddForce((Vector3.up + randomVectorUp) * 400);
        this._rigidbody.AddTorque(Random.value * 5, Random.value * 5, Random.value * 5);
        isFlying = true;
        StartCoroutine(RollCoroutine());
    }


    public IEnumerator RollCoroutine()
    {
        while (isFlying)
        {
            yield return new WaitForSeconds(1f);
            isFlying = this._rigidbody.velocity.magnitude > 0.0001f;
        }
        OnStopFlying.Invoke();
        yield return null;
    }

    public int GetResult()
    {
        int result = 0;
        for (int i = 1; i < sides.Length; i++)
        {
            if (sides[i].transform.position.y > sides[result].transform.position.y)
            {
                result = i;
            }
        }
        return result + 1;
    }

}
