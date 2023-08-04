using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FishScript : MonoBehaviour
{
    public int fishIdx;
    public bool isBad;


    [SerializeField]
    public string fishName;
    public int price;
    public float weight;

    public float speedZ;

    public AudioClip getSound;
    Transform net;

    // fishIdx�� ���� ����� Ư�� �ʱ�ȭ
    public void Initialize()
    {
        switch (fishIdx)
        {
            case 11: // ��ġ
                fishName = "��ġ";
                weight = 0.2f;
                price = 3;
                speedZ = -1.5f;
                break;
            case 12: // ��ġ
                fishName = "��ġ";
                weight = 0.5f;
                price = 7;
                speedZ = -1.25f;
                break;
            case 13: // ����
                fishName = "����";
                weight = 2.0f;
                price = 18;
                speedZ = -0.7f;
                break;
            case 14: // ����
                fishName = "����";
                weight = 4.0f;
                price = 20;
                speedZ = -1.2f;
                break;
            case 15: // �۾�
                fishName = "�۾�";
                weight = 7.0f;
                price = 56;
                speedZ = -0.9f;
                break;
            case 16: // ����
                fishName = "����";
                weight = 10.0f;
                price = 80;
                speedZ = -0.6f;
                break;
                // ������ ����⵵ ���⿡ �߰�...
        }

    }

    void PerformBehavior(int idx)
    {
        switch (idx)
        {
            case 11: // ��ġ
                break;
            case 12: // ��ġ
                // ��ġ�� �ൿ ����
                break;
            case 13: // ����
                break;
            case 14: // ����
                MoveTowardNet(10f, 3f); // �׹��� ���� ���� (�׹� ������ ������ �̵�)
                break;
            case 15: // �۾�
                MoveTowardNet(10f, -1.5f); // �׹��� ���� (�׹����� �־���)
                break;
            case 16: // ����
                // ������ �ൿ ����
                break;
                // ������ ����⵵ ���⿡ �߰�...
        }
    }

    // Features

    void MoveTowardNet(float startDistance, float speedMultiplier)
    {
        
        var xDistoNet = Mathf.Abs(transform.position.x - net.transform.position.x); //  ������ �׹� ������ �������(X) �Ÿ�
        var zDistoNet = Mathf.Abs(transform.position.z - net.transform.position.z); //  ������ �׹� ������ ��������(Z) �Ÿ�

        var netWidth = GameController.GetInstance().player.netWidth; // ���� �׹��� ����

        if(zDistoNet < startDistance) // ���� ���� �ȿ����� �׹� ����
        {
            // ������ �׹� ������ �������(X) �Ÿ��� �׹� ������ 25% ������ ��� (�׹� ���� ��� 50% ����)
            if (xDistoNet <= 0.5f * 0.5f *netWidth)
            {   
                // �״�� ���� (X���� �̵� ����)
            }
            else // �׹� ���� ��
            {
                float directionToNet = Mathf.Sign(net.position.x - transform.position.x); // �׹��� x ��ǥ ������ ���
                transform.position += new Vector3(directionToNet * speedMultiplier * Time.deltaTime, 0, 0); // �׹��� x ��ǥ �������� �̵�
            }
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
        // -z�������� �̵�

        var newZ = transform.position.z + speedZ * GameController.GetInstance().player.speedZ *Time.deltaTime;
        transform.position = new Vector3(transform.position.x, 0, newZ);

        // Ư���� ���� �ൿ ����
        PerformBehavior(fishIdx);
    }
}
