using UnityEngine;
public sealed class CombatHurtbox : MonoBehaviour
{
    [SerializeField] private GameObject owener;

    public GameObject Owner => owener != null ? owener : gameObject;
}
