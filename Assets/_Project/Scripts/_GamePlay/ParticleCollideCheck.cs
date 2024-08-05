using UnityEngine;

public class ParticleCollideCheck : MonoBehaviour
{
    public string ID;
    public void Init(string id)
    {
        ID = id;
    }
    
    private void OnTriggerEnter(Collider other)
    {
       // Debug.Log(other.gameObject);
        var isParticle = other.gameObject.GetComponent<ParticleCollideCheck>();
        if (isParticle!=null)
        {
            if (ID!=isParticle.ID)
            {
                //gameObject.SetActive(false);
            }
        }
    }
}
