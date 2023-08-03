using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FishScript : MonoBehaviour
{
    public int fishIdx;
    public bool isBad;

    // ���� ��
    [SerializeField]
    public float price;
    public float weight;

    // �� ����� ������
    public string fishName;
    float minWeight;
    float maxWeight;
    float priceMultiplier;

    public AudioClip getSound;
    Transform net;

    // fishIdx�� ���� ����� Ư�� �ʱ�ȭ
    public void Initialize()
    {
        switch (fishIdx)
        {
            case 11: // ��ġ
                fishName = "��ġ";
                minWeight = 0.005f;
                maxWeight = 0.015f;
                priceMultiplier = 1f;
                break;
            case 13: // ����
                fishName = "����";
                minWeight = 0.5f;
                maxWeight = 0.7f;
                priceMultiplier = 0.2f;
                break;
            case 15: // �۾�
                fishName = "�۾�";
                minWeight = 3f;
                maxWeight = 5f;
                priceMultiplier = 0.3f;
                break;
            case 16: // ����
                fishName = "����";
                minWeight = 5f;
                maxWeight = 10f;
                priceMultiplier = 0.4f;
                break;
            case 27: // ����
                fishName = "����";
                minWeight = 0f;
                maxWeight = 0f;
                priceMultiplier = 0f;
                break;
                // ������ ����⵵ ���⿡ �߰�...
        }

        weight = Random.Range(minWeight, maxWeight);
        price = weight * priceMultiplier;

        // ... ������ �ʵ� �ʱ�ȭ ...
    }

    void PerformBehavior(int idx)
    {
        switch (idx)
        {
            case 11: // ��ġ
                // ��ġ�� �ൿ ����
                break;
            case 13: // ����
                MoveTowardNet(10f, 3f); // �׹��� ���� ���� (�׹� ������ ������ �̵�)
                break;
            case 15: // �۾�
                MoveTowardNet(10f, -1.5f); // �׹��� ���� (�׹����� �־���)
                break;
            case 16: // ����
                // ������ �ൿ ����
                break;
            case 27: // ����
                InflateNearNet(10f, 10f, 3.0f); // �׹� �����̿��� ��Ǯ�����
                break;
                // ������ ����⵵ ���⿡ �߰�...
        }
    }

    // Features

    void MoveTowardNet(float startDistance, float speedMultiplier)
    {
        // ������ �׹� ������ �Ÿ��� ���
        float distanceToNet = Vector3.Distance(transform.position, net.transform.position);

        // �Ÿ��� startDistance ������ ���
        if (distanceToNet <= startDistance)
        {
            // �׹��� x ��ǥ ������ ���
            float directionToNet = Mathf.Sign(net.position.x - transform.position.x);

            // �׹��� x ��ǥ �������� �̵�
            transform.position += new Vector3(directionToNet * speedMultiplier * Time.deltaTime, 0, 0);
        }


    }

    private bool hasStartedInflating = false;  // ��Ǯ������� ������ ���� �ִ����� ��Ÿ���� ����
    void InflateNearNet(float inflateDistance, float inflateSpeed, float maxScale)
    {
        // ������ �׹� ������ �Ÿ��� ���
        float distanceToNet = Vector3.Distance(transform.position, net.transform.position);

        // �Ÿ��� inflateDistance �����̰ų� �̹� ��Ǯ������� ������ ���� �ִٸ�
        if (distanceToNet <= inflateDistance || hasStartedInflating)
        {
            // ��Ǯ������� ������ ���� �ִٴ� ���� ǥ��
            hasStartedInflating = true;

            // ��Ǯ�� �������� �������� ���������� ����
            float newScale = Mathf.Min(transform.localScale.x + inflateSpeed * Time.deltaTime, maxScale);
            transform.localScale = new Vector3(newScale, newScale, newScale);
        }
    }




    private void Start()
    {
        net = GameObject.FindGameObjectWithTag("net").transform;

        Initialize();
    }
    private void Update()
    {
        // Ư���� ���� �ൿ ����
        PerformBehavior(fishIdx);
    }
}
