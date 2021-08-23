using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class PlayerController : NetworkBehaviour {

    [SyncVar]
    public bool aquario = false;

    [SyncVar]
    public GameObject fish;

    [SyncVar]
    public GameObject parede;
    public NetworkIdentity identity;

    public GameObject PlayerObj;

    [SyncVar]
    public bool atualizado = false;

    void Start () {
        identity = GetComponentInParent<NetworkIdentity> ();
    }

    void ativaParede () {
        foreach (CameraDevice c in PlayerObj.GetComponentsInChildren<CameraDevice>()) {
            c.gameObject.SetActive (true);
            atualizado = true;
        }
    }

    void Update () {
        if (identity.isLocalPlayer) {
            if ((aquario) && (!atualizado)) {
                this.ativaParede ();
            }
        }
    }
}