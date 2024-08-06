using UnityEngine;

public class ParticleCollideCheck : MonoBehaviour
{
    public string ID;
    [SerializeField] private LayerMask layerCollide;
    [SerializeField, Range(0, 1)] private float range;
   
    public void Init(string id)
    {
        ID = id;
    }
    public bool Iscollide()
    {
        var collideRange= Physics.OverlapSphere(transform.position, range, layerCollide);
        if (collideRange.Length!=0)
        {
            int countOther=0;
            foreach (var collide in collideRange)
            {
                var getOther = collide.GetComponent<ParticleCollideCheck>();
                if (ID!=getOther.ID)
                {
                    countOther++;
                }
            }
            if (countOther!=0)
            {
                return true;
            }
            return false;
        }
        return false;
    }
    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(transform.position,range);
    }


}
