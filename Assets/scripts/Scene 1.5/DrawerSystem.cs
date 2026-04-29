using UnityEngine;
// manages all 600 drawers
// picks one random drawer at scene start to hold the card
// when OpenCardDrawer() is called (by computer on correct code), animates that
// drawer open and spawns the card prefab inside it
// all drawers must be children of the drawersParent object
public class DrawerSystem : MonoBehaviour
{
    // the parent object that contains all 600 drawer children
    public Transform drawersParent;
    // the card prefab to spawn when the right drawer opens
    public GameObject cardPrefab;
    // local position offset inside drawer where card will appear
    public Vector3 cardOffsetInDrawer = new Vector3(0f, 0.05f, 0f);
    // how far the drawer slides out in units
    public float drawerOpenDistance = 0.25f;
    public float drawerOpenSpeed = 3f;
    // which direction the drawer slides, forward by default, change if needed
    public Vector3 drawerOpenAxis = Vector3.forward;
    private Transform[] allDrawers;
    private Transform cardDrawer;
    private Vector3 drawerClosedLocalPos;
    private bool drawerOpened = false;
    private bool isAnimating = false;
    private GameObject spawnedCard;

    void Start()
    {
        // build array from all children of the parent object
        if (drawersParent != null)
        {
            allDrawers = new Transform[drawersParent.childCount];
            for (int i = 0; i < allDrawers.Length; i++)
                allDrawers[i] = drawersParent.GetChild(i);
        }

        // randomly pick which drawer gets the card this playthrough
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
        cardDrawer.localPosition = Vector3.MoveTowards(
            cardDrawer.localPosition, target, drawerOpenSpeed * Time.deltaTime);

        // when close enough to target, snap and spawn the card
        if (Vector3.Distance(cardDrawer.localPosition, target) < 0.005f)
        {
            cardDrawer.localPosition = target;
            isAnimating = false;
            SpawnCard();
        }
    }

    // called by ComputerInteraction when player enters the correct code
    public void OpenCardDrawer()
    {
        if (drawerOpened || cardDrawer == null) return;
        drawerOpened = true;
        isAnimating = true;
    }

    void SpawnCard()
    {
        if (cardPrefab == null) return;
        spawnedCard = Instantiate(cardPrefab,
            cardDrawer.TransformPoint(cardOffsetInDrawer), cardDrawer.rotation);
    }
}