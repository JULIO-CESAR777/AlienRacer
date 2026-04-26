using System;
using UnityEngine;
using UnityEngine.Serialization;

public class VFXController : MonoBehaviour
{
    public static VFXController Instance { get; private set; }

    //[SerializeField] private Transform _trailParticle;
    [Header("Collision VFX")]
    [FormerlySerializedAs("_collisionParticle")] [SerializeField] private Transform _collisionVFXTransform = default;
    [SerializeField] private GameObject _collisionVFXPrefab = default;
    [Header("Boost VFX")]
    [SerializeField] private Transform _boostVFXTransform = default;
    [SerializeField] private GameObject _boostVFXPrefab = default;
    [Header("Dead VFX")]
    [SerializeField] private Transform _deadVFXTransform = default;
    [SerializeField] private GameObject _deadVFXPrefab = default;
    [Header("Victory VFX")]
    [SerializeField] private Transform _victoryVFXTransform = default;
    [SerializeField] private GameObject _victoryVFXPrefab = default;

    private void Awake()
    {
        Instance = this;
    }

    public static VFXController GetInstance()
    {
        if (Instance == null)
        {
            Debug.LogError("VFXController instance is null. Make sure there is a VFXController Object in the scene.");
        }
        return Instance;
    }
    
    public void SpawnCollisionVFX(Vector3 position)
    {
        Instantiate(_collisionVFXPrefab, position, Quaternion.identity, _collisionVFXTransform);
    }
}
