using System.Collections;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float speed;
    public float lifetime = 5f; // 총알이 자동으로 파괴되기까지의 시간

    void Start()
    {
        // 일정 시간 후 총알 파괴
        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        // 총알 이동
        transform.position += transform.forward * speed * Time.deltaTime;
    }

    void OnTriggerEnter(Collider other)
    {
        // "Fish" 태그를 가진 오브젝트와의 충돌 검사
        if (other.tag == "Fish")
        {
            // 총알과 물고기 파괴
            Destroy(other.gameObject);
            Destroy(gameObject);
        }
    }
}
