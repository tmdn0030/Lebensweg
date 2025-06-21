using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DissolveTrigger : MonoBehaviour
{
    [Header("Zu auflösende Objekte")]
    public List<GameObject> objectsToDissolve;

    [Header("Dissolve-Einstellungen")]
    public string dissolveParameter = "_DissolveAmount";
    public float dissolveSpeed = 1f;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("MainCamera"))
        {
            foreach (GameObject obj in objectsToDissolve)
            {
                StartCoroutine(DissolveObject(obj));
            }
        }
    }

    private IEnumerator DissolveObject(GameObject obj)
    {
        if (obj == null) yield break;

        Renderer rend = obj.GetComponent<Renderer>();
        if (rend == null) yield break;

        Material mat = rend.material;
        float dissolveAmount = 0f;

        while (dissolveAmount < 1f)
        {
            dissolveAmount += Time.deltaTime * dissolveSpeed;
            mat.SetFloat(dissolveParameter, dissolveAmount);
            yield return null;
        }

        // Optional: Objekt deaktivieren oder zerstören
        obj.SetActive(false);
    }
}
