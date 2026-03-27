using UnityEngine;
public class DrawerSystem : MonoBehaviour
{
    public Transform drawersParent;
    public GameObject cardPrefab;
    public Vector3 cardOffsetInDrawer = new Vector3(0f, 0.05f, 0f);
    public float drawerOpenDistance = 0.25f;
    public float drawerOpenSpeed = 3f;
    public Vector3 drawerOpenAxis = Vector3.forward;
    private Transform[] allDrawers;
    private Transform cardDrawer;
    private Vector3 drawerClosedLocalPos;
    private bool drawerOpened = false, isAnimating = false;
    private GameObject spawnedCard;

    void Start()
    {
        if (drawersParent != null)
        {
            allDrawers = new Transform[drawersParent.childCount];
            for (int i = 0; i < allDrawers.Length; i++) allDrawers[i] = drawersParent.GetChild(i);
        }
        if (allDrawers != null && allDrawers.Length > 0)
        {
            cardDrawer = allDrawers[Random.Range(0, allDrawers.Length)];
            drawerClosedLocalPos = cardDrawer.localPosition;
        }
    }

    void Update()
    {
        if (!isAnimating || cardDrawer == null) return;
        Vector3 target = drawerClosedLocalPos + drawerOpenAxis * drawerOpenDistance;
        cardDrawer.localPosition = Vector3.MoveTowards(cardDrawer.localPosition, target, drawerOpenSpeed * Time.deltaTime);
        if (Vector3.Distance(cardDrawer.localPosition, target) < 0.005f)
        { cardDrawer.localPosition = target; isAnimating = false; SpawnCard(); }
    }

    public void OpenCardDrawer()
    {
        if (drawerOpened || cardDrawer == null) return;
        drawerOpened = true; isAnimating = true;
    }

    void SpawnCard()
    {
        if (cardPrefab == null) return;
        spawnedCard = Instantiate(cardPrefab, cardDrawer.TransformPoint(cardOffsetInDrawer), cardDrawer.rotation);
    }
}