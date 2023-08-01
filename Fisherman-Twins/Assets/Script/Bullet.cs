using System.Collections;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float speed;
    public float lifetime = 5f; // �Ѿ��� �ڵ����� �ı��Ǳ������ �ð�

    void Start()
    {
        // ���� �ð� �� �Ѿ� �ı�
        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        // �Ѿ� �̵�
        transform.position += transform.forward * speed * Time.deltaTime;
    }

    void OnTriggerEnter(Collider other)
    {
        // "Fish" �±׸� ���� ������Ʈ���� �浹 �˻�
        if (other.tag == "Fish")
        {
            // �Ѿ˰� ����� �ı�
            Destroy(other.gameObject);
            Destroy(gameObject);
        }
    }
}
