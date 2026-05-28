using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class GravityBody : MonoBehaviour
{
    private static float GRAVITY_FORCE = 800f;
    [HideInInspector] public bool isActive = true;

    private bool _hasForced = false;
    private Vector3 _forcedDirection = Vector3.zero;
    private int _blockFrames = 0;

    public Vector3 GravityDirection
    {
        get
        {
            if (_hasForced) return _forcedDirection;
            if (_gravityAreas.Count == 0) return Vector3.zero;
            _gravityAreas.Sort((a, b) => a.Priority.CompareTo(b.Priority));
            return _gravityAreas.Last().GetGravityDirection(this).normalized;
        }
    }

    private Rigidbody _rigidbody;
    private List<GravityArea> _gravityAreas;

    void Start()
    {
        _rigidbody = GetComponent<Rigidbody>();
        _gravityAreas = new List<GravityArea>();
    }

    void FixedUpdate()
    {
        if (_blockFrames > 0)
        {
            _blockFrames--;

            // Quand le blocage se termine, refresh propre de la liste
            if (_blockFrames == 0)
            {
                // ← On garde _gravityAreas tel quel (il contient déjà la bonne zone)
                // On lève juste le flag forcé pour repasser en mode normal
                _hasForced = false;
                UnityEngine.Debug.Log($"[GravityBody] Blocage terminé, zone active : " +
                          $"{(_gravityAreas.Count > 0 ? _gravityAreas[0].name : "aucune")}");
            }
        }

        if (!isActive) return;

        _rigidbody.AddForce(
            GravityDirection * (GRAVITY_FORCE * Time.fixedDeltaTime),
            ForceMode.Acceleration);

        Quaternion upRotation = Quaternion.FromToRotation(transform.up, -GravityDirection);
        Quaternion newRotation = Quaternion.Slerp(
            _rigidbody.rotation,
            upRotation * _rigidbody.rotation,
            Time.fixedDeltaTime * 3f);
        _rigidbody.MoveRotation(newRotation);
    }

    // ← Bloqué tant que _blockFrames > 0
    public void AddGravityArea(GravityArea area)
    {
        if (_blockFrames > 0) return;
        _gravityAreas.Add(area);
    }

    public void RemoveGravityArea(GravityArea area)
    {
        if (_blockFrames > 0) return;
        _gravityAreas.Remove(area);
    }

    // Appelé par StarLauncher à l'atterrissage
    public void ForceGravityAtPosition(Vector3 worldPosition)
    {
        // Calculer la bonne direction via OverlapSphere
        Collider[] hits = Physics.OverlapSphere(worldPosition, 0.5f);
        GravityArea target = null;

        foreach (var hit in hits)
        {
            GravityArea area = hit.GetComponent<GravityArea>();
            if (area != null) { target = area; break; }
        }

        // Fallback : zone la plus proche
        if (target == null)
        {
            GravityArea[] all = FindObjectsByType<GravityArea>(FindObjectsSortMode.None);
            float best = float.MaxValue;
            foreach (var a in all)
            {
                float d = Vector3.Distance(worldPosition, a.transform.position);
                if (d < best) { best = d; target = a; }
            }
        }

        if (target == null) return;

        // Forcer la direction et bloquer tous les triggers pendant 30 frames
        _gravityAreas.Clear();
        _gravityAreas.Add(target);
        _forcedDirection = target.GetGravityDirection(this).normalized;
        _hasForced = true;
        _blockFrames = 30;

        UnityEngine.Debug.Log($"[GravityBody] Forcé sur : {target.name}, " +
                  $"direction : {_forcedDirection}, blocage 30 frames");
    }
}