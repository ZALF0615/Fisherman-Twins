using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FishScript : MonoBehaviour
{
    public int fishIdx;

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
                // ������ ����⵵ ���⿡ �߰�...
        }

        weight = Random.Range(minWeight, maxWeight);
        price = weight * priceMultiplier;

        // ... ������ �ʵ� �ʱ�ȭ ...
    }

    private void Start()
    {
        Initialize();
    }
}
