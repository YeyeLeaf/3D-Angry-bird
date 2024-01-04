using Unity.VisualScripting;
using UnityEngine;

public class BombCollider : BirdCommonVar
{
    [SerializeField] public float triggerForce;
    [SerializeField] float explosionRadius;
    [SerializeField] float explosionForce;
    [SerializeField] GameObject particles;
    [SerializeField] AudioSource audioSource;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        base.Update();

        if(Input.GetKeyDown(KeyCode.R)) 
        {
            Explosion();
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.tag != "Player")
        {
            //若碰撞到物件則三秒後炸彈消失
            HasCollider = true;
            Destroy(gameObject, 3.0f);
        }
    }

    void closeRenderer(GameObject Obj)
    {
        if (Obj.GetComponent<Renderer>() != null)
            Obj.GetComponent<Renderer>().enabled = false;

        foreach (Transform child in Obj.transform)
            closeRenderer(child.gameObject);
    }

    public void Explosion()
    {
        audioSource.Play();
        //抓取圓形範圍內所有物件
        var surroundingObject = Physics.OverlapSphere(transform.position, explosionRadius);

        //遍歷物件，如果物件有Rigidbody就施加爆炸給它
        foreach (var obj in surroundingObject)
        {
            var rb = obj.GetComponent<Rigidbody>();
            if (rb == null) continue;

            //參數為爆炸總力, 當前位置, 爆炸半徑, 因此會自動計算此物體距離中心的距離進行不同位置爆炸力不同的計算;
            rb.AddExplosionForce(explosionForce, transform.position, explosionRadius);
            if (obj.GetComponent<Pig>() != null)
                obj.GetComponent<Pig>().explosiveDamage(explosionForce);
        }

        //生成爆炸特效
        Instantiate(particles, transform.position, Quaternion.identity);

        //刪除炸彈
        Destroy(gameObject, 0.5f);
        closeRenderer(gameObject);
        gameObject.GetComponent<Rigidbody>().isKinematic = true;
    }
}
